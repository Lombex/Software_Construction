using Microsoft.EntityFrameworkCore;
using CSharpAPI.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Globalization;
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
                var rootFolder = Directory.GetCurrentDirectory();
                var databasePath = Path.Combine(rootFolder, "Database", "parking.db");
                optionsBuilder.UseSqlite($"Data Source={databasePath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var birthYearConverter = new ValueConverter<DateTime, string>(
                v => v.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                v => ParseDateValue(v));

            modelBuilder.Entity<M_Users>()
                .Property(u => u.birth_year)
                .HasConversion(birthYearConverter);

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

        private static DateTime ParseDateValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DateTime.MinValue;
            }

            var formats = new[]
            {
                "yyyy",
                "yyyy-MM",
                "yyyy-MM-dd",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss.fffffff",
                "yyyy-MM-ddTHH:mm:ss.fffffffZ"
            };

            if (DateTime.TryParseExact(
                    value,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out parsed))
            {
                return parsed;
            }

            return DateTime.MinValue;
        }
    }
}
