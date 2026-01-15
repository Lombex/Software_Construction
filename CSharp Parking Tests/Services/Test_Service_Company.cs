using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Company
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

        private IBillingService CreateBillingService(SQLite_Database db)
        {
            return new S_Billing(db);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Companies()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var company1 = new M_Company
            {
                id = Guid.NewGuid(),
                name = "Company 1",
                active = true,
                created_at = DateTime.UtcNow
            };
            var company2 = new M_Company
            {
                id = Guid.NewGuid(),
                name = "Company 2",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.AddRange(company1, company2);
            await db.SaveChangesAsync();

            var result = await service.GetAll();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_With_Valid_Id_Should_Return_Company()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var companyId = Guid.NewGuid();
            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);
            await db.SaveChangesAsync();

            var result = await service.GetById(companyId);
            result.Should().NotBeNull();
            result!.id.Should().Be(companyId);
        }

        [Fact]
        public async Task Create_With_Valid_Data_Should_Create_Company()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var company = new M_Company
            {
                name = "New Company",
                tax_id = "TAX123",
                email = "company@test.com"
            };

            var result = await service.Create(company);
            result.Should().NotBeNull();
            result.id.Should().NotBe(Guid.Empty);
            result.active.Should().BeTrue();
        }

        [Fact]
        public async Task Update_With_Valid_Data_Should_Update_Company()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var companyId = Guid.NewGuid();
            var company = new M_Company
            {
                id = companyId,
                name = "Old Name",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);
            await db.SaveChangesAsync();

            var updatedCompany = new M_Company
            {
                id = companyId,
                name = "Updated Name",
                tax_id = "TAX456",
                email = "updated@test.com"
            };

            var result = await service.Update(updatedCompany);
            result.Should().BeTrue();
            var updated = await db.Companies.FirstOrDefaultAsync(c => c.id == companyId);
            updated.Should().NotBeNull();
            updated!.name.Should().Be("Updated Name");
        }

        [Fact]
        public async Task Delete_With_Valid_Id_Should_Soft_Delete_Company()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var companyId = Guid.NewGuid();
            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);
            await db.SaveChangesAsync();

            var result = await service.Delete(companyId);
            result.Should().BeTrue();
            var deleted = await db.Companies.FirstOrDefaultAsync(c => c.id == companyId);
            deleted.Should().NotBeNull();
            deleted!.active.Should().BeFalse();
        }

        [Fact]
        public async Task GetCompanyVehicles_Should_Return_Vehicles_For_Company_Users()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);

            var user1 = new M_Users
            {
                id = userId1,
                username = "user1",
                password = "hash",
                name = "User 1",
                email = "user1@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            var user2 = new M_Users
            {
                id = userId2,
                username = "user2",
                password = "hash",
                name = "User 2",
                email = "user2@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.AddRange(user1, user2);

            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId1,
                role = CompanyUserRole.Employee,
                joined_at = DateTime.UtcNow
            });
            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId2,
                role = CompanyUserRole.Employee,
                joined_at = DateTime.UtcNow
            });

            var vehicle1 = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId1,
                license_plate = "COMP-001",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            var vehicle2 = new M_Vehicles
            {
                id = Guid.NewGuid(),
                user_id = userId2,
                license_plate = "COMP-002",
                make = "Make",
                model = "Model",
                color = "Blue",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            };
            db.Vehicles.AddRange(vehicle1, vehicle2);
            await db.SaveChangesAsync();

            var result = await service.GetCompanyVehicles(companyId);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddUserToCompany_With_Existing_User_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);

            var user = new M_Users
            {
                id = userId,
                username = "user",
                password = "hash",
                name = "User",
                email = "user@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);

            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId,
                role = CompanyUserRole.Employee,
                joined_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await service.AddUserToCompany(companyId, userId, CompanyUserRole.Employee);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveUserFromCompany_With_Valid_Data_Should_Return_True()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);

            var user = new M_Users
            {
                id = userId,
                username = "user",
                password = "hash",
                name = "User",
                email = "user@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);

            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId,
                role = CompanyUserRole.Employee,
                joined_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await service.RemoveUserFromCompany(companyId, userId);
            result.Should().BeTrue();
            var removed = await db.CompanyUsers.FirstOrDefaultAsync(cu => cu.company_id == companyId && cu.user_id == userId);
            removed.Should().BeNull();
        }

        [Fact]
        public async Task GetCompanyUsers_Should_Return_Users_In_Company()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);

            var user1 = new M_Users
            {
                id = userId1,
                username = "user1",
                password = "hash",
                name = "User 1",
                email = "user1@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            var user2 = new M_Users
            {
                id = userId2,
                username = "user2",
                password = "hash",
                name = "User 2",
                email = "user2@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.AddRange(user1, user2);

            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId1,
                role = CompanyUserRole.Employee,
                joined_at = DateTime.UtcNow
            });
            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId2,
                role = CompanyUserRole.Manager,
                joined_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var result = await service.GetCompanyUsers(companyId);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GenerateMonthlyBundle_With_Valid_Data_Should_Create_Bundle()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                monthly_billing_enabled = true,
                primary_contact_user_id = userId,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);

            var user = new M_Users
            {
                id = userId,
                username = "user",
                password = "hash",
                name = "User",
                email = "user@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);

            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId,
                role = CompanyUserRole.Employee,
                joined_at = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            var targetMonth = new DateTime(2024, 1, 15);
            var result = await service.GenerateMonthlyBundle(companyId, targetMonth);
            result.Should().HaveCount(1);
            result.First().type.Should().Be(BillingType.MonthlyBundle);
        }

        [Fact]
        public async Task GenerateMonthlyBundle_With_Billing_Disabled_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();

            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                monthly_billing_enabled = false,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);
            await db.SaveChangesAsync();

            var targetMonth = new DateTime(2024, 1, 15);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.GenerateMonthlyBundle(companyId, targetMonth));
        }

        [Fact]
        public async Task GenerateMonthlyBundle_With_Sessions_Should_Create_Bundle()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var company = new M_Company
            {
                id = companyId,
                name = "Test Company",
                active = true,
                monthly_billing_enabled = true,
                primary_contact_user_id = userId,
                created_at = DateTime.UtcNow
            };
            db.Companies.Add(company);

            var user = new M_Users
            {
                id = userId,
                username = "user",
                password = "hash",
                name = "User",
                email = "user@test.com",
                role = M_Users.UserRole.ParkingUser,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            };
            db.Users.Add(user);

            db.CompanyUsers.Add(new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId,
                role = CompanyUserRole.Employee,
                joined_at = DateTime.UtcNow
            });

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
                user_id = userId,
                license_plate = "COMP-123",
                make = "Make",
                model = "Model",
                color = "Red",
                year = new DateTime(2020, 1, 1),
                created_at = DateTime.UtcNow
            });

            var targetMonth = new DateTime(2024, 1, 15);
            var session = new M_Session
            {
                id = Guid.NewGuid(),
                user = userId.ToString(),
                vehicle_id = vehicleId,
                parking_lot_id = lotId,
                started = new DateTime(2024, 1, 10),
                cost = 50.0f,
                status = M_Session.PaymentStatus.Paid
            };
            db.Sessions.Add(session);
            await db.SaveChangesAsync();

            var result = await service.GenerateMonthlyBundle(companyId, targetMonth);
            result.Should().HaveCount(1);
            result.First().amount.Should().Be(50.0m);
        }

        [Fact]
        public async Task GetAll_Should_Exclude_Inactive_Companies()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            db.Companies.Add(new M_Company { id = Guid.NewGuid(), name = "Active", active = true, created_at = DateTime.UtcNow });
            db.Companies.Add(new M_Company { id = Guid.NewGuid(), name = "Inactive", active = false, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await service.GetAll();
            result.Should().HaveCount(1);
            result.First().name.Should().Be("Active");
        }

        [Fact]
        public async Task GetById_With_NonExistent_Id_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var result = await service.GetById(Guid.NewGuid());
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetById_With_Inactive_Company_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();

            db.Companies.Add(new M_Company { id = companyId, name = "Inactive", active = false, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await service.GetById(companyId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var result = await service.Update(new M_Company { id = Guid.NewGuid(), name = "Test" });
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var result = await service.Delete(Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetCompanyVehicles_With_No_Users_Should_Return_Empty_List()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();

            db.Companies.Add(new M_Company { id = companyId, name = "Test", active = true, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await service.GetCompanyVehicles(companyId);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task RemoveUserFromCompany_With_NonExistent_User_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var result = await service.RemoveUserFromCompany(Guid.NewGuid(), Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetCompanyUsers_With_No_Users_Should_Return_Empty_List()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);
            var companyId = Guid.NewGuid();

            db.Companies.Add(new M_Company { id = companyId, name = "Test", active = true, created_at = DateTime.UtcNow });
            await db.SaveChangesAsync();

            var result = await service.GetCompanyUsers(companyId);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateMonthlyBundle_With_NonExistent_Company_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.GenerateMonthlyBundle(Guid.NewGuid(), DateTime.UtcNow));
        }

        [Fact]
        public async Task Create_With_Empty_Guid_Should_Generate_New_Guid()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var company = new M_Company { id = Guid.Empty, name = "Test" };
            var result = await service.Create(company);
            result.id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Create_With_Default_CreatedAt_Should_Set_Current_Time()
        {
            var db = CreateInMemoryDatabase();
            var billingService = CreateBillingService(db);
            var service = new S_Company(db, billingService);

            var company = new M_Company { name = "Test", created_at = default };
            var before = DateTime.UtcNow;
            var result = await service.Create(company);
            var after = DateTime.UtcNow;

            result.created_at.Should().BeAfter(before.AddSeconds(-1));
            result.created_at.Should().BeBefore(after.AddSeconds(1));
        }
    }
}
