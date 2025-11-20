using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            // Find active user with matching credentials (NOTE: passwords should be hashed in production)
            var user = await _db.Users.FirstOrDefaultAsync(u => 
                u.username == request.Username && 
                u.password == request.Password && 
                u.active);
            
            if (user == null)
                return Unauthorized();

            // Generate JWT token with user id, username, and role claims
            var token = _tokenService.GenerateToken(
                user.id.ToString(), 
                user.username ?? string.Empty, 
                user.role.ToString(), 
                out var expiresAt);
            
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
    }
}


