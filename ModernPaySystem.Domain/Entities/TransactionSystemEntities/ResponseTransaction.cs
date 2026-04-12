using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public enum TransactionStatus
{
    PendingAction = 0,
    Transferred = 1,
}

public class ResponseTransaction : Entity<Guid>, IAuditableEntity
{
    public Guid ResponseId { get; set; }
    public Response Response { get; set; } = null!;

    public TransactionStatus Status { get; set; } = TransactionStatus.PendingAction;

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

    public ResponseTransactionDto ToDto()
    {
        return new ResponseTransactionDto
        {
            Id = this.Id,
            ResponseId = this.ResponseId,
            Status = this.Status,
            Notes = this.Notes,
            Level = this.Level,
            Path = this.Path,
            ParentTransactionId = this.ParentTransactionId,
            CurrentUserHolderId = this.CurrentUserHolderId,
            CreatedAt = this.CreatedAt,
            CreatedByUserId = this.CreatedByUserId,
            UpdatedAt = this.UpdatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            ParentTransaction = this.ParentTransaction?.ToDto(),
            ChildTransactions = this.ChildTransactions?.Select(c => c.ToDto()).ToList() ?? [],
            ResponseTransactionAttachments = this.ResponseTransactionAttachments?.Select(a => a.ToDto()).ToList() ?? []
        };
    }
}

public class ResponseTransactionDto
{
    public Guid Id { get; set; }
    public Guid ResponseId { get; set; }
    public TransactionStatus Status { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;
    public Guid? ParentTransactionId { get; set; }
    public Guid CurrentUserHolderId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }

    public ResponseTransactionDto? ParentTransaction { get; set; }
    public List<ResponseTransactionDto> ChildTransactions { get; set; } = [];
    public List<ResponseTransactionAttachmentDto> ResponseTransactionAttachments { get; set; } = [];
    public int AttachmentCount => ResponseTransactionAttachments.Count;
    public int ChildCount => ChildTransactions.Count;
}

public class CreateResponseTransactionDto
{
    public Guid ResponseId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? ParentTransactionId { get; set; }
    public Guid CurrentUserHolderId { get; set; }
    public IFormFileCollection? Files { get; set; }
}

public class AddInitialResponseTransactionDto
{
    public Guid ResponseId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid CurrentUserHolderId { get; set; }
    public IFormFileCollection? Files { get; set; }
}