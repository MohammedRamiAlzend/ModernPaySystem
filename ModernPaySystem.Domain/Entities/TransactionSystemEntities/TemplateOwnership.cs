using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class TemplateOwnership : Entity<Guid>
{
    public required Guid TemplateId { get; set; }
    public Template? Template { get; set; }

    public required Guid UserId { get; set; }
    public User? User { get; set; }
}