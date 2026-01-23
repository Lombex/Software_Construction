using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Vehicles
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

        private async Task<(Guid userId, Guid vehicleId)> SetupUserAndVehicle(SQLite_Database db)
        {
            var userId = Guid.NewGuid();
            var user = new M_Users
            {
                id = userId,
                username = "testuser",
                password = "hash",
                name = "Test User",
                email = "test@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);

            var vehicleId = Guid.NewGuid();
            var vehicle = new M_Vehicles
            {
                id = vehicleId,
                user_id = userId,
                license_plate = "EXIST-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle);

            await db.SaveChangesAsync();
            return (userId, vehicleId);
        }

        [Fact]
        public async Task GetAllVehicles_Should_Return_All_Vehicles()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle2 = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "XYZ-789",
                make = "Make2",
                model = "Model2",
                color = "Blue",
                year = new DateTime(2021, 1, 1),
                created_at = DateTime.UtcNow
            };

            db.Vehicles.Add(vehicle2);
            await db.SaveChangesAsync();

            var result = await service.GetAllVehicles();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByID_With_Valid_Id_Should_Return_Vehicle()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);

            var userId = Guid.NewGuid();
            var user = new M_Users
            {
                id = userId,
                username = "testuser",
                password = "hash",
                name = "Test User",
                email = "test@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);

            var vehicleId = Guid.NewGuid();
            var vehicle = new M_Vehicles
            {
                id = vehicleId,
                user_id = userId,
                license_plate = "TEST-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();

            var result = await service.GetByID(vehicleId);
            result.Should().NotBeNull();
            result.id.Should().Be(vehicleId);
        }

        [Fact]
        public async Task CreateVehicle_With_Valid_Data_Should_Create_Vehicle()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "NEW-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };

            await service.CreateVehicle(vehicle);
            var result = await db.Vehicles.FirstOrDefaultAsync(v => v.id == vehicle.id);
            result.Should().NotBeNull();
            result!.license_plate.Should().Be("NEW-123");
        }

        [Fact]
        public async Task CreateVehicle_With_Duplicate_LicensePlate_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle1 = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "DUP-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle1);
            await db.SaveChangesAsync();

            var vehicle2 = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "dup-123", // Same plate, different case
                make = "Make",
                model = "Model",
                color = "Blue",
                year = new DateTime(2021, 1, 1),
                created_at = DateTime.UtcNow
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.CreateVehicle(vehicle2));
        }

        [Fact]
        public async Task UpdateVehicle_With_Valid_Data_Should_Update_Vehicle()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);

            var userId = Guid.NewGuid();
            var user = new M_Users
            {
                id = userId,
                username = "testuser",
                password = "hash",
                name = "Test User",
                email = "test@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);

            var vehicleId = Guid.NewGuid();
            var vehicle = new M_Vehicles
            {
                id = vehicleId,
                user_id = userId,
                license_plate = "OLD-123",
                make = "OldMake",
                model = "OldModel",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();

            var updatedVehicle = new M_Vehicles
            {
                license_plate = "NEW-456",
                make = "NewMake",
                model = "NewModel",
                color = "Blue",
                user_id = vehicle.user_id
            };

            var result = await service.UpdateVehicle(vehicleId, updatedVehicle);
            result.Should().NotBeNull();
            result!.license_plate.Should().Be("NEW-456");
            result.make.Should().Be("NewMake");
        }

        [Fact]
        public async Task DeleteVehicle_With_Valid_Id_Should_Delete_Vehicle()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);

            var vehicleId = Guid.NewGuid();
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle = new M_Vehicles
            {
                id = vehicleId,
                user_id = userId,
                license_plate = "DEL-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();

            await service.DeleteVehicle(vehicleId);
            var result = await db.Vehicles.FirstOrDefaultAsync(v => v.id == vehicleId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateVehicle_With_Duplicate_LicensePlate_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle2 = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "OTHER-456",
                make = "Make",
                model = "Model",
                color = "Blue",
                year = new DateTime(2021, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle2);
            await db.SaveChangesAsync();

            var updatedVehicle = new M_Vehicles
            {
                license_plate = "exist-123", // Same as vehicle1, different case
                make = "NewMake",
                model = "NewModel",
                color = "Green",
                user_id = userId
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.UpdateVehicle(vehicle2.id, updatedVehicle));
        }

        [Fact]
        public async Task GetByID_With_NonExistent_Id_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);

            var result = await service.GetByID(Guid.NewGuid());
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateVehicle_With_Null_LicensePlate_Should_Not_Validate_Uniqueness()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "OLD-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();

            // Update with null license plate - should skip uniqueness check
            var updatedVehicle = new M_Vehicles
            {
                license_plate = null,
                make = "NewMake",
                model = "NewModel",
                color = "Blue",
                user_id = userId
            };

            var result = await service.UpdateVehicle(vehicle.id, updatedVehicle);
            result.Should().NotBeNull();
            result!.license_plate.Should().BeNull();
        }

        [Fact]
        public async Task UpdateVehicle_With_Same_LicensePlate_Case_Insensitive_Should_Not_Validate()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "ABC-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();

            // Update with same license plate (different case) - should not validate
            var updatedVehicle = new M_Vehicles
            {
                license_plate = "abc-123", // Same but lowercase
                make = "NewMake",
                model = "NewModel",
                color = "Blue",
                user_id = userId
            };

            var result = await service.UpdateVehicle(vehicle.id, updatedVehicle);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateVehicle_With_Same_LicensePlate_Should_Not_Throw()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Vehicles(db);
            var (userId, _) = await SetupUserAndVehicle(db);

            var vehicle = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId,
                license_plate = "SAME-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.Add(vehicle);
            await db.SaveChangesAsync();

            var updatedVehicle = new M_Vehicles
            {
                license_plate = "same-123", // Same plate, different case
                make = "NewMake",
                model = "NewModel",
                color = "Blue",
                user_id = userId
            };

            // Should not throw because it's the same vehicle
            var result = await service.UpdateVehicle(vehicle.id, updatedVehicle);
            result.Should().NotBeNull();
        }
    }
}
