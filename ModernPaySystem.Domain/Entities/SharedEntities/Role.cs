using ModernPaySystem.Domain.DTOs.AuthDtos;
using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class Role : Entity<Guid>
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public RoleDto DTO => new()
    {
        //Id = Id,
        Name = Name,
        Description = Description,
        //Permissions = RolePermissions.Select(rp => rp.Permission.ToDto).ToList()
    };
}
