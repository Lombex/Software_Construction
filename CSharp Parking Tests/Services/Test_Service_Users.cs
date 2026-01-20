using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Users
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
        public async Task GetAllUsers_Should_Return_All_Users()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Users(db);

            var user1 = new M_Users
            {
                id = Guid.NewGuid(),
                username = "user1",
                password = "hash1",
                name = "User 1",
                email = "user1@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            var user2 = new M_Users
            {
                id = Guid.NewGuid(),
                username = "user2",
                password = "hash2",
                name = "User 2",
                email = "user2@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };

            db.Users.AddRange(user1, user2);
            await db.SaveChangesAsync();

            var result = await service.GetAllUsers();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByID_With_Valid_Id_Should_Return_User()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Users(db);

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
            await db.SaveChangesAsync();

            var result = await service.getByID(userId);
            result.Should().NotBeNull();
            result.id.Should().Be(userId);
        }

        [Fact]
        public async Task CreateUser_With_Valid_Data_Should_Create_User()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Users(db);

            var user = new M_Users
            {
                id = Guid.NewGuid(),
                username = "newuser",
                password = "hash",
                name = "New User",
                email = "new@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };

            await service.CreateUser(user);
            var result = await db.Users.FirstOrDefaultAsync(u => u.id == user.id);
            result.Should().NotBeNull();
            result!.username.Should().Be("newuser");
        }

        [Fact]
        public async Task UpdateProfile_With_Valid_Data_Should_Update_User()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Users(db);

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
            await db.SaveChangesAsync();

            var updatedUser = new M_Users
            {
                username = "updateduser",
                password = "newhash",
                name = "Updated User",
                email = "updated@test.com",
                phone = "1234567890",
                role = M_Users.UserRole.ParkingUser,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };

            await service.UpdateProfile(userId, updatedUser);
            var result = await db.Users.FirstOrDefaultAsync(u => u.id == userId);
            result.Should().NotBeNull();
            result!.name.Should().Be("Updated User");
            result.email.Should().Be("updated@test.com");
        }

        [Fact]
        public async Task DeleteUser_With_Valid_Id_Should_Delete_User()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Users(db);

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
            await db.SaveChangesAsync();

            await service.DeleteUser(userId);
            var result = await db.Users.FirstOrDefaultAsync(u => u.id == userId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByID_With_Invalid_Id_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Users(db);

            var result = await service.getByID(Guid.NewGuid());
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateProfile_With_All_Fields_Should_Update_All_Properties()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Users(db);

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
            await db.SaveChangesAsync();

            var updatedUser = new M_Users
            {
                username = "updateduser",
                password = "newhash",
                name = "Updated User",
                email = "updated@test.com",
                phone = "1234567890",
                role = M_Users.UserRole.ParkingLotAdmin,
                birth_year = new DateTime(1985, 1, 1),
                active = false
            };

            await service.UpdateProfile(userId, updatedUser);
            var result = await db.Users.FirstOrDefaultAsync(u => u.id == userId);
            result.Should().NotBeNull();
            result!.username.Should().Be("updateduser");
            result.name.Should().Be("Updated User");
            result.email.Should().Be("updated@test.com");
            result.phone.Should().Be("1234567890");
            result.role.Should().Be(M_Users.UserRole.ParkingLotAdmin);
            result.active.Should().BeFalse();
        }
    }
}
