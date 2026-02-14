namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class SubSystemUser : Entity<Guid>
{
    public SubSystem? SubSystem { get; set; }
    public required Guid UserId { get; set; }
    public User? User { get; set; }
}
