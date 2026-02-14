namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class PermissionEntity : Entity<Guid>
{
    public required string Key { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }

    public PermissionType Type { get; set; }
    public SubSystem SubSystem { get; set; }
    public ICollection<Role> Roles { get; set; } = [];
}