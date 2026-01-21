using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Token
    {
        private IConfiguration CreateTestConfiguration()
        {
            var config = new Dictionary<string, string?>
            {
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:Key", "TestSecretKeyThatIsAtLeast32CharactersLong!" }
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();
        }

        [Fact]
        public void GenerateToken_Should_Return_Valid_Token()
        {
            var config = CreateTestConfiguration();
            var service = new TokenService(config);

            var token = service.GenerateToken("user123", "testuser", "ParkingUser", out var expiresAt);
            token.Should().NotBeNullOrEmpty();
            expiresAt.Should().BeAfter(DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_Should_Include_User_Claims()
        {
            var config = CreateTestConfiguration();
            var service = new TokenService(config);

            var userId = "user123";
            var username = "testuser";
            var role = "SuperAdmin";
            var token = service.GenerateToken(userId, username, role, out var expiresAt);

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jsonToken = handler.ReadJwtToken(token);

            jsonToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value.Should().Be(userId);
            jsonToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value.Should().Be(username);
            jsonToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value.Should().Be(role);
        }

        [Fact]
        public void GenerateToken_Should_Set_Expiration()
        {
            var config = CreateTestConfiguration();
            var service = new TokenService(config);

            var token = service.GenerateToken("user123", "testuser", "ParkingUser", out var expiresAt);
            expiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(2), TimeSpan.FromMinutes(1));
        }

        [Fact]
        public void GenerateToken_With_Missing_Key_Should_Throw_Exception()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
            var service = new TokenService(config);

            Assert.Throws<InvalidOperationException>(() => 
                service.GenerateToken("user123", "testuser", "ParkingUser", out var expiresAt));
        }
    }
}
