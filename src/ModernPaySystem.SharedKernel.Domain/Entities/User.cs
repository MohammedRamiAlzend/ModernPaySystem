using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.SharedKernel.Domain.Entities;

public class User : Entity<Guid>, IAuditableEntity
{
    public required string UserName { get; set; }
    public required string HashedPassword { get; set; }

    public Guid? SubSystemUserId { get; set; }
    public SubSystemUser? SubSystemUser { get; set; }

    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public bool IsDepartmentHead { get; set; }
    public Guid? HeadedDepartmentId { get; set; }
    public Department? HeadedDepartment { get; set; }

    public ICollection<Role> Roles { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public UserDto ToDto()
    {
        return new UserDto
        {
            Id = this.Id,
            UserName = this.UserName,
            SubSystemUserId = this.SubSystemUserId,
            SubSystem = this.SubSystemUser?.SubSystem,
            DepartmentId = this.DepartmentId,
            DepartmentName = this.Department?.Name,
            IsDepartmentHead = this.IsDepartmentHead,
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt,
        };
    }
}

public class UserDto
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public Guid? SubSystemUserId { get; set; }
    public SubSystem? SubSystem { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsDepartmentHead { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserDto
{
    public required string UserName { get; set; }
    public string? Password { get; set; }
    public SubSystem? SubSystem { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsDepartmentHead { get; set; }
}
