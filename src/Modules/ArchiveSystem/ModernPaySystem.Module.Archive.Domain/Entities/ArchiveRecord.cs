using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ModernPaySystem.Module.Archive.Domain.Entities;

public class ArchiveRecord : Entity<Guid>, IAuditableEntity
{
    public string? Name { get; set; }
    public Guid FolderId { get; set; }
    public Folder Folder { get; set; } = default!;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? FormId { get; set; }
    public ArchiveFormTemplate? Form { get; set; } 

    public Guid? ArchiveRecordTemplateValues { get; set; }
    public ArchiveRecordTemplateValues? ArchiveRecordTemplateValuesId { get; set; }

    public ICollection<PhysicalFile> PhysicalFiles { get; set; } = [];

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }
    public Guid? DeletedByRequestId { get; set; }
    public Guid? ApprovedByRequestId { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ArchiveRecordDto ToDto()
    {
        return new ArchiveRecordDto
        {
            Id = Id,
            Name = Name,
            FolderId = FolderId,
            DepartmentId = DepartmentId,
            FormId = FormId,
            ArchiveRecordTemplateValues = ArchiveRecordTemplateValuesId?.ToDto(),
            PhysicalFiles = [.. PhysicalFiles.Where(pf => !pf.IsDeleted && pf.EditArchiveRequestId == null).Select(pf => pf.ToDto())],
            IsDeleted = IsDeleted,
            DeletedAt = DeletedAt,
            DeletedByUserId = DeletedByUserId,
            DeletedByRequestId = DeletedByRequestId,
            ApprovedByRequestId = ApprovedByRequestId,
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            UpdatedByUserId = UpdatedByUserId,
            UpdatedAt = UpdatedAt
        };
    }
}

public class ArchiveRecordDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid FolderId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? FormId { get; set; }
    public ArchiveRecordTemplateValuesDto? ArchiveRecordTemplateValues { get; set; }
    public List<PhysicalFileDto> PhysicalFiles { get; set; } = [];
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }
    public Guid? DeletedByRequestId { get; set; }
    public Guid? ApprovedByRequestId { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ArchiveRecordTemplateValuesDto
{
    public Guid Id { get; set; }
    public Guid ArchiveRecordId { get; set; }
    public Guid ArchiveFormTemplateId { get; set; }
    public List<ArchiveRecordFormInputValueDto> ArchiveRecordFormInputValues { get; set; } = [];
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ArchiveRecordFormInputValueDto
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}

public class CreateArchiveRecordDto
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public Guid FolderId { get; set; }
    public Guid? FormId { get; set; } = null;
    public Guid? DepartmentId { get; set; }
    public IFormFileCollection? Files { get; set; } = default!;
    public List<ArchiveRecordFormInputValueDto> Content { get; set; } = [];
}

public class MoveArchiveRecordDto
{
    public Guid DestinationFolderId { get; set; }
}

public class UpdateArchiveRecordDto
{
    public string? Name { get; set; }
    public Guid FolderId { get; set; }
    public Guid? FormId { get; set; }
    public List<ArchiveRecordFormInputValueDto> Content { get; set; } = [];
    public IFormFileCollection? Files { get; set; }
    public List<Guid> FileIdsToRemove { get; set; } = [];
    public bool ReplaceFiles { get; set; }
}
