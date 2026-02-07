using Bogus;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeder for Role entities
/// Order: 2 (roles depend on being created, but permissions must exist first for relationships)
/// </summary>
public class RoleSeeder : EntitySeederBase<Role>
{
    public override int Order => 2;

    public override async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        var roles = GenerateRoles(configuration.Quantities.RoleCount);
        await AddEntitiesAsync(context, roles);
    }

    /// <summary>
    /// Generate role data
    /// </summary>
    private List<Role> GenerateRoles(int count)
    {
        var roleNames = new[]
        {
            "Admin",
            "Manager",
            "Approver",
            "User",
            "Viewer"
        };

        var roles = new List<Role>();

        for (int i = 0; i < count && i < roleNames.Length; i++)
        {
            roles.Add(new Role
            {
                Id = Guid.NewGuid(),
                Name = roleNames[i],
                Description = $"{roleNames[i]} role with appropriate permissions"
            });
        }

        return roles;
    }
}
