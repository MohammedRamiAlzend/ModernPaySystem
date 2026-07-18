namespace ModernPaySystem.Domain.Entities.PaySystemEntities.FastOperations;

public class OperationStatus : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}
