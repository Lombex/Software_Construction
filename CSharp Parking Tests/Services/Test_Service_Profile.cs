using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Profile
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
        public async Task GetById_With_Valid_Id_Should_Return_User()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

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

            var result = await service.GetById(userId);
            result.Should().NotBeNull();
            result!.id.Should().Be(userId);
        }

        [Fact]
        public async Task GetById_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

            await Assert.ThrowsAsync<Exception>(async () => await service.GetById(Guid.NewGuid()));
        }

        [Fact]
        public async Task UpdateProfile_With_Valid_Id_Should_Update_User()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

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

            var result = await service.UpdateProfile(userId);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ChangePassword_With_Valid_Id_Should_Change_Password()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

            var userId = Guid.NewGuid();
            var user = new M_Users
            {
                id = userId,
                username = "testuser",
                password = "oldhash",
                name = "Test User",
                email = "test@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var result = await service.ChangePassword(userId, "newhash");
            result.Should().NotBeNull();
            result.password.Should().Be("newhash");
        }

        [Fact]
        public async Task DeleteProfile_With_Valid_Id_Should_Delete_User()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

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

            var result = await service.DeleteProfile(userId);
            result.Should().BeTrue();
            var deleted = await db.Users.FirstOrDefaultAsync(u => u.id == userId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task ChangePassword_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

            await Assert.ThrowsAsync<Exception>(async () => 
                await service.ChangePassword(Guid.NewGuid(), "newpass"));
        }

        [Fact]
        public async Task UpdateProfile_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

            await Assert.ThrowsAsync<Exception>(async () => 
                await service.UpdateProfile(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteProfile_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new Service_Profile(db);

            await Assert.ThrowsAsync<Exception>(async () => 
                await service.DeleteProfile(Guid.NewGuid()));
        }
    }
}
