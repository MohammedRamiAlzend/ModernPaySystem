namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public class InputValue : Entity<Guid>
{
    public Guid RequestTemplateValuesId { get; set; }
    public RequestTemplateValues RequestTemplateValues { get; set; } = null!;

    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
public class InputValueDto
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
