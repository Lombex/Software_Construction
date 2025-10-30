using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CSharp_Parking_API.Tests
{
    public class AuthIntegrationTests : IClassFixture<TestingWebAppFactory>
    {
        private readonly TestingWebAppFactory _factory;
        public AuthIntegrationTests(TestingWebAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_Should_Return_Token_For_Valid_Credentials()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "adminpass" });
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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "user", Password = "userpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Get_Admin_Only_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "adminpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private class TokenResponse
        {
            public string token { get; set; } = string.Empty;
            public DateTime expiresAt { get; set; }
        }
    }
}


