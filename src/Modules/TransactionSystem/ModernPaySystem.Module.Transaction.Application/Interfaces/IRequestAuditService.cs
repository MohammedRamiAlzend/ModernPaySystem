using ModernPaySystem.Module.Transaction.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Application.Interfaces;

public interface IRequestAuditService
{
    Task LogAsync(Guid requestId, RequestAuditAction action, string? details = null);
}
