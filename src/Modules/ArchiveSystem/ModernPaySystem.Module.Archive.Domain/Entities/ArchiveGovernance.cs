using ModernPaySystem.SharedKernel.Domain.Entities;
using Microsoft.AspNetCore.Http;
using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;
using System.Text.Json;
using System.Linq;

namespace ModernPaySystem.Module.Archive.Domain.Entities;

public enum ArchiveDeletionTargetType
{
    Folder = 0,
    Record = 1
}

public enum DeleteArchiveRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Executed = 3
}

public class DepartmentArchiveLeader : Entity<Guid>, IAuditableEntity
{
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DeleteArchiveRequest : Entity<Guid>, IAuditableEntity
{
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;

    public ArchiveDeletionTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }

    public Guid RequesterId { get; set; }
    public User Requester { get; set; } = default!;

    public Guid ApproverId { get; set; }
    public User Approver { get; set; } = default!;

    public DeleteArchiveRequestStatus Status { get; set; } = DeleteArchiveRequestStatus.Pending;
    public string Justification { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? ApprovalNotes { get; set; }

    public string TargetSnapshotJson { get; set; } = string.Empty;
    public string DependenciesSnapshotJson { get; set; } = string.Empty;
    public string? ActivitySnapshotJson { get; set; }

    public Guid? SourceFolderId { get; set; }
    public string? TargetDisplayName { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ExecutedByUserId { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTime? RejectedAt { get; set; }

    public string? RequesterNotificationMessage { get; set; }
    public DateTime? RequesterNotifiedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DeleteArchiveRequestDto ToDto()
        => DeleteArchiveRequestDto.FromEntity(this);
}

public sealed record ArchiveDeletionTargetSnapshotDto(
    ArchiveDeletionTargetType TargetType,
    Guid TargetId,
    Guid DepartmentId,
    string DisplayName,
    string? ParentPath,
    int ChildFolderCount,
    int DescendantFolderCount,
    int RecordCount,
    int FileCount,
    string? MetadataJson);

public sealed record ArchiveDeletionDependencyDto(
    string Kind,
    Guid Id,
    string? DisplayName,
    string? Details);

public class ArchiveLeaderAssignmentDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateArchiveLeaderAssignmentDto
{
    public Guid UserId { get; set; }
}

public class CreateDeleteArchiveRequestDto
{
    public ArchiveDeletionTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public string Justification { get; set; } = string.Empty;
}

public class DeleteArchiveRequestDecisionDto
{
    public string? Notes { get; set; }
}

public class DeleteArchiveRequestRejectDto
{
    public string Reason { get; set; } = string.Empty;
}

public class DeleteArchiveRequestDto
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public ArchiveDeletionTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }
    public DeleteArchiveRequestStatus Status { get; set; }
    public Guid RequesterId { get; set; }
    public string? RequesterName { get; set; }
    public Guid ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? TargetDisplayName { get; set; }
    public ArchiveDeletionTargetSnapshotDto? TargetSnapshot { get; set; }
    public List<ArchiveDeletionDependencyDto> Dependencies { get; set; } = [];
    public string? ActivitySnapshotJson { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ExecutedByUserId { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RequesterNotificationMessage { get; set; }
    public DateTime? RequesterNotifiedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static DeleteArchiveRequestDto FromEntity(DeleteArchiveRequest entity)
    {
        var snapshot = JsonSerializer.Deserialize<ArchiveDeletionTargetSnapshotDto>(entity.TargetSnapshotJson);
        var dependencies = JsonSerializer.Deserialize<List<ArchiveDeletionDependencyDto>>(entity.DependenciesSnapshotJson) ?? [];

        return new DeleteArchiveRequestDto
        {
            Id = entity.Id,
            DepartmentId = entity.DepartmentId,
            TargetType = entity.TargetType,
            TargetId = entity.TargetId,
            Status = entity.Status,
            RequesterId = entity.RequesterId,
            RequesterName = entity.Requester?.UserName,
            ApproverId = entity.ApproverId,
            ApproverName = entity.Approver?.UserName,
            Justification = entity.Justification,
            RejectionReason = entity.RejectionReason,
            ApprovalNotes = entity.ApprovalNotes,
            TargetDisplayName = entity.TargetDisplayName,
            TargetSnapshot = snapshot,
            Dependencies = dependencies,
            ActivitySnapshotJson = entity.ActivitySnapshotJson,
            ApprovedByUserId = entity.ApprovedByUserId,
            ApprovedAt = entity.ApprovedAt,
            ExecutedByUserId = entity.ExecutedByUserId,
            ExecutedAt = entity.ExecutedAt,
            RejectedByUserId = entity.RejectedByUserId,
            RejectedAt = entity.RejectedAt,
            RequesterNotificationMessage = entity.RequesterNotificationMessage,
            RequesterNotifiedAt = entity.RequesterNotifiedAt,
            CreatedByUserId = entity.CreatedByUserId,
            CreatedAt = entity.CreatedAt,
            UpdatedByUserId = entity.UpdatedByUserId,
            UpdatedAt = entity.UpdatedAt
        };
    }
}

public enum EditArchiveRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class EditArchiveRequest : Entity<Guid>, IAuditableEntity
{
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = default!;

    public Guid ArchiveRecordId { get; set; }
    public ArchiveRecord ArchiveRecord { get; set; } = default!;

    public Guid RequesterId { get; set; }
    public User Requester { get; set; } = default!;

    public Guid? ApproverId { get; set; }
    public User? Approver { get; set; }

    public EditArchiveRequestStatus Status { get; set; } = EditArchiveRequestStatus.Pending;

    public string Justification { get; set; } = string.Empty;

    public string RequestedChangesJson { get; set; } = string.Empty;

    public string? RequestedRecordName { get; set; }

    public string? RequestedFileDeletionIdsJson { get; set; }

    public string OriginalSnapshotJson { get; set; } = string.Empty;

    public string? RejectionReason { get; set; }
    public string? ApprovalNotes { get; set; }

    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTime? RejectedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<PhysicalFile> PhysicalFiles { get; set; } = [];
}

public class CreateEditArchiveRequestDto
{
    public Guid ArchiveRecordId { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string? RequestedRecordName { get; set; }
    public List<ArchiveRecordFormInputValueDto> RequestedChanges { get; set; } = [];
    public IFormFileCollection? Files { get; set; }
    public List<Guid>? FileIdsToDelete { get; set; }
}

public class EditArchiveRequestDecisionDto
{
    public string? Notes { get; set; }
}

public class EditArchiveRequestRejectDto
{
    public string Reason { get; set; } = string.Empty;
}

public class EditArchiveRequestDto
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid ArchiveRecordId { get; set; }
    public EditArchiveRequestStatus Status { get; set; }
    public Guid RequesterId { get; set; }
    public string? RequesterName { get; set; }
    public Guid? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public string Justification { get; set; } = string.Empty;
    public List<ArchiveRecordFormInputValueDto> RequestedChanges { get; set; } = [];
    public string? RequestedRecordName { get; set; }
    public string? OriginalSnapshotJson { get; set; }
    public string? RejectionReason { get; set; }
    public string? ApprovalNotes { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedByUserId { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public List<PhysicalFileDto> AttachedFiles { get; set; } = [];
    public List<Guid>? FileIdsToDelete { get; set; }

    public static EditArchiveRequestDto FromEntity(EditArchiveRequest entity)
    {
        var changes = string.IsNullOrEmpty(entity.RequestedChangesJson)
            ? new List<ArchiveRecordFormInputValueDto>()
            : JsonSerializer.Deserialize<List<ArchiveRecordFormInputValueDto>>(entity.RequestedChangesJson, JsonOptions) ?? [];

        var fileDeletionIds = string.IsNullOrEmpty(entity.RequestedFileDeletionIdsJson)
            ? null
            : JsonSerializer.Deserialize<List<Guid>>(entity.RequestedFileDeletionIdsJson, JsonOptions);

        return new EditArchiveRequestDto
        {
            Id = entity.Id,
            DepartmentId = entity.DepartmentId,
            ArchiveRecordId = entity.ArchiveRecordId,
            Status = entity.Status,
            RequesterId = entity.RequesterId,
            RequesterName = entity.Requester?.UserName,
            ApproverId = entity.ApproverId,
            ApproverName = entity.Approver?.UserName,
            Justification = entity.Justification,
            RequestedChanges = changes,
            RequestedRecordName = entity.RequestedRecordName,
            OriginalSnapshotJson = entity.OriginalSnapshotJson,
            RejectionReason = entity.RejectionReason,
            ApprovalNotes = entity.ApprovalNotes,
            ApprovedByUserId = entity.ApprovedByUserId,
            ApprovedAt = entity.ApprovedAt,
            RejectedByUserId = entity.RejectedByUserId,
            RejectedAt = entity.RejectedAt,
            CreatedByUserId = entity.CreatedByUserId,
            CreatedAt = entity.CreatedAt,
            AttachedFiles = entity.PhysicalFiles != null
                ? [.. entity.PhysicalFiles.Where(f => !f.IsDeleted).Select(f => f.ToDto())]
                : [],
            FileIdsToDelete = fileDeletionIds
        };
    }
}
