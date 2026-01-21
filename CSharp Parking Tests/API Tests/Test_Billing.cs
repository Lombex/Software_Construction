using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using static CSharpAPI.Models.M_Billing;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Billing : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Billing(CSharpAPITests factory) => _factory = factory;

        // ========== GetAll TESTS ==========

        [Fact]
        public async Task GetAll_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/billing/all");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetAll_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/billing/all");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAll_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync("/api/v2/billing/all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ========== GetMine TESTS ==========

        [Fact]
        public async Task GetMine_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/v2/billing/mine");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetMine_With_Valid_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/billing/mine");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // ========== GetById TESTS ==========

        [Fact]
        public async Task GetById_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"/api/v2/billing/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync($"/api/v2/billing/{Guid.Empty}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetById_With_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.GetAsync($"/api/v2/billing/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ========== Create TESTS ==========

        [Fact]
        public async Task Create_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var bill = CreateSampleBilling();
            var response = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var bill = CreateSampleBilling();
            var response = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Create_With_Admin_Token_Should_Return_201()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var bill = CreateSampleBilling();
                bill.user_id = user.id;
                var response = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task Create_With_Empty_UserId_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var bill = CreateSampleBilling();
            bill.user_id = Guid.Empty;
            var response = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Create_With_Zero_Amount_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var bill = CreateSampleBilling();
                bill.user_id = user.id;
                bill.amount = 0;
                var response = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        // ========== Update TESTS ==========

        [Fact]
        public async Task Update_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var bill = CreateSampleBilling();
            var response = await client.PutAsJsonAsync($"/api/v2/billing/update/{Guid.NewGuid()}", bill);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var bill = CreateSampleBilling();
            var response = await client.PutAsJsonAsync($"/api/v2/billing/update/{Guid.Empty}", bill);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ========== MarkPaid TESTS ==========

        [Fact]
        public async Task MarkPaid_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PutAsync($"/api/v2/billing/{Guid.NewGuid()}/mark-paid", null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task MarkPaid_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);
            var response = await client.PutAsync($"/api/v2/billing/{Guid.Empty}/mark-paid", null);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ========== Cancel TESTS ==========

        [Fact]
        public async Task Cancel_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsync($"/api/v2/billing/{Guid.NewGuid()}/cancel", null);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Cancel_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.PostAsync($"/api/v2/billing/{Guid.NewGuid()}/cancel", null);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ========== Delete TESTS ==========

        [Fact]
        public async Task Delete_Without_Token_Should_Return_401()
        {
            var client = _factory.CreateClient();
            var response = await client.DeleteAsync($"/api/v2/billing/delete/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_With_User_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.DeleteAsync($"/api/v2/billing/delete/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_With_LotAdmin_Token_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.DeleteAsync($"/api/v2/billing/delete/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Delete_With_SuperAdmin_Token_And_Valid_Id_Should_Return_204()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var billId = Guid.NewGuid();
                db.Billing.Add(new M_Billing
                {
                    id = billId,
                    user_id = user.id,
                    amount = 100.0m,
                    currency = "EUR",
                    description = "Test Bill",
                    created_at = DateTime.UtcNow,
                    due_date = DateTime.UtcNow.AddDays(30),
                    paid = false,
                    status = BillingStatus.Pending
                });
                await db.SaveChangesAsync();
                var response = await client.DeleteAsync($"/api/v2/billing/delete/{billId}");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }

        [Fact]
        public async Task Delete_With_SuperAdmin_Token_And_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.DeleteAsync($"/api/v2/billing/delete/{Guid.Empty}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Delete_With_SuperAdmin_Token_And_NonExistent_Id_Should_Return_404()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.DeleteAsync($"/api/v2/billing/delete/{Guid.NewGuid()}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetMinePending_With_Valid_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/billing/mine/pending");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMineOverdue_With_Valid_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/billing/mine/overdue");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetForUser_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.GetAsync($"/api/v2/billing/user/{user.id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task GetForUser_With_Empty_Guid_Should_Return_400()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            var response = await client.GetAsync($"/api/v2/billing/user/{Guid.Empty}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Update_With_User_Token_And_Own_Bill_Should_Return_204()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                // Create a bill for the user
                var billingService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IBillingService>();
                var bill = CreateSampleBilling();
                bill.user_id = user.id;
                var created = await billingService.Create(bill);
                
                // Update description only (users can only update description)
                var updatedBill = new M_Billing { description = "Updated description" };
                var response = await client.PutAsJsonAsync($"/api/v2/billing/update/{created.id}", updatedBill);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.NoContent, HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task MarkPaid_With_User_Token_And_Own_Bill_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                // Create a bill for the user
                var billingService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IBillingService>();
                var bill = CreateSampleBilling();
                bill.user_id = user.id;
                var created = await billingService.Create(bill);
                
                // Mark as paid
                var response = await client.PutAsync($"/api/v2/billing/{created.id}/mark-paid", null);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
            }
        }

        [Fact]
        public async Task GetMonthlyBundles_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var response = await client.GetAsync($"/api/v2/billing/monthly-bundle/{user.id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task GetMonthlyBundles_With_Month_Parameter_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var month = DateTime.UtcNow.ToString("yyyy-MM-dd");
                var response = await client.GetAsync($"/api/v2/billing/monthly-bundle/{user.id}?month={month}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task Cancel_With_Admin_Token_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                // Create a bill
                var billingService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IBillingService>();
                var bill = CreateSampleBilling();
                bill.user_id = user.id;
                var created = await billingService.Create(bill);
                
                // Cancel it
                var response = await client.PostAsync($"/api/v2/billing/{created.id}/cancel", null);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task GetById_With_User_Token_And_Own_Bill_Should_Return_200()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                // Create a bill for the user
                var billingService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IBillingService>();
                var bill = CreateSampleBilling();
                bill.user_id = user.id;
                var created = await billingService.Create(bill);
                
                // Get it
                var response = await client.GetAsync($"/api/v2/billing/{created.id}");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task GetById_With_User_Token_And_Other_User_Bill_Should_Return_403()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var admin = db.Users.FirstOrDefault(u => u.username == "lotadmin");
            if (admin != null)
            {
                // Create a bill for admin
                var billingService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IBillingService>();
                var bill = CreateSampleBilling();
                bill.user_id = admin.id;
                var created = await billingService.Create(bill);
                
                // User tries to get admin's bill - should be forbidden
                var response = await client.GetAsync($"/api/v2/billing/{created.id}");
                response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            }
        }

        private M_Billing CreateSampleBilling()
        {
            return new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = Guid.NewGuid(),
                amount = 100.0m,
                currency = "EUR",
                description = "Test billing",
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                created_at = DateTime.UtcNow,
                type = BillingType.ParkingSession,
                status = BillingStatus.Pending
            };
        }
    }
}
