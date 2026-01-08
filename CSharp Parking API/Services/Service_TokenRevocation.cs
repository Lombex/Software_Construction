using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CSharpAPI.Services
{
    // Interface for token revocation service (logout functionality)
    public interface ITokenRevocationService
    {
        Task<bool> IsTokenRevokedAsync(string token);
        Task RevokeTokenAsync(string token, string userId, DateTime expiresAt);
        Task CleanupExpiredTokensAsync();
    }

    // Service that manages revoked JWT tokens for logout functionality
    public class TokenRevocationService : ITokenRevocationService
    {
        private readonly SQLite_Database _dbContext;

        public TokenRevocationService(SQLite_Database dbContext)
        {
            _dbContext = dbContext;
        }

        // Check if a token has been revoked (used during JWT validation)
        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            var tokenHash = HashToken(token);
            var revokedToken = await _dbContext.RevokedTokens
                .FirstOrDefaultAsync(t => t.TokenId == tokenHash);
            
            return revokedToken != null;
        }

        // Revoke a token (add to blacklist) - called during logout
        public async Task RevokeTokenAsync(string token, string userId, DateTime expiresAt)
        {
            var tokenHash = HashToken(token);
            
            // Check if already revoked
            var existing = await _dbContext.RevokedTokens
                .FirstOrDefaultAsync(t => t.TokenId == tokenHash);
            
            if (existing == null)
            {
                var revokedToken = new M_RevokedTokens
                {
                    TokenId = tokenHash,
                    UserId = userId,
                    RevokedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                };

                await _dbContext.RevokedTokens.AddAsync(revokedToken);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Clean up expired tokens from the database (maintenance task)
        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _dbContext.RevokedTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _dbContext.RevokedTokens.RemoveRange(expiredTokens);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Hash token for storage (SHA256)
        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(token);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

