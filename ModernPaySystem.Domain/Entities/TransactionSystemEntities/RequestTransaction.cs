using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public enum TransactionStatus
{
    PendingAction = 0,
    Transferred = 1,
}

public class RequestTransaction : Entity<Guid>, IAuditableEntity
{
    public Guid RequestId { get; set; }
    public Request Request { get; set; } = null!;

    public TransactionStatus Status { get; set; } = TransactionStatus.PendingAction;

    public string Notes { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Path { get; set; } = string.Empty;

    public Guid? ParentTransactionId { get; set; }
    public RequestTransaction? ParentTransaction { get; set; }

    public Guid CurrentUserHolderId { get; set; }
    public User CurrentUserHolder { get; set; } = null!;

    public ICollection<RequestTransaction> ChildTransactions { get; set; } = [];
    public ICollection<RequestTransactionAttachment> RequestTransactionAttachments { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RequestTransactionDto ToDto()
    {
        return new RequestTransactionDto
        {
            Id = this.Id,
            RequestId = this.RequestId,
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
            RequestTransactionAttachments = this.RequestTransactionAttachments?.Select(a => a.ToDto()).ToList() ?? []
        };
    }
}

public class RequestTransactionDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
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

    public RequestTransactionDto? ParentTransaction { get; set; }
    public List<RequestTransactionDto> ChildTransactions { get; set; } = [];
    public List<RequestTransactionAttachmentDto> RequestTransactionAttachments { get; set; } = [];
    public int AttachmentCount => RequestTransactionAttachments.Count;
    public int ChildCount => ChildTransactions.Count;
}

public class CreateRequestTransactionDto
{
    public Guid RequestId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? ParentTransactionId { get; set; }
    public Guid CurrentUserHolderId { get; set; }
    public IFormFileCollection? Files { get; set; }
}

public class AddInitialRequestTransactionDto
{
    public Guid RequestId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid CurrentUserHolderId { get; set; }
    public IFormFileCollection? Files { get; set; }
}
