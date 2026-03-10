using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class User : Entity<Guid>, IAuditableEntity
{
    public required string UserName { get; set; }
    public required string HashedPassword { get; set; }

    public Guid? SubSystemUserId { get; set; }
    public SubSystemUser? SubSystemUser { get; set; }

    public ICollection<Request> RequestsAsRequester { get; set; } = new List<Request>();
    public ICollection<Request> RequestsAsApprover { get; set; } = new List<Request>();

    public ICollection<TemplateOwnership> TemplateOwnerships { get; set; } = new List<TemplateOwnership>();

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
            SubSystem = this.SubSystemUser?.SubSystem,  // Include subsystem from the relationship
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt
        };
    }
}

public class UserDto
{
    public Guid Id { get; set; }
    public required string UserName { get; set; }
    public Guid? SubSystemUserId { get; set; }
    public SubSystem? SubSystem { get; set; }  // Added subsystem information
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserDto
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public SubSystem? SubSystem { get; set; }
}
