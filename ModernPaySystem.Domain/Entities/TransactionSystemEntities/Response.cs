namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class Response : Entity<Guid>, IAuditableEntity
{
    public required Guid RequestId { get; set; }
    public Request? Request { get; set; }

    public required Guid RespondedByUserId { get; set; }

    public string? Comment { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
