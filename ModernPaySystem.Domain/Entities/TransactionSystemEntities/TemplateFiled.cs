namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class TemplateFiled : Entity<Guid>, IAuditableEntity
{

    public required string FieldName { get; set; }
    public required string FieldType { get; set; }
    public required string FieldValue { get; set; }
    public required string FieldLabel { get; set; }
    public required string? PlaceHolder { get; set; }
    public required string DefaultValue { get; set; }
    public required string DataSource { get; set; }
    public required bool Hidden { get; set; }
    public required bool Disabled { get; set; }
    public required bool ReadOnly { get; set; }
    public required string InitialVisibility { get; set; }
    public required string InitialEnabled { get; set; }
    public required string Direction { get; set; }
    public required int? Rows { get; set; }
    public required string NumberSpelling { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
