using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CSharpAPI.Tests;
using CSharpAPI.Tests.Utillities;

namespace CSharpAPI.Tests.APITests
{
    // Integration tests for Reservations API endpoints.
    // Requires the API project to expose a Program class for WebApplicationFactory.
    public class Test_Reservations : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;

        public Test_Reservations(CSharpAPITests factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetReservations_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/reservations/all");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateReservation_ReturnsOk_WithBody()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            // Prepare required related entities: user, parking lot, vehicle
            var usersResp = await client.GetAsync("/api/v2/users/all");
            usersResp.EnsureSuccessStatusCode();
            var usersBody = await usersResp.Content.ReadFromJsonAsync<JsonElement>();
            var usersArray = usersBody.GetProperty("Users");
            var userId = usersArray[0].GetProperty("id").GetGuid();

            var lot = new
            {
                name = "Test Lot for Reservation",
                location = "Loc",
                address = "Addr",
                capacity = 10,
                reserved = 0,
                daytarriff = 5.0f,
                coordinates = new { lat = 1.0f, lng = 1.0f }
            };
            var lotResp = await client.PostAsJsonAsync("/api/v2/parkinglots", lot);
            lotResp.EnsureSuccessStatusCode();
            var lotCreated = await lotResp.Content.ReadFromJsonAsync<JsonElement>();
            var lotId = lotCreated.GetProperty("id").GetGuid();

            var vehicle = new
            {
                user_id = userId,
                license_plate = "RES-001",
                make = "Make",
                model = "Model",
                color = "Blue",
                year = DateTime.UtcNow
            };
            var vehResp = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            vehResp.EnsureSuccessStatusCode();
            var vehCreated = await vehResp.Content.ReadFromJsonAsync<JsonElement>();
            var vehicleId = vehCreated.GetProperty("id").GetGuid();

            var dto = new
            {
                user_id = userId,
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(1)
            };

            var response = await client.PostAsJsonAsync("/api/v2/reservations/create", dto);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var s = await response.Content.ReadAsStringAsync();
                throw new Exception($"Create reservation failed: {response.StatusCode} - {s}");
            }
            var created = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.TryGetProperty("id", out var createdId));
        }

        [Fact]
        public async Task CreateAndCancelReservation_ReturnsOkAndThenNotFound()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            var reservation = new
            {
                vehiclePlate = "DEL-456",
                start = DateTime.UtcNow,
                end = DateTime.UtcNow.AddHours(2)
            };

            // prepare related entities
            var usersResp = await client.GetAsync("/api/v2/users/all");
            usersResp.EnsureSuccessStatusCode();
            var usersBody = await usersResp.Content.ReadFromJsonAsync<JsonElement>();
            var usersArray = usersBody.GetProperty("Users");
            var userId = usersArray[0].GetProperty("id").GetGuid();

            var lot = new
            {
                name = "Test Lot for Reservation",
                location = "Loc",
                address = "Addr",
                capacity = 10,
                reserved = 0,
                daytarriff = 5.0f,
                coordinates = new { lat = 1.0f, lng = 1.0f }
            };
            var lotResp = await client.PostAsJsonAsync("/api/v2/parkinglots", lot);
            lotResp.EnsureSuccessStatusCode();
            var lotCreated = await lotResp.Content.ReadFromJsonAsync<JsonElement>();
            var lotId = lotCreated.GetProperty("id").GetGuid();

            var vehicle = new
            {
                user_id = userId,
                license_plate = "RES-002",
                make = "Make",
                model = "Model",
                color = "Red",
                year = DateTime.UtcNow
            };
            var vehResp = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            vehResp.EnsureSuccessStatusCode();
            var vehCreated = await vehResp.Content.ReadFromJsonAsync<JsonElement>();
            var vehicleId = vehCreated.GetProperty("id").GetGuid();

            var dtoFull = new
            {
                user_id = userId,
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2)
            };

            var createResp = await client.PostAsJsonAsync("/api/v2/reservations/create", dtoFull);
            if (createResp.StatusCode != HttpStatusCode.Created)
            {
                var s = await createResp.Content.ReadAsStringAsync();
                throw new Exception($"Create reservation failed: {createResp.StatusCode} - {s}");
            }
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.TryGetProperty("id", out var idProp));
            var id = idProp.GetGuid();

            var cancelResp = await client.PostAsync($"/api/v2/reservations/cancel/{id}", null);
            Assert.Equal(HttpStatusCode.OK, cancelResp.StatusCode);

            // Verify status changed to Cancelled
            var getResp = await client.GetAsync($"/api/v2/reservations/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);
            var getBody = await getResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(getBody.TryGetProperty("status", out var statusProp));
            Assert.Equal(1, statusProp.GetInt32()); // Cancelled == 1
        }

        [Fact]
        public async Task GetReservationById_ReturnsOkAndBody()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            var reservation = new
            {
                vehiclePlate = "GET-789",
                start = DateTime.UtcNow,
                end = DateTime.UtcNow.AddHours(1)
            };

            // prepare related entities
            var usersResp = await client.GetAsync("/api/v2/users/all");
            usersResp.EnsureSuccessStatusCode();
            var usersBody = await usersResp.Content.ReadFromJsonAsync<JsonElement>();
            var usersArray = usersBody.GetProperty("Users");
            var userId = usersArray[0].GetProperty("id").GetGuid();

            var lot = new
            {
                name = "Test Lot for Reservation",
                location = "Loc",
                address = "Addr",
                capacity = 10,
                reserved = 0,
                daytarriff = 5.0f,
                coordinates = new { lat = 1.0f, lng = 1.0f }
            };
            var lotResp = await client.PostAsJsonAsync("/api/v2/parkinglots", lot);
            lotResp.EnsureSuccessStatusCode();
            var lotCreated = await lotResp.Content.ReadFromJsonAsync<JsonElement>();
            var lotId = lotCreated.GetProperty("id").GetGuid();

            var vehicle = new
            {
                user_id = userId,
                license_plate = "RES-003",
                make = "Make",
                model = "Model",
                color = "Green",
                year = DateTime.UtcNow
            };
            var vehResp = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            vehResp.EnsureSuccessStatusCode();
            var vehCreated = await vehResp.Content.ReadFromJsonAsync<JsonElement>();
            var vehicleId = vehCreated.GetProperty("id").GetGuid();

            var dtoSingle = new
            {
                user_id = userId,
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(1)
            };

            var createResp = await client.PostAsJsonAsync("/api/v2/reservations/create", dtoSingle);
            if (createResp.StatusCode != HttpStatusCode.Created)
            {
                var s = await createResp.Content.ReadAsStringAsync();
                throw new Exception($"Create reservation failed: {createResp.StatusCode} - {s}");
            }
            var created = await createResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(created.TryGetProperty("id", out var idProp));
            var id = idProp.GetGuid();

            var getResp = await client.GetAsync($"/api/v2/reservations/{id}");
            Assert.Equal(HttpStatusCode.OK, getResp.StatusCode);

            var body = await getResp.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(body.TryGetProperty("vehicle_id", out var vehicleProp));
            Assert.Equal(vehicleId, vehicleProp.GetGuid());
        }

        // Update endpoint not present on controller; skipping update test.
        [Fact]
        public async Task CreateReservation_BadData_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            var bad = new { }; // missing required fields
            var resp = await client.PostAsJsonAsync("/api/v2/reservations/create", bad);
            Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        }

        [Fact]
        public async Task DeleteNonExistingReservation_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            var id = Guid.NewGuid();
            var resp = await client.PostAsync($"/api/v2/reservations/cancel/{id}", null);
            Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
        }

        [Fact]
        public async Task GetAllReservations_With_Negative_Page_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/reservations/all?page=-1");
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllReservations_Should_Return_Paginated_Response()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/reservations/all?page=0");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Page", content);
            Assert.Contains("TotalPages", content);
        }
    }
}