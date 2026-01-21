using System.ComponentModel.DataAnnotations;

namespace CSharpAPI.Models
{
    // Model for tracking revoked JWT tokens (for logout functionality)
    public class M_RevokedTokens
    {
        [Key]
        public string TokenId { get; set; } = string.Empty; // JWT ID (jti claim) or full token hash
        public string UserId { get; set; } = string.Empty; // User ID who revoked the token
        public DateTime RevokedAt { get; set; } // When the token was revoked
        public DateTime ExpiresAt { get; set; } // Original token expiration time
    }
}

