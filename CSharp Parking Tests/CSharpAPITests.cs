using CSharpAPI.Database;
using CSharpAPI.Models;
using CSharpAPI.Services;
using CSharpAPI.Tests.Utillities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CSharpAPI.Tests
{
    public class CSharpAPITests : WebApplicationFactory<Program>
    {
        private SqliteConnection Connection = null!;
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<SQLite_Database>));
                if (descriptor != null) services.Remove(descriptor);

                Connection = new SqliteConnection("DataSource=:memory:");
                Connection.Open();

                services.AddDbContext<SQLite_Database>(options => { options.UseSqlite(Connection); });

                // Register all services needed for tests
                services.AddScoped<IUsersService, S_Users>();
                services.AddScoped<IVehiclesService, S_Vehicles>();
                services.AddScoped<IPaymentsService, S_Payments>();
                services.AddScoped<ITokenService, TokenService>();
                services.AddScoped<IReservationsService, S_Reservations>();
                services.AddScoped<IParkinglotsService, S_Parkinglots>();
                services.AddScoped<IProfileService, Service_Profile>();
                services.AddScoped<ISessionsService, S_Sessions>();
                services.AddScoped<ITokenRevocationService, TokenRevocationService>();
                services.AddScoped<IBillingService, S_Billing>();
                services.AddScoped<IUserBalanceService, S_UserBalance>();
                services.AddScoped<ICompanyService, S_Company>();
                services.AddScoped<IHotelService, S_Hotel>();

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
    
                db.Database.EnsureCreated();

                this.AddUsers(db);
                db.SaveChanges();
            });
        }

        private void AddUsers(SQLite_Database db)
        {
            var parkingLotId = Guid.NewGuid();

            db.Users.Add(new M_Users
            {
                id = Guid.NewGuid(),
                username = "superadmin",
                password = Utils.HashPassword("superpass"),
                name = "Super Administrator",
                email = "super@example.com",
                phone = "",
                role = M_Users.UserRole.SuperAdmin,
                parking_lot_id = null,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1985, 1, 1),
                active = true
            });

            db.Users.Add(new M_Users
            {
                id = Guid.NewGuid(),
                username = "lotadmin",
                password = Utils.HashPassword("lotpass"),
                name = "Lot Administrator",
                email = "lotadmin@example.com",
                phone = "",
                role = M_Users.UserRole.ParkingLotAdmin,
                parking_lot_id = parkingLotId,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1990, 1, 1),
                active = true
            });

            db.Users.Add(new M_Users
            {
                id = Guid.NewGuid(),
                username = "user",
                password = Utils.HashPassword("userpass"),
                name = "Regular User",
                email = "user@example.com",
                phone = "",
                role = M_Users.UserRole.ParkingUser,
                parking_lot_id = null,
                created_at = DateTime.UtcNow,
                birth_year = new DateTime(1995, 1, 1),
                active = true
            });
        }
    }
}