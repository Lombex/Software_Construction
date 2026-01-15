using CSharpAPI.Models;
using CSharpAPI.Tests;
using CSharpAPI.Tests.Utillities;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

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
            var getUpdatedResponse = await client.GetAsync($"/api/vehicles/{createdVehicle.id}");
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
    }
}
