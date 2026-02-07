using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class RequestAttachment
{
    public required Guid RequestId { get; set; }
    public Request? Request { get; set; }
    public required Guid AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }
}
