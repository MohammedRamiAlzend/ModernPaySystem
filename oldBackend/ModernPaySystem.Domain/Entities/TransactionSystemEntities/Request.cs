using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public enum RequestStatus
{
    Pending = 0,
    Delivered = 1,
    InProcess = 2,
    Managed = 3,
}


public class Request : Entity<Guid>, IAuditableEntity
{
    public Guid RequestTemplateValuesId { get; set; }
    public RequestTemplateValues? RequestTemplateValues { get; set; }


    public required Guid RequesterId { get; set; }
    public User? Requester { get; set; }
    public required Guid RequesterDepartmentId { get; set; }
    public required Guid ApproverId { get; set; }
    public User? Approver { get; set; }
    public required Guid ApproverDepartmentId { get; set; }

    public Guid? ResponseId { get; set; }
    public Response? Response { get; set; }

    public int RequestNumber { get; set; }
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public Guid? FirstTransactionId { get; set; }
    public RequestTransaction? FirstTransaction { get; set; }

    public Guid? CurrentTransactionId { get; set; }
    public RequestTransaction? CurrentTransaction { get; set; }

    public ICollection<RequestAttachment> RequestAttachments { get; set; } = [];
    public required ICollection<User>? ReadOnlyUsers { get; set; } = [];
    public ICollection<RequestRelation> OutgoingRelations { get; set; } = [];
    public ICollection<RequestRelation> IncomingRelations { get; set; } = [];
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool CanEdit(Guid userId)
    {

        if (userId == this.RequesterId) return true;

        if (userId == this.ApproverId) return false;

        if (ReadOnlyUsers?.Any(u => u.Id == userId) == true) return false;

        return false;
    }

    public bool CanView(Guid userId)
    {

        if (userId == this.RequesterId) return true;
        if (userId == this.ApproverId) return true;
        if (ReadOnlyUsers?.Any(u => u.Id == userId) == true) return true;

        return false;
    }

    public RequestDto ToDto()
    {
        return new RequestDto
        {
            Id = this.Id,
            TemplateId = this.RequestTemplateValues?.TemplateId ?? throw new Exception("RequestTemplateValues is null"),
            RequestNumber = this.RequestNumber,
            RequesterId = this.RequesterId,
            ApproverId = this.ApproverId,
            Content = this.RequestTemplateValues?.InputValues.Select(iv => new InputValueDto
            {
                Key = iv.Key,
                Value = iv.Value
            }).ToList() ?? throw new InvalidOperationException("RequestTemplateValues is null"),
            Status = this.Status,
            RequestAttachmentDtos = [.. this.RequestAttachments.Select(ra => ra.ToDto())],
            Template = this.RequestTemplateValues?.Template?.ToDto(),
            Requester = this.Requester?.ToDto(),
            Approver = this.Approver?.ToDto(),
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt,
            ResponseId = this.ResponseId,
            FirstTransactionId = this.FirstTransactionId,
            CurrentTransactionId = this.CurrentTransactionId,
            ReadOnlyUsers = [.. this.ReadOnlyUsers!.Select(u => u.Id)],
            RelatedRequests = [.. this.OutgoingRelations.Select(r => new RelatedRequestDto
                            {
                               RequestId = r.TargetRequestId,
                               RequestNumber = r.TargetRequest.RequestNumber,
                               RelationType = r.RelationType,
                               Notes = r.Notes,
                               Status = r.TargetRequest.Status
                            })]
        };
    }
}

public class RequestDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public int RequestNumber { get; set; }
    public Guid RequesterId { get; set; }
    public Guid ApproverId { get; set; }
    public Guid? ResponseId { get; set; }
    public Guid? FirstTransactionId { get; set; }
    public Guid? CurrentTransactionId { get; set; }
    public RequestStatus Status { get; set; }
    public required ICollection<InputValueDto> Content { get; set; } = [];
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
    public List<RelatedRequestDto> RelatedRequests { get; set; } = [];

}

public class CreateRequestDto
{
    public required Guid TemplateId { get; set; }
    public required Guid DepartmentId { get; set; }
    public required ICollection<Guid> ReadOnlyUsers { get; set; } = [];
    public required List<InputValueDto> Content { get; set; } = [];
    public List<CreateRequestRelatedRequestDto>? RelatedRequests { get; set; } = null;
    public List<IFormFile>? Files { get; set; } = [];
}

public class CreateRequestRelatedRequestDto
{
    public Guid TargetRequestId { get; set; }
    public RequestRelationType RelationType { get; set; } = RequestRelationType.Reference;
    public string? Notes { get; set; }
}

public class RelatedRequestDto
{
    public Guid RequestId { get; set; }
    public int RequestNumber { get; set; }
    public RequestRelationType RelationType { get; set; }
    public string? Notes { get; set; }
    public RequestStatus Status { get; set; } // مفيد للواجهة
}
