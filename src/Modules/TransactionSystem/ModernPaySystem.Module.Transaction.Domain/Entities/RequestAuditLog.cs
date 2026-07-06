using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.Module.Transaction.Domain.Entities;

public enum RequestAuditAction
{
    Created = 0,
    Responded = 1,
    Transferred = 2,
    AttachmentAdded = 3,
    AttachmentDownloaded = 4,
    Viewed = 5,
    StatusChanged = 6
}

public class RequestAuditLog : Entity<Guid>, IAuditableEntity
{
    public Guid RequestId { get; set; }
    public Request? Request { get; set; }

    public Guid UserId { get; set; }
    public RequestAuditAction Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
