using Bogus;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Domain.Attrs;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeder for Permission entities
/// Order: 1 (must be seeded first as it has no dependencies).
/// </summary>
public class PermissionSeeder : EntitySeederBase<PermissionEntity>
{
    public override int Order => 1;

    public override async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        var permissions = GeneratePermissions(configuration.Quantities.PermissionCount);
        await AddEntitiesAsync(context, permissions);
    }

    /// <summary>
    /// Generate random permission data using Bogus.
    /// </summary>
    private List<PermissionEntity> GeneratePermissions(int count)
    {
        string[] permissionNames = new[]
        {
            "ViewTransactions", "CreateTransaction", "UpdateTransaction", "DeleteTransaction",
            "ViewUsers", "CreateUser", "UpdateUser", "DeleteUser",
            "ViewRoles", "CreateRole", "UpdateRole", "DeleteRole",
            "ViewPermissions", "AssignPermissions", "RevokePermissions",
            "ViewTemplates", "CreateTemplate", "UpdateTemplate", "DeleteTemplate",
            "ApproveRequest", "RejectRequest",
            "ViewAuditLogs", "ManageSystem"
        };

        var permissions = new List<PermissionEntity>();
        var usedNames = new HashSet<string>();

        for (int i = 0; i < count && i < permissionNames.Length; i++)
        {
            permissions.Add(new PermissionEntity
            {
                Id = Guid.NewGuid(),
                Key = permissionNames[i], // Set the required Key property
                Name = permissionNames[i],
                Description = $"Permission to {permissionNames[i].ToLower()}",
                Type = PermissionType.Read, // Set a default value
                SubSystem = SubSystem.None // Set a default value - using available enum value
            });
        }

        return permissions;
    }
}
