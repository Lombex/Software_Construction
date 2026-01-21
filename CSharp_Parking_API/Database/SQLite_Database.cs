using Microsoft.EntityFrameworkCore;
using CSharpAPI.Models;
using System.IO;

namespace CSharpAPI.Database
{
    public class SQLite_Database : DbContext
    {
        public SQLite_Database(DbContextOptions<SQLite_Database> options) : base(options) { }
        public DbSet<M_Users> Users { get; set; }
        public DbSet<M_Parkinglots> Parkinglots { get; set; }
        public DbSet<M_Vehicles> Vehicles { get; set; }
        public DbSet<M_Reservations> Reservations { get; set; }
        public DbSet<M_Payments> Payments { get; set; }
        public DbSet<M_Session> Sessions { get; set; }
        public DbSet<M_RevokedTokens> RevokedTokens { get; set; }
        public DbSet<M_Billing> Billing { get; set; }
        public DbSet<M_UserBalance> UserBalances { get; set; }
        public DbSet<M_BalanceTransaction> BalanceTransactions { get; set; }
        public DbSet<M_Company> Companies { get; set; }
        public DbSet<M_CompanyUser> CompanyUsers { get; set; }
        public DbSet<M_Hotel> Hotels { get; set; }
        public DbSet<M_HotelGuest> HotelGuests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string RootFolder = Directory.GetCurrentDirectory();
                string MainFolder = Path.GetFullPath(Path.Combine(RootFolder, "..", "..", ".."));
                string DatabasePath = Path.Combine(MainFolder, "Database", "Parking.db");
                optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<M_Payments>().HasKey(p => p.id);
            modelBuilder.Entity<M_Payments>().OwnsOne(t => t.t_data);
            modelBuilder.Entity<M_Parkinglots>().OwnsOne(c => c.coordinates);
            modelBuilder.Entity<M_Payments>().HasKey(p => p.hash);
            modelBuilder.Entity<M_RevokedTokens>().HasKey(r => r.TokenId);
            modelBuilder.Entity<M_Billing>().HasKey(b => b.id);
            modelBuilder.Entity<M_UserBalance>().HasKey(b => b.id);
            modelBuilder.Entity<M_BalanceTransaction>().HasKey(t => t.id);
            modelBuilder.Entity<M_Company>().HasKey(c => c.id);
            modelBuilder.Entity<M_CompanyUser>().HasKey(cu => cu.id);
            modelBuilder.Entity<M_Hotel>().HasKey(h => h.id);
            modelBuilder.Entity<M_HotelGuest>().HasKey(hg => hg.id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
