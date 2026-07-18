namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;


public enum RequestRelationType
{
    Reference = 0,    // مرجع فقط
    FollowUp = 1,     // متابعة / استكمال
    Replacement = 2,  // استبدال لطلب سابق
    Duplicate = 3     // تكرار
}

public class RequestRelation : Entity<Guid>, IAuditableEntity
{
    public Guid SourceRequestId { get; set; }
    public Request SourceRequest { get; set; } = null!;

    public Guid TargetRequestId { get; set; }
    public Request TargetRequest { get; set; } = null!;

    public RequestRelationType RelationType { get; set; } = RequestRelationType.Reference;
    public string? Notes { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public RequestRelationDto ToDto()
    {
        return new RequestRelationDto
        {
            Id = Id,
            SourceRequestId = SourceRequestId,
            TargetRequestId = TargetRequestId,
            SourceRequestNumber = SourceRequest?.RequestNumber ?? 0,
            TargetRequestNumber = TargetRequest?.RequestNumber ?? 0,
            SourceRequestStatus = SourceRequest?.Status ?? RequestStatus.Pending,
            TargetRequestStatus = TargetRequest?.Status ?? RequestStatus.Pending,
            RelationType = RelationType,
            Notes = Notes,
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            UpdatedByUserId = UpdatedByUserId,
            UpdatedAt = UpdatedAt
        };
    }
}

public class RequestRelationDto
{
    public Guid Id { get; set; }
    public Guid SourceRequestId { get; set; }
    public Guid TargetRequestId { get; set; }
    public int SourceRequestNumber { get; set; }
    public int TargetRequestNumber { get; set; }
    public RequestStatus SourceRequestStatus { get; set; }
    public RequestStatus TargetRequestStatus { get; set; }
    public RequestRelationType RelationType { get; set; }
    public string? Notes { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateRequestRelationDto
{
    public Guid SourceRequestId { get; set; }
    public Guid TargetRequestId { get; set; }
    public RequestRelationType RelationType { get; set; } = RequestRelationType.Reference;
    public string? Notes { get; set; }
}

public class UpdateRequestRelationDto
{
    public Guid SourceRequestId { get; set; }
    public Guid TargetRequestId { get; set; }
    public RequestRelationType RelationType { get; set; } = RequestRelationType.Reference;
    public string? Notes { get; set; }
}
