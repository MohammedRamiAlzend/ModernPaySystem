using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.Module.Transaction.Domain.Entities;

public class RequestTemplateValues : Entity<Guid>
{
    public Guid TemplateId { get; set; }
    public Template? Template { get; set; } = null!;

    public Guid RequestId { get; set; }
    public Request? Request { get; set; } = null!;

    public ICollection<InputValue> InputValues { get; set; } = [];
}
public class RequestTemplateValuesDto
{
    public Guid TemplateId { get; set; }
    public ICollection<InputValueDto> InputValues { get; set; } = [];
}
