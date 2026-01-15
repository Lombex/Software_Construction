using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Parkinglots
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

        [Fact]
        public async Task GetAllParkinglots_Should_Return_All_Parkinglots()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            var lot1 = new M_Parkinglots
            {
                id = Guid.NewGuid(),
                name = "Lot 1",
                location = "Location 1",
                address = "Address 1",
                capacity = 100,
                reserved = 10,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };
            var lot2 = new M_Parkinglots
            {
                id = Guid.NewGuid(),
                name = "Lot 2",
                location = "Location 2",
                address = "Address 2",
                capacity = 200,
                reserved = 20,
                daytarriff = 15.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 53.0f, lng = 6.0f }
            };
            db.Parkinglots.AddRange(lot1, lot2);
            await db.SaveChangesAsync();

            var result = await service.GetAllParkinglots();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_With_Valid_Id_Should_Return_Parkinglot()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            var lotId = Guid.NewGuid();
            var lot = new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test Location",
                address = "Test Address",
                capacity = 100,
                reserved = 10,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };
            db.Parkinglots.Add(lot);
            await db.SaveChangesAsync();

            var result = await service.GetById(lotId);
            result.Should().NotBeNull();
            result.id.Should().Be(lotId);
        }

        [Fact]
        public async Task GetById_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            await Assert.ThrowsAsync<Exception>(async () => await service.GetById(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateParkinglot_With_Valid_Data_Should_Create_Parkinglot()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            var lot = new M_Parkinglots
            {
                id = Guid.NewGuid(),
                name = "New Lot",
                location = "New Location",
                address = "New Address",
                capacity = 100,
                reserved = 0,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };

            await service.CreateParkinglot(lot);
            var result = await db.Parkinglots.FirstOrDefaultAsync(p => p.id == lot.id);
            result.Should().NotBeNull();
            result!.name.Should().Be("New Lot");
        }

        [Fact]
        public async Task CreateParkinglot_With_Null_Model_Should_Throw_ArgumentNullException()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.CreateParkinglot(null!));
        }

        [Fact]
        public async Task UpdateParkinglot_With_Valid_Data_Should_Update_Parkinglot()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            var lotId = Guid.NewGuid();
            var lot = new M_Parkinglots
            {
                id = lotId,
                name = "Old Name",
                location = "Old Location",
                address = "Old Address",
                capacity = 100,
                reserved = 10,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };
            db.Parkinglots.Add(lot);
            await db.SaveChangesAsync();

            var updatedLot = new M_Parkinglots
            {
                name = "Updated Name",
                location = "Updated Location",
                address = "Updated Address",
                capacity = 200,
                reserved = 20,
                daytarriff = 15.0f,
                created_at = lot.created_at,
                coordinates = new Coordinates { lat = 53.0f, lng = 6.0f }
            };

            await service.UpdateParkinglot(lotId, updatedLot);
            var result = await db.Parkinglots.FirstOrDefaultAsync(p => p.id == lotId);
            result.Should().NotBeNull();
            result!.name.Should().Be("Updated Name");
            result.capacity.Should().Be(200);
        }

        [Fact]
        public async Task DeleteParkinglot_With_Valid_Id_Should_Delete_Parkinglot()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            var lotId = Guid.NewGuid();
            var lot = new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test Location",
                address = "Test Address",
                capacity = 100,
                reserved = 10,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };
            db.Parkinglots.Add(lot);
            await db.SaveChangesAsync();

            await service.DeleteParkinglot(lotId);
            var result = await db.Parkinglots.FirstOrDefaultAsync(p => p.id == lotId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetReservationsForLot_Should_Return_Reservations()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);
            var lotId = Guid.NewGuid();

            var reservation1 = new M_Reservations
            {
                id = Guid.NewGuid(),
                user_id = Guid.NewGuid(),
                vehicle_id = Guid.NewGuid(),
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 20.0f
            };
            var reservation2 = new M_Reservations
            {
                id = Guid.NewGuid(),
                user_id = Guid.NewGuid(),
                vehicle_id = Guid.NewGuid(),
                parking_lot_id = lotId,
                start_time = DateTime.UtcNow,
                end_time = DateTime.UtcNow.AddHours(2),
                status = M_Reservations.Status.Active,
                created_at = DateTime.UtcNow,
                cost = 25.0f
            };
            db.Reservations.AddRange(reservation1, reservation2);
            await db.SaveChangesAsync();

            var result = await service.GetReservationsForLot(lotId);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByID_With_Valid_Id_Should_Return_Parkinglot()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);
            var lotId = Guid.NewGuid();
            var lot = new M_Parkinglots
            {
                id = lotId,
                name = "Test Lot",
                location = "Test Location",
                address = "Test Address",
                capacity = 100,
                reserved = 10,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };
            db.Parkinglots.Add(lot);
            await db.SaveChangesAsync();

            var result = await service.GetByID(lotId);
            result.Should().NotBeNull();
            result.id.Should().Be(lotId);
        }

        [Fact]
        public async Task SearchNearbyParkinglots_With_Query_Should_Filter_Results()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            var lot1 = new M_Parkinglots
            {
                id = Guid.NewGuid(),
                name = "Downtown Lot",
                location = "Location 1",
                address = "Address 1",
                capacity = 100,
                reserved = 10,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };
            var lot2 = new M_Parkinglots
            {
                id = Guid.NewGuid(),
                name = "Airport Lot",
                location = "Location 2",
                address = "Address 2",
                capacity = 200,
                reserved = 20,
                daytarriff = 15.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 53.0f, lng = 6.0f }
            };
            db.Parkinglots.AddRange(lot1, lot2);
            await db.SaveChangesAsync();

            var result = await service.SearchNearbyParkinglots((52.0, 5.0), 1000, "Downtown");
            result.Should().HaveCount(1);
            result.First().name.Should().Be("Downtown Lot");
        }

        [Fact]
        public async Task SearchNearbyParkinglots_Without_Query_Should_Return_All()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Parkinglots(db);

            var lot1 = new M_Parkinglots
            {
                id = Guid.NewGuid(),
                name = "Lot 1",
                location = "Location 1",
                address = "Address 1",
                capacity = 100,
                reserved = 10,
                daytarriff = 10.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 52.0f, lng = 5.0f }
            };
            var lot2 = new M_Parkinglots
            {
                id = Guid.NewGuid(),
                name = "Lot 2",
                location = "Location 2",
                address = "Address 2",
                capacity = 200,
                reserved = 20,
                daytarriff = 15.0f,
                created_at = DateTime.UtcNow,
                coordinates = new Coordinates { lat = 53.0f, lng = 6.0f }
            };
            db.Parkinglots.AddRange(lot1, lot2);
            await db.SaveChangesAsync();

            var result = await service.SearchNearbyParkinglots((52.0, 5.0), 1000, null);
            result.Should().HaveCount(2);
        }
    }
}
