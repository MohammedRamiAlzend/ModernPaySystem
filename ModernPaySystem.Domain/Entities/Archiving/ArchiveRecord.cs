using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class ArchiveRecord : Entity<Guid>, IAuditableEntity
{
    public Guid FolderId { get; set; }
    public Folder Folder { get; set; } = default!;
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public Guid? FormId { get; set; }
    public ArchiveFormTemplate? Form { get; set; } 

    public string ArchivalNumber { get; set; } = string.Empty;

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
            FolderId = FolderId,
            DepartmentId = DepartmentId,
            FormId = FormId,
            ArchivalNumber = ArchivalNumber,
            ArchiveRecordTemplateValues = ArchiveRecordTemplateValuesId?.ToDto(),
            PhysicalFiles = [.. PhysicalFiles.Where(pf => !pf.IsDeleted).Select(pf => pf.ToDto())],
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
    public Guid FolderId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? FormId { get; set; }
    public string ArchivalNumber { get; set; } = string.Empty;
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
    public Guid FolderId { get; set; }
    public Guid? FormId { get; set; } = null;
    public string ArchivalNumber { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public IFormFileCollection? Files { get; set; } = default!;
    public List<ArchiveRecordFormInputValueDto> Content { get; set; } = [];
}

public class UpdateArchiveRecordDto
{
    public Guid FolderId { get; set; }
    public Guid FormId { get; set; }
    public string ArchivalNumber { get; set; } = string.Empty;
    public List<ArchiveRecordFormInputValueDto> Content { get; set; } = [];
    public IFormFileCollection? Files { get; set; }
    public List<Guid> FileIdsToRemove { get; set; } = [];
    public bool ReplaceFiles { get; set; }
}
