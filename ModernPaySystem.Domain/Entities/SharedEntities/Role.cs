namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class Role : Entity<Guid>
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = [];
    public ICollection<PermissionEntity> Permissions { get; set; } = [];

    public RoleDto ToDto()
    {
        return new RoleDto
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description
        };
    }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class CreateRoleDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}

public class UpdateRoleDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
