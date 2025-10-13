using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseRouting();
app.UseCors("AllowAll");
// Midelware
// Authorization
// app.MapControllers();

app.Urls.Add("http://localhost:5001");

app.Run();