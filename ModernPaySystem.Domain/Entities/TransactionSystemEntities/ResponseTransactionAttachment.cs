namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class ResponseTransactionAttachment : Entity<Guid>, IAuditableEntity
{
    public required Guid ResponseTransactionId { get; set; }
    public ResponseTransaction? ResponseTransaction { get; set; }
    public required Guid AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ResponseTransactionAttachmentDto ToDto()
    {
        return new ResponseTransactionAttachmentDto
        {
            Id = this.Id,
            ResponseTransactionId = this.ResponseTransactionId,
            AttachmentId = this.AttachmentId,
            AttachmentDto = this.Attachment?.ToDto()
        };
    }
}

public class ResponseTransactionAttachmentDto
{
    public Guid Id { get; set; }
    public Guid ResponseTransactionId { get; set; }
    public Guid AttachmentId { get; set; }
    public AttachmentDto? AttachmentDto { get; set; }
}