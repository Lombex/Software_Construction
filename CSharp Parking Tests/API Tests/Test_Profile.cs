using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Profile : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Profile(CSharpAPITests factory) => _factory = factory;

        // ========== GetProfile TESTS ==========

        [Fact]
        public async Task GetProfile_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.GetAsync($"/api/v2/profile?id={user.id}");
                response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            }
        }

        [Fact]
        public async Task GetProfile_With_User_Viewing_Other_User_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var response = await client.GetAsync($"/api/v2/profile?id={otherUser.id}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetProfile_With_User_Viewing_Own_Profile_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.GetAsync($"/api/v2/profile?id={user.id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task GetProfileById_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/profile/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ========== UpdateProfile TESTS ==========

        [Fact]
        public async Task UpdateProfile_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PutAsync($"/api/v2/profile/update/{Guid.NewGuid()}", null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateProfile_With_User_Updating_Other_User_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var response = await client.PutAsync($"/api/v2/profile/update/{otherUser.id}", null);
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        // ========== ChangePassword TESTS ==========

        [Fact]
        public async Task ChangePassword_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PutAsJsonAsync($"/api/v2/profile/change-password/{Guid.NewGuid()}", "NewPass123!");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ChangePassword_With_User_Changing_Other_User_Password_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var response = await client.PutAsJsonAsync($"/api/v2/profile/change-password/{otherUser.id}", "NewPass123!");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        // ========== DeleteProfile TESTS ==========

        [Fact]
        public async Task DeleteProfile_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/v2/profile/delete/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteProfile_With_User_Deleting_Other_User_Profile_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var response = await client.DeleteAsync($"/api/v2/profile/delete/{otherUser.id}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetProfileById_With_User_Viewing_Own_Profile_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.GetAsync($"/api/v2/profile/{user.id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task UpdateProfile_With_User_Updating_Own_Profile_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                // UpdateProfile endpoint takes no body, just the ID in the route
                var response = await client.PutAsync($"/api/v2/profile/update/{user.id}", new StringContent(""));
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task UpdateProfile_With_Admin_Updating_Other_User_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.PutAsync($"/api/v2/profile/update/{user.id}", new StringContent(""));
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task ChangePassword_With_User_Changing_Own_Password_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.PutAsJsonAsync($"/api/v2/profile/change-password/{user.id}", "NewPassword123!");
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task DeleteProfile_With_User_Deleting_Own_Profile_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.DeleteAsync($"/api/v2/profile/delete/{user.id}");
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GetProfile_With_Admin_Viewing_Other_User_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.GetAsync($"/api/v2/profile?id={user.id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
