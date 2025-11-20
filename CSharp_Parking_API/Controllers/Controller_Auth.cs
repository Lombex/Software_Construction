using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class C_Auth : ControllerBase
    {
        private readonly SQLite_Database _db;
        private readonly ITokenService _tokenService;
        public C_Auth(SQLite_Database db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Username) || string.IsNullOrWhiteSpace(request?.Password))
                return BadRequest("Username and password are required.");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.username == request.Username && u.password == request.Password && u.active);
            if (user == null)
                return Unauthorized();

            var token = _tokenService.GenerateToken(user.id.ToString(), user.username ?? string.Empty, user.role.ToString(), out var expiresAt);
            return Ok(new { token, expiresAt });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var username = User.Identity?.Name;
            var id = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var role = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value;
            return Ok(new { id, username, role });
        }
    }
}


