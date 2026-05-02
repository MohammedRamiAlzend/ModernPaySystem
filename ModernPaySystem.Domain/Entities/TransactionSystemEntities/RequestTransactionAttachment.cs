namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class RequestTransactionAttachment : Entity<Guid>, IAuditableEntity
{
    public required Guid RequestTransactionId { get; set; }
    public RequestTransaction? RequestTransaction { get; set; }
    public required Guid AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RequestTransactionAttachmentDto ToDto()
    {
        return new RequestTransactionAttachmentDto
        {
            Id = this.Id,
            RequestTransactionId = this.RequestTransactionId,
            AttachmentId = this.AttachmentId,
            AttachmentDto = this.Attachment?.ToDto()
        };
    }
}

public class RequestTransactionAttachmentDto
{
    public Guid Id { get; set; }
    public Guid RequestTransactionId { get; set; }
    public Guid AttachmentId { get; set; }
    public AttachmentDto? AttachmentDto { get; set; }
}
