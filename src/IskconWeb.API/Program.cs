using IskconWeb.Core.Data;
using IskconWeb.Core.Models;
using IskconWeb.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("ApiConfig:CorsAllowedOrigins")
        .Get<string[]>() ?? new[] { "https://localhost:7002", "https://localhost:7003" };

    options.AddPolicy("AllowWebAndAdmin", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database contexts
var masterConnStr = builder.Configuration.GetConnectionString("MasterDb");
var webConnStr = builder.Configuration.GetConnectionString("WebDb");

builder.Services.AddDbContext<IskonMasterDbContext>(options =>
    options.UseSqlServer(masterConnStr));

builder.Services.AddDbContext<IskonWebDbContext>(options =>
    options.UseSqlServer(webConnStr));

// ASP.NET Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<IskonMasterDbContext>()
.AddDefaultTokenProviders();

// Services
builder.Services.AddScoped<DbInitializationService>();
builder.Services.AddScoped<ContentSyncService>();

builder.Services.AddLogging();

var app = builder.Build();

// Database initialization middleware
using (var scope = app.Services.CreateScope())
{
    var initService = scope.ServiceProvider.GetRequiredService<DbInitializationService>();
    try
    {
        await initService.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowWebAndAdmin");

app.UseAuthorization();
app.MapControllers();

app.Run();
