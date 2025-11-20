using CSharpAPI.Tests.Utillities;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Username = "superadmin", Password = "superpass" });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var payload = await response.Content.ReadFromJsonAsync<TokenResponse>();
            payload!.token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Get_Admin_Only_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Get_Admin_Only_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync("/api/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_Admin_Only_With_SuperAdmin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            var response = await client.GetAsync("/api/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_With_Invalid_Credentials_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Username = "invalid", Password = "invalid" });
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Get_Me_Endpoint_With_Valid_Token_Should_Return_User_Info()
        {
            var client = _factory.CreateClient();
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "adminpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/auth/me");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var userInfo = await response.Content.ReadFromJsonAsync<UserInfoResponse>();
            userInfo!.username.Should().Be("admin");
            userInfo.role.Should().Be("Admin");
        }

        [Fact]
        public async Task Get_Me_Endpoint_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/auth/me");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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


