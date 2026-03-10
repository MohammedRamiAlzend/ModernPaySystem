using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class Request : Entity<Guid>, IAuditableEntity
{
    public required Guid TemplateId { get; set; }
    public Template? Template { get; set; }

    public required Guid RequesterId { get; set; }
    public User? Requester { get; set; }

    public required Guid ApproverId { get; set; }
    public User? Approver { get; set; }

    public Guid? ResponseId { get; set; }
    public Response? Response { get; set; }
    public bool Responsed { get; set; } = false;
    public required string ContentAsJson { get; set; }

    // Navigation property for attachments
    public ICollection<RequestAttachment> RequestAttachments { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public RequestDto ToDto()
    {
        return new RequestDto
        {
            Id = this.Id,
            TemplateId = this.TemplateId,
            RequesterId = this.RequesterId,
            ApproverId = this.ApproverId,
            Content = this.ContentAsJson,
            RequestAttachmentDtos = [.. this.RequestAttachments.Select(ra => ra.ToDto())],
            Template = this.Template?.ToDto(),
            Requester = this.Requester?.ToDto(),
            Approver = this.Approver?.ToDto(),
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt,
            ResponseId = this.ResponseId
        };
    }
}

public class RequestDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ApproverId { get; set; }
    public Guid? ResponseId { get; set; }
    public required string Content { get; set; }
    public List<RequestAttachmentDto> RequestAttachmentDtos { get; set; } = [];
    public TemplateDto? Template { get; set; }
    public UserDto? Requester { get; set; }
    public UserDto? Approver { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int AttachmentCount => RequestAttachmentDtos?.Count ?? 0;

}

public class CreateRequestDto
{
    public Guid TemplateId { get; set; }
    public Guid ApproverId { get; set; }
    public required string Content { get; set; }
    public List<IFormFile>? Files { get; set; } = [];
}
