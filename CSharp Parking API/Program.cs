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
using System.Net.Sockets;
using CSharpAPI.Controllers.Utils;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/parking-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog for logging
builder.Host.UseSerilog();

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
builder.Services.AddScoped<IParkinglotsService, S_Parkinglots>();
builder.Services.AddScoped<IProfileService, Service_Profile>();
builder.Services.AddScoped<IPaymentsService, S_Payments>();
builder.Services.AddScoped<ISessionsService, S_Sessions>();
builder.Services.AddScoped<IReservationsService, S_Reservations>();
builder.Services.AddScoped<IVehiclesService, S_Vehicles>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITokenRevocationService, TokenRevocationService>();
builder.Services.AddScoped<IBillingService, S_Billing>();
builder.Services.AddScoped<IUserBalanceService, S_UserBalance>();
builder.Services.AddScoped<ICompanyService, S_Company>();
builder.Services.AddScoped<IHotelService, S_Hotel>();
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

            // Add custom token validation to check if token is revoked
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var tokenRevocationService = context.HttpContext.RequestServices
                        .GetRequiredService<ITokenRevocationService>();
                    
                    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    if (!string.IsNullOrEmpty(token))
                    {
                        var isRevoked = await tokenRevocationService.IsTokenRevokedAsync(token);
                        if (isRevoked)
                        {
                            context.Fail("Token has been revoked (user logged out).");
                        }
                    }
                }
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
    c.SwaggerDoc("v2", new OpenApiInfo { Title = "CSharpAPI", Version = "v2" });
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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v2/swagger.json", "CSharpAPI v2"));
}

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Might cause issues on CI/CD pipelines or test environments
/*static bool IsPortInUse(int port)
{
    try
    {
        // Try to bind to the port - if it fails, the port is in use
        using var listener = new TcpListener(System.Net.IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return false; // Port is available (binding succeeded)
    }
    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
    {
        return true; // Port is in use
    }
    catch
    {
        // Other exceptions - assume port is available
        return false;
    }
}

// Check if port 5001 is available before binding (skip in Testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    int port = 8401;
    if (IsPortInUse(port))
    {
        Log.Error($"Port {port} is already in use. Please stop the other application using this port or change the port configuration.");
        Console.WriteLine($"\n[ERROR] Port {port} is already in use.");
        Console.WriteLine($"Please stop the other application using this port or change the port configuration.");
        Console.WriteLine($"You can check which process is using the port with: netstat -ano | findstr :{port}");
        Environment.Exit(1);
    }
    app.Urls.Add($"http://145.24.223.213:{port}");
}*/

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
