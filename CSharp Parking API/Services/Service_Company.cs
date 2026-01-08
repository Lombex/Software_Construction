using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CSharpAPI.Services
{
    // Interface for company service
    public interface ICompanyService
    {
        Task<List<M_Company>> GetAll();
        Task<M_Company?> GetById(Guid id);
        Task<M_Company> Create(M_Company company);
        Task<bool> Update(M_Company company);
        Task<bool> Delete(Guid id);
        Task<List<M_Vehicles>> GetCompanyVehicles(Guid companyId);
        Task<bool> AddUserToCompany(Guid companyId, Guid userId, CompanyUserRole role);
        Task<bool> RemoveUserFromCompany(Guid companyId, Guid userId);
        Task<List<M_Users>> GetCompanyUsers(Guid companyId);
        Task<List<M_Billing>> GenerateMonthlyBundle(Guid companyId, DateTime month);
    }

    // Service for managing companies
    public class S_Company : ICompanyService
    {
        private readonly SQLite_Database DbContext;
        private readonly IBillingService _billingService;

        public S_Company(SQLite_Database dbContext, IBillingService billingService)
        {
            DbContext = dbContext;
            _billingService = billingService;
        }

        public async Task<List<M_Company>> GetAll() =>
            await DbContext.Companies.AsNoTracking().Where(c => c.active).ToListAsync();

        public async Task<M_Company?> GetById(Guid id) =>
            await DbContext.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.id == id && c.active);

        public async Task<M_Company> Create(M_Company company)
        {
            company.id = company.id == Guid.Empty ? Guid.NewGuid() : company.id;
            company.created_at = company.created_at == default ? DateTime.UtcNow : company.created_at;
            company.active = true;

            await DbContext.Companies.AddAsync(company);
            await DbContext.SaveChangesAsync();
            return company;
        }

        public async Task<bool> Update(M_Company company)
        {
            var existing = await DbContext.Companies.FindAsync(company.id);
            if (existing == null) return false;

            existing.name = company.name;
            existing.tax_id = company.tax_id;
            existing.email = company.email;
            existing.phone = company.phone;
            existing.address = company.address;
            existing.primary_contact_user_id = company.primary_contact_user_id;
            existing.monthly_billing_enabled = company.monthly_billing_enabled;

            DbContext.Companies.Update(existing);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(Guid id)
        {
            var company = await DbContext.Companies.FindAsync(id);
            if (company == null) return false;

            company.active = false; // Soft delete
            DbContext.Companies.Update(company);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<M_Vehicles>> GetCompanyVehicles(Guid companyId)
        {
            // Get all users in the company
            var companyUserIds = await DbContext.CompanyUsers
                .Where(cu => cu.company_id == companyId)
                .Select(cu => cu.user_id)
                .ToListAsync();

            // Get all vehicles for those users
            return await DbContext.Vehicles
                .AsNoTracking()
                .Where(v => companyUserIds.Contains(v.user_id))
                .ToListAsync();
        }

        public async Task<bool> AddUserToCompany(Guid companyId, Guid userId, CompanyUserRole role)
        {
            var existing = await DbContext.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.company_id == companyId && cu.user_id == userId);
            if (existing != null) return false;

            var companyUser = new M_CompanyUser
            {
                id = Guid.NewGuid(),
                company_id = companyId,
                user_id = userId,
                role = role,
                joined_at = DateTime.UtcNow
            };

            await DbContext.CompanyUsers.AddAsync(companyUser);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveUserFromCompany(Guid companyId, Guid userId)
        {
            var companyUser = await DbContext.CompanyUsers
                .FirstOrDefaultAsync(cu => cu.company_id == companyId && cu.user_id == userId);
            if (companyUser == null) return false;

            DbContext.CompanyUsers.Remove(companyUser);
            await DbContext.SaveChangesAsync();
            return true;
        }

        public async Task<List<M_Users>> GetCompanyUsers(Guid companyId)
        {
            var userIds = await DbContext.CompanyUsers
                .Where(cu => cu.company_id == companyId)
                .Select(cu => cu.user_id)
                .ToListAsync();

            return await DbContext.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.id))
                .ToListAsync();
        }

        // Generate monthly bundle invoice for a company
        public async Task<List<M_Billing>> GenerateMonthlyBundle(Guid companyId, DateTime month)
        {
            var company = await GetById(companyId);
            if (company == null)
                throw new InvalidOperationException("Company not found.");

            if (!company.monthly_billing_enabled)
                throw new InvalidOperationException("Monthly billing is not enabled for this company.");

            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Get all company users
            var companyUsers = await GetCompanyUsers(companyId);

            // Get all sessions for company users in the month
            var userIds = companyUsers.Select(u => u.id).ToList();
            var sessions = await DbContext.Sessions
                .Where(s => userIds.Contains(Guid.Parse(s.user ?? "")) &&
                           s.started >= startOfMonth &&
                           s.started <= endOfMonth)
                .ToListAsync();

            // Calculate total cost
            var totalCost = sessions.Sum(s => (decimal)s.cost);

            // Create bundle billing entry
            var bundleBill = new M_Billing
            {
                id = Guid.NewGuid(),
                user_id = company.primary_contact_user_id ?? companyUsers.First().id,
                amount = totalCost,
                currency = "EUR",
                description = $"Monthly bundle invoice for {company.name} - {month:yyyy-MM}",
                due_date = endOfMonth.AddDays(30), // Due 30 days after month end
                paid = false,
                created_at = DateTime.UtcNow,
                type = BillingType.MonthlyBundle,
                status = BillingStatus.Pending
            };

            await _billingService.Create(bundleBill);

            return new List<M_Billing> { bundleBill };
        }
    }
}

