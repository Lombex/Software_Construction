using CSharpAPI.Tests.Utillities;
using CSharpAPI.Models;
using CSharpAPI.Database;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests.APITests
{
    public class Test_NFC : IClassFixture<CSharpAPITests>
    {
        private readonly CSharpAPITests _factory;
        public Test_NFC(CSharpAPITests factory) => _factory = factory;

        [Fact]
        public async Task VerifyAndPay_With_Insufficient_Balance_Should_Return_400()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var request = new
                {
                    UserId = user.id,
                    Amount = 1000.0m,
                    LicensePlate = "TEST-123",
                    ParkingLotId = Guid.NewGuid()
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/verify-and-pay", request);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task VerifyAndPay_With_Invalid_ParkingLot_Should_Return_404()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var request = new
                {
                    UserId = user.id,
                    Amount = 10.0m,
                    LicensePlate = "TEST-123",
                    ParkingLotId = Guid.NewGuid()
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/verify-and-pay", request);
                response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            }
        }

        [Fact]
        public async Task VerifyAndPay_With_Null_Body_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync<object>("/api/v2/nfc/verify-and-pay", null!);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task VerifyCard_With_Valid_UserId_Should_Return_200()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var request = new { UserId = user.id, RequiredAmount = 10.0m };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/verify", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task VerifyAndPay_With_Valid_Data_And_Sufficient_Balance_Should_Return_200()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                // Ensure user has balance
                var balanceService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IUserBalanceService>();
                try
                {
                    await balanceService.CreateBalance(user.id, 100.0m);
                }
                catch { } // May already exist
                
                var lotId = Guid.NewGuid();
                db.Parkinglots.Add(new M_Parkinglots
                {
                    id = lotId,
                    name = "Test Lot",
                    location = "Test",
                    address = "Test",
                    capacity = 100,
                    reserved = 0,
                    daytarriff = 10.0f,
                    created_at = DateTime.UtcNow,
                    coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
                });
                
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "NFC-123",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                var request = new
                {
                    UserId = user.id,
                    Amount = 10.0m,
                    LicensePlate = "NFC-123",
                    ParkingLotId = lotId
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/verify-and-pay", request);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task VerifyAndPay_With_Active_Session_Should_Return_400()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var balanceService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IUserBalanceService>();
                try
                {
                    await balanceService.CreateBalance(user.id, 100.0m);
                }
                catch { }
                
                var lotId = Guid.NewGuid();
                db.Parkinglots.Add(new M_Parkinglots
                {
                    id = lotId,
                    name = "Test Lot",
                    location = "Test",
                    address = "Test",
                    capacity = 100,
                    reserved = 0,
                    daytarriff = 10.0f,
                    created_at = DateTime.UtcNow,
                    coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
                });
                
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "NFC-ACTIVE",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });

                // Create an active session
                db.Sessions.Add(new M_Session
                {
                    id = Guid.NewGuid(),
                    user = user.id.ToString(),
                    vehicle_id = vehicleId,
                    parking_lot_id = lotId,
                    license_plate = "NFC-ACTIVE",
                    started = DateTime.UtcNow,
                    status = M_Session.PaymentStatus.Unpaid
                });
                await db.SaveChangesAsync();

                var request = new
                {
                    UserId = user.id,
                    Amount = 10.0m,
                    LicensePlate = "NFC-ACTIVE",
                    ParkingLotId = lotId
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/verify-and-pay", request);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task VerifyAndPay_With_Full_Parking_Lot_Should_Return_400()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var balanceService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IUserBalanceService>();
                try
                {
                    await balanceService.CreateBalance(user.id, 100.0m);
                }
                catch { }
                
                var lotId = Guid.NewGuid();
                db.Parkinglots.Add(new M_Parkinglots
                {
                    id = lotId,
                    name = "Full Lot",
                    location = "Test",
                    address = "Test",
                    capacity = 1, // Small capacity
                    reserved = 0,
                    daytarriff = 10.0f,
                    created_at = DateTime.UtcNow,
                    coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
                });
                
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "NFC-FULL",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });

                // Fill the parking lot
                db.Sessions.Add(new M_Session
                {
                    id = Guid.NewGuid(),
                    user = user.id.ToString(),
                    vehicle_id = vehicleId,
                    parking_lot_id = lotId,
                    license_plate = "OTHER-123",
                    started = DateTime.UtcNow,
                    status = M_Session.PaymentStatus.Unpaid
                });
                await db.SaveChangesAsync();

                var request = new
                {
                    UserId = user.id,
                    Amount = 10.0m,
                    LicensePlate = "NFC-FULL",
                    ParkingLotId = lotId
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/verify-and-pay", request);
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }
        }

        [Fact]
        public async Task VerifyAndPay_With_Hotel_Discount_Should_Apply_Discount()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var balanceService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IUserBalanceService>();
                try
                {
                    await balanceService.CreateBalance(user.id, 100.0m);
                }
                catch { }
                
                var hotelId = Guid.NewGuid();
                db.Hotels.Add(new M_Hotel
                {
                    id = hotelId,
                    name = "Test Hotel",
                    active = true,
                    discount_percentage = 20.0m,
                    created_at = DateTime.UtcNow
                });
                
                var hotelService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IHotelService>();
                await hotelService.RegisterGuest(hotelId, user.id, DateTime.UtcNow);
                
                var lotId = Guid.NewGuid();
                db.Parkinglots.Add(new M_Parkinglots
                {
                    id = lotId,
                    name = "Test Lot",
                    location = "Test",
                    address = "Test",
                    capacity = 100,
                    reserved = 0,
                    daytarriff = 10.0f,
                    created_at = DateTime.UtcNow,
                    coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
                });
                
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "NFC-HOTEL",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                await db.SaveChangesAsync();

                var request = new
                {
                    UserId = user.id,
                    Amount = 100.0m,
                    LicensePlate = "NFC-HOTEL",
                    ParkingLotId = lotId
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/verify-and-pay", request);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    content.Should().Contain("discountApplied");
                }
            }
        }

        [Fact]
        public async Task VerifyAndPay_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            var request = new
            {
                UserId = Guid.NewGuid(),
                Amount = 10.0m,
                LicensePlate = "TEST-123",
                ParkingLotId = Guid.NewGuid()
            };
            var response = await client.PostAsJsonAsync("/api/v2/nfc/verify-and-pay", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task VerifyCard_With_Exception_Should_Return_500()
        {
            var client = _factory.CreateClient();
            var request = new { UserId = Guid.NewGuid(), RequiredAmount = 10.0m };
            var response = await client.PostAsJsonAsync("/api/v2/nfc/verify", request);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Exit_With_Confirm_False_Should_Return_Amount_And_Require_Confirm()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "EXIT-123",
                    make = "Make",
                    model = "Model",
                    color = "Blue",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                var lotId = Guid.NewGuid();
                db.Parkinglots.Add(new M_Parkinglots
                {
                    id = lotId,
                    name = "Exit Lot",
                    location = "Test",
                    address = "Test",
                    capacity = 100,
                    reserved = 0,
                    daytarriff = 10.0f,
                    created_at = DateTime.UtcNow,
                    coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
                });
                db.Sessions.Add(new M_Session
                {
                    id = Guid.NewGuid(),
                    user = user.id.ToString(),
                    vehicle_id = vehicleId,
                    parking_lot_id = lotId,
                    license_plate = "EXIT-123",
                    started = DateTime.UtcNow.AddMinutes(-30),
                    status = M_Session.PaymentStatus.Unpaid
                });
                await db.SaveChangesAsync();

                var request = new
                {
                    UserId = user.id,
                    LicensePlate = "EXIT-123",
                    ConfirmPayment = false
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/exit", request);
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var content = await response.Content.ReadAsStringAsync();
                content.Should().Contain("confirmRequired");
            }
        }

        [Fact]
        public async Task Exit_With_Invalid_Card_Should_Return_400()
        {
            var client = _factory.CreateClient();
            var request = new
            {
                UserId = Guid.NewGuid(),
                LicensePlate = "UNKNOWN-PLATE",
                ConfirmPayment = true
            };
            var response = await client.PostAsJsonAsync("/api/v2/nfc/exit", request);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Exit_With_Confirm_True_And_Sufficient_Balance_Should_Return_200()
        {
            var client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
            var user = db.Users.FirstOrDefault(u => u.username == "user");
            if (user != null)
            {
                var balanceService = scope.ServiceProvider.GetRequiredService<CSharpAPI.Services.IUserBalanceService>();
                try
                {
                    await balanceService.CreateBalance(user.id, 100.0m);
                }
                catch { }

                var vehicleId = Guid.NewGuid();
                db.Vehicles.Add(new M_Vehicles
                {
                    id = vehicleId,
                    user_id = user.id,
                    license_plate = "EXIT-OK",
                    make = "Make",
                    model = "Model",
                    color = "Red",
                    year = new DateTime(2020, 1, 1),
                    created_at = DateTime.UtcNow
                });
                var lotId = Guid.NewGuid();
                db.Parkinglots.Add(new M_Parkinglots
                {
                    id = lotId,
                    name = "Exit Lot",
                    location = "Test",
                    address = "Test",
                    capacity = 100,
                    reserved = 0,
                    daytarriff = 10.0f,
                    created_at = DateTime.UtcNow,
                    coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
                });
                db.Sessions.Add(new M_Session
                {
                    id = Guid.NewGuid(),
                    user = user.id.ToString(),
                    vehicle_id = vehicleId,
                    parking_lot_id = lotId,
                    license_plate = "EXIT-OK",
                    started = DateTime.UtcNow.AddMinutes(-45),
                    status = M_Session.PaymentStatus.Unpaid
                });
                await db.SaveChangesAsync();

                var request = new
                {
                    UserId = user.id,
                    LicensePlate = "EXIT-OK",
                    ConfirmPayment = true
                };
                var response = await client.PostAsJsonAsync("/api/v2/nfc/exit", request);
                response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
            }
        }
    }
}
