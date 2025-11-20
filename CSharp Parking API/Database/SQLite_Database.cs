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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbFolder = Path.Combine(Directory.GetCurrentDirectory(), "Database");
                Directory.CreateDirectory(dbFolder);
                var dbPath = Path.Combine(dbFolder, "parking.db");
                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<M_Payments>().HasKey(p => p.id);
            modelBuilder.Entity<M_Payments>().OwnsOne(t => t.t_data);
            modelBuilder.Entity<M_Parkinglots>().OwnsOne(c => c.coordinates);
            modelBuilder.Entity<M_Payments>().HasKey(p => p.hash);

            base.OnModelCreating(modelBuilder);
        }
    }
}
