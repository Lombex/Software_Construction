using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSharpAPI.Controllers.Utils;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CSharpAPI.Controllers
{
    // Authentication controller - handles login and JWT token validation
    [Route("api/v2/auth")]
    [ApiController]
    public class C_Auth : ControllerBase
    {
        private readonly SQLite_Database _db; // Database context for user queries
        private readonly ITokenService _tokenService; // Service for generating JWT tokens
        private readonly ITokenRevocationService _tokenRevocationService; // Service for token revocation (logout)

        public C_Auth(SQLite_Database db, ITokenService tokenService, ITokenRevocationService tokenRevocationService)
        {
            _db = db;
            _tokenService = tokenService;
            _tokenRevocationService = tokenRevocationService;
        }

        // DTO for login credentials
        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        // POST /api/auth/login - Authenticates user and returns JWT token
        // Returns: 200 OK with token, 400 BadRequest if missing fields, 401 Unauthorized if invalid credentials
        [HttpPost("login")]
        [AllowAnonymous] // Public endpoint - no authentication required
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Validate username and password are provided
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest("Username and password are required.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.username == request.Username && u.active);

            if (user == null || string.IsNullOrEmpty(user.password)) return Unauthorized();

            var verifyPassword = C_Utils.VerifyPassword(request.Password, user.password);

            if (!verifyPassword)
            {
                Log.Warning("Failed login attempt for username: {Username}", request.Username);
                return Unauthorized();
            }

            Log.Information("User {Username} logged in successfully", request.Username);

            // If password was verified using legacy SHA256, upgrade to BCrypt
            if (C_Utils.IsLegacyHash(user.password))
            {
                user.password = C_Utils.HashPassword(request.Password);
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }

            // Generate JWT token with user id, username, and role claims
            var token = _tokenService.GenerateToken(user.id.ToString(), user.username ??
                string.Empty, user.role.ToString(), out var expiresAt);

            return Ok(new { token, expiresAt });
        }

        // GET /api/auth/me - Returns current user info from JWT token
        // Returns: 200 OK with user details, 401 Unauthorized if no valid token
        [HttpGet("me")]
        [Authorize] // Requires valid JWT token in Authorization header
        public IActionResult Me()
        {
            // Extract user information from JWT claims
            var username = User.Identity?.Name; // Username from token
            var id = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value; // User ID
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value; // User role (Admin/User)
            
            return Ok(new { id, username, role });
        }


        public class RegisterRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; } 
            public string? ConfirmPassword { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? Name { get; set; }
            public DateTime? BirthYear { get; set; }

        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validate username and password are provided
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password)) return BadRequest("Username and password are required.");
            if (request.Password != request.ConfirmPassword) return BadRequest("Passwords do not match.");
            if (!C_Utils.IsValidEmail(request.Email ?? "")) return BadRequest("Invalid email format.");
            if (!C_Utils.IsValidPhoneNumber(request.Phone ?? "")) return BadRequest("Invalid phone number format.");
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.username == request.Username);
            var existingEmail = await _db.Users.FirstOrDefaultAsync(u => u.email == request.Email);
            if (existingEmail != null) return BadRequest("Email already registered.");
            if (existingUser != null) return BadRequest("Username already exists.");
            
            var newUser = new M_Users
            {
                id = Guid.NewGuid(),
                username = request.Username,
                password = C_Utils.HashPassword(request.Password),
                name = request.Name,
                email = request.Email,
                phone = request.Phone,
                role = M_Users.UserRole.ParkingUser, // this is not getting assinged?
                created_at = DateTime.UtcNow,
                birth_year = request.BirthYear ?? DateTime.Now,
                active = true
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();
            return Ok(new { message = "User registered successfully." });
        }

        // POST /api/auth/logout - Revokes the current JWT token (logout)
        // Returns: 200 OK on success, 401 Unauthorized if no valid token
        [HttpPost("logout")]
        [Authorize] // Requires valid JWT token
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Get token from Authorization header
                var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(token))
                    return Unauthorized("No token provided.");

                // Get user ID from claims
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                string? expClaim = User.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                // Fallback: parse token if claims are missing (e.g., test environment)
                if (string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwt = handler.ReadJwtToken(token);
                        userId = jwt.Claims.FirstOrDefault(c =>
                            c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                        expClaim = jwt.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                    }
                    catch
                    {
                        return Unauthorized("Invalid token.");
                    }
                }

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("Invalid token claims.");

                // Get token expiration from claims (if available) or use default 2 hours
                var expiresAt = DateTime.UtcNow.AddHours(2);
                if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expUnix))
                {
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).DateTime;
                }

                // Revoke the token
                await _tokenRevocationService.RevokeTokenAsync(token, userId, expiresAt);
                Log.Information("User {UserId} logged out successfully", userId);

                // Cleanup expired tokens (background maintenance)
                _ = Task.Run(async () => await _tokenRevocationService.CleanupExpiredTokensAsync());

                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during logout.", error = ex.Message });
            }
        }
    }
}


