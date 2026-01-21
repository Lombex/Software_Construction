using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;
using static CSharpAPI.Models.M_Billing;

namespace CSharpAPI.Services
{
    // Interface for billing service
    public interface IBillingService
    {
        Task<List<M_Billing>> GetAll();
        Task<List<M_Billing>> GetForUser(Guid userId);
        Task<List<M_Billing>> GetPendingForUser(Guid userId);
        Task<List<M_Billing>> GetOverdueForUser(Guid userId);
        Task<M_Billing?> GetById(Guid id);
        Task<M_Billing> Create(M_Billing bill);
        Task<bool> Update(M_Billing bill);
        Task<bool> MarkPaid(Guid id, DateTime? paidAt = null);
        Task<bool> MarkOverdue(Guid id);
        Task<bool> Cancel(Guid id);
        Task<bool> Delete(Guid id);
        Task<string> GenerateInvoiceNumber();
        Task<List<M_Billing>> GetMonthlyBundlesForUser(Guid userId, DateTime month);
    }

    // Service for managing billing/invoice records
    public class S_Billing : IBillingService
    {
        private readonly SQLite_Database DbContext;

        public S_Billing(SQLite_Database dbContext)
        {
            DbContext = dbContext;
        }

        // Get all billing records (admin only)
        public async Task<List<M_Billing>> GetAll() => 
            await DbContext.Billing.AsNoTracking().OrderByDescending(b => b.created_at).ToListAsync();

        // Get all billing records for a specific user
        public async Task<List<M_Billing>> GetForUser(Guid userId) =>
            await DbContext.Billing.AsNoTracking()
                .Where(b => b.user_id == userId)
                .OrderByDescending(b => b.created_at)
                .ToListAsync();

        // Get pending (unpaid) billing records for a user
        public async Task<List<M_Billing>> GetPendingForUser(Guid userId) =>
            await DbContext.Billing.AsNoTracking()
                .Where(b => b.user_id == userId && !b.paid && b.status != BillingStatus.Cancelled)
                .OrderBy(b => b.due_date)
                .ToListAsync();

        // Get overdue billing records for a user
        public async Task<List<M_Billing>> GetOverdueForUser(Guid userId) =>
            await DbContext.Billing.AsNoTracking()
                .Where(b => b.user_id == userId && 
                           !b.paid && 
                           b.due_date < DateTime.UtcNow && 
                           b.status != BillingStatus.Cancelled)
                .OrderBy(b => b.due_date)
                .ToListAsync();

        // Get billing record by ID
        public async Task<M_Billing?> GetById(Guid id) => 
            await DbContext.Billing.AsNoTracking().FirstOrDefaultAsync(b => b.id == id);

        // Create a new billing record
        public async Task<M_Billing> Create(M_Billing bill)
        {
            // Validate user exists
            var userExists = await DbContext.Users.AnyAsync(u => u.id == bill.user_id && u.active);
            if (!userExists) 
                throw new InvalidOperationException("User not found for billing entry.");

            // Set defaults
            bill.id = bill.id == Guid.Empty ? Guid.NewGuid() : bill.id;
            bill.created_at = bill.created_at == default ? DateTime.UtcNow : bill.created_at;
            bill.status = bill.status == default ? BillingStatus.Pending : bill.status;
            
            // Generate invoice number if not provided
            if (string.IsNullOrWhiteSpace(bill.invoice_number))
            {
                bill.invoice_number = await GenerateInvoiceNumber();
            }

            // Set due date if not provided (default: 30 days from creation)
            if (bill.due_date == default)
            {
                bill.due_date = bill.created_at.AddDays(30);
            }

            // Update status based on due date
            if (bill.due_date < DateTime.UtcNow && !bill.paid)
            {
                bill.status = BillingStatus.Overdue;
            }
            else if (bill.due_date >= DateTime.UtcNow && !bill.paid)
            {
                bill.status = BillingStatus.Due;
            }

            await DbContext.Billing.AddAsync(bill);
            await DbContext.SaveChangesAsync();
            return bill;
        }

        // Update a billing record
        public async Task<bool> Update(M_Billing bill)
        {
            var existing = await DbContext.Billing.FindAsync(bill.id);
            if (existing == null) return false;

            // Update fields
            existing.amount = bill.amount;
            existing.currency = bill.currency;
            existing.description = bill.description;
            existing.due_date = bill.due_date;
            existing.status = bill.status;
            
            // Update status based on due date and paid status
            if (!existing.paid)
            {
                if (existing.due_date < DateTime.UtcNow)
                    existing.status = BillingStatus.Overdue;
                else
                    existing.status = BillingStatus.Due;
            }

            DbContext.Billing.Update(existing);
            await DbContext.SaveChangesAsync();
            return true;
        }

        // Mark a billing record as paid
        public async Task<bool> MarkPaid(Guid id, DateTime? paidAt = null)
        {
            var bill = await DbContext.Billing.FindAsync(id);
            if (bill == null) return false;

            bill.paid = true;
            bill.paid_at = paidAt ?? DateTime.UtcNow;
            bill.status = BillingStatus.Paid;

            DbContext.Billing.Update(bill);
            await DbContext.SaveChangesAsync();
            return true;
        }

        // Mark a billing record as overdue
        public async Task<bool> MarkOverdue(Guid id)
        {
            var bill = await DbContext.Billing.FindAsync(id);
            if (bill == null) return false;

            if (!bill.paid && bill.due_date < DateTime.UtcNow)
            {
                bill.status = BillingStatus.Overdue;
                DbContext.Billing.Update(bill);
                await DbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // Cancel a billing record
        public async Task<bool> Cancel(Guid id)
        {
            var bill = await DbContext.Billing.FindAsync(id);
            if (bill == null) return false;

            if (bill.paid)
                throw new InvalidOperationException("Cannot cancel a paid bill. Use refund instead.");

            bill.status = BillingStatus.Cancelled;
            DbContext.Billing.Update(bill);
            await DbContext.SaveChangesAsync();
            return true;
        }

        // Delete a billing record (admin only)
        public async Task<bool> Delete(Guid id)
        {
            var bill = await DbContext.Billing.FindAsync(id);
            if (bill == null) return false;

            DbContext.Billing.Remove(bill);
            await DbContext.SaveChangesAsync();
            return true;
        }

        // Generate unique invoice number (format: INV-YYYYMMDD-XXXXX)
        public async Task<string> GenerateInvoiceNumber()
        {
            var datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
            var existingCount = await DbContext.Billing
                .CountAsync(b => b.invoice_number != null && b.invoice_number.StartsWith($"INV-{datePrefix}-"));
            
            var sequence = (existingCount + 1).ToString("D5");
            return $"INV-{datePrefix}-{sequence}";
        }

        // Get monthly bundle invoices for a user (for companies)
        public async Task<List<M_Billing>> GetMonthlyBundlesForUser(Guid userId, DateTime month)
        {
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await DbContext.Billing.AsNoTracking()
                .Where(b => b.user_id == userId && 
                           b.type == BillingType.MonthlyBundle &&
                           b.created_at >= startOfMonth && 
                           b.created_at <= endOfMonth)
                .OrderByDescending(b => b.created_at)
                .ToListAsync();
        }
    }
}

