using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;

public sealed class RoleSeeder : EntitySeederBase<Role>
{
    private static readonly string[] RoleNames = ["SuperAdmin", "Manager", "Employee", "Viewer"];

    public override int Order => 1;

    public override async Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var existingRoleNames = await context.Roles
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        var newRoles = RoleNames
            .Where(roleName => !existingRoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            .Select(roleName => new Role
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                Description = $"{roleName} role"
            })
            .ToList();

        if (newRoles.Count == 0)
        {
            return;
        }

        await context.Roles.AddRangeAsync(newRoles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}