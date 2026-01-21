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
            var response = await client.GetAsync("/api/v2/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllReservations_WithUserToken_Returns403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllReservations_WithLotAdminToken_Returns200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

        }

        [Fact]
        public async Task GetAllReservations_WithSuperAdminToken_Returns200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.GetAsync("/api/v2/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateReservation_ValidData_Returns201()
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

            var response = await client.PostAsJsonAsync("/api/v2/reservations/create", newReservation);
            response.StatusCode.Should().Be(HttpStatusCode.Created);

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

            var response = await client.PostAsJsonAsync("/api/v2/reservations/create", badReservation);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReservationById_UnknownId_Returns404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync($"/api/v2/reservations/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetReservationById_EmptyId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync($"/api/v2/reservations/{Guid.Empty}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CancelReservation_InvalidId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.PostAsync($"/api/v2/reservations/cancel/{Guid.Empty}", null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CancelReservation_NotFound_Returns404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.PostAsync($"/api/v2/reservations/cancel/{Guid.NewGuid()}", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CancelReservation_HappyFlow_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var reservation = new M_Reservations
            {
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 10.0f
            };
            var create = await client.PostAsJsonAsync("/api/v2/reservations/create", reservation);
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            // Get the ID from the created reservation
            var createdReservation = await create.Content.ReadFromJsonAsync<M_Reservations>();
            createdReservation.Should().NotBeNull();

            var cancel = await client.PostAsync($"/api/v2/reservations/cancel/{createdReservation!.id}", null);
            cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ListReservationsByUser_InvalidUserId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var response = await client.GetAsync($"/api/v2/reservations/user/{Guid.Empty}?Status=Active");
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
            var create = await client.PostAsJsonAsync("/api/v2/reservations/create", reservation);
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var response = await client.GetAsync($"/api/v2/reservations/user/{userId}?Status=Active");
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
            var response = await client.GetAsync($"/api/v2/reservations/check-availability/parking-lots/{lotId}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CheckAvailability_InvalidLotId_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddHours(1);
            var response = await client.GetAsync($"/api/v2/reservations/check-availability/parking-lots/{Guid.Empty}?from={from:o}&to={to:o}");
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
            var response = await client.GetAsync($"/api/v2/reservations/check-availability/parking-lots/{lotId}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllReservations_PageBeyondTotal_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");

            var response = await client.GetAsync("/api/v2/reservations/all?page=999");
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

            // Use empty GUIDs to trigger validation failure
            var incompleteReservation = new
            {
                id = Guid.NewGuid(),
                user_id = Guid.Empty,
                vehicle_id = Guid.Empty,
                parking_lot_id = Guid.Empty,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 20.0f
            };

            var response = await client.PostAsJsonAsync("/api/v2/reservations/create", incompleteReservation);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ========== CreateReservationForUser TESTS ==========

        [Fact]
        public async Task CreateReservationForUser_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var dto = new { user_id = Guid.NewGuid(), vehicle_id = Guid.NewGuid(), parking_lot_id = Guid.NewGuid(), start_time = DateTime.UtcNow, end_time = DateTime.UtcNow.AddHours(1) };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReservationForUser_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var dto = new { user_id = Guid.NewGuid(), vehicle_id = Guid.NewGuid(), parking_lot_id = Guid.NewGuid(), start_time = DateTime.UtcNow, end_time = DateTime.UtcNow.AddHours(1) };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateReservationForUser_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var dto = new
            {
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                cost = 20.0f
            };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateReservationForUser_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var dto = new { user_id = Guid.Empty, vehicle_id = Guid.Empty, parking_lot_id = Guid.Empty, start_time = DateTime.UtcNow, end_time = DateTime.UtcNow.AddHours(1) };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReservationForUser_With_Invalid_TimeRange_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var dto = new { user_id = userId, vehicle_id = vehicleId, parking_lot_id = lotId, start_time = DateTime.UtcNow.AddHours(2), end_time = DateTime.UtcNow };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ========== ADDITIONAL AUTHORIZATION TESTS ==========

        [Fact]
        public async Task GetReservationById_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/reservations/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetReservationById_With_User_Viewing_Other_User_Reservation_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var (_, lotId, vehicleId) = await SeedReservationDependenciesAsync();
                var resId = Guid.NewGuid();
                db.Reservations.Add(new M_Reservations
                {
                    id = resId,
                    user_id = otherUser.id,
                    vehicle_id = vehicleId,
                    parking_lot_id = lotId,
                    start_time = DateTime.UtcNow,
                    end_time = DateTime.UtcNow.AddHours(1),
                    status = M_Reservations.Status.Active,
                    created_at = DateTime.UtcNow,
                    cost = 10.0f
                });
                await db.SaveChangesAsync();
                var response = await client.GetAsync($"/api/v2/reservations/{resId}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task CancelReservation_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync($"/api/v2/reservations/cancel/{Guid.NewGuid()}", null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CheckAvailability_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var from = DateTime.UtcNow;
            var to = DateTime.UtcNow.AddHours(1);
            var response = await client.GetAsync($"/api/v2/reservations/check-availability/parking-lots/{Guid.NewGuid()}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReservation_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var dto = new { user_id = Guid.NewGuid(), vehicle_id = Guid.NewGuid(), parking_lot_id = Guid.NewGuid(), start_time = DateTime.UtcNow, end_time = DateTime.UtcNow.AddHours(1) };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/create", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateReservation_With_StartTime_Equals_EndTime_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var time = DateTime.UtcNow;
            var dto = new { user_id = userId, vehicle_id = vehicleId, parking_lot_id = lotId, start_time = time, end_time = time };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/create", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ListReservationsByUser_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/reservations/user/{Guid.NewGuid()}?Status=Active");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ListReservationsByUser_With_User_Viewing_Other_User_Reservations_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var response = await client.GetAsync($"/api/v2/reservations/user/{otherUser.id}?Status=Active");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task CreateReservation_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.PostAsJsonAsync<object>("/api/v2/reservations/create", null!);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReservationForUser_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.PostAsJsonAsync<object>("/api/v2/reservations/admin/create-for-user", null!);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CheckAvailability_With_Valid_Data_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var (_, lotId, _) = await SeedReservationDependenciesAsync();
            var from = DateTime.UtcNow.AddHours(1);
            var to = DateTime.UtcNow.AddHours(2);
            var response = await client.GetAsync($"/api/v2/reservations/check-availability/parking-lots/{lotId}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CheckAvailability_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var from = DateTime.UtcNow.AddHours(1);
            var to = DateTime.UtcNow.AddHours(2);
            var response = await client.GetAsync($"/api/v2/reservations/check-availability/parking-lots/{Guid.Empty}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CheckAvailability_With_Invalid_TimeRange_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var (_, lotId, _) = await SeedReservationDependenciesAsync();
            var from = DateTime.UtcNow.AddHours(2);
            var to = DateTime.UtcNow.AddHours(1);
            var response = await client.GetAsync($"/api/v2/reservations/check-availability/parking-lots/{lotId}?from={from:o}&to={to:o}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetAllReservations_With_Page_Exceeding_Total_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/reservations/all?page=999");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetReservationById_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync($"/api/v2/reservations/{Guid.Empty}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CancelReservation_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.PostAsync($"/api/v2/reservations/cancel/{Guid.Empty}", null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ListReservationsByUser_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync($"/api/v2/reservations/user/{Guid.Empty}?Status=Active");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReservationForUser_With_Empty_User_Id_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var (_, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var dto = new { user_id = Guid.Empty, vehicle_id = vehicleId, parking_lot_id = lotId, start_time = DateTime.UtcNow.AddHours(1), end_time = DateTime.UtcNow.AddHours(2) };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReservationForUser_With_Empty_Vehicle_Id_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var (userId, lotId, _) = await SeedReservationDependenciesAsync();
            var dto = new { user_id = userId, vehicle_id = Guid.Empty, parking_lot_id = lotId, start_time = DateTime.UtcNow.AddHours(1), end_time = DateTime.UtcNow.AddHours(2) };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateReservationForUser_With_Empty_ParkingLot_Id_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var (userId, _, vehicleId) = await SeedReservationDependenciesAsync();
            var dto = new { user_id = userId, vehicle_id = vehicleId, parking_lot_id = Guid.Empty, start_time = DateTime.UtcNow.AddHours(1), end_time = DateTime.UtcNow.AddHours(2) };
            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}