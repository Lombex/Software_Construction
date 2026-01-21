using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_UserBalance
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
        public async Task GetBalanceForUser_With_Valid_UserId_Should_Return_Balance()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            var balance = new M_UserBalance
            {
                id = Guid.NewGuid(),
                user_id = userId,
                balance = 100.0m,
                currency = "EUR",
                created_at = DateTime.UtcNow,
                last_updated = DateTime.UtcNow
            };
            db.UserBalances.Add(balance);
            await db.SaveChangesAsync();

            var result = await service.GetBalanceForUser(userId);
            result.Should().NotBeNull();
            result!.balance.Should().Be(100.0m);
        }

        [Fact]
        public async Task GetBalanceForUser_With_No_Balance_Should_Return_Null()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var result = await service.GetBalanceForUser(Guid.NewGuid());
            result.Should().BeNull();
        }

        [Fact]
        public async Task CreateBalance_With_Valid_UserId_Should_Create_Balance()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            var result = await service.CreateBalance(userId, 50.0m);
            result.Should().NotBeNull();
            result.balance.Should().Be(50.0m);
            result.user_id.Should().Be(userId);
        }

        [Fact]
        public async Task CreateBalance_With_Existing_Balance_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 50.0m);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.CreateBalance(userId, 100.0m));
        }

        [Fact]
        public async Task AddToBalance_With_Valid_Amount_Should_Increase_Balance()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 50.0m);
            var result = await service.AddToBalance(userId, 25.0m, "Test deposit");
            result.balance.Should().Be(75.0m);
        }

        [Fact]
        public async Task AddToBalance_With_Negative_Amount_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 50.0m);
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await service.AddToBalance(userId, -10.0m));
        }

        [Fact]
        public async Task DeductFromBalance_With_Sufficient_Balance_Should_Decrease_Balance()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            var result = await service.DeductFromBalance(userId, 30.0m, "Test deduction");
            result.balance.Should().Be(70.0m);
        }

        [Fact]
        public async Task DeductFromBalance_With_Insufficient_Balance_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 50.0m);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.DeductFromBalance(userId, 100.0m));
        }

        [Fact]
        public async Task HasSufficientBalance_With_Sufficient_Balance_Should_Return_True()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            var result = await service.HasSufficientBalance(userId, 50.0m);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task HasSufficientBalance_With_Insufficient_Balance_Should_Return_False()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 50.0m);
            var result = await service.HasSufficientBalance(userId, 100.0m);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetTransactionHistory_Should_Return_Transactions()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            await service.AddToBalance(userId, 50.0m, "Deposit");
            await service.DeductFromBalance(userId, 25.0m, "Withdrawal");

            var transactions = await service.GetTransactionHistory(userId);
            transactions.Should().HaveCountGreaterThan(0);
        }

        [Fact]
        public async Task RecordTransaction_Should_Create_Transaction_Record()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            var transaction = await service.RecordTransaction(userId, 50.0m, TransactionType.Credit, "Test transaction");
            transaction.Should().NotBeNull();
            transaction.amount.Should().Be(50.0m);
        }

        [Fact]
        public async Task RecordTransaction_With_No_Balance_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.RecordTransaction(userId, 50.0m, TransactionType.Credit, "Test"));
        }

        [Fact]
        public async Task GetBalanceAmount_With_No_Balance_Should_Return_Zero()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            var result = await service.GetBalanceAmount(userId);
            result.Should().Be(0);
        }

        [Fact]
        public async Task AddToBalance_With_No_Existing_Balance_Should_Create_Balance()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            var result = await service.AddToBalance(userId, 50.0m, "Initial deposit");
            result.Should().NotBeNull();
            result.balance.Should().Be(50.0m);
        }

        [Fact]
        public async Task DeductFromBalance_With_Zero_Amount_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await service.DeductFromBalance(userId, 0));
        }

        [Fact]
        public async Task AddToBalance_With_Zero_Amount_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            await Assert.ThrowsAsync<ArgumentException>(async () => 
                await service.AddToBalance(userId, 0));
        }

        [Fact]
        public async Task GetTransactionHistory_With_Limit_Should_Return_Limited_Results()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            await service.AddToBalance(userId, 10.0m, "Deposit 1");
            await service.AddToBalance(userId, 20.0m, "Deposit 2");
            await service.AddToBalance(userId, 30.0m, "Deposit 3");

            var result = await service.GetTransactionHistory(userId, 2);
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task RecordTransaction_With_PaymentId_Should_Create_Transaction_With_PaymentId()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            var transaction = await service.RecordTransaction(userId, 50.0m, TransactionType.Credit, "Payment transaction", paymentId);
            transaction.Should().NotBeNull();
            transaction.payment_id.Should().Be(paymentId);
        }

        [Fact]
        public async Task RecordTransaction_With_SessionId_Should_Create_Transaction_With_SessionId()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            var transaction = await service.RecordTransaction(userId, 50.0m, TransactionType.Debit, "Session transaction", null, sessionId);
            transaction.Should().NotBeNull();
            transaction.session_id.Should().Be(sessionId);
        }

        [Fact]
        public async Task RecordTransaction_With_Both_PaymentId_And_SessionId_Should_Create_Transaction_With_Both()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();
            var paymentId = Guid.NewGuid();
            var sessionId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            var transaction = await service.RecordTransaction(userId, 50.0m, TransactionType.Credit, "Both IDs", paymentId, sessionId);
            transaction.Should().NotBeNull();
            transaction.payment_id.Should().Be(paymentId);
            transaction.session_id.Should().Be(sessionId);
        }

        [Fact]
        public async Task GetTransactionHistory_With_No_Transactions_Should_Return_Empty_List()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 0); // Create with 0 amount so no initial transaction
            var result = await service.GetTransactionHistory(userId);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTransactionHistory_With_Null_Limit_Should_Return_All_Transactions()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_UserBalance(db);
            var userId = Guid.NewGuid();

            await service.CreateBalance(userId, 100.0m);
            await service.AddToBalance(userId, 10.0m, "Deposit 1");
            await service.AddToBalance(userId, 20.0m, "Deposit 2");
            await service.AddToBalance(userId, 30.0m, "Deposit 3");

            var result = await service.GetTransactionHistory(userId, null);
            result.Should().HaveCountGreaterThan(2);
        }
    }
}
