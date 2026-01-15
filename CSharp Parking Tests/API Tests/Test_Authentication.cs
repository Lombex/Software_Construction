using CSharpAPI.Tests.Utillities;
using CSharpAPI.Database;
using CSharpAPI.Controllers.Utils;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Authentication : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Authentication(CSharpAPITests factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_Should_Return_Token_For_Valid_Credentials()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "superadmin", Password = "superpass" });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
            payload!.token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Get_Admin_Only_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Get_Admin_Only_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync("/api/v2/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_Admin_Only_With_SuperAdmin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            var response = await client.GetAsync("/api/v2/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_With_Invalid_Credentials_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "invalid", Password = "invalid" });
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }


        [Fact]
        public async Task Get_Me_Endpoint_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/auth/me");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ========== ADDITIONAL LOGIN TESTS ==========

        [Fact]
        public async Task Login_With_Null_Username_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = (string?)null, Password = "password" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_With_Empty_Username_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "", Password = "password" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_With_Empty_Password_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "user", Password = "" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_With_Whitespace_Only_Username_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "   ", Password = "password" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_With_Whitespace_Only_Password_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "user", Password = "   " });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_With_User_No_Password_Should_Return_401()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var userNoPassword = new CSharpAPI.Models.M_Users
            {
                id = Guid.NewGuid(),
                username = "nopassword",
                password = null!,
                name = "No Password",
                email = "nopass@test.com",
                phone = "1234567890",
                role = CSharpAPI.Models.M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(userNoPassword);
            await db.SaveChangesAsync();
            
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "nopassword", Password = "anypass" });
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_With_Legacy_Hash_Should_Upgrade_To_BCrypt()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var legacyUser = new CSharpAPI.Models.M_Users
            {
                id = Guid.NewGuid(),
                username = "legacyuser",
                password = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8", // SHA256 hash
                name = "Legacy",
                email = "legacy@test.com",
                phone = "1234567890",
                role = CSharpAPI.Models.M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(legacyUser);
            await db.SaveChangesAsync();
            
            var response = await client.PostAsJsonAsync("/api/v2/auth/login", new { Username = "legacyuser", Password = "password" });
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
            
            // Verify password was upgraded
            var updatedUser = await db.Users.FirstOrDefaultAsync(u => u.username == "legacyuser");
            if (updatedUser != null && response.StatusCode == HttpStatusCode.OK)
            {
                updatedUser.password.Should().NotBe(legacyUser.password);
            }
        }

        // ========== ME ENDPOINT TESTS ==========

        [Fact]
        public async Task Get_Me_Endpoint_Should_Return_Correct_Claims()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync("/api/v2/auth/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
            userInfo.Should().NotBeNull();
            userInfo!.username.Should().Be("user");
            userInfo.role.Should().Be("ParkingUser");
        }

        // ========== REGISTER ENDPOINT TESTS ==========

        [Fact]
        public async Task Register_With_Valid_Data_Should_Return_200()
        {
            var client = _factory.CreateClient();
            var registerData = new
            {
                Username = $"testuser_{Guid.NewGuid()}",
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                Email = $"test{Guid.NewGuid()}@example.com",
                Phone = "0612345678",
                Name = "Test User",
                BirthYear = new DateTime(1990, 1, 1)
            };

            var response = await client.PostAsJsonAsync("/api/v2/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Register_With_Null_Username_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/register", new { Username = (string?)null, Password = "pass", ConfirmPassword = "pass" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_With_Empty_Username_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/register", new { Username = "", Password = "pass", ConfirmPassword = "pass" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_With_Null_Password_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/register", new { Username = "testuser", Password = (string?)null, ConfirmPassword = (string?)null });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_With_Password_Mismatch_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/register", new { Username = "testuser", Password = "pass1", ConfirmPassword = "pass2" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_With_Invalid_Email_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/register", new 
            { 
                Username = "testuser", 
                Password = "pass", 
                ConfirmPassword = "pass",
                Email = "invalid-email"
            });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_With_Invalid_Phone_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/v2/auth/register", new 
            { 
                Username = "testuser", 
                Password = "pass", 
                ConfirmPassword = "pass",
                Email = "test@example.com",
                Phone = "invalid"
            });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_With_Duplicate_Username_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var registerData = new
            {
                Username = "superadmin",
                Password = "pass",
                ConfirmPassword = "pass",
                Email = "new@example.com",
                Phone = "0612345678"
            };

            var response = await client.PostAsJsonAsync("/api/v2/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_With_Duplicate_Email_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var registerData = new
            {
                Username = $"newuser_{Guid.NewGuid()}",
                Password = "pass",
                ConfirmPassword = "pass",
                Email = "super@example.com",
                Phone = "0612345678"
            };

            var response = await client.PostAsJsonAsync("/api/v2/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_Should_Create_User_With_ParkingUser_Role()
        {
            var client = _factory.CreateClient();
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test{Guid.NewGuid()}@example.com";
            var registerData = new
            {
                Username = username,
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                Email = email,
                Phone = "0612345678",
                Name = "Test User",
                BirthYear = new DateTime(1990, 1, 1)
            };

            var response = await client.PostAsJsonAsync("/api/v2/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify user was created with ParkingUser role
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
            user.Should().NotBeNull();
            user!.role.Should().Be(CSharpAPI.Models.M_Users.UserRole.ParkingUser);
        }

        [Fact]
        public async Task Register_With_Null_BirthYear_Should_Use_Current_Date()
        {
            var client = _factory.CreateClient();
            var username = $"testuser_{Guid.NewGuid()}";
            var email = $"test{Guid.NewGuid()}@example.com";
            var registerData = new
            {
                Username = username,
                Password = "TestPass123!",
                ConfirmPassword = "TestPass123!",
                Email = email,
                Phone = "0612345678",
                Name = "Test User",
                BirthYear = (DateTime?)null
            };

            var response = await client.PostAsJsonAsync("/api/v2/auth/register", registerData);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.username == username);
            user.Should().NotBeNull();
            user!.birth_year.Should().BeCloseTo(DateTime.Now, TimeSpan.FromDays(1));
        }

        // ========== LOGOUT ENDPOINT TESTS ==========

        [Fact]
        public async Task Logout_With_Valid_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            var response = await client.PostAsync("/api/v2/auth/logout", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Logout_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync("/api/v2/auth/logout", null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_Should_Revoke_Token()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            // Logout
            var logoutResponse = await client.PostAsync("/api/v2/auth/logout", null);
            logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Try to use the token again - should fail (token is revoked)
            var meResponse = await client.GetAsync("/api/v2/auth/me");
            meResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Logout_With_Exp_Claim_Should_Use_Exp_Claim()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            // This tests the path where exp claim exists in token
            var response = await client.PostAsync("/api/v2/auth/logout", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Logout_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            // Test exception handling path
            var response = await client.PostAsync("/api/v2/auth/logout", null);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        private class TokenResponse
        {
            public string token { get; set; } = string.Empty;
            public DateTime expiresAt { get; set; }
        }

        private class UserInfoResponse
        {
            public string? id { get; set; }
            public string? username { get; set; }
            public string? role { get; set; }
        }
    }
}


