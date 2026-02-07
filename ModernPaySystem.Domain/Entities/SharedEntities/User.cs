using ModernPaySystem.Domain.Entities.Abstraction;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class User : Entity<Guid>, IAuditableEntity
{
    public required string UserName { get; set; }
    public required string HashedPassword { get; set; }

    public Guid? SubSystemUserId { get; set; }
    public SubSystemUser? SubSystemUser { get; set; }

    // Navigation properties for requests
    public ICollection<Request> RequestsAsRequester { get; set; } = new List<Request>();
    public ICollection<Request> RequestsAsApprover { get; set; } = new List<Request>();

    // Navigation properties for template ownership
    public ICollection<TemplateOwnership> TemplateOwnerships { get; set; } = new List<TemplateOwnership>();

    // Navigation properties for roles
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
