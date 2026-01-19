using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using CSharpAPI.Models.DTOs.Reservations;

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
            var create = await client.PostAsJsonAsync("/api/v2/reservations/create", reservation);
            create.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdReservation = await create.Content.ReadFromJsonAsync<M_Reservations>();
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

        [Fact]
        public async Task GetAllReservations_Admin_PageZero_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");

            var response = await client.GetAsync("/api/v2/reservations/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Admin_CreateReservationForUser_HappyFlow_ReturnsOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");

            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();

            var dto = new CreateReservationForUserDto
            {
                id = Guid.Empty,
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow.AddHours(1),
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 0f
            };

            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var created = await response.Content.ReadFromJsonAsync<M_Reservations>();
            created.Should().NotBeNull();
            created!.id.Should().NotBe(Guid.Empty);
            created.user_id.Should().Be(userId);
            created.parking_lot_id.Should().Be(lotId);
        }

        [Fact]
        public async Task Admin_CreateReservationForUser_WithInvalidIds_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");

            var dto = new CreateReservationForUserDto
            {
                id = Guid.Empty,
                user_id = Guid.Empty,
                vehicle_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid(),
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(1),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 0f
            };

            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_CreateReservationForUser_InvalidTimeRange_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");

            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();

            var dto = new CreateReservationForUserDto
            {
                id = Guid.Empty,
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow.AddHours(2),
                end_time = DateTime.UtcNow.AddHours(1),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 0f
            };

            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Admin_CreateReservationForUser_AsUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();

            var dto = new CreateReservationForUserDto
            {
                id = Guid.Empty,
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(1),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 0f
            };

            var response = await client.PostAsJsonAsync("/api/v2/reservations/admin/create-for-user", dto);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetReservationById_ForAnotherUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();

            // Seed a second regular user and their credentials
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
                if (!db.Users.Any(u => u.username == "user2"))
                {
                    db.Users.Add(new M_Users
                    {
                        id = Guid.NewGuid(),
                        username = "user2",
                        password = Utils.HashPassword("user2pass"),
                        name = "User Two",
                        email = "user2@example.com",
                        phone = "",
                        role = M_Users.UserRole.ParkingUser,
                        parking_lot_id = null,
                        created_at = DateTime.UtcNow,
                        birth_year = new DateTime(1996, 1, 1),
                        active = true
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Create a reservation for 'user'
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var createDto = new ReservationCreateDto
            {
                user_id = userId,
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(1)
            };
            var createResp = await client.PostAsJsonAsync("/api/v2/reservations/create", createDto);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<M_Reservations>();

            // Try to fetch as different non-admin user
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user2", "user2pass");
            var getResp = await client.GetAsync($"/api/v2/reservations/{created!.id}");
            getResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CancelReservation_ForAnotherUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();

            // Ensure second user exists
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
                if (!db.Users.Any(u => u.username == "user2"))
                {
                    db.Users.Add(new M_Users
                    {
                        id = Guid.NewGuid(),
                        username = "user2",
                        password = Utils.HashPassword("user2pass"),
                        name = "User Two",
                        email = "user2@example.com",
                        phone = "",
                        role = M_Users.UserRole.ParkingUser,
                        parking_lot_id = null,
                        created_at = DateTime.UtcNow,
                        birth_year = new DateTime(1996, 1, 1),
                        active = true
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Create a reservation for 'user'
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var (userId, lotId, vehicleId) = await SeedReservationDependenciesAsync();
            var createDto = new ReservationCreateDto
            {
                user_id = userId,
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(1)
            };
            var createResp = await client.PostAsJsonAsync("/api/v2/reservations/create", createDto);
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<M_Reservations>();

            // Try to cancel as different non-admin user
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user2", "user2pass");
            var cancelResp = await client.PostAsync($"/api/v2/reservations/cancel/{created!.id}", null);
            cancelResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
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
        public async Task CreateReservation_NullBody_Returns400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            using var empty = new StringContent("", System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/v2/reservations/create", empty);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        
    }
}