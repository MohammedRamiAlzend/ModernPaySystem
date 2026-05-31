using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class ArchiveFormTemplate : Entity<Guid>, IAuditableEntity
{
    public string FormName { get; set; } = string.Empty;
    public string ContentAsJson { get; set; } = "{}";
    public string? FormDescription { get; set; }
    public ICollection<ArchiveRecord> ArchiveRecords { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ArchiveFormTemplateDto ToDto()
    {
        return new ArchiveFormTemplateDto
        {
            Id = Id,
            TemplateFormName = FormName,
            ContentAsJson = ContentAsJson,
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            UpdatedByUserId = UpdatedByUserId,
            UpdatedAt = UpdatedAt
        };
    }
}

public class ArchiveFormTemplateDto
{
    public Guid Id { get; set; }
    public string TemplateFormName { get; set; } = string.Empty;
    public string ContentAsJson { get; set; } = string.Empty;
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateDynamicFormTemplateDto
{
    public string TemplateFormName { get; set; } = string.Empty;
    public string ContentAsJson { get; set; } = string.Empty;
}

public class UpdateDynamicFormTemplateDto
{
    public string TemplateFormName { get; set; } = string.Empty;
    public string ContentAsJson { get; set; } = string.Empty;
}
