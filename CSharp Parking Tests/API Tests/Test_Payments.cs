using CSharpAPI.Tests.Utillities;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CSharpAPI.Tests.APITests
{
    public class Test_Payments : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_Payments(CSharpAPITests factory)
        {
            _factory = factory;
        }

        // Test: Unauthenticated user cannot access payment endpoints
        [Fact]
        public async Task GetAllPayments_WithoutToken_Returns401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/payments/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // Test: Regular user cannot view all payments (admin only)
        [Fact]
        public async Task GetAllPayments_WithUserToken_Returns403()
        {
            var client = _factory.CreateClient();

            // Login as regular user
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            // Try to access all payments (admin only endpoint)
            var response = await client.GetAsync("/api/payments/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // Test: ParkingLotAdmin can view all payments
        [Fact]
        public async Task GetAllPayments_WithLotAdminToken_Returns200()
        {
            var client = _factory.CreateClient();

            // Login as lot admin
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");

            // Access all payments
            var response = await client.GetAsync("/api/payments/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Test: SuperAdmin can view all payments
        [Fact]
        public async Task GetAllPayments_WithSuperAdminToken_Returns200()
        {
            var client = _factory.CreateClient();

            // Login as super admin
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            // Access all payments
            var response = await client.GetAsync("/api/payments/all?page=0");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Test: Regular user can create payment for their reservation
        [Fact]
        public async Task CreatePayment_WithUserToken_Returns200()
        {
            var client = _factory.CreateClient();

            // Login as regular user
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            // Create payment
            var payment = new
            {
                id = Guid.NewGuid(),
                reservation_id = Guid.NewGuid(),
                paid_at = DateTime.UtcNow,
                transactions = "TRX123",
                amount = 50.0f,
                initiator = "user",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow.AddMinutes(1),
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            var response = await client.PostAsJsonAsync("/api/payments/create", payment);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Test: Regular user cannot update payments (admin only)
        [Fact]
        public async Task UpdatePayment_WithUserToken_Returns403()
        {
            var client = _factory.CreateClient();

            // Login as regular user
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            // Try to update payment
            var paymentId = Guid.NewGuid();
            var payment = new { amount = 75.0f };
            var response = await client.PutAsJsonAsync($"/api/payments/update/{paymentId}", payment);
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // Test: ParkingLotAdmin can update payments
        [Fact]
        public async Task UpdatePayment_WithLotAdminToken_Returns200()
        {
            var client = _factory.CreateClient();

            // Login as lot admin
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");

            // First create a payment
            var paymentId = Guid.NewGuid();
            var createPayment = new
            {
                id = paymentId,
                reservation_id = Guid.NewGuid(),
                paid_at = DateTime.UtcNow,
                transactions = "TRX456",
                amount = 50.0f,
                initiator = "lotadmin",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow.AddMinutes(1),
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };
            await client.PostAsJsonAsync("/api/payments/create", createPayment);

            // Update payment
            var response = await client.PutAsJsonAsync($"/api/payments/update/{paymentId}", createPayment);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Test: Regular user cannot delete payments (super admin only)
        [Fact]
        public async Task DeletePayment_WithUserToken_Returns403()
        {
            var client = _factory.CreateClient();

            // Login as regular user
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "user", "userpass");

            // Try to delete payment
            var paymentId = Guid.NewGuid();
            var response = await client.DeleteAsync($"/api/payments/delete/{paymentId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // Test: ParkingLotAdmin cannot delete payments (super admin only)
        [Fact]
        public async Task DeletePayment_WithLotAdminToken_Returns403()
        {
            var client = _factory.CreateClient();

            // Login as lot admin
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client, "lotadmin", "lotpass");

            // Try to delete payment
            var paymentId = Guid.NewGuid();
            var response = await client.DeleteAsync($"/api/payments/delete/{paymentId}");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // Test: SuperAdmin can delete payments
        [Fact]
        public async Task DeletePayment_WithSuperAdminToken_Returns200()
        {
            var client = _factory.CreateClient();

            // Login as super admin
            client.DefaultRequestHeaders.Authorization = await Utils.AuthenticateAsync(client);

            // Create a payment first
            var paymentId = Guid.NewGuid();
            var payment = new
            {
                id = paymentId,
                reservation_id = Guid.NewGuid(),
                paid_at = DateTime.UtcNow,
                transactions = "TRX789",
                amount = 100.0f,
                initiator = "superadmin",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow.AddMinutes(1),
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };
            await client.PostAsJsonAsync("/api/payments/create", payment);

            // Delete payment
            var response = await client.DeleteAsync($"/api/payments/delete/{paymentId}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}


