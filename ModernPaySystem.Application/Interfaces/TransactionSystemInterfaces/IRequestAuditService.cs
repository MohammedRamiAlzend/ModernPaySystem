using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Application.Interfaces.TransactionSystemInterfaces;

public interface IRequestAuditService
{
    Task LogAsync(Guid requestId, RequestAuditAction action, string? details = null);
}
