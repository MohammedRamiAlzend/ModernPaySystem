namespace ModernPaySystem.Domain.Entities.PaySystemEntities.FastOperations;

public class Gender : Entity<Guid>
{
    public string Desc { get; set; } = string.Empty;
}
