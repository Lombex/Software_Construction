using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Billing : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Billing(CSharpAPITests factory) => _factory = factory;

        [Fact]
        public async Task Test_CreateBill_ShouldReturnCreated()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            // regular user uit test db
            using var scope = _factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.First(u => u.username == "user");
            var bill = new M_Billing
            {
                user_id = user.id,
                amount = 25.50m,
                currency = "EUR",
                due_date = DateTime.UtcNow.AddDays(7),
                type = BillingType.ParkingSession,
                status = BillingStatus.Pending,
                description = "Parking session bill"
            };
            var response = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
            response.StatusCode.Should().Be(HttpStatusCode.Created, $"Error: {await response.Content.ReadAsStringAsync()}");
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var created = await response.Content.ReadFromJsonAsync<M_Billing>();
                created.Should().NotBeNull();
                created!.amount.Should().Be(bill.amount);
            }
        }

        [Fact]
        public async Task Test_GetAllBills_AdminOnly()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var response = await client.GetAsync("/api/v2/billing/all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_GetMineBills_User()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/billing/mine");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_GetMinePendingBills_User()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/v2/billing/mine/pending");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_MarkBillPaid_ShouldReturnNoContent() // test error hier
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            // reg user uit test db
            using var scope = _factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.First(u => u.username == "user");
            var bill = new M_Billing
            {
                user_id = user.id,
                amount = 10.00m,
                currency = "EUR",
                due_date = DateTime.UtcNow.AddDays(3),
                type = BillingType.ParkingSession,
                status = BillingStatus.Pending,
                description = "Test bill for payment"
            };
            var createResponse = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                var created = await createResponse.Content.ReadFromJsonAsync<M_Billing>();
                var id = created!.id;
                var payUrl = $"/api/v2/billing/{id}/mark-paid";
                // Debug output
                Console.WriteLine($"Created bill id: {id}");
                Console.WriteLine($"Pay endpoint: {payUrl}");
                var payResponse = await client.PutAsync(payUrl, null);
                Console.WriteLine($"Pay response status: {payResponse.StatusCode}");
                payResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            else
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"Create bill failed: {error}");
                Assert.True(false, $"Create bill failed: {error}");
            }
        }

        [Fact]
        public async Task Test_DeleteBill_ShouldReturnNoContent() // test error V
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            // reg user uit test db
            using var scope = _factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CSharpAPI.Database.SQLite_Database>();
            var user = db.Users.First(u => u.username == "user");
            var bill = new M_Billing
            {
                user_id = user.id,
                amount = 5.00m,
                currency = "EUR",
                due_date = DateTime.UtcNow.AddDays(2),
                type = BillingType.ParkingSession,
                status = BillingStatus.Pending,
                description = "Test bill for deletion"
            };
            var createResponse = await client.PostAsJsonAsync("/api/v2/billing/create", bill);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                var created = await createResponse.Content.ReadFromJsonAsync<M_Billing>();
                var id = created!.id;
                var deleteResponse = await client.DeleteAsync($"/api/v2/billing/delete/{id}");
                deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
            else
            {
                Assert.True(false, $"Create bill failed: {await createResponse.Content.ReadAsStringAsync()}");
            }
        }
    }
}
