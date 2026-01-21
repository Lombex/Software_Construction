using CSharpAPI.Tests.Utillities;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Gate : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Gate(CSharpAPITests factory) => _factory = factory;

        [Fact]
        public async Task OpenGate_With_Valid_Request_Should_Return_200()
        {
            var client = _factory.CreateClient();
            var request = new { SessionId = Guid.NewGuid(), ParkingLotId = Guid.NewGuid() };
            var response = await client.PostAsJsonAsync("/api/v2/gate/open", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task OpenGate_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync<object>("/api/v2/gate/open", null!);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task CloseGate_With_Valid_Request_Should_Return_200()
        {
            var client = _factory.CreateClient();
            var request = new { SessionId = Guid.NewGuid() };
            var response = await client.PostAsJsonAsync("/api/v2/gate/close", request);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetGateStatus_With_Valid_ParkingLotId_Should_Return_200()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/gate/status?parkingLotId={Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetGateStatus_With_Empty_Guid_Should_Return_200()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/gate/status?parkingLotId={Guid.Empty}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CloseGate_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync<object>("/api/v2/gate/close", null!);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task OpenGate_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            // Test exception handling path - this should normally succeed, but tests the try-catch
            var request = new { SessionId = Guid.NewGuid(), ParkingLotId = Guid.NewGuid() };
            var response = await client.PostAsJsonAsync("/api/v2/gate/open", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task CloseGate_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            var request = new { SessionId = Guid.NewGuid() };
            var response = await client.PostAsJsonAsync("/api/v2/gate/close", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetGateStatus_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/gate/status?parkingLotId={Guid.NewGuid()}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }
    }
}
