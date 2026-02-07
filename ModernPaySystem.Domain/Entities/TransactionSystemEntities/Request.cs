using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class Request : Entity<Guid>, IAuditableEntity
{
    public required Guid TemplateId { get; set; }
    public Template? Template { get; set; }

    public required Guid RequesterId { get; set; }
    public User? Requester { get; set; }

    public required Guid ApproverId { get; set; }
    public User? Approver { get; set; }

    // Navigation property for attachments
    public ICollection<RequestAttachment> RequestAttachments { get; set; } = new List<RequestAttachment>();

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
