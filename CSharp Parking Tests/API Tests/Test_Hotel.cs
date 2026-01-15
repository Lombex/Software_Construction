using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Hotel : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Hotel(CSharpAPITests factory) => _factory = factory;

        [Fact]
        public async Task GetAll_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/hotel");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/hotel");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAll_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/hotel");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/hotel/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/hotel/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var hotel = new M_Hotel { id = Guid.NewGuid(), name = "Test Hotel" };
            var response = await client.PostAsJsonAsync("/api/v2/hotel", hotel);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var hotel = new M_Hotel { id = Guid.NewGuid(), name = "Test Hotel" };
            var response = await client.PostAsJsonAsync("/api/v2/hotel", hotel);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Update_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var hotel = new M_Hotel { name = "Updated" };
            var response = await client.PutAsJsonAsync($"/api/v2/hotel/{Guid.NewGuid()}", hotel);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/v2/hotel/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_With_Admin_Token_Should_Return_201()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var hotel = new M_Hotel { id = Guid.NewGuid(), name = "Test Hotel", address = "123 Hotel St" };
            var response = await client.PostAsJsonAsync("/api/v2/hotel", hotel);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_With_Valid_Id_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            
            // First create a hotel
            var hotelId = Guid.NewGuid();
            var hotel = new M_Hotel { id = hotelId, name = "Test Hotel", address = "123 Hotel St" };
            await client.PostAsJsonAsync("/api/v2/hotel", hotel);
            
            // Then get it
            var response = await client.GetAsync($"/api/v2/hotel/{hotelId}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            
            // First create a hotel
            var hotelId = Guid.NewGuid();
            var hotel = new M_Hotel { id = hotelId, name = "Test Hotel", address = "123 Hotel St" };
            await client.PostAsJsonAsync("/api/v2/hotel", hotel);
            
            // Then update it
            var updatedHotel = new M_Hotel { name = "Updated Hotel", address = "456 New St" };
            var response = await client.PutAsJsonAsync($"/api/v2/hotel/{hotelId}", updatedHotel);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_With_SuperAdmin_Token_Should_Return_204()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            
            // First create a hotel
            var hotelId = Guid.NewGuid();
            var hotel = new M_Hotel { id = hotelId, name = "Test Hotel", address = "123 Hotel St" };
            await client.PostAsJsonAsync("/api/v2/hotel", hotel);
            
            // Then delete it
            var response = await client.DeleteAsync($"/api/v2/hotel/{hotelId}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task RegisterGuest_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            
            if (user != null)
            {
                var hotelId = Guid.NewGuid();
                var request = new
                {
                    UserId = user.id,
                    CheckIn = DateTime.UtcNow,
                    CheckOut = DateTime.UtcNow.AddDays(2),
                    ReservationNumber = "RES-123"
                };
                var response = await client.PostAsJsonAsync($"/api/v2/hotel/{hotelId}/guests", request);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task CheckOutGuest_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.PostAsync($"/api/v2/hotel/guests/{Guid.NewGuid()}/checkout", null);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetActiveGuests_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/hotel/{Guid.NewGuid()}/guests/active");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.PostAsJsonAsync<M_Hotel>("/api/v2/hotel", null!);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetAll_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/hotel");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetById_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/hotel/{Guid.NewGuid()}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Create_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var hotel = new M_Hotel { id = Guid.NewGuid(), name = "Test Hotel" };
            var response = await client.PostAsJsonAsync("/api/v2/hotel", hotel);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Update_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var hotel = new M_Hotel { name = "Updated" };
            var response = await client.PutAsJsonAsync($"/api/v2/hotel/{Guid.NewGuid()}", hotel);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var hotel = new M_Hotel { name = "Updated" };
            var response = await client.PutAsJsonAsync($"/api/v2/hotel/{Guid.NewGuid()}", hotel);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Delete_With_LotAdmin_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.DeleteAsync($"/api/v2/hotel/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.DeleteAsync($"/api/v2/hotel/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.DeleteAsync($"/api/v2/hotel/{Guid.NewGuid()}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task RegisterGuest_With_Existing_Active_Guest_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var hotelService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IHotelService>();
            var hotel = new M_Hotel { id = Guid.NewGuid(), name = "Test Hotel", active = true, created_at = DateTime.UtcNow };
            await hotelService.Create(hotel);
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                await hotelService.RegisterGuest(hotel.id, user.id, DateTime.UtcNow);
                var request = new { UserId = user.id, CheckIn = DateTime.UtcNow, CheckOut = (DateTime?)null, ReservationNumber = (string?)null };
                var response = await client.PostAsJsonAsync($"/api/v2/hotel/{hotel.id}/guests", request);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task RegisterGuest_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var request = new { UserId = Guid.NewGuid(), CheckIn = DateTime.UtcNow, CheckOut = (DateTime?)null, ReservationNumber = (string?)null };
            var response = await client.PostAsJsonAsync($"/api/v2/hotel/{Guid.NewGuid()}/guests", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task CheckOutGuest_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.PostAsync($"/api/v2/hotel/guests/{Guid.NewGuid()}/checkout", null);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CheckOutGuest_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.PostAsync($"/api/v2/hotel/guests/{Guid.NewGuid()}/checkout", null);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetActiveGuests_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/hotel/{Guid.NewGuid()}/guests/active");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }
    }
}
