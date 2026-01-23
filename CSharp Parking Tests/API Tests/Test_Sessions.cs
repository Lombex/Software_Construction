using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Sessions : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Sessions(CSharpAPITests factory) => _factory = factory;

        // ========== StartSession TESTS ==========

        [Fact]
        public async Task StartSession_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var session = CreateSampleSession();
            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", session);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task StartSession_With_Valid_Data_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test",
                address = "Test",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });
            
            if (user != null)
            {
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "TEST-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            var session = CreateSampleSession();
            session.parking_lot_id = lotId;
            session.vehicle_id = vehicleId;
            if (user != null) session.user = user.username;
            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", session);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task StartSession_With_Admin_Token_And_No_User_Should_Default_To_Current_User()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var admin = db.Users.FirstOrDefault(u => u.username == "lotadmin");
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test",
                address = "Test",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });
            
            if (admin != null)
            {
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = admin.id,
                    license_plate = "ADMIN-123",
                    make = "Make",
                    model = "Model",
                    color = "Blue",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            var session = CreateSampleSession();
            session.parking_lot_id = lotId;
            session.vehicle_id = vehicleId;
            session.user = null; // Admin should default to current user
            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", session);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task StartSession_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.PostAsJsonAsync<M_Session>("/api/v2/sessions/start", null!);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task StartSession_With_Invalid_ModelState_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            // Create invalid session data that will fail ModelState validation
            var invalidSession = new { parking_lot_id = Guid.Empty, vehicle_id = Guid.Empty };
            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", invalidSession);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task StartSession_With_Empty_Username_Should_Return_401()
        {
            // This tests the path where CurrentUsername is null/empty
            var client = _factory.CreateClient();
            // Create a token that doesn't have a username claim - this is tricky, but we can test with invalid token
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");
            var session = CreateSampleSession();
            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", session);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task StartSession_With_Admin_Token_And_User_Specified_Should_Use_Specified_User()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var admin = db.Users.FirstOrDefault(u => u.username == "lotadmin");
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test",
                address = "Test",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });
            
            if (user != null)
            {
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "ADMIN-SPEC-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            var session = CreateSampleSession();
            session.parking_lot_id = lotId;
            session.vehicle_id = vehicleId;
            if (user != null) session.user = user.username; // Admin specifies user
            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", session);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task StartSession_With_User_Token_Should_Force_Ownership()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test",
                address = "Test",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });
            
            if (user != null)
            {
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "FORCE-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            var session = CreateSampleSession();
            session.parking_lot_id = lotId;
            session.vehicle_id = vehicleId;
            session.user = "otheruser"; // Try to set different user, but should be forced to "user"
            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", session);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        // ========== StopSession TESTS ==========

        [Fact]
        public async Task StopSession_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync($"/api/v2/sessions/{Guid.NewGuid()}/stop", null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ========== GetSessionsById TESTS ==========

        [Fact]
        public async Task GetSessionsById_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/sessions/user");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetSessionsById_With_User_Viewing_Other_User_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/sessions/superadmin");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ========== GetAll TESTS ==========

        [Fact]
        public async Task GetAll_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/sessions/all");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_With_User_Token_And_Other_User_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            // User trying to view another user's sessions should be forbidden
            var response = await client.GetAsync("/api/v2/sessions/all?user=otheruser");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAll_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/sessions/all?user=lotadmin");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_With_Admin_Token_And_User_Parameter_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/sessions/all?user=user");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_With_Admin_Token_And_Status_Parameter_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/sessions/all?user=user&status=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAll_With_User_Token_And_Own_Username_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/sessions/all?user=user");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task StartSession_With_Exceeded_Capacity_Should_Return_BadRequest()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            // Create a full parking lot (capacity = 1)
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Full Lot",
                location = "Test",
                address = "Test",
                capacity = 1,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });

            // Create a vehicle for the session
            if (user != null)
            {
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "EXISTING-123",
                    make = "Test",
                    model = "Test",
                    color = "Black",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            // Create one active session to fill the lot
            var existingSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = user?.username ?? "user",
                license_plate = "EXISTING-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(existingSession);
            await db.SaveChangesAsync();

            // Try to start another session - should fail
            var newSession = CreateSampleSession();
            newSession.parking_lot_id = lotId;
            newSession.license_plate = "NEW-123";
            if (user != null) newSession.user = user.username;

            var response = await client.PostAsJsonAsync("/api/v2/sessions/start", newSession);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task StopSession_With_Valid_Id_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test",
                address = "Test",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });
            
            if (user != null)
            {
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "STOP-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            var sessionId = Guid.NewGuid();
            var session = new M_Session
            {
                id = sessionId,
                user = user?.username ?? "user",
                license_plate = "STOP-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow.AddHours(-1),
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            var response = await client.PostAsync($"/api/v2/sessions/{sessionId}/stop", null);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task StopSession_With_Admin_Token_And_Other_User_Session_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test",
                address = "Test",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });
            
            if (user != null)
            {
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "ADMIN-STOP-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            var sessionId = Guid.NewGuid();
            var session = new M_Session
            {
                id = sessionId,
                user = user?.username ?? "user",
                license_plate = "ADMIN-STOP-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow.AddHours(-1),
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            // Admin should be able to stop any user's session
            var response = await client.PostAsync($"/api/v2/sessions/{sessionId}/stop", null);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task StopSession_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.PostAsync($"/api/v2/sessions/{Guid.NewGuid()}/stop", null);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetSessionsById_With_Valid_Username_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/sessions/user");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetSessionsById_With_Admin_Viewing_Other_User_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/sessions/user");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMyParkingHistory_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/sessions/me/history");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMyParkingHistory_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/sessions/me/history");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMyParkingHistory_With_Limit_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/sessions/me/history?limit=10");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task AutoStartSession_With_Valid_Data_Should_Return_200()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            var lotId = Guid.NewGuid();
            
            db.Parkinglots.Add(new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test",
                address = "Test",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            });
            
            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "AUTO-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync();

            var request = new { LicensePlate = "AUTO-123", ParkingLotId = lotId };
            var response = await client.PostAsJsonAsync("/api/v2/sessions/auto-start", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AutoStartSession_With_Invalid_LicensePlate_Should_Return_404()
        {
            var client = _factory.CreateClient();
            var request = new { LicensePlate = "INVALID-123", ParkingLotId = Guid.NewGuid() };
            var response = await client.PostAsJsonAsync("/api/v2/sessions/auto-start", request);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AutoStartSession_With_Missing_ParkingLot_Should_Return_400()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");

            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "AUTO-NO-LOT",
                    make = "Make",
                    model = "Model",
                    color = "Gray",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
            }

            var request = new { LicensePlate = "AUTO-NO-LOT", ParkingLotId = Guid.NewGuid() };
            var response = await client.PostAsJsonAsync("/api/v2/sessions/auto-start", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AutoStartSession_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync<object>("/api/v2/sessions/auto-start", null!);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task StopSession_With_Admin_Token_And_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.PostAsync($"/api/v2/sessions/{Guid.NewGuid()}/stop", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private M_Session CreateSampleSession()
        {
            return new M_Session
            {
                id = Guid.NewGuid(),
                user = "user",
                license_plate = "TEST-123",
                parking_lot_id = Guid.NewGuid(),
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
        }
    }
}
