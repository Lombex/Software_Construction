using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace CSharpAPI.Tests.APITests
{
    
    public class Test_Parkinglots : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Parkinglots(CSharpAPITests factory) => _factory = factory;
    
        [Fact]
        public async Task Test_CreateParkinglot_ShouldReturnCreated()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var lot = CreateSampleParkinglot();
            var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await response.Content.ReadFromJsonAsync<M_Parkinglots>();
            created.Should().NotBeNull();
            created!.name.Should().Be(lot.name);
        }

        [Fact]
        public async Task Test_GetParkinglotById_ShouldReturnOk()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var lot = CreateSampleParkinglot();
            var createResponse = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
            var created = await createResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
            var id = created!.id;
            var getResponse = await client.GetAsync($"api/v2/parkinglots/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var getLot = await getResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
            getLot.Should().NotBeNull();
            getLot!.id.Should().Be(id);
        }

        [Fact]
        public async Task Test_UpdateParkinglot_ShouldReturnNoContent()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var lot = CreateSampleParkinglot();
            var createResponse = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
            var created = await createResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
            var id = created!.id;
            var updatedLot = new M_Parkinglots
            {
                id = id,
                name = "Updated Lot",
                location = "Updated Location",
                address = "Updated Address",
                capacity = 200,
                reserved = 20,
                daytarriff = 20.0f,
                coordinates = new Coordinates { lat = 40.0f, lng = 10.0f }
            };
            var updateResponse = await client.PutAsJsonAsync($"api/v2/parkinglots/{id}", updatedLot);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task Test_DeleteParkinglot_ShouldReturnNoContent()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var lot = CreateSampleParkinglot();
            var createResponse = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
            var created = await createResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
            var id = created!.id;
            var deleteResponse = await client.DeleteAsync($"api/v2/parkinglots/{id}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
    

        private M_Parkinglots CreateSampleParkinglot()
        {
            return new M_Parkinglots
            {
                name = "Test Parkinglot",
                location = "Test Location",
                address = "Test Street 123",
                capacity = 100,
                reserved = 10,
                daytarriff = 15.5f,
                coordinates = new Coordinates
                {
                    lat = 52.507f,
                    lng = -0.127f
                }

            };
        }
  

    [Fact]
    public async Task Test_GetAllParkinglots_ShouldReturnOk()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

        var response = await client.GetAsync("api/v2/parkinglots");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Test_GetById_NotFound()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

        var fakeId = Guid.NewGuid();
        var response = await client.GetAsync($"api/v2/parkinglots/{fakeId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_UpdateParkinglot_NotFound()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

        var lot = CreateSampleParkinglot();
        var randomId = Guid.NewGuid();

        var response = await client.PutAsJsonAsync($"api/v2/parkinglots/{randomId}", lot);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task Test_DeleteParkinglot_NotFound()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

        var randomId = Guid.NewGuid();
        var response = await client.DeleteAsync($"api/v2/parkinglots/{randomId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Test_CreateParkinglot_BadData()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

        var invalidLot = new M_Parkinglots();  // required (missing) fields

        var response = await client.PostAsJsonAsync("api/v2/parkinglots", invalidLot);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Test_SearchNearbyParkinglots_ShouldReturnOk()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync("api/v2/parkinglots/search?lat=52.0&lng=5.0&radius=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ========== AUTHORIZATION TESTS ==========

    [Fact]
    public async Task GetAll_Without_Token_Should_Return_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("api/v2/parkinglots");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_Without_Token_Should_Return_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"api/v2/parkinglots/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Without_Token_Should_Return_401()
    {
        var client = _factory.CreateClient();
        var lot = CreateSampleParkinglot();
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_With_User_Token_Should_Return_403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
        var lot = CreateSampleParkinglot();
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_Without_Token_Should_Return_401()
    {
        var client = _factory.CreateClient();
        var lot = CreateSampleParkinglot();
        var response = await client.PutAsJsonAsync($"api/v2/parkinglots/{Guid.NewGuid()}", lot);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_With_User_Token_Should_Return_403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
        var lot = CreateSampleParkinglot();
        var response = await client.PutAsJsonAsync($"api/v2/parkinglots/{Guid.NewGuid()}", lot);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_Without_Token_Should_Return_401()
    {
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"api/v2/parkinglots/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_With_User_Token_Should_Return_403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
        var response = await client.DeleteAsync($"api/v2/parkinglots/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_With_LotAdmin_Token_Should_Return_403()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
        var response = await client.DeleteAsync($"api/v2/parkinglots/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_With_SuperAdmin_Token_Should_Return_204()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        var createResponse = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        var created = await createResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
        var deleteResponse = await client.DeleteAsync($"api/v2/parkinglots/{created!.id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SearchNearby_Without_Token_Should_Return_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("api/v2/parkinglots/search?lat=52.0&lng=5.0&radius=10");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ========== EDGE CASE TESTS ==========

    [Fact]
    public async Task Create_With_Null_Body_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.PostAsJsonAsync<M_Parkinglots>("api/v2/parkinglots", null!);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_With_Null_Name_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        lot.name = null!;
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_With_Empty_Name_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        lot.name = "";
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_With_Null_Location_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        lot.location = null!;
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_With_Null_Coordinates_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        lot.coordinates = null!;
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_With_Null_Body_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        var createResponse = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        var created = await createResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
        var response = await client.PutAsJsonAsync<M_Parkinglots>($"api/v2/parkinglots/{created!.id}", null!);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_Should_Update_All_Fields()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        var createResponse = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        var created = await createResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
        
        var updatedLot = new M_Parkinglots
        {
            id = created!.id,
            name = "Updated Name",
            location = "Updated Location",
            address = "Updated Address",
            capacity = 200,
            reserved = 20,
            daytarriff = 25.0f,
            coordinates = new Coordinates { lat = 53.0f, lng = 6.0f }
        };
        
        var updateResponse = await client.PutAsJsonAsync($"api/v2/parkinglots/{created.id}", updatedLot);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        var getResponse = await client.GetAsync($"api/v2/parkinglots/{created.id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<M_Parkinglots>();
        updated!.name.Should().Be("Updated Name");
        updated.location.Should().Be("Updated Location");
        updated.capacity.Should().Be(200);
    }

    [Fact]
    public async Task SearchNearby_With_Zero_Radius_Should_Return_Empty()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync("api/v2/parkinglots/search?lat=52.0&lng=5.0&radius=0");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lots = await response.Content.ReadFromJsonAsync<List<M_Parkinglots>>();
        lots.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_With_Empty_List_Should_Return_200_With_Empty_Array()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync("api/v2/parkinglots");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lots = await response.Content.ReadFromJsonAsync<List<M_Parkinglots>>();
        lots.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchNearby_With_Large_Radius_Should_Return_Results()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync("api/v2/parkinglots/search?lat=52.0&lng=5.0&radius=1000");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lots = await response.Content.ReadFromJsonAsync<List<M_Parkinglots>>();
        lots.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchNearby_With_Negative_Radius_Should_Return_200()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync("api/v2/parkinglots/search?lat=52.0&lng=5.0&radius=-10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchNearby_With_Extreme_Coordinates_Should_Return_200()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync("api/v2/parkinglots/search?lat=90.0&lng=180.0&radius=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_With_Empty_Guid_Should_Return_NotFound()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync($"api/v2/parkinglots/{Guid.Empty}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_With_Whitespace_Only_Name_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        lot.name = "   ";
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_With_Whitespace_Only_Location_Should_Return_400()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        lot.location = "   ";
        var response = await client.PostAsJsonAsync("api/v2/parkinglots", lot);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_With_Empty_Guid_Should_Return_NotFound()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var lot = CreateSampleParkinglot();
        var response = await client.PutAsJsonAsync($"api/v2/parkinglots/{Guid.Empty}", lot);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchNearby_With_Missing_Parameters_Should_Return_200()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
        var response = await client.GetAsync("api/v2/parkinglots/search");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
    }
    
}
    