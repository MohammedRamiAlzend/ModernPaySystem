using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeder for default roles and super admin user
/// Order: 1 (runs early to establish foundational data).
/// </summary>
public class DefaultDataSeeder : IEntitySeeder
{
    public int Order => 1;

    public async Task<bool> HasDataAsync(AppDbContext context)
    {
        // Check if default roles exist
        var roleCount = await context.Roles.CountAsync();
        return roleCount >= 3; // At least SuperAdmin, Admin, NormalUser
    }

    public async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        // Add default roles if they don't exist
        await SeedDefaultRoles(context);

        // Add super admin user if it doesn't exist
        await SeedSuperAdminUser(context);
    }

    public string GetEntityName() => "DefaultData";

    /// <summary>
    /// Seeds default roles: SuperAdmin, Admin, NormalUser
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
    private async Task SeedSuperAdminUser(AppDbContext context)
    {
        var existingSuperAdmin = await context.Users
            .Where(u => u.UserName == "superadmin")
            .Include(u => u.Roles)
            .FirstOrDefaultAsync();

        if (existingSuperAdmin != null)
        {
            var hasSuperAdminRole = existingSuperAdmin.Roles
                .Any(ur => ur.Name == "SuperAdmin");

            if (!hasSuperAdminRole)
            {
                var superAdminRole2 = await context.Roles
                    .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

                if (superAdminRole2 != null)
                {
                    existingSuperAdmin.Roles.Add(superAdminRole2);
                    await context.SaveChangesAsync();
                }
            }

            return;
        }

        var superAdminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.Name == "SuperAdmin");

        if (superAdminRole == null)
        {
            return;
        }

        var superAdminUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "superadmin",
            HashedPassword = HashPassword("SuperAdmin123!")
        };

        await context.Users.AddAsync(superAdminUser);
        await context.SaveChangesAsync();

        superAdminUser.Roles.Add(superAdminRole);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Simple password hashing (use same as in AuthenticationService).
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}