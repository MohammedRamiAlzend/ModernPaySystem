using ModernPaySystem.SharedKernel.Domain.Entities.Abstraction;

namespace ModernPaySystem.SharedKernel.Domain.Entities;

public class SubSystemUser : Entity<Guid>
{
    public SubSystem? SubSystem { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
