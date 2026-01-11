using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using System;
using System.Threading.Tasks;

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
            var bill = new M_Billing
            {
                user_id = Guid.Parse("1abe31ef-255a-4c52-88c7-d4ca4c36fdb1"), 
                amount = 25.50m,
                currency = "EUR",
                due_date = DateTime.UtcNow.AddDays(7),
                type = BillingType.ParkingSession,
                status = BillingStatus.Pending,
                description = "Parking session bill"
            };
            var response = await client.PostAsJsonAsync("/api/billing/create", bill);
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
            var response = await client.GetAsync("/api/billing/all");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_GetMineBills_User()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/billing/mine");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_GetMinePendingBills_User()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");
            var response = await client.GetAsync("/api/billing/mine/pending");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Test_MarkBillPaid_ShouldReturnNoContent()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var bill = new M_Billing
            {
                user_id = Guid.Parse("1abe31ef-255a-4c52-88c7-d4ca4c36fdb1"),
                amount = 10.00m,
                currency = "EUR",
                due_date = DateTime.UtcNow.AddDays(3),
                type = BillingType.ParkingSession,
                status = BillingStatus.Pending,
                description = "Test bill for payment"
            };
            var createResponse = await client.PostAsJsonAsync("/api/billing/create", bill);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                var created = await createResponse.Content.ReadFromJsonAsync<M_Billing>();
                var id = created!.id;
                var payResponse = await client.PutAsync($"/api/billing/{id}/paid", null);
                payResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
            else
            {
                Assert.True(false, $"Create bill failed: {await createResponse.Content.ReadAsStringAsync()}");
            }
        }

        [Fact]
        public async Task Test_DeleteBill_ShouldReturnNoContent()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "superadmin", "superpass");
            var bill = new M_Billing
            {
                user_id = Guid.Parse("1abe31ef-255a-4c52-88c7-d4ca4c36fdb1"), 
                amount = 5.00m,
                currency = "EUR",
                due_date = DateTime.UtcNow.AddDays(2),
                type = BillingType.ParkingSession,
                status = BillingStatus.Pending,
                description = "Test bill for deletion"
            };
            var createResponse = await client.PostAsJsonAsync("/api/billing/create", bill);
            if (createResponse.StatusCode == HttpStatusCode.Created)
            {
                var created = await createResponse.Content.ReadFromJsonAsync<M_Billing>();
                var id = created!.id;
                var deleteResponse = await client.DeleteAsync($"/api/billing/delete/{id}");
                deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
            else
            {
                Assert.True(false, $"Create bill failed: {await createResponse.Content.ReadAsStringAsync()}");
            }
        }
    }
}
