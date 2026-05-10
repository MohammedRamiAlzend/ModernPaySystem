using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class Department : Entity<Guid>, IAuditableEntity
{
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }

    public Guid? ParentDepartmentId { get; set; }
    public Department? ParentDepartment { get; set; }

    public Guid DepartmentHeadId { get; set; }
    public User DepartmentHead { get; set; } = null!;

    public int Level { get; set; }
    public string? MaterializedPath { get; set; }
    public DepartmentType Type { get; set; }

    public ICollection<Department> ChildDepartments { get; set; } = [];
    public ICollection<User> Users { get; set; } = [];
    public ICollection<TemplateDepartmentOwnership> TemplateOwnerships { get; set; } = new List<TemplateDepartmentOwnership>();

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DepartmentDto MapToDto()
    {
        return new DepartmentDto
        {
            Id = this.Id,
            Name = this.Name,
            Code = this.Code,
            Description = this.Description,
            ParentDepartmentId = this.ParentDepartmentId,
            Level = this.Level,
            MaterializedPath = this.MaterializedPath,
            Type = this.Type,
            ChildrenCount = 0,
            UsersCount = this.Users?.Count ?? 0,
            CreatedAt = this.CreatedAt
        };
    }
}

public enum DepartmentType
{
    Country = 1,
    Governorate = 2,
    District = 3,
    Municipality = 4,
    Office = 5,
    Unit = 6
}
