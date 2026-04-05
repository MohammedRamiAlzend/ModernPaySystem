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
    public required string ContentAsJson { get; set; }

    public ICollection<RequestAttachment> RequestAttachments { get; set; } = [];
    public required ICollection<User>? ReadOnlyUsers { get; set; } = [];
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public override bool CanEdit(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return false;

        if (userId == this.RequesterId.ToString()) return true;

        if (userId == this.ApproverId.ToString()) return false;

        if (ReadOnlyUsers?.Any(u => u.Id.ToString() == userId) == true) return false;

        return false;
    }

    public override bool CanView(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return false;

        if (userId == this.RequesterId.ToString()) return true;
        if (userId == this.ApproverId.ToString()) return true;
        if (ReadOnlyUsers?.Any(u => u.Id.ToString() == userId) == true) return true;

        return false;
    }

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
            ResponseId = this.ResponseId,
            ReadOnlyUsers = [.. this.ReadOnlyUsers.Select(u => u.Id)]
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
    public required ICollection<Guid> ReadOnlyUsers { get; set; } = [];
}

public class CreateRequestDto
{
    public required Guid TemplateId { get; set; }
    public required Guid ApproverId { get; set; }
    public required ICollection<Guid> ReadOnlyUsers { get; set; } = [];
    public required string Content { get; set; }
    public List<IFormFile>? Files { get; set; } = [];
}
