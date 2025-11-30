using CSharpAPI.Database;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CSharpAPI.Controllers.Utils;

var builder = WebApplication.CreateBuilder(args);

string projectRoot = AppContext.BaseDirectory;
string projectFolder = Path.GetFullPath(Path.Combine(projectRoot, "..", "..", ".."));
string DatabasePath = Path.Combine(projectFolder, "Database", "parking.db");

// Ensure the Database directory exists
var dbDirectory = Path.GetDirectoryName(DatabasePath);
if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory)) Directory.CreateDirectory(dbDirectory);

// Load configuration (appsettings.json)
builder.Configuration.AddJsonFile(Path.Combine(projectFolder, "appsettings.json"), optional: true, reloadOnChange: true);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddDbContext<SQLite_Database>(options => options.UseSqlite($"Data Source={DatabasePath}"));

builder.Services.AddScoped<IUsersService, S_Users>();
builder.Services.AddScoped<IVehiclesService, S_Vehicles>();
builder.Services.AddScoped<ITokenService, TokenService>();
// Add other services here

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Authentication & Authorization (must be added BEFORE building the app)
var jwtSection = builder.Configuration.GetSection("Jwt");
var issuer = jwtSection["Issuer"];
var audience = jwtSection["Audience"];
var key = jwtSection["Key"];
if (!string.IsNullOrWhiteSpace(key))
{
    var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = signingKey,
                ClockSkew = TimeSpan.FromMinutes(1)
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        // SuperAdmin can do everything
        options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
        // ParkingLotAdmin or SuperAdmin
        options.AddPolicy("AdminOrAbove", policy => policy.RequireRole("ParkingLotAdmin", "SuperAdmin"));
        // All authenticated users
        options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
    });
}

// Add Swagger with API Key support

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CSharpAPI", Version = "v1" });
    // Add JWT Authentication to Swagger
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSharpAPI v1"));
}

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Urls.Add("http://localhost:5001");

if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
        // authentication
        // logger 
        try
        {
            var isSqliteInMemory = dbContext.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true && (dbContext.Database.GetConnectionString()?.Contains(":memory:") == true);
            if (isSqliteInMemory) await dbContext.Database.EnsureCreatedAsync();
            else await dbContext.Database.MigrateAsync();
            
            // Seed minimal data for development/test with all three roles
            if (!dbContext.Users.Any())
            {
                var parkingLotId = Guid.NewGuid(); // Sample parking lot for admin

                // SuperAdmin - full system access
                dbContext.Users.Add(new CSharpAPI.Models.M_Users
                {
                    id = Guid.NewGuid(),
                    username = "superadmin",
                    password = C_Utils.HashPassword("superpass"),
                    name = "Super Administrator",
                    email = "super@example.com",
                    phone = "",
                    role = CSharpAPI.Models.M_Users.UserRole.SuperAdmin,
                    parking_lot_id = null, // SuperAdmin not tied to specific lot
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1985, 1, 1),
                    active = true
                });

                // ParkingLotAdmin - manages specific parking lot
                dbContext.Users.Add(new CSharpAPI.Models.M_Users
                {
                    id = Guid.NewGuid(),
                    username = "lotadmin",
                    password = C_Utils.HashPassword("lotpass"),
                    name = "Lot Administrator",
                    email = "lotadmin@example.com",
                    phone = "",
                    role = CSharpAPI.Models.M_Users.UserRole.ParkingLotAdmin,
                    parking_lot_id = parkingLotId, // Tied to specific parking lot
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1990, 1, 1),
                    active = true
                });

                // Regular ParkingUser
                dbContext.Users.Add(new CSharpAPI.Models.M_Users
                {
                    id = Guid.NewGuid(),
                    username = "user",
                    password = C_Utils.HashPassword("userpass"),
                    name = "Regular User",
                    email = "user@example.com",
                    phone = "",
                    role = CSharpAPI.Models.M_Users.UserRole.ParkingUser,
                    parking_lot_id = null, // Regular users not tied to lots
                    created_at = DateTime.UtcNow,
                    birth_year = new DateTime(1995, 1, 1),
                    active = true
                });

                await dbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
        }
    }
}

app.Run();

// Expose Program for WebApplicationFactory in tests
public partial class Program { }