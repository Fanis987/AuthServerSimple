using AuthServerSimple.Application.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthServerSimple.Infrastructure.Identity;

/// <summary>
/// Provides methods to initialize the database with default roles and users.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// Seeds the database with default roles (Support, Dev, Admin) if they do not already exist.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = { "Support", "Dev", "Admin" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    /// <summary>
    /// Seeds the database with default users and assigns them to roles if they do not already exist.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;

        // Verify proper loading of the seed options
        if (string.IsNullOrWhiteSpace(seedOptions.SupportPassword))
            throw new ApplicationException("No password found for the seeded support user. Check your appsettings.json");
        if(string.IsNullOrWhiteSpace(seedOptions.DevPassword))
            throw new ApplicationException("No password found for the seeded dev user. Check your appsettings.json");
        if(string.IsNullOrWhiteSpace(seedOptions.AdminPassword))
            throw new ApplicationException("No password found for the seeded admin. Check your appsettings.json");
        
        // Seed one user for each of the three roles
        var usersToSeed = new[]
        {
            (Email: "support@example.com", Password: seedOptions.SupportPassword, Role: "Support"),
            (Email: "dev@example.com", Password: seedOptions.DevPassword, Role: "Dev"),
            (Email: "admin@example.com", Password: seedOptions.AdminPassword, Role: "Admin")
        };

        foreach (var (email, password, role) in usersToSeed)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
