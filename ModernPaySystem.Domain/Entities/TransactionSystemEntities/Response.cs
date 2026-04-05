using Microsoft.AspNetCore.Http;
using System.Linq;
namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public enum ResponseStatus
{
    Delivered = 0,
    Pending = 1,

}

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

    public ResponseDto ToDto()
    {
        return new ResponseDto
        {
            Id = this.Id,
            RequestId = this.RequestId,
            RespondedByUserId = this.RespondedByUserId,
            Comment = this.Comment,
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt,
            Request = this.Request?.ToDto(),
            ResponseAttachments = this.ResponseAttachments?.Select(ra => ra.ToDto()).ToList() ?? new List<ResponseAttachmentDto>()

        };
    }

    // Navigation property for attachments
    public ICollection<ResponseAttachment> ResponseAttachments { get; set; } = [];
}

public class ResponseDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public Guid RespondedByUserId { get; set; }
    public string? Comment { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public RequestDto? Request { get; set; }
    public List<ResponseAttachmentDto> ResponseAttachments { get; set; } = [];
    public int AttachmentCount => ResponseAttachments?.Count ?? 0;
}

public class CreateResponseDto
{
    public Guid RequestId { get; set; }
    public Guid RespondedByUserId { get; set; }
    public string? Comment { get; set; }
    public IFormFileCollection? Files { get; set; }
}

public class UpdateResponseDto
{
    public Guid RequestId { get; set; }
    public Guid RespondedByUserId { get; set; }
    public string? Comment { get; set; }
}
