using CSharpAPI.Database;
using CSharpAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});

var contentRoot = builder.Environment.ContentRootPath;
var dbFolder = Path.Combine(contentRoot, "Database");
Directory.CreateDirectory(dbFolder);
var dbPath = Path.Combine(dbFolder, "parking.db");
Console.WriteLine($"SQLite DB path: {dbPath}");

builder.Services.AddDbContext<SQLite_Database>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IUsersService, S_Users>();
// Add other services here

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");
// Midelware
// Authorization
app.MapControllers();

app.Urls.Add("http://localhost:5001");

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SQLite_Database>();
    // authentication
    // logger 
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}

app.Run();