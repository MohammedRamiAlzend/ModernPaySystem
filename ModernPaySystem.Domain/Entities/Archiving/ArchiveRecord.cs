using Microsoft.AspNetCore.Http;
using System.Linq;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class ArchiveRecord : Entity<Guid>, IAuditableEntity
{
    public Guid FolderId { get; set; }
    public Folder Folder { get; set; } = default!;

    public Guid? FormId { get; set; }
    public ArchiveFormTemplate? Form { get; set; } 

    public string ArchivalNumber { get; set; } = string.Empty;

    public Guid? ArchiveRecordTemplateValues { get; set; }
    public ArchiveRecordTemplateValues? ArchiveRecordTemplateValuesId { get; set; }

    public ICollection<PhysicalFile> PhysicalFiles { get; set; } = [];

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
            FormId = FormId,
            ArchivalNumber = ArchivalNumber,
            ArchiveRecordTemplateValues = ArchiveRecordTemplateValuesId?.ToDto(),
            PhysicalFiles = [.. PhysicalFiles.Where(pf => !pf.IsDeleted).Select(pf => pf.ToDto())],
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
    public Guid? FormId { get; set; }
    public string ArchivalNumber { get; set; } = string.Empty;
    public ArchiveRecordTemplateValuesDto? ArchiveRecordTemplateValues { get; set; }
    public List<PhysicalFileDto> PhysicalFiles { get; set; } = [];
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
    public Guid FolderId { get; set; }
    public Guid? FormId { get; set; } = null;
    public string ArchivalNumber { get; set; } = string.Empty;
    public IFormFileCollection Files { get; set; } = default!;
    public List<ArchiveRecordFormInputValueDto> Content { get; set; } = [];
}

public class UpdateArchiveRecordDto
{
    public Guid FolderId { get; set; }
    public Guid FormId { get; set; }
    public string ArchivalNumber { get; set; } = string.Empty;
    public List<ArchiveRecordFormInputValueDto> Content { get; set; } = [];
    public IFormFileCollection Files { get; set; } = default!;
    public List<Guid> FileIdsToRemove { get; set; } = [];
    public bool ReplaceFiles { get; set; }
}
