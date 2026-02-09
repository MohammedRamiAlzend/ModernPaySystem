namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class Request : Entity<Guid>, IAuditableEntity
{
    public required Guid TemplateId { get; set; }
    public Template? Template { get; set; }

    public required Guid RequesterId { get; set; }
    public User? Requester { get; set; }

    public required Guid ApproverId { get; set; }
    public User? Approver { get; set; }

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
            Content = this.ContentAsJson,
            Id = this.Id,
            TemplateId = this.TemplateId,
            RequesterId = this.RequesterId,
            ApproverId = this.ApproverId,
            RequestAttachmentDtos = [.. this.RequestAttachments.Select(ra => ra.ToDto())]
        };
    }
}

public class RequestDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ApproverId { get; set; }
    public required string Content { get; set; }
    public List<RequestAttachmentDto> RequestAttachmentDtos { get; set; } = [];
}

public class CreateRequestDto
{
    public Guid TemplateId { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ApproverId { get; set; }
    public required string Content { get; set; }
}