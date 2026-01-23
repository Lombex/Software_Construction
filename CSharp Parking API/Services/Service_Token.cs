using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CSharpAPI.Services
{
    // Interface for JWT token generation service
    public interface ITokenService
    {
        string GenerateToken(string userId, string username, string role, out DateTime expiresAt);
    }

    // Service that creates and signs JWT tokens for authentication
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration; // Access to appsettings.json for JWT config

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Generates a signed JWT token containing user claims (id, username, role)
        // Returns the token string and outputs the expiration time
        public string GenerateToken(string userId, string username, string role, out DateTime expiresAt)
        {
            // Load JWT configuration from appsettings.json
            var jwtSection = _configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"]; // Token issuer (who created the token)
            var audience = jwtSection["Audience"]; // Token audience (who can use the token)
            var key = jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key is not configured"); // Secret key for signing

            // Create signing credentials using HMAC SHA256 algorithm
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Build claims (user information stored in the token)
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId), // Subject (user ID)
                new Claim(JwtRegisteredClaimNames.UniqueName, username), // Unique name
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique token ID
                new Claim(ClaimTypes.NameIdentifier, userId), // ASP.NET Core user ID claim
                new Claim(ClaimTypes.Name, username), // ASP.NET Core username claim
                new Claim(ClaimTypes.Role, role) // User role for RBAC (Admin/User)
            };

            expiresAt = DateTime.UtcNow.AddHours(2); // Token valid for 2 hours

            // Create the JWT token with all configuration
            var token = new JwtSecurityToken(
                issuer: string.IsNullOrWhiteSpace(issuer) ? null : issuer,
                audience: string.IsNullOrWhiteSpace(audience) ? null : audience,
                claims: claims,
                notBefore: DateTime.UtcNow, // Token valid from now
                expires: expiresAt, // Token expires in 2 hours
                signingCredentials: credentials // Sign with secret key
            );

            // Serialize token to string format
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


