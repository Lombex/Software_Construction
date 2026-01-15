using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;

namespace CSharpAPI.Tests.APITests
{
    public class UsersTest : IClassFixture<CSharpAPITests> 
    {
        private readonly CSharpAPITests _factory;
        public UsersTest(CSharpAPITests factory) => _factory = factory;
        
        [Fact]
        public async Task Test_Pagination_HappyFlow()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/users/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    
        [Fact]
        public async Task Test_Pagination_NegativePage()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/users/all?page=-1");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task Test_Pagination_EmptyPage()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync("/api/v2/users/all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_GetById_UnknownID()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var unknownId = Guid.NewGuid();
            var response = await client.GetAsync($"/api/v2/users/{unknownId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_Create_NotPossible_BirthDate()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var user = new
            {
                username = "test_user_1",
                password = "Password123!",
                name = "Test User",
                email = "user1@test.com",
                phone = "0612345678",
                role = M_Users.UserRole.ParkingUser,
                parking_lot_id = Guid.NewGuid(),
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(DateTime.Now.Year + 1, 1, 1),
                active = true
            };

            var response = await client.PostAsJsonAsync("/api/v2/users/create", user);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Create_WrongData()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var badUser = new
            {
                username = "", // invalid
                password = "", // invalid
                email = "not-an-email" // invalid
            };

            var response = await client.PostAsJsonAsync("/api/v2/users/create", badUser);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Invalid_Email()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var user = new
            {
                username = "test_user_2",
                password = "Password123!",
                name = "Test User 2",
                email = "invalid-email-format",
                phone = "0612345678",
                role = M_Users.UserRole.ParkingUser,
                parking_lot_id = Guid.NewGuid(),
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 5, 20),
                active = true
            };
            var response = await client.PostAsJsonAsync("/api/v2/users/create", user);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Test_Create_HappyFlow()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var user = new
            {
                username = "test_user_1",
                password = "Password123!",
                name = "Test User",
                email = "user1@test.com",
                phone = "0612345678",
                role = M_Users.UserRole.ParkingUser,
                parking_lot_id = Guid.NewGuid(),
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1995, 1, 1),
                active = true
            };

            var response = await client.PostAsJsonAsync("/api/v2/users/create", user);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Test_Update_GoodData_WrongID()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var updateUser = new
            {
                username = "updated",
                password = "NewPassword123!",
                name = "Updated User",
                email = "updated@test.com",
                phone = "0612345678",
                role = 0,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1995, 1, 1),
                active = true
            };

            var response = await client.PutAsJsonAsync($"/api/v2/users/update/{Guid.NewGuid()}", updateUser);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_Update_WrongData()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var updateUser = new
            {
                username = "updated",
                password = "NewPassword123!",
                name = "Updated User",
                email = "bbbb",
                phone = "aaaaa",
                role = "TESTING",
                created_at = DateTime.UtcNow,
                birth_year = 12354,
                active = true
            };

            var response = await client.PutAsJsonAsync($"/api/v2/users/update/{Guid.NewGuid()}", updateUser);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        
        [Fact]
        public async Task Test_Delete_WrongID()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.DeleteAsync($"/api/v2/users/delete/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Test_User_HappyFlow()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            // 1. CREATE
            var newUser = new
            {
                username = "flow_user",
                password = "Password123!",
                name = "Flow User",
                email = "flow@test.com",
                phone = "0612345678",
                role = M_Users.UserRole.ParkingUser,
                parking_lot_id = (Guid?)null,                 
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1995, 1, 1),
                active = true
            };

            var createResponse = await client.PostAsJsonAsync("/api/v2/users/create", newUser);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Body is the created M_Users, mapped to UserDto
            var createdUser = await createResponse.Content.ReadFromJsonAsync<UserDto>();
            createdUser.Should().NotBeNull();
            createdUser!.id.Should().NotBe(Guid.Empty);
            createdUser.username.Should().Be("flow_user");

            var id = createdUser.id;

            // 2. GET BY ID
            var getResponse = await client.GetAsync($"/api/v2/users/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getUser = await getResponse.Content.ReadFromJsonAsync<UserDto>();
            getUser.Should().NotBeNull();
            getUser!.id.Should().Be(id);

            // 3. UPDATE
            var updateUser = new
            {
                id = id,
                username = createdUser.username,
                password = "NewPass123!",
                name = "Updated Flow User",
                email = createdUser.email,
                phone = "0699999999",
                role = createdUser.role,                     
                parking_lot_id = (Guid?)null,
                created_at = createdUser.created_at,
                birth_year = createdUser.birth_year,        
                active = true
            };

            var updateResponse = await client.PutAsJsonAsync($"/api/v2/users/update/{id}", updateUser);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 4. GET AFTER UPDATE
            var afterUpdate = await client.GetAsync($"/api/v2/users/{id}");
            afterUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

            var updatedUser = await afterUpdate.Content.ReadFromJsonAsync<UserDto>();
            updatedUser.Should().NotBeNull();
            updatedUser!.name.Should().Be("Updated Flow User");
            updatedUser.phone.Should().Be("0699999999");

            // 5. DELETE
            var deleteResponse = await client.DeleteAsync($"/api/v2/users/delete/{id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 6. CONFIRM DELETE
            var checkDeleted = await client.GetAsync($"/api/v2/users/{id}");
            checkDeleted.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private class UserDto
        {
            public Guid id { get; set; }
            public string? username { get; set; }
            public string? email { get; set; }
            public string? phone { get; set; }
            public string? name { get; set; }

            // Match the model / JSON
            public M_Users.UserRole role { get; set; }
            public bool active { get; set; }
            public DateTime birth_year { get; set; }
            public DateTime created_at { get; set; }
        }
    }
}
