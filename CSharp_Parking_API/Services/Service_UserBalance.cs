using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    // Interface for user balance service
    public interface IUserBalanceService
    {
        Task<M_UserBalance?> GetBalanceForUser(Guid userId);
        Task<M_UserBalance> CreateBalance(Guid userId, decimal initialAmount = 0);
        Task<decimal> GetBalanceAmount(Guid userId);
        Task<bool> HasSufficientBalance(Guid userId, decimal amount);
        Task<M_UserBalance> AddToBalance(Guid userId, decimal amount, string? description = null);
        Task<M_UserBalance> DeductFromBalance(Guid userId, decimal amount, string? description = null);
        Task<List<M_BalanceTransaction>> GetTransactionHistory(Guid userId, int? limit = null);
        Task<M_BalanceTransaction> RecordTransaction(Guid userId, decimal amount, TransactionType type, string? description = null, Guid? paymentId = null, Guid? sessionId = null);
    }

    // Service for managing user balances and transactions
    public class S_UserBalance : IUserBalanceService
    {
        private readonly SQLite_Database DbContext;

        public S_UserBalance(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }

        // Get balance record for a user
        public async Task<M_UserBalance?> GetBalanceForUser(Guid userId)
        {
            return await DbContext.UserBalances
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.user_id == userId);
        }

        // Create a new balance record for a user
        public async Task<M_UserBalance> CreateBalance(Guid userId, decimal initialAmount = 0)
        {
            var existing = await GetBalanceForUser(userId);
            if (existing != null)
                throw new InvalidOperationException("User already has a balance record.");

            var balance = new M_UserBalance
            {
                id = Guid.NewGuid(),
                user_id = userId,
                balance = initialAmount,
                currency = "EUR",
                created_at = DateTime.UtcNow,
                last_updated = DateTime.UtcNow
            };

            await DbContext.UserBalances.AddAsync(balance);
            await DbContext.SaveChangesAsync();

            // Record initial transaction if amount > 0
            if (initialAmount > 0)
            {
                await RecordTransaction(userId, initialAmount, TransactionType.Deposit, "Initial balance");
            }

            return balance;
        }

        // Get current balance amount for a user
        public async Task<decimal> GetBalanceAmount(Guid userId)
        {
            var balance = await GetBalanceForUser(userId);
            return balance?.balance ?? 0;
        }

        // Check if user has sufficient balance
        public async Task<bool> HasSufficientBalance(Guid userId, decimal amount)
        {
            var currentBalance = await GetBalanceAmount(userId);
            return currentBalance >= amount;
        }

        // Add money to user's balance
        public async Task<M_UserBalance> AddToBalance(Guid userId, decimal amount, string? description = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            var balance = await DbContext.UserBalances.FirstOrDefaultAsync(b => b.user_id == userId);
            if (balance == null)
            {
                balance = await CreateBalance(userId, 0);
            }

            balance.balance += amount;
            balance.last_updated = DateTime.UtcNow;

            DbContext.UserBalances.Update(balance);
            await DbContext.SaveChangesAsync();

            // Record transaction
            await RecordTransaction(userId, amount, TransactionType.Credit, description ?? "Balance credit");

            return balance;
        }

        // Deduct money from user's balance
        public async Task<M_UserBalance> DeductFromBalance(Guid userId, decimal amount, string? description = null)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            var balance = await DbContext.UserBalances.FirstOrDefaultAsync(b => b.user_id == userId);
            if (balance == null)
                throw new InvalidOperationException("User does not have a balance record.");

            if (balance.balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            balance.balance -= amount;
            balance.last_updated = DateTime.UtcNow;

            DbContext.UserBalances.Update(balance);
            await DbContext.SaveChangesAsync();

            // Record transaction
            await RecordTransaction(userId, -amount, TransactionType.Debit, description ?? "Balance debit");

            return balance;
        }

        // Get transaction history for a user
        public async Task<List<M_BalanceTransaction>> GetTransactionHistory(Guid userId, int? limit = null)
        {
            var query = DbContext.BalanceTransactions
                .AsNoTracking()
                .Where(t => t.user_id == userId)
                .OrderByDescending(t => t.created_at);

            if (limit.HasValue)
                query = (IOrderedQueryable<M_BalanceTransaction>)query.Take(limit.Value);

            return await query.ToListAsync();
        }

        // Record a balance transaction
        public async Task<M_BalanceTransaction> RecordTransaction(Guid userId, decimal amount, TransactionType type, string? description = null, Guid? paymentId = null, Guid? sessionId = null)
        {
            var balance = await DbContext.UserBalances.FirstOrDefaultAsync(b => b.user_id == userId);
            if (balance == null)
                throw new InvalidOperationException("User does not have a balance record.");

            var transaction = new M_BalanceTransaction
            {
                id = Guid.NewGuid(),
                user_id = userId,
                balance_id = balance.id,
                amount = amount,
                currency = "EUR",
                type = type,
                description = description,
                payment_id = paymentId,
                session_id = sessionId,
                created_at = DateTime.UtcNow
            };

            await DbContext.BalanceTransactions.AddAsync(transaction);
            await DbContext.SaveChangesAsync();

            return transaction;
        }
    }
}

