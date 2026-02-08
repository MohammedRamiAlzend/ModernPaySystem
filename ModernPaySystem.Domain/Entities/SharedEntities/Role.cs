using ModernPaySystem.Domain.DTOs.AuthDtos;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class Role : Entity<Guid>
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = [];
    public ICollection<PermissionEntity> Permissions { get; set; } = [];

    public RoleDto DTO => new()
    {
        //Id = Id,
        Name = Name,
        Description = Description,
        //Permissions = RolePermissions.Select(rp => rp.Permission.ToDto).ToList()
    };
}
