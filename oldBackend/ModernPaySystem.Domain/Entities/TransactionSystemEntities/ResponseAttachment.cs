namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class ResponseAttachment : Entity<Guid>, IAuditableEntity
{
    public required Guid ResponseId { get; set; }
    public Response? Response { get; set; }
    public required Guid AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ResponseAttachmentDto ToDto()
    {
        return new ResponseAttachmentDto
        {
            Id = this.Id,
            ResponseId = this.ResponseId,
            AttachmentId = this.AttachmentId,
            AttachmentDto = this.Attachment?.ToDto()
        };
    }
}

public class ResponseAttachmentDto
{
    public Guid Id { get; set; }
    public required Guid ResponseId { get; set; }
    public required Guid AttachmentId { get; set; }
    public AttachmentDto? AttachmentDto { get; set; }
}
