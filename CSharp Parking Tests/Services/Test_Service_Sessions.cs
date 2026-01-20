using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Sessions
    {
        private SQLite_Database CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<SQLite_Database>()
                .UseSqlite(connection)
                .Options;
            var db = new SQLite_Database(options);
            db.Database.EnsureCreated();
            return db;
        }

        private async Task<(Guid userId, Guid lotId, Guid vehicleId)> SetupTestData(SQLite_Database db)
        {
            var userId = Guid.NewGuid();
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            db.Users.Add(new M_Users
            {
                id = userId,
                username = "testuser",
                password = "hash",
                name = "Test",
                email = "test@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            });

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

            db.Vehicles.Add(new M_Vehicles
            {
                id = vehicleId,
                user_id = userId,
                license_plate = "TEST-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            return (userId, lotId, vehicleId);
        }

        [Fact]
        public async Task Start_With_Valid_Session_Should_Create_Session()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var userId = Guid.NewGuid();
            var lotId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();

            // Create user
            db.Users.Add(new M_Users
            {
                id = userId,
                username = "testuser",
                password = "hash",
                name = "Test",
                email = "test@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            });

            // Create parking lot
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

            // Create vehicle
            db.Vehicles.Add(new M_Vehicles
            {
                id = vehicleId,
                user_id = userId,
                license_plate = "TEST-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var session = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };

            var result = await service.Start(session);
            result.Should().NotBeNull();
            result.id.Should().Be(session.id);
        }

        [Fact]
        public async Task Start_With_Null_Session_Should_Throw_ArgumentNullException()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Start(null!));
        }

        [Fact]
        public async Task Start_With_Active_Session_Exists_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var existingSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(existingSession);
            await db.SaveChangesAsync();

            var newSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Start(newSession));
        }

        [Fact]
        public async Task Stop_With_Valid_Id_Should_Stop_Session()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var sessionId = Guid.NewGuid();
            var session = new M_Session
            {
                id = sessionId,
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow.AddHours(-2),
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            var result = await service.Stop(sessionId);
            result.Should().NotBeNull();
            result!.stopped.Should().NotBe(default);
            result.duration_minutes.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Stop_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);

            await Assert.ThrowsAsync<Exception>(async () => await service.Stop(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetSessionById_Should_Return_Sessions()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var session1 = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            var session2 = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-456",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.AddRange(session1, session2);
            await db.SaveChangesAsync();

            var result = await service.GetSessionById("testuser");
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Sessions()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var session1 = new M_Session
            {
                id = Guid.NewGuid(),
                user = "user1",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            var session2 = new M_Session
            {
                id = Guid.NewGuid(),
                user = "user2",
                license_plate = "TEST-456",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.AddRange(session1, session2);
            await db.SaveChangesAsync();

            var result = await service.GetAll();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Pay_With_Valid_Id_Should_Mark_Session_As_Paid()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var sessionId = Guid.NewGuid();
            var session = new M_Session
            {
                id = sessionId,
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow.AddHours(-2),
                stopped = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            var result = await service.Pay(sessionId);
            result.Should().NotBeNull();
            result!.status.Should().Be(M_Session.PaymentStatus.Paid);
        }

        [Fact]
        public async Task Pay_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);

            await Assert.ThrowsAsync<Exception>(async () => await service.Pay(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllSessions_With_Status_Filter_Should_Return_Filtered_Sessions()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var paidSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Paid
            };
            var unpaidSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-456",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.AddRange(paidSession, unpaidSession);
            await db.SaveChangesAsync();

            var result = await service.GetAllSessions("testuser", M_Session.PaymentStatus.Paid);
            result.Should().HaveCount(1);
            result.First().status.Should().Be(M_Session.PaymentStatus.Paid);
        }

        [Fact]
        public async Task Start_With_Full_Parking_Lot_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            // Create a parking lot with capacity 1
            var smallLot = await db.Parkinglots.FirstOrDefaultAsync(p => p.id == lotId);
            if (smallLot != null)
            {
                smallLot.capacity = 1;
                db.Parkinglots.Update(smallLot);
                await db.SaveChangesAsync();
            }

            // Create an active session to fill the lot
            var existingSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = "otheruser",
                license_plate = "OTHER-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(existingSession);
            await db.SaveChangesAsync();

            var newSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Start(newSession));
        }

        [Fact]
        public async Task Start_With_Invalid_ParkingLot_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, _, vehicleId) = await SetupTestData(db);

            var session = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = Guid.NewGuid(), // Non-existent parking lot
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Start(session));
        }

        [Fact]
        public async Task Start_With_Default_Started_Time_Should_Set_Current_Time()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var session = new M_Session
            {
                id = Guid.NewGuid(),
                user = "testuser",
                license_plate = "TEST-789",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = default, // Not set
                status = M_Session.PaymentStatus.Unpaid
            };

            var result = await service.Start(session);
            result.started.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Stop_With_Short_Duration_Should_Calculate_Cost()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            var sessionId = Guid.NewGuid();
            var started = DateTime.UtcNow.AddMinutes(-10);
            var session = new M_Session
            {
                id = sessionId,
                user = "testuser",
                license_plate = "TEST-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = started,
                status = M_Session.PaymentStatus.Unpaid
            };
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            var result = await service.Stop(sessionId);
            result.Should().NotBeNull();
            result!.duration_minutes.Should().BeGreaterThan(0);
            result.cost.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Start_With_Active_Reservation_Should_Count_Towards_Capacity()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Sessions(db);
            var (userId, lotId, vehicleId) = await SetupTestData(db);

            // Create a parking lot with capacity 1
            var smallLot = await db.Parkinglots.FirstOrDefaultAsync(p => p.id == lotId);
            if (smallLot != null)
            {
                smallLot.capacity = 1;
                db.Parkinglots.Update(smallLot);
                await db.SaveChangesAsync();
            }

            // Create an active reservation
            var reservation = new M_Reservations
            {
                id = Guid.NewGuid(),
                user_id = userId,
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow.AddHours(-1),
                end_time = DateTime.UtcNow.AddHours(1),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 20.0f
            };
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();

            // Try to start a session - should fail because lot is full (reservation occupies it)
            var newSession = new M_Session
            {
                id = Guid.NewGuid(),
                user = "otheruser",
                license_plate = "OTHER-123",
                parking_lot_id = lotId,
                vehicle_id = vehicleId,
                started = DateTime.UtcNow,
                status = M_Session.PaymentStatus.Unpaid
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Start(newSession));
        }
    }
}
