using ModernPaySystem.Domain.Attrs;
using ModernPaySystem.Domain.Entities.SharedEntities;

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
            "ViewAuditLogs", "ManageSystem",
            "archiving.records.get-paged", "archiving.records.get-by-id", "archiving.records.get-by-folder", "archiving.records.get-by-form",
            "archiving.records.create", "archiving.records.update", "archiving.records.delete",
            "archiving.records.add-files", "archiving.records.remove-file", "archiving.records.get-files-metadata",
            "archiving.records.download-file", "archiving.records.download-zip", "archiving.records.get-files-paginated",
            "archiving.records.print", "archiving.records.get-audit-logs"
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
