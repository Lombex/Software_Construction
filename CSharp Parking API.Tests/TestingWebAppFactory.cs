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

                // Seed users: one admin and one regular user
                db.Users.Add(new M_Users
                {
                    id = Guid.NewGuid(),
                    username = "admin",
                    password = "adminpass",
                    name = "Admin",
                    email = "admin@example.com",
                    phone = "",
                    role = M_Users.UserRole.Admin,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                });
                db.Users.Add(new M_Users
                {
                    id = Guid.NewGuid(),
                    username = "user",
                    password = "userpass",
                    name = "User",
                    email = "user@example.com",
                    phone = "",
                    role = M_Users.UserRole.User,
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1995, 1, 1),
                    active = true
                });
                db.SaveChanges();
            });
        }
    }
}


