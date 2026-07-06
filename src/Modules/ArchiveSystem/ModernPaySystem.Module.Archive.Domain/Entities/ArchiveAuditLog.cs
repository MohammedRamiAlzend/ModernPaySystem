using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.Module.Archive.Domain.Entities;

public class ArchiveAuditLog : Entity<Guid>, IAuditableEntity
{
    public Guid ArchiveRecordId { get; set; }
    public ArchiveRecord ArchiveRecord { get; set; } = default!;

    public string UserId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum AuditAction
{
    View = 1,
    Update = 2,
    Download = 3,
    Print = 4,
    Create = 5,
    Delete = 6,
    AddFiles = 7,
    RemoveFiles = 8,
    ApproveEdit = 9,
    RejectEdit = 10,
    ApproveDelete = 11,
    RejectDelete = 12,
    SubmitEditRequest = 13,
    SubmitDeleteRequest = 14,
    Move = 15
}
