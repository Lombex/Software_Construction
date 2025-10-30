using Microsoft.EntityFrameworkCore;
using CSharpAPI.Models;

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
            if (!optionsBuilder.IsConfigured) optionsBuilder.UseSqlite("Data Source=./Database/Data.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<M_Payments>().OwnsOne(t => t.t_data);
            modelBuilder.Entity<M_Parkinglots>().OwnsOne(c => c.coordinates);
            modelBuilder.Entity<M_Payments>().HasKey(p => p.hash);

            base.OnModelCreating(modelBuilder);
        }
    }
}
