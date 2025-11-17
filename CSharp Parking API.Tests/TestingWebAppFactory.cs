using System;
using System.Collections.Generic;
using System.Linq;
using CSharpAPI.Database;
using CSharpAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace CSharp_Parking_API.Tests
{
    public class TestingWebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                var settings = new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                    ["Jwt:Issuer"] = "TestIssuer",
                    ["Jwt:Audience"] = "TestAudience",
                    ["Jwt:Key"] = "test-secret-key-12345678901234567890"
                };
                config.AddInMemoryCollection(settings!);
            });
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<SQLite_Database>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<SQLite_Database>(options =>
                {
                    options.UseSqlite("DataSource=:memory:");
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
                db.Database.OpenConnection();
                db.Database.EnsureCreated();

                // Seed users: SuperAdmin, ParkingLotAdmin, and Regular User
                var parkingLotId = Guid.NewGuid();
                
                db.Users.Add(new M_Users
                {
                    id = Guid.NewGuid(),
                    username = "superadmin",
                    password = "superpass",
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
                    password = "lotpass",
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
                    password = "userpass",
                    name = "Regular User",
                    email = "user@example.com",
                    phone = "",
                    role = M_Users.UserRole.ParkingUser,
                    parking_lot_id = null,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1995, 1, 1),
                    active = true
                });
                
                db.SaveChanges();
            });
        }
    }
}


