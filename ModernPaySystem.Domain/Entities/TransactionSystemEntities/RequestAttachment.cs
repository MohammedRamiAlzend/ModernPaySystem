using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class RequestAttachment : Entity<Guid>, IAuditableEntity
{
    public Request? Request { get; set; }
    public required Guid RequestId { get; set; }
    public required Guid AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public RequestAttachmentDto ToDto()
    {
        return new RequestAttachmentDto
        {
            RequestId = this.RequestId,
            AttachmentId = this.AttachmentId
        };
    }
}

public class RequestAttachmentDto
{
    public required Guid RequestId { get; set; }
    public required Guid AttachmentId { get; set; }
}
