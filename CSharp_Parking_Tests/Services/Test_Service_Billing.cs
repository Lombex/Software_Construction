using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static CSharpAPI.Models.M_Billing;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Billing
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
        public async Task GetAll_Should_Return_All_Billing_Records()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill1 = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Bill 1",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            var bill2 = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 200.0m,
                currency = "EUR",
                description = "Bill 2",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.AddRange(bill1, bill2);
            await db.SaveChangesAsync();

            var result = await service.GetAll();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetById_With_Valid_Id_Should_Return_Billing()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test Bill",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var result = await service.GetById(billId);
            result.Should().NotBeNull();
            result!.id.Should().Be(billId);
        }

        [Fact]
        public async Task Create_With_Valid_Data_Should_Create_Billing()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test Bill"
            };

            var result = await service.Create(bill);
            result.Should().NotBeNull();
            result.id.Should().NotBe(Guid.Empty);
            result.invoice_number.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Create_With_Invalid_User_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);

            var bill = new M_Billing
            {
                user_id = Guid.NewGuid(),
                amount = 100.0m,
                currency = "EUR",
                description = "Test Bill"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Create(bill));
        }

        [Fact]
        public async Task MarkPaid_With_Valid_Id_Should_Mark_As_Paid()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test Bill",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var result = await service.MarkPaid(billId);
            result.Should().BeTrue();
            var updated = await db.Billing.FirstOrDefaultAsync(b => b.id == billId);
            updated.Should().NotBeNull();
            updated!.paid.Should().BeTrue();
            updated.status.Should().Be(BillingStatus.Paid);
        }

        [Fact]
        public async Task Cancel_With_Valid_Id_Should_Cancel_Billing()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test Bill",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var result = await service.Cancel(billId);
            result.Should().BeTrue();
            var updated = await db.Billing.FirstOrDefaultAsync(b => b.id == billId);
            updated.Should().NotBeNull();
            updated!.status.Should().Be(BillingStatus.Cancelled);
        }

        [Fact]
        public async Task Cancel_With_Paid_Bill_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test Bill",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = true,
                status = BillingStatus.Paid
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.Cancel(billId));
        }

        [Fact]
        public async Task GenerateInvoiceNumber_Should_Return_Unique_Number()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var number1 = await service.GenerateInvoiceNumber();

            // Create a billing record with the first invoice number so the next call returns different number
            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                invoice_number = number1
            };
            await service.Create(bill);

            var number2 = await service.GenerateInvoiceNumber();

            number1.Should().NotBeNullOrEmpty();
            number2.Should().NotBeNullOrEmpty();
            number1.Should().NotBe(number2);
            number1.Should().StartWith("INV-");
        }

        [Fact]
        public async Task GetForUser_Should_Return_User_Billing_Records()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

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
            await db.SaveChangesAsync();

            var bill1 = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId1,
                amount = 100.0m,
                currency = "EUR",
                description = "Bill 1",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            var bill2 = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId1,
                amount = 200.0m,
                currency = "EUR",
                description = "Bill 2",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            var bill3 = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId2,
                amount = 300.0m,
                currency = "EUR",
                description = "Bill 3",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.AddRange(bill1, bill2, bill3);
            await db.SaveChangesAsync();

            var result = await service.GetForUser(userId1);
            result.Should().HaveCount(2);
            result.All(b => b.user_id == userId1).Should().BeTrue();
        }

        [Fact]
        public async Task GetPendingForUser_Should_Return_Only_Pending_Bills()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var pendingBill = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Pending",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            var paidBill = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 200.0m,
                currency = "EUR",
                description = "Paid",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = true,
                status = BillingStatus.Paid
            };
            var cancelledBill = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 300.0m,
                currency = "EUR",
                description = "Cancelled",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Cancelled
            };
            db.Billing.AddRange(pendingBill, paidBill, cancelledBill);
            await db.SaveChangesAsync();

            var result = await service.GetPendingForUser(userId);
            result.Should().HaveCount(1);
            result.First().id.Should().Be(pendingBill.id);
        }

        [Fact]
        public async Task GetOverdueForUser_Should_Return_Only_Overdue_Bills()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var overdueBill = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Overdue",
                created_at = DateTime.UtcNow.AddDays(-60),
                due_date = DateTime.UtcNow.AddDays(-30),
                paid = false,
                status = BillingStatus.Overdue
            };
            var futureBill = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 200.0m,
                currency = "EUR",
                description = "Future",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.AddRange(overdueBill, futureBill);
            await db.SaveChangesAsync();

            var result = await service.GetOverdueForUser(userId);
            result.Should().HaveCount(1);
            result.First().id.Should().Be(overdueBill.id);
        }

        [Fact]
        public async Task Update_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);

            var bill = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = Guid.NewGuid(),
                amount = 100.0m,
                currency = "EUR",
                description = "Test"
            };

            var result = await service.Update(bill);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MarkPaid_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);

            var result = await service.MarkPaid(Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MarkOverdue_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);

            var result = await service.MarkOverdue(Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MarkOverdue_With_Paid_Bill_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(-30),
                paid = true,
                status = BillingStatus.Paid
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var result = await service.MarkOverdue(billId);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_With_NonExistent_Id_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);

            var result = await service.Delete(Guid.NewGuid());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMonthlyBundlesForUser_Should_Return_Bundles_For_Month()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var targetMonth = new DateTime(2024, 1, 15);
            var bundle1 = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Bundle 1",
                created_at = new DateTime(2024, 1, 10),
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                type = BillingType.MonthlyBundle,
                status = BillingStatus.Pending
            };
            var bundle2 = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = userId,
                amount = 200.0m,
                currency = "EUR",
                description = "Bundle 2",
                created_at = new DateTime(2024, 2, 10),
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                type = BillingType.MonthlyBundle,
                status = BillingStatus.Pending
            };
            db.Billing.AddRange(bundle1, bundle2);
            await db.SaveChangesAsync();

            var result = await service.GetMonthlyBundlesForUser(userId, targetMonth);
            result.Should().HaveCount(1);
            result.First().id.Should().Be(bundle1.id);
        }

        [Fact]
        public async Task MarkOverdue_With_Valid_Overdue_Bill_Should_Mark_As_Overdue()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Overdue Bill",
                created_at = DateTime.UtcNow.AddDays(-60),
                due_date = DateTime.UtcNow.AddDays(-30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var result = await service.MarkOverdue(billId);
            result.Should().BeTrue();
            var updated = await db.Billing.FirstOrDefaultAsync(b => b.id == billId);
            updated.Should().NotBeNull();
            updated!.status.Should().Be(BillingStatus.Overdue);
        }

        [Fact]
        public async Task MarkOverdue_With_Future_Due_Date_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Future Bill",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var result = await service.MarkOverdue(billId);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task Create_With_Invoice_Number_Should_Use_Provided_Number()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                invoice_number = "CUSTOM-123"
            };

            var result = await service.Create(bill);
            result.invoice_number.Should().Be("CUSTOM-123");
        }

        [Fact]
        public async Task Create_With_Due_Date_Should_Use_Provided_Date()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var dueDate = DateTime.UtcNow.AddDays(60);
            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                due_date = dueDate
            };

            var result = await service.Create(bill);
            result.due_date.Should().Be(dueDate);
        }

        [Fact]
        public async Task Create_With_Past_Due_Date_Should_Set_Overdue_Status()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                due_date = DateTime.UtcNow.AddDays(-1)
            };

            var result = await service.Create(bill);
            result.status.Should().Be(BillingStatus.Overdue);
        }

        [Fact]
        public async Task Create_With_Future_Due_Date_Should_Set_Due_Status()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                due_date = DateTime.UtcNow.AddDays(30)
            };

            var result = await service.Create(bill);
            result.status.Should().Be(BillingStatus.Due);
        }

        [Fact]
        public async Task Update_With_Valid_Data_Should_Update_Billing()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Original",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var updatedBill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 200.0m,
                currency = "USD",
                description = "Updated",
                due_date = DateTime.UtcNow.AddDays(60),
                status = BillingStatus.Due
            };

            var result = await service.Update(updatedBill);
            result.Should().BeTrue();
            var updated = await db.Billing.FirstOrDefaultAsync(b => b.id == billId);
            updated.Should().NotBeNull();
            updated!.amount.Should().Be(200.0m);
            updated.description.Should().Be("Updated");
        }

        [Fact]
        public async Task Update_With_Past_Due_Date_Should_Set_Overdue_Status()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var updatedBill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                due_date = DateTime.UtcNow.AddDays(-1),
                status = BillingStatus.Pending
            };

            var result = await service.Update(updatedBill);
            result.Should().BeTrue();
            var updated = await db.Billing.FirstOrDefaultAsync(b => b.id == billId);
            updated.Should().NotBeNull();
            updated!.status.Should().Be(BillingStatus.Overdue);
        }

        [Fact]
        public async Task Delete_With_Valid_Id_Should_Delete_Billing()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var billId = Guid.NewGuid();
            var bill = new M_Billing
            {
                id = billId,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                created_at = DateTime.UtcNow,
                due_date = DateTime.UtcNow.AddDays(30),
                paid = false,
                status = BillingStatus.Pending
            };
            db.Billing.Add(bill);
            await db.SaveChangesAsync();

            var result = await service.Delete(billId);
            result.Should().BeTrue();
            var deleted = await db.Billing.FirstOrDefaultAsync(b => b.id == billId);
            deleted.Should().BeNull();
        }

        [Fact]
        public async Task GetById_With_NonExistent_Id_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);

            var result = await service.GetById(Guid.NewGuid());
            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_With_Empty_Guid_Should_Generate_New_Guid()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill = new M_Billing
            {
                id = Guid.Empty,
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test"
            };

            var result = await service.Create(bill);
            result.id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task Create_With_Default_CreatedAt_Should_Set_Current_Time()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                created_at = default
            };

            var before = DateTime.UtcNow;
            var result = await service.Create(bill);
            var after = DateTime.UtcNow;

            result.created_at.Should().BeAfter(before.AddSeconds(-1));
            result.created_at.Should().BeBefore(after.AddSeconds(1));
        }

        [Fact]
        public async Task Create_With_Default_Status_Should_Set_Due()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Billing(db);
            var userId = Guid.NewGuid();

            var user = new M_Users
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
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var bill = new M_Billing
            {
                user_id = userId,
                amount = 100.0m,
                currency = "EUR",
                description = "Test",
                status = default
            };

            // Service sets status to Due when due_date is in the future (default is 30 days from now)
            var result = await service.Create(bill);
            result.status.Should().Be(BillingStatus.Due);
        }

        [Fact]
        public void Database_OnConfiguring_When_Not_Configured_Should_Set_Sqlite_Connection()
        {
            // Create a database context with no options configured to test OnConfiguring
            var options = new DbContextOptionsBuilder<SQLite_Database>().Options;
            var db = new SQLite_Database(options);

            // The OnConfiguring method should have been called and configured the database
            // We can't easily verify the exact connection string, but we can verify the context works
            db.Database.IsSqlite().Should().BeTrue();
        }
    }
}
