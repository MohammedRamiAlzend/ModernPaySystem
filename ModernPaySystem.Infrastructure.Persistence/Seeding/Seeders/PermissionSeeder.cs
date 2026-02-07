using Bogus;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeder for Permission entities
/// Order: 1 (must be seeded first as it has no dependencies)
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
    /// Generate random permission data using Bogus
    /// </summary>
    private List<PermissionEntity> GeneratePermissions(int count)
    {
        var permissionNames = new[]
        {
            // Transaction System Permissions
            "ViewTransactions", "CreateTransaction", "UpdateTransaction", "DeleteTransaction",
            // User Management
            "ViewUsers", "CreateUser", "UpdateUser", "DeleteUser",
            // Role Management
            "ViewRoles", "CreateRole", "UpdateRole", "DeleteRole",
            // Permission Management
            "ViewPermissions", "AssignPermissions", "RevokePermissions",
            // Template Management
            "ViewTemplates", "CreateTemplate", "UpdateTemplate", "DeleteTemplate",
            // Approval
            "ApproveRequest", "RejectRequest",
            // System
            "ViewAuditLogs", "ManageSystem"
        };

        var permissions = new List<PermissionEntity>();
        var usedNames = new HashSet<string>();

        for (int i = 0; i < count && i < permissionNames.Length; i++)
        {
            permissions.Add(new PermissionEntity
            {
                Id = Guid.NewGuid(),
                Name = permissionNames[i],
                Description = $"Permission to {permissionNames[i].ToLower()}"
            });
        }

        return permissions;
    }
}
