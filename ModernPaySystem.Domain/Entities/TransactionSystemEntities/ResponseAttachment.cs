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
}