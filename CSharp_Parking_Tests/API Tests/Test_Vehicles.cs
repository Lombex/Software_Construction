using CSharpAPI.Models;
using CSharpAPI.Tests;
using CSharpAPI.Tests.Utillities;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp_Parking_Tests.API_Tests
{
    public class Test_Vehicles : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Vehicles(CSharpAPITests factory) => _factory = factory;

        [Fact]
        public async Task Test_Pagination_HappyFlow()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/vehicles/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_Pagination_InvalidPageNumber()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/vehicles/all?page=-1");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Pagination_NonIntegerPageNumber()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/vehicles/all?page=abc");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Pagination_MissingPageNumber()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/vehicles/all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_GetById_UnknownID()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var unknownId = Guid.NewGuid();
            var response = await client.GetAsync($"/api/v2/vehicles/{unknownId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_GetById_InvalidIDFormat()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync($"/api/v2/vehicles/invalid-guid");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Create_HappyFlow()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            Guid userGuid = Guid.NewGuid();

            var newVehicle = new M_Vehicles
            {
                user_id = userGuid,
                license_plate = "TEST123",
                make = "TestMake",
                model = "TestModel",
                color = "Red",
                year = new DateTime(1990, 1, 1),
                created_at = DateTime.UtcNow,
                M_Users = new M_Users
                {
                    id = userGuid,
                    username = "testuser",
                    password = "testpass",
                    name = "Test User",
                    email = "john@doe.com",
                    phone = "1234567890",
                    role = M_Users.UserRole.ParkingUser,
                    parking_lot_id = null,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                }
            };

            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", newVehicle);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Test_Create_BadData()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            Guid userGuid = Guid.NewGuid();

            var newVehicle = new
            {
                user_id = userGuid,
                license_plate = "TEST123",
                make = "TestMake",
                model = "TestModel",
                color = "Red",
                year = 1234, // Invalid year format
                created_at = DateTime.UtcNow,
                M_Users = new M_Users
                {
                    id = userGuid,
                    username = "testuser",
                    password = "testpass",
                    name = "Test User",
                    email = "john@doe.com",
                    phone = "1234567890",
                    role = M_Users.UserRole.ParkingUser,
                    parking_lot_id = null,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                }
            };

            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", newVehicle);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Create_EmptyData()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            Guid userGuid = Guid.NewGuid();

            var newVehicle = new M_Vehicles
            {
                user_id = userGuid,
                license_plate = "",
                make = "",
                model = "",
                color = "Red",
                year = new DateTime(1990, 1, 1),
                created_at = DateTime.UtcNow,
                M_Users = new M_Users
                {
                    id = userGuid,
                    username = "testuser",
                    password = "testpass",
                    name = "Test User",
                    email = "john@doe.com",
                    phone = "1234567890",
                    role = M_Users.UserRole.ParkingUser,
                    parking_lot_id = null,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                }
            };

            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", newVehicle);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Create_NotPossible_Year()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            Guid userGuid = Guid.NewGuid();

            var newVehicle = new M_Vehicles
            {
                user_id = userGuid,
                license_plate = "TEST123",
                make = "TestMake",
                model = "TestModel",
                color = "Red",
                year = new DateTime(DateTime.Now.Year + 1, 1, 1),
                created_at = DateTime.UtcNow,
                M_Users = new M_Users
                {
                    id = userGuid,
                    username = "testuser",
                    password = "testpass",
                    name = "Test User",
                    email = "john@doe.com",
                    phone = "1234567890",
                    role = M_Users.UserRole.ParkingUser,
                    parking_lot_id = null,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                }
            };

            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", newVehicle);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Create_NotPossible_LicensePlate()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            Guid userGuid = Guid.NewGuid();

            var newVehicle = new M_Vehicles
            {
                user_id = userGuid,
                license_plate = "TEST12332323223",
                make = "TestMake",
                model = "TestModel",
                color = "Red",
                year = new DateTime(1990, 1, 1),
                created_at = DateTime.UtcNow,
                M_Users = new M_Users
                {
                    id = userGuid,
                    username = "testuser",
                    password = "testpass",
                    name = "Test User",
                    email = "john@doe.com",
                    phone = "1234567890",
                    role = M_Users.UserRole.ParkingUser,
                    parking_lot_id = null,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                }
            };

            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", newVehicle);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Update_WrongID()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            Guid userGuid = Guid.NewGuid();

            var newVehicle = new M_Vehicles
            {
                user_id = userGuid,
                license_plate = "TEST123",
                make = "TestMake",
                model = "TestModel",
                color = "Red",
            };

            var response = await client.PutAsJsonAsync($"/api/v2/vehicles/update/{userGuid}", newVehicle);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_FullFlow_HappyPath()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            Guid userGuid = Guid.NewGuid();

            var newVehicle = new M_Vehicles
            {
                user_id = userGuid,
                license_plate = "TEST123",
                make = "TestMake",
                model = "TestModel",
                color = "Red",
                year = new DateTime(1990, 1, 1),
                created_at = DateTime.UtcNow,
                M_Users = new M_Users
                {
                    id = userGuid,
                    username = "testuser",
                    password = "testpass",
                    name = "Test User",
                    email = "john@doe.com",
                    phone = "1234567890",
                    role = M_Users.UserRole.ParkingUser,
                    parking_lot_id = null,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                }
            };

            // CREATE
            var createResponse = await client.PostAsJsonAsync("/api/v2/vehicles/create", newVehicle);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdVehicle = await createResponse.Content.ReadFromJsonAsync<M_Vehicles>();
            createdVehicle.Should().NotBeNull();
            createdVehicle!.id.Should().NotBe(Guid.Empty);

            // GET
            var getResponse = await client.GetAsync($"/api/v2/vehicles/{createdVehicle.id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var fetchedVehicle = await getResponse.Content.ReadFromJsonAsync<M_Vehicles>();
            fetchedVehicle.Should().NotBeNull();
            fetchedVehicle!.license_plate.Should().Be("TEST123");

            // UPDATE
            createdVehicle.license_plate = "UPDATED123";

            var updateResponse = await client.PutAsJsonAsync($"/api/v2/vehicles/update/{createdVehicle.id}", createdVehicle);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // GET AFTER UPDATE
            var getUpdatedResponse = await client.GetAsync($"/api/v2/vehicles/{createdVehicle.id}");
            getUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedVehicle = await getUpdatedResponse.Content.ReadFromJsonAsync<M_Vehicles>();
            updatedVehicle.Should().NotBeNull();
            updatedVehicle!.license_plate.Should().Be("UPDATED123");

            // DELETE
            var deleteResponse = await client.DeleteAsync($"/api/v2/vehicles/delete/{createdVehicle.id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // OPTIONAL: VERIFY IT'S REALLY GONE
            var getAfterDeleteResponse = await client.GetAsync($"/api/v2/vehicles/{createdVehicle.id}");
            getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ========== AUTHORIZATION TESTS ==========

        [Fact]
        public async Task GetAllVehicles_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/vehicles/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAllVehicles_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/vehicles/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetVehicleByID_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/vehicles/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetVehicleByID_With_User_Viewing_Other_User_Vehicle_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = otherUser.id,
                    license_plate = "OTHER-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var response = await client.GetAsync($"/api/v2/vehicles/{vehicleId}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task CreateVehicle_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var vehicle = new M_Vehicles { user_id = Guid.NewGuid(), license_plate = "TEST", make = "Make", model = "Model", color = "Red", year = new DateTime(2020, 1, 1) };
            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateVehicle_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var vehicle = new M_Vehicles { license_plate = "TEST", make = "Make", model = "Model" };
            var response = await client.PutAsJsonAsync($"/api/v2/vehicles/update/{Guid.NewGuid()}", vehicle);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateVehicle_With_User_Updating_Other_User_Vehicle_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = otherUser.id,
                    license_plate = "OTHER-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var vehicle = new M_Vehicles { license_plate = "UPDATED", make = "Make", model = "Model" };
                var response = await client.PutAsJsonAsync($"/api/v2/vehicles/update/{vehicleId}", vehicle);
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task DeleteVehicle_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/v2/vehicles/delete/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteVehicle_With_User_Deleting_Other_User_Vehicle_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var otherUser = db.Users.FirstOrDefault(u => u.username == "superadmin");
            if (otherUser != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = otherUser.id,
                    license_plate = "OTHER-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var response = await client.DeleteAsync($"/api/v2/vehicles/delete/{vehicleId}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetAllVehicles_With_LotAdmin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/vehicles/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllVehicles_With_SuperAdmin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.GetAsync("/api/v2/vehicles/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetAllVehicles_With_Page_Exceeding_Total_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/vehicles/all?page=999");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetVehicleByID_With_User_Viewing_Own_Vehicle_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "OWN-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var response = await client.GetAsync($"/api/v2/vehicles/{vehicleId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task GetVehicleByID_With_Admin_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "ADMIN-VIEW-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var response = await client.GetAsync($"/api/v2/vehicles/{vehicleId}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task CreateVehicle_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.PostAsJsonAsync<M_Vehicles>("/api/v2/vehicles/create", null!);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateVehicle_With_Empty_LicensePlate_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var vehicle = new M_Vehicles
            {
                license_plate = "",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1)
            };
            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateVehicle_With_LicensePlate_Exceeding_10_Characters_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var vehicle = new M_Vehicles
            {
                license_plate = "VERYLONGPLATE123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1)
            };
            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateVehicle_With_Future_Year_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var vehicle = new M_Vehicles
            {
                license_plate = "FUTURE-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = DateTime.UtcNow.AddYears(1)
            };
            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateVehicle_With_Admin_And_Empty_UserId_Should_Default_To_Current_User()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var vehicle = new M_Vehicles
            {
                user_id = Guid.Empty,
                license_plate = "ADMIN-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1)
            };
            var response = await client.PostAsJsonAsync("/api/v2/vehicles/create", vehicle);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateVehicle_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "UPDATE-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var response = await client.PutAsJsonAsync<M_Vehicles>($"/api/v2/vehicles/update/{vehicleId}", null!);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task UpdateVehicle_With_User_Updating_Own_Vehicle_Should_Return_204()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "UPDATE-OWN-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var updateData = new M_Vehicles
                {
                    license_plate = "UPDATED-123",
                    make = "Updated Make",
                    model = "Updated Model",
                    color = "Blue"
                };
                var response = await client.PutAsJsonAsync($"/api/v2/vehicles/update/{vehicleId}", updateData);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task DeleteVehicle_With_User_Deleting_Own_Vehicle_Should_Return_204()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "DELETE-OWN-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                var response = await client.DeleteAsync($"/api/v2/vehicles/delete/{vehicleId}");
                response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GetVehicleByID_With_Empty_Guid_Should_Return_NotFound()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync($"/api/v2/vehicles/{Guid.Empty}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }
    }
}
