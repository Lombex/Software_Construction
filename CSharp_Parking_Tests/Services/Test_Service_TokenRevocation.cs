using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_TokenRevocation
    {
        private SQLite_Database CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<SQLite_Database>()
                .UseSqlite(connection)
                .Options;
            var db = new SQLite_Database(options);
            db.Database.EnsureCreated();
            return db;
        }

        [Fact]
        public async Task RevokeToken_Should_Add_Token_To_Revoked_List()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            var token = "test-token-123";
            var userId = "user123";
            var expiresAt = DateTime.UtcNow.AddHours(2);

            await service.RevokeTokenAsync(token, userId, expiresAt);
            var isRevoked = await service.IsTokenRevokedAsync(token);
            isRevoked.Should().BeTrue();
        }

        [Fact]
        public async Task IsTokenRevoked_With_Revoked_Token_Should_Return_True()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            var token = "test-token-123";
            await service.RevokeTokenAsync(token, "user123", DateTime.UtcNow.AddHours(2));

            var result = await service.IsTokenRevokedAsync(token);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsTokenRevoked_With_NonRevoked_Token_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            var result = await service.IsTokenRevokedAsync("non-revoked-token");
            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsTokenRevoked_With_Empty_Token_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            var result = await service.IsTokenRevokedAsync("");
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CleanupExpiredTokens_Should_Remove_Expired_Tokens()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            var expiredToken = new M_RevokedTokens
            {
                TokenId = "expired-hash",
                UserId = "user123",
                RevokedAt = DateTime.UtcNow.AddDays(-1),
                ExpiresAt = DateTime.UtcNow.AddHours(-1)
            };
            var activeToken = new M_RevokedTokens
            {
                TokenId = "active-hash",
                UserId = "user123",
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };

            db.RevokedTokens.AddRange(expiredToken, activeToken);
            await db.SaveChangesAsync();

            await service.CleanupExpiredTokensAsync();

            var expired = await db.RevokedTokens.FirstOrDefaultAsync(t => t.TokenId == "expired-hash");
            var active = await db.RevokedTokens.FirstOrDefaultAsync(t => t.TokenId == "active-hash");

            expired.Should().BeNull();
            active.Should().NotBeNull();
        }

        [Fact]
        public async Task RevokeToken_With_Already_Revoked_Token_Should_Not_Duplicate()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            var token = "test-token-123";
            var userId = "user123";
            var expiresAt = DateTime.UtcNow.AddHours(2);

            await service.RevokeTokenAsync(token, userId, expiresAt);
            await service.RevokeTokenAsync(token, userId, expiresAt);

            // Should only have one entry (no duplicate)
            var allTokens = await db.RevokedTokens.ToListAsync();
            allTokens.Count.Should().Be(1);
        }

        [Fact]
        public async Task CleanupExpiredTokens_With_No_Expired_Tokens_Should_Not_Throw()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            await service.CleanupExpiredTokensAsync();
            // Should not throw
        }

        [Fact]
        public async Task IsTokenRevoked_With_Null_Token_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new TokenRevocationService(db);

            var result = await service.IsTokenRevokedAsync(null!);
            result.Should().BeFalse();
        }
    }
}
