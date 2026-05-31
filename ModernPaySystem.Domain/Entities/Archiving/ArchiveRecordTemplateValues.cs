using System.Linq;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class ArchiveRecordTemplateValues : Entity<Guid>, IAuditableEntity
{


    public Guid ArchiveRecordId { get; set; }
    public ArchiveRecord? ArchiveRecord { get; set; }
    public Guid ArchiveFormTemplateId { get; set; }
    public ArchiveFormTemplate? ArchiveFormTemplate { get; set; } = null!;


    public ICollection<ArchiveRecordFormInputValue> ArchiveRecordFormInputValues { get; set; } = [];


    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ArchiveRecordTemplateValuesDto ToDto()
    {
        return new ArchiveRecordTemplateValuesDto
        {
            Id = Id,
            ArchiveRecordId = ArchiveRecordId,
            ArchiveFormTemplateId = ArchiveFormTemplateId,
            ArchiveRecordFormInputValues = [.. ArchiveRecordFormInputValues.Select(x => x.ToDto())],
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            UpdatedByUserId = UpdatedByUserId,
            UpdatedAt = UpdatedAt
        };
    }
}
