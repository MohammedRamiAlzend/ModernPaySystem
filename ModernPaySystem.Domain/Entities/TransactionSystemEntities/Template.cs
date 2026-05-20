namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class Template : Entity<Guid>, IAuditableEntity
{
    public ICollection<User> VisitedByUsers { get; set; } = new List<User>();
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public string? TemplateDescription { get; set; }
    public required bool IsRequireAttachments { get; set; } = false;
    public Department DefaultReceiverDepartment { get; set; } = null!;
    public Guid DefaultReceiverDepartmentId { get; set; }

    // Navigation properties
    public ICollection<Request> Requests { get; set; } = [];
    public ICollection<TemplateDepartmentOwnership>? DepartmentOwnerships { get; set; } = null;
    public ICollection<UserTemplateOwnership>? UserOwnerships { get; set; } = null;
    public ICollection<LookUpField> LookUpFields { get; set; } = [];
    public ICollection<DepartmentTemplateNumber> DepartmentTemplateNumbers { get; set; } = [];

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
            DefaultReceiverDepartmentId = this.DefaultReceiverDepartmentId,
            CreatedByUserId = this.CreatedByUserId,
            CreatedAt = this.CreatedAt,
            UpdatedByUserId = this.UpdatedByUserId,
            UpdatedAt = this.UpdatedAt,
            DepartmentId = this.DefaultReceiverDepartmentId,
            IsRequireAttachments = this.IsRequireAttachments
        };
    }
}

#region DTOs
public class TemplateDto
{
    public Guid Id { get; set; }
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public required Guid? DepartmentId { get; set; }
    public string? TemplateDescription { get; set; }
    public bool IsRequireAttachments { get; set; }
    public Guid DefaultReceiverDepartmentId { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid OwnerId { get; set; }
}

public class CreateTemplateDto
{
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public string? TemplateDescription { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsRequireAttachments { get; set; }
    public Guid DefaultReceiverDepartmentId { get; set; }
}


public class UpdateTemplateDto
{
    public required string ContentAsJson { get; set; }
    public required string TemplateName { get; set; }
    public string? TemplateDescription { get; set; }
    public bool IsRequireAttachments { get; set; }
    public Guid DefaultReceiverDepartmentId { get; set; }
}

#endregion
