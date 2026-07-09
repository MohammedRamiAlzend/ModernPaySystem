using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;

public sealed class RoleSeeder : EntitySeederBase<Role>
{
    private static readonly (string Name, string Description)[] SeedRoles =
    [
        ("SuperAdmin", "Super administrator with all permissions"),
        ("Admin", "Administrator with most permissions"),
        ("NormalUser", "Regular user with standard permissions")
    ];

    public override int Order => 1;

    public override async Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var existingRoleNames = await context.Roles
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        var newRoles = SeedRoles
            .Where(seed => !existingRoleNames.Contains(seed.Name, StringComparer.OrdinalIgnoreCase))
            .Select(seed => new Role
            {
                Id = Guid.NewGuid(),
                Name = seed.Name,
                Description = seed.Description
            })
            .ToList();

        if (newRoles.Count == 0)
            return;

        await context.Roles.AddRangeAsync(newRoles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
