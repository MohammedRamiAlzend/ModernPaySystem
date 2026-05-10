using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeder for default roles only
/// Order: 1 (runs early to establish foundational data).
/// </summary>
public class DefaultDataSeeder(IPasswordHasher passwordHasher) : IEntitySeeder
{
    public int Order => 1;

    public async Task<bool> HasDataAsync(AppDbContext context)
    {
        // Check if default roles exist
        int roleCount = await context.Roles.CountAsync();
        return roleCount >= 3; // At least SuperAdmin, Admin, NormalUser
    }

    public async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        // Add default roles if they don't exist
        await SeedDefaultRoles(context);

        // User seeding is now handled by UserSeeder
    }

    public string GetEntityName() => "DefaultData";

    /// <summary>
    /// Seeds default roles: SuperAdmin, Admin, NormalUser.
    /// </summary>
    private async Task SeedDefaultRoles(AppDbContext context)
    {
        var existingRoles = await context.Roles.Select(r => r.Name).ToListAsync();
        var defaultRoles = new[]
        {
            new { Name = "SuperAdmin", Description = "Super administrator with all permissions" },
            new { Name = "Admin", Description = "Administrator with most permissions" },
            new { Name = "NormalUser", Description = "Regular user with standard permissions" }
        };

        var rolesToAdd = new List<Role>();

        foreach (var roleData in defaultRoles)
        {
            if (!existingRoles.Contains(roleData.Name))
            {
                rolesToAdd.Add(new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleData.Name,
                    Description = roleData.Description
                });
            }
        }

        if (rolesToAdd.Any())
        {
            await context.Roles.AddRangeAsync(rolesToAdd);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Seeds super admin user with SuperAdmin role
    /// </summary>
    

}
