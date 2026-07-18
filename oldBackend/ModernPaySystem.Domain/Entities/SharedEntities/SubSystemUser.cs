namespace ModernPaySystem.Domain.Entities.SharedEntities;

public class SubSystemUser : Entity<Guid>
{
    public SubSystem? SubSystem { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
