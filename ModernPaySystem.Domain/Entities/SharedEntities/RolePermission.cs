namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class RolePermission
{
    public required Guid RoleId { get; set; }
    public Role? Role { get; set; }

    public required Guid PermissionId { get; set; }
    public PermissionEntity? Permission { get; set; }
}
