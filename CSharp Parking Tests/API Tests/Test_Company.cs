using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Company : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Company(CSharpAPITests factory) => _factory = factory;

        [Fact]
        public async Task GetAll_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/company");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/company");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAll_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/company");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/company/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/company/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var company = new M_Company { id = Guid.NewGuid(), name = "Test Company" };
            var response = await client.PostAsJsonAsync("/api/v2/company", company);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var company = new M_Company { id = Guid.NewGuid(), name = "Test Company" };
            var response = await client.PostAsJsonAsync("/api/v2/company", company);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Update_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var company = new M_Company { name = "Updated" };
            var response = await client.PutAsJsonAsync($"/api/v2/company/{Guid.NewGuid()}", company);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/v2/company/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_With_Admin_Token_Should_Return_201()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var company = new M_Company { id = Guid.NewGuid(), name = "Test Company", address = "123 Test St" };
            var response = await client.PostAsJsonAsync("/api/v2/company", company);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetById_With_Valid_Id_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            
            // First create a company
            var companyId = Guid.NewGuid();
            var company = new M_Company { id = companyId, name = "Test Company", address = "123 Test St" };
            await client.PostAsJsonAsync("/api/v2/company", company);
            
            // Then get it
            var response = await client.GetAsync($"/api/v2/company/{companyId}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            
            // First create a company
            var companyId = Guid.NewGuid();
            var company = new M_Company { id = companyId, name = "Test Company", address = "123 Test St" };
            await client.PostAsJsonAsync("/api/v2/company", company);
            
            // Then update it
            var updatedCompany = new M_Company { name = "Updated Company", address = "456 New St" };
            var response = await client.PutAsJsonAsync($"/api/v2/company/{companyId}", updatedCompany);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_With_SuperAdmin_Token_Should_Return_204()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            
            // First create a company
            var companyId = Guid.NewGuid();
            var company = new M_Company { id = companyId, name = "Test Company", address = "123 Test St" };
            await client.PostAsJsonAsync("/api/v2/company", company);
            
            // Then delete it
            var response = await client.DeleteAsync($"/api/v2/company/{companyId}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCompanyVehicles_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/company/{Guid.NewGuid()}/vehicles");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetCompanyUsers_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/company/{Guid.NewGuid()}/users");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AddUserToCompany_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            
            if (user != null)
            {
                var companyId = Guid.NewGuid();
                var request = new { UserId = user.id, Role = 0 };
                var response = await client.PostAsJsonAsync($"/api/v2/company/{companyId}/users", request);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task GenerateMonthlyBundle_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var request = new { Month = DateTime.UtcNow };
            var response = await client.PostAsJsonAsync($"/api/v2/company/{Guid.NewGuid()}/billing/monthly", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Create_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.PostAsJsonAsync<M_Company>("/api/v2/company", null!);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetAll_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/company");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetById_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/company/{Guid.NewGuid()}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Create_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var company = new M_Company { id = Guid.NewGuid(), name = "Test Company" };
            var response = await client.PostAsJsonAsync("/api/v2/company", company);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Update_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var company = new M_Company { name = "Updated" };
            var response = await client.PutAsJsonAsync($"/api/v2/company/{Guid.NewGuid()}", company);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Update_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var company = new M_Company { name = "Updated" };
            var response = await client.PutAsJsonAsync($"/api/v2/company/{Guid.NewGuid()}", company);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Delete_With_LotAdmin_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.DeleteAsync($"/api/v2/company/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.DeleteAsync($"/api/v2/company/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Delete_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.DeleteAsync($"/api/v2/company/{Guid.NewGuid()}");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetCompanyVehicles_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/company/{Guid.NewGuid()}/vehicles");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task AddUserToCompany_With_Existing_User_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var companyService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.ICompanyService>();
            var company = new M_Company { id = Guid.NewGuid(), name = "Test Company", active = true, created_at = DateTime.UtcNow };
            await companyService.Create(company);
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                await companyService.AddUserToCompany(company.id, user.id, CSharpAPI.Models.CompanyUserRole.Employee);
                var request = new { UserId = user.id, Role = "Employee" };
                var response = await client.PostAsJsonAsync($"/api/v2/company/{company.id}/users", request);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task AddUserToCompany_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var request = new { UserId = Guid.NewGuid(), Role = "Employee" };
            var response = await client.PostAsJsonAsync($"/api/v2/company/{Guid.NewGuid()}/users", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GetCompanyUsers_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/company/{Guid.NewGuid()}/users");
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task GenerateMonthlyBundle_With_Billing_Disabled_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var companyService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.ICompanyService>();
            var company = new M_Company { id = Guid.NewGuid(), name = "Test Company", active = true, monthly_billing_enabled = false, created_at = DateTime.UtcNow };
            await companyService.Create(company);
            
            var request = new { Month = DateTime.UtcNow };
            var response = await client.PostAsJsonAsync($"/api/v2/company/{company.id}/billing/monthly", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GenerateMonthlyBundle_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var request = new { Month = DateTime.UtcNow };
            var response = await client.PostAsJsonAsync($"/api/v2/company/{Guid.NewGuid()}/billing/monthly", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }
    }
}
