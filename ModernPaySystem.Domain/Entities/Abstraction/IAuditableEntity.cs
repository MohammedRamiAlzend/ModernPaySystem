namespace ModernPaySystem.Domain.Entities.Abstraction;

public interface IAuditableEntity
{
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
