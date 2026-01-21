using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Hotel
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
        public async Task GetAll_Should_Return_All_Hotels()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var hotel1 = new M_Hotel
            {
                id = Guid.NewGuid(),
                name = "Hotel 1",
                active = true,
                created_at = DateTime.UtcNow
            };
            var hotel2 = new M_Hotel
            {
                id = Guid.NewGuid(),
                name = "Hotel 2",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Hotels.AddRange(hotel1, hotel2);
            await db.SaveChangesAsync();

            var result = await service.GetAll();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_With_Valid_Id_Should_Return_Hotel()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var hotelId = Guid.NewGuid();
            var hotel = new M_Hotel
            {
                id = hotelId,
                name = "Test Hotel",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Hotels.Add(hotel);
            await db.SaveChangesAsync();

            var result = await service.GetById(hotelId);
            result.Should().NotBeNull();
            result!.id.Should().Be(hotelId);
        }

        [Fact]
        public async Task Create_With_Valid_Data_Should_Create_Hotel()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var hotel = new M_Hotel
            {
                name = "New Hotel",
                address = "Hotel Address",
                email = "hotel@test.com",
                phone = "1234567890",
                discount_percentage = 10.0m
            };

            var result = await service.Create(hotel);
            result.Should().NotBeNull();
            result.id.Should().NotBe(Guid.Empty);
            result.active.Should().BeTrue();
        }

        [Fact]
        public async Task Update_With_Valid_Data_Should_Update_Hotel()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var hotelId = Guid.NewGuid();
            var hotel = new M_Hotel
            {
                id = hotelId,
                name = "Old Name",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Hotels.Add(hotel);
            await db.SaveChangesAsync();

            var updatedHotel = new M_Hotel
            {
                id = hotelId,
                name = "Updated Name",
                address = "Updated Address",
                discount_percentage = 15.0m
            };

            var result = await service.Update(updatedHotel);
            result.Should().BeTrue();
            var updated = await db.Hotels.FirstOrDefaultAsync(h => h.id == hotelId);
            updated.Should().NotBeNull();
            updated!.name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task Delete_With_Valid_Id_Should_Soft_Delete_Hotel()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var hotelId = Guid.NewGuid();
            var hotel = new M_Hotel
            {
                id = hotelId,
                name = "Test Hotel",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Hotels.Add(hotel);
            await db.SaveChangesAsync();

            var result = await service.Delete(hotelId);
            result.Should().BeTrue();
            var deleted = await db.Hotels.FirstOrDefaultAsync(h => h.id == hotelId);
            deleted.Should().NotBeNull();
            deleted!.active.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterGuest_With_Valid_Data_Should_Register_Guest()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel
            {
                id = hotelId,
                name = "Test Hotel",
                active = true,
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await service.RegisterGuest(hotelId, userId, DateTime.UtcNow);
            result.Should().NotBeNull();
            result.hotel_id.Should().Be(hotelId);
            result.user_id.Should().Be(userId);
        }

        [Fact]
        public async Task IsHotelGuest_With_Active_Guest_Should_Return_True()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel
            {
                id = hotelId,
                name = "Test Hotel",
                active = true,
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            await service.RegisterGuest(hotelId, userId, DateTime.UtcNow);
            var result = await service.IsHotelGuest(userId);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsHotelGuest_With_No_Guest_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var result = await service.IsHotelGuest(Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterGuest_With_Existing_Active_Guest_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel
            {
                id = hotelId,
                name = "Test Hotel",
                active = true,
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            await service.RegisterGuest(hotelId, userId, DateTime.UtcNow);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.RegisterGuest(hotelId, userId, DateTime.UtcNow));
        }

        [Fact]
        public async Task CheckOutGuest_With_NonExistent_Guest_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var result = await service.CheckOutGuest(Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetActiveGuests_Should_Return_Only_Active_Guests()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel
            {
                id = hotelId,
                name = "Test Hotel",
                active = true,
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var guest1 = await service.RegisterGuest(hotelId, userId1, DateTime.UtcNow);
            var guest2 = await service.RegisterGuest(hotelId, userId2, DateTime.UtcNow.AddDays(-5));
            await service.CheckOutGuest(guest2.id);

            var result = await service.GetActiveGuests(hotelId);
            result.Should().HaveCount(1);
            result.First().id.Should().Be(guest1.id);
        }

        [Fact]
        public async Task GetDiscountPercentage_With_Active_Guest_Should_Return_Discount()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel
            {
                id = hotelId,
                name = "Test Hotel",
                active = true,
                discount_percentage = 15.0m,
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            await service.RegisterGuest(hotelId, userId, DateTime.UtcNow);
            var result = await service.GetDiscountPercentage(userId);
            result.Should().Be(15.0m);
        }

        [Fact]
        public async Task GetDiscountPercentage_With_No_Guest_Should_Return_Zero()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var result = await service.GetDiscountPercentage(Guid.NewGuid());
            result.Should().Be(0);
        }

        [Fact]
        public async Task GetById_With_Inactive_Hotel_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel
            {
                id = hotelId,
                name = "Inactive Hotel",
                active = false,
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await service.GetById(hotelId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAll_Should_Exclude_Inactive_Hotels()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            db.Hotels.Add(new M_Hotel
            {
                id = Guid.NewGuid(),
                name = "Active Hotel",
                active = true,
                created_at = DateTime.UtcNow
            });
            db.Hotels.Add(new M_Hotel
            {
                id = Guid.NewGuid(),
                name = "Inactive Hotel",
                active = false,
                created_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await service.GetAll();
            result.Should().HaveCount(1);
            result.First().name.Should().Be("Active Hotel");
        }

        [Fact]
        public async Task GetById_With_NonExistent_Id_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var result = await service.GetById(Guid.NewGuid());
            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var result = await service.Update(new M_Hotel { id = Guid.NewGuid(), name = "Test" });
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var result = await service.Delete(Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Create_With_Empty_Guid_Should_Generate_New_Guid()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var hotel = new M_Hotel { id = Guid.Empty, name = "Test" };
            var result = await service.Create(hotel);
            result.id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Create_With_Default_CreatedAt_Should_Set_Current_Time()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);

            var hotel = new M_Hotel { name = "Test", created_at = default };
            var before = DateTime.UtcNow;
            var result = await service.Create(hotel);
            var after = DateTime.UtcNow;

            result.created_at.Should().BeAfter(before.AddSeconds(-1));
            result.created_at.Should().BeBefore(after.AddSeconds(1));
        }

        [Fact]
        public async Task CheckOutGuest_With_Valid_Guest_Should_Return_True()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel { id = hotelId, name = "Test", active = true, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var guest = await service.RegisterGuest(hotelId, userId, DateTime.UtcNow);
            var result = await service.CheckOutGuest(guest.id);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetActiveGuests_With_No_Guests_Should_Return_Empty_List()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel { id = hotelId, name = "Test", active = true, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await service.GetActiveGuests(hotelId);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDiscountPercentage_With_No_Hotel_Should_Return_Zero()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            db.Hotels.Add(new M_Hotel { id = hotelId, name = "Test", active = false, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var guest = await service.RegisterGuest(hotelId, userId, DateTime.UtcNow);
            var result = await service.GetDiscountPercentage(userId);
            result.Should().Be(0);
        }

        [Fact]
        public async Task RegisterGuest_With_CheckOut_Date_Should_Set_CheckOut()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Hotel(db);
            var hotelId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var checkOut = DateTime.UtcNow.AddDays(2);

            db.Hotels.Add(new M_Hotel { id = hotelId, name = "Test", active = true, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await service.RegisterGuest(hotelId, userId, DateTime.UtcNow, checkOut, "RES-123");
            result.check_out.Should().Be(checkOut);
            result.reservation_number.Should().Be("RES-123");
        }
    }
}
