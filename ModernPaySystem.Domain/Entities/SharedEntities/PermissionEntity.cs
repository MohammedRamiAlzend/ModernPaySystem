using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class PermissionEntity : Entity<Guid>
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
