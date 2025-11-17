using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace CSharp_Parking_API.Tests
{
    public class PaymentIntegrationTests : IClassFixture<TestingWebAppFactory>
    {
        private readonly TestingWebAppFactory _factory;
        public PaymentIntegrationTests(TestingWebAppFactory factory)
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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "user", Password = "userpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "lotadmin", Password = "lotpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "superadmin", Password = "superpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "user", Password = "userpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "user", Password = "userpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "lotadmin", Password = "lotpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "user", Password = "userpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "lotadmin", Password = "lotpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
            var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "superadmin", Password = "superpass" });
            var token = (await login.Content.ReadFromJsonAsync<TokenResponse>())!.token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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

        private class TokenResponse
        {
            public string token { get; set; } = string.Empty;
            public DateTime expiresAt { get; set; }
        }
    }
}


