namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class Template : Entity<Guid>, IAuditableEntity
{
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public string? TemplateDescription { get; set; }

    // Navigation properties
    public ICollection<Request> Requests { get; set; } = new List<Request>();
    public ICollection<TemplateOwnership> Ownerships { get; set; } = new List<TemplateOwnership>();

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public TemplateDto ToDto()
    {
        return new TemplateDto
        {
            Id = this.Id,
            ContentAsJson = this.ContentAsJson,
            TemplateName = this.TemplateName,
            TemplateDescription = this.TemplateDescription,
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt
        };
    }
}

public class TemplateDto
{
    public Guid Id { get; set; }
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public string? TemplateDescription { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTemplateDto
{
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public string? TemplateDescription { get; set; }
}

public class UpdateTemplateDto
{
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public string? TemplateDescription { get; set; }
}
