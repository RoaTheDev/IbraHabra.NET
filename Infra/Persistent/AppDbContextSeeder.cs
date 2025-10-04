using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IbraHabra.NET.Infra.Persistent;

public static class AppDbContextSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        // --- Roles ---
        var roles = new[] { "Super", "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new Role { Name = role, NormalizedName = role.ToUpper() });
            }
        }

        // --- Super User ---
        await EnsureUserAsync(userManager, "superuser", "super@inbrahabra.io", "Super!123", "Super", "User", "Super");

        // --- Admin User ---
        await EnsureUserAsync(userManager, "adminuser", "admin@inbrahabra.io", "Admin!123", "Admin", "User", "Admin", "User");

        // --- Normal User ---
        await EnsureUserAsync(userManager, "normaluser", "user@inbrahabra.io", "User!123", "Normal", "User", "User");
    }

    private static async Task EnsureUserAsync(
        UserManager<User> userManager,
        string username,
        string email,
        string password,
        string firstName,
        string lastName,
        params string[] roles)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new User
            {
                UserName = username,
                NormalizedUserName = username.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                foreach (var role in roles)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
        else
        {
            foreach (var role in roles)
            {
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}