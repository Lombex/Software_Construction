using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Reservation : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Reservation(CSharpAPITests factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetAllReservations_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllReservations_WithUserToken_Returns403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllReservations_WithLotAdminToken_Returns200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Fact]
        public async Task GetAllReservations_WithSuperAdminToken_Returns200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.GetAsync("/api/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateReservation_ValidData_Returns200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();

            var user = db.Users.First(u => u.username == "user");

            var parkingLotId = Guid.NewGuid();
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = parkingLotId,
                name = "Test Lot",
                location = "Test City",
                address = "123 Test Street",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });

            var vehicleId = Guid.NewGuid();
            db.Vehicles.Add(new M_Vehicles
            {
                id = vehicleId,
                user_id = user.id,
                license_plate = "TEST-123",
                make = "TestMake",
                model = "TestModel",
                color = "Black",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            });

            await db.SaveChangesAsync();

            var newReservation = new M_Reservations
            {
                id = Guid.NewGuid(),
                user_id = user.id,
                vehicle_id = vehicleId,
                parking_lot_id = parkingLotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 20.0f
            };

            var response = await client.PostAsJsonAsync("/api/reservations/create", newReservation);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var createdReservation = await response.Content.ReadFromJsonAsync<M_Reservations>();
            createdReservation.Should().NotBeNull();
            createdReservation!.id.Should().NotBe(Guid.Empty);
            createdReservation.user_id.Should().Be(newReservation.user_id);
            createdReservation.parking_lot_id.Should().Be(newReservation.parking_lot_id);
        }

        [Fact]
        public async Task CreateReservation_InvalidTimeRange_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();

            var badReservation = new M_Reservations
            {
                id = Guid.NewGuid(),
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow.AddHours(2),
                end_time = DateTime.UtcNow, 
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 15.0f
            };

            var response = await client.PostAsJsonAsync("/api/reservations/create", badReservation);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReservationById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync($"/api/reservations/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReservationById_EmptyId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync($"/api/reservations/{Guid.Empty}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CancelReservation_InvalidId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.PostAsync($"/api/reservations/cancel/{Guid.Empty}", null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CancelReservation_NotFound_Returns404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.PostAsync($"/api/reservations/cancel/{Guid.NewGuid()}", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CancelReservation_HappyFlow_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var resId = Guid.NewGuid();
            var reservation = new M_Reservations
            {
                id = resId,
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 10.0f
            };
            var create = await client.PostAsJsonAsync("/api/reservations/create", reservation);
            create.StatusCode.Should().Be(HttpStatusCode.OK);

            var cancel = await client.PostAsync($"/api/reservations/cancel/{resId}", null);
            cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ListReservationsByUser_InvalidUserId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync($"/api/reservations/user/{Guid.Empty}?Status=Active");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ListReservationsByUser_HappyFlow_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var resId = Guid.NewGuid();
            var reservation = new M_Reservations
            {
                id = resId,
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(1),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 5.0f
            };
            var create = await client.PostAsJsonAsync("/api/reservations/create", reservation);
            create.StatusCode.Should().Be(HttpStatusCode.OK);

            var response = await client.GetAsync($"/api/reservations/user/{userId}?Status=Active");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CheckAvailability_InvalidRange_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var (_, lotId, _) = await SeedReservationDependenciesAsync();
            var from = DateTime.UtcNow.AddHours(2);
            var to = DateTime.UtcNow;
            var response = await client.GetAsync($"/api/reservations/check-availability/parking-lots/{lotId}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CheckAvailability_InvalidLotId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddHours(1);
            var response = await client.GetAsync($"/api/reservations/check-availability/parking-lots/{Guid.Empty}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CheckAvailability_HappyFlow_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var (_, lotId, _) = await SeedReservationDependenciesAsync();
            var from = DateTime.UtcNow.AddHours(1);
            var to = DateTime.UtcNow.AddHours(2);
            var response = await client.GetAsync($"/api/reservations/check-availability/parking-lots/{lotId}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllReservations_PageBeyondTotal_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");

            var response = await client.GetAsync("/api/reservations/all?page=999");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        private async Task<(Guid userId, Guid lotId, Guid vehicleId)> SeedReservationDependenciesAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();

            var user = db.Users.First(u => u.username == "user");
            var lotId = Guid.NewGuid();
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Seed Lot",
                location = "Seed City",
                address = "1 Seed St",
                capacity = 50,
                reserved = 0,
                daytarriff = 8.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.1f, lng = 5.1f }
            });

            var vehicleId = Guid.NewGuid();
            db.Vehicles.Add(new M_Vehicles
            {
                id = vehicleId,
                user_id = user.id,
                license_plate = $"TEST-{Guid.NewGuid().ToString()[..6]}",
                make = "Make",
                model = "Model",
                color = "Blue",
                year = new DateTime(2021, 1, 1),
                created_at = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return (user.id, lotId, vehicleId);
        }

        [Fact]
        public async Task CreateReservation_MissingData_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var incompleteReservation = new
            {
                id = Guid.NewGuid(),
                user_id = Guid.NewGuid(),
                vehicle_id = Guid.NewGuid(),
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 20.0f
            };

            var response = await client.PostAsJsonAsync("/api/reservations/create", incompleteReservation);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        
    }
}