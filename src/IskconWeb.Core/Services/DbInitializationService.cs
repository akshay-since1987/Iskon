using IskconWeb.Core.Data;
using IskconWeb.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IskconWeb.Core.Services;

/// <summary>
/// Initializes databases on application startup
/// Handles migrations and seeding for both Master and Web databases
/// </summary>
public class DbInitializationService
{
    private readonly IskonMasterDbContext _masterContext;
    private readonly IskonWebDbContext _webContext;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<DbInitializationService> _logger;

    public DbInitializationService(
        IskonMasterDbContext masterContext,
        IskonWebDbContext webContext,
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<DbInitializationService> logger)
    {
        _masterContext = masterContext;
        _webContext = webContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");

            // Apply migrations and create databases if needed
            await ApplyMigrationsAsync();

            // Seed initial data
            await SeedDataAsync();

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database initialization");
            throw;
        }
    }

    private async Task ApplyMigrationsAsync()
    {
        _logger.LogInformation("Applying migrations to Master database...");
        await _masterContext.Database.MigrateAsync();

        _logger.LogInformation("Applying migrations to Web database...");
        await _webContext.Database.MigrateAsync();
    }

    private async Task SeedDataAsync()
    {
        _logger.LogInformation("Checking if seeding is needed...");

        // Check if data already exists
        if (await _masterContext.Temples.AnyAsync())
        {
            _logger.LogInformation("Database already seeded, skipping seed operation");
            return;
        }

        _logger.LogInformation("Starting data seeding...");

        // Create roles
        await SeedRolesAsync();

        // Create temples
        var temples = SeedTemples();

        // Create admin user
        await SeedAdminUserAsync(temples.First());

        _logger.LogInformation("Data seeding completed");
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Creating roles...");

        var roles = new[] { "Admin", "Manager", "User" };

        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid> { Name = roleName });
                _logger.LogInformation("Role created: {RoleName}", roleName);
            }
        }
    }

    private List<Temple> SeedTemples()
    {
        _logger.LogInformation("Creating temples...");

        var temples = new List<Temple>
        {
            new Temple
            {
                Id = new Guid("11111111-1111-1111-1111-111111111111"),
                Name = "Sri Sri Radha-Krishna Temple, Dhule",
                City = "Dhule",
                Address = "City Center, Dhule, Maharashtra",
                Phone = "+91-9999999999",
                Email = "dhule@iskcon.com"
            },
            new Temple
            {
                Id = new Guid("22222222-2222-2222-2222-222222222222"),
                Name = "Sri Sri Radha-Krishna Temple, Chalisgaon",
                City = "Chalisgaon",
                Address = "Main Road, Chalisgaon, Maharashtra",
                Phone = "+91-8888888888",
                Email = "chalisgaon@iskcon.com"
            },
            new Temple
            {
                Id = new Guid("33333333-3333-3333-3333-333333333333"),
                Name = "Sri Sri Radha-Krishna Temple, Shirpur",
                City = "Shirpur",
                Address = "Temple Nagar, Shirpur, Maharashtra",
                Phone = "+91-7777777777",
                Email = "shirpur@iskcon.com"
            },
            new Temple
            {
                Id = new Guid("44444444-4444-4444-4444-444444444444"),
                Name = "Sri Sri Radha-Krishna Temple, Nashik Road",
                City = "Nashik",
                Address = "Nashik Road, Nashik, Maharashtra",
                Phone = "+91-6666666666",
                Email = "nashik@iskcon.com"
            }
        };

        _masterContext.Temples.AddRange(temples);
        _masterContext.SaveChanges();

        _logger.LogInformation("Temples created: {Count}", temples.Count);
        return temples;
    }

    private async Task SeedAdminUserAsync(Temple temple)
    {
        _logger.LogInformation("Creating admin user...");

        var adminUser = new User
        {
            Id = new Guid("99999999-9999-9999-9999-999999999999"),
            UserName = "admin@iskcon.com",
            Email = "admin@iskcon.com",
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            TempleId = temple.Id,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(adminUser, "Admin@123456");

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
            _logger.LogInformation("Admin user created: {Email}", adminUser.Email);
        }
        else
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create admin user: {Errors}", errors);
        }
    }
}
