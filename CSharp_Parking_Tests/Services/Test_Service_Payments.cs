using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CSharpAPI.Tests.Services
{
    public class Test_Service_Payments
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
        public async Task GetAllPayments_Should_Return_All_Payments()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var payment1 = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };
            var payment2 = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 75.0f,
                transactions = "TRX2",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            db.Payments.AddRange(payment1, payment2);
            await db.SaveChangesAsync();

            var result = await service.GetAllPayments();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByID_With_Valid_Id_Should_Return_Payment()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var paymentId = Guid.NewGuid();
            var payment = new M_Payments
            {
                id = paymentId,
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            var result = await service.getByID(paymentId);
            result.Should().NotBeNull();
            result.id.Should().Be(paymentId);
        }

        [Fact]
        public async Task GetByID_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            await Assert.ThrowsAsync<Exception>(async () => await service.getByID(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreatePayment_With_Valid_Data_Should_Create_Payment()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var payment = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 50.0f,
                transactions = "TRX1",
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            await service.CreatePayment(payment);
            var result = await db.Payments.FirstOrDefaultAsync(p => p.id == payment.id);
            result.Should().NotBeNull();
            result!.amount.Should().Be(50.0f);
        }

        [Fact]
        public async Task CreatePayment_With_Null_Model_Should_Throw_ArgumentNullException()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.CreatePayment(null!));
        }

        [Fact]
        public async Task CreatePayment_Should_Set_CreatedAt_If_Not_Provided()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var payment = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 50.0f,
                transactions = "TRX1",
                created_at = default,
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            await service.CreatePayment(payment);
            payment.created_at.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task CreatePayment_Should_Generate_Hash_If_Not_Provided()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var payment = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 50.0f,
                transactions = "TRX1",
                hash = Guid.Empty,
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            await service.CreatePayment(payment);
            payment.hash.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public async Task UpdatePayment_With_Valid_Data_Should_Update_Payment()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var paymentId = Guid.NewGuid();
            var payment = new M_Payments
            {
                id = paymentId,
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            var updatedPayment = new M_Payments
            {
                id = paymentId,
                amount = 100.0f,
                transactions = "TRX1-UPDATED",
                created_at = payment.created_at,
                completed = DateTime.UtcNow,
                hash = payment.hash,
                session_id = payment.session_id,
                parking_lot_id = payment.parking_lot_id
            };

            await service.UpdatePayment(paymentId, updatedPayment);
            var result = await db.Payments.FirstOrDefaultAsync(p => p.id == paymentId);
            result.Should().NotBeNull();
            result!.amount.Should().Be(100.0f);
            result.transactions.Should().Be("TRX1-UPDATED");
        }

        [Fact]
        public async Task DeletePayment_With_Valid_Id_Should_Delete_Payment()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var paymentId = Guid.NewGuid();
            var payment = new M_Payments
            {
                id = paymentId,
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            await service.DeletePayment(paymentId);
            var result = await db.Payments.FirstOrDefaultAsync(p => p.id == paymentId);
            result.Should().BeNull();
        }

        [Fact]
        public async Task RefundPayment_With_Valid_Data_Should_Refund_Payment()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var paymentId = Guid.NewGuid();
            var payment = new M_Payments
            {
                id = paymentId,
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            var refund = await service.RefundPayment(paymentId, "Test refund", Guid.NewGuid());
            refund.Should().NotBeNull();
            refund.amount.Should().Be(-50.0f);
            refund.transactions.Should().Contain("REFUND");
        }

        [Fact]
        public async Task RefundPayment_With_Invalid_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            await Assert.ThrowsAsync<Exception>(async () => 
                await service.RefundPayment(Guid.NewGuid(), "Test", Guid.NewGuid()));
        }

        [Fact]
        public async Task RefundPayment_With_Already_Refunded_Payment_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var paymentId = Guid.NewGuid();
            var payment = new M_Payments
            {
                id = paymentId,
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };
            db.Payments.Add(payment);
            await db.SaveChangesAsync();

            await service.RefundPayment(paymentId, "First refund", Guid.NewGuid());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await service.RefundPayment(paymentId, "Second refund", Guid.NewGuid()));
        }

        [Fact]
        public async Task CreatePayment_Should_Set_Completed_If_Not_Provided()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var payment = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                completed = default,
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            await service.CreatePayment(payment);
            payment.completed.Should().BeCloseTo(payment.created_at, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task UpdatePayment_With_NonExistent_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var updatedPayment = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 100.0f,
                transactions = "TRX-UPDATE",
                created_at = DateTime.UtcNow,
                completed = DateTime.UtcNow,
                hash = Guid.NewGuid(),
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            await Assert.ThrowsAsync<Exception>(async () => 
                await service.UpdatePayment(Guid.NewGuid(), updatedPayment));
        }

        [Fact]
        public async Task DeletePayment_With_NonExistent_Id_Should_Throw_Exception()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            await Assert.ThrowsAsync<Exception>(async () => 
                await service.DeletePayment(Guid.NewGuid()));
        }

        [Fact]
        public async Task CreatePayment_With_Provided_Hash_That_Matches_Should_Accept()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var sessionId = Guid.NewGuid();
            var created_at = DateTime.UtcNow;
            var transactions = "TRX1";
            var amount = 50.0f;

            // Calculate expected hash manually (simulating what the service does)
            var hashString = $"{amount}|{sessionId}|{created_at:yyyyMMddHHmmss}|{transactions}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(hashString);
                var hash = sha256.ComputeHash(bytes);
                var guidBytes = new byte[16];
                Array.Copy(hash, guidBytes, 16);
                var expectedHash = new Guid(guidBytes);

                var payment = new M_Payments
                {
                    id = Guid.NewGuid(),
                    amount = amount,
                    transactions = transactions,
                    created_at = created_at,
                    hash = expectedHash, // Matching hash
                    session_id = sessionId,
                    parking_lot_id = Guid.NewGuid()
                };

                // Should accept matching hash
                await service.CreatePayment(payment);
                var result = await db.Payments.FirstOrDefaultAsync(p => p.id == payment.id);
                result.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task CreatePayment_With_Provided_Hash_That_Mismatches_Should_Still_Accept()
        {
            var db = CreateInMemoryDatabase();
            var service = new S_Payments(db);

            var payment = new M_Payments
            {
                id = Guid.NewGuid(),
                amount = 50.0f,
                transactions = "TRX1",
                created_at = DateTime.UtcNow,
                hash = Guid.NewGuid(), // Mismatched hash - but validation is lenient
                session_id = Guid.NewGuid(),
                parking_lot_id = Guid.NewGuid()
            };

            // Should still accept (validation is lenient for backward compatibility)
            await service.CreatePayment(payment);
            var result = await db.Payments.FirstOrDefaultAsync(p => p.id == payment.id);
            result.Should().NotBeNull();
        }
    }
}
