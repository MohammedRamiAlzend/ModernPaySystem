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
}
