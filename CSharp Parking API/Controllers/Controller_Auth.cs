using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSharpAPI.Controllers.Utils;
using Microsoft.AspNetCore.Identity;

namespace CSharpAPI.Controllers
{
    // Authentication controller - handles login and JWT token validation
    [Route("api/auth")]
    [ApiController]
    public class C_Auth : ControllerBase
    {
        private readonly SQLite_Database _db; // Database context for user queries
        private readonly ITokenService _tokenService; // Service for generating JWT tokens

        public C_Auth(SQLite_Database db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
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

            if (!verifyPassword) return Unauthorized();

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
    }
}


