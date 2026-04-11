namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class ResponseTransaction : Entity<Guid>, IAuditableEntity
{
    public Guid ResponseId { get; set; }
    public Response Response { get; set; } = null!;

    public string Notes { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;

    public Guid? ParentTransactionId { get; set; }
    public ResponseTransaction? ParentTransaction { get; set; }

    public Guid CurrentUserHolderId { get; set; }
    public User CurrentUserHolder { get; set; } = null!;


    public ICollection<ResponseTransaction> ChildTransactions { get; set; } = [];
    public ICollection<ResponseTransactionAttachment> ResponseTransactionAttachments { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

}
