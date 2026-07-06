using ModernPaySystem.SharedKernel.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.Module.Transaction.Domain.Entities;

public class TemplateDepartmentOwnership : Entity<Guid>
{
    public required Guid TemplateId { get; set; }
    public Template? Template { get; set; }

    public required Guid DepartmentId { get; set; }
}
