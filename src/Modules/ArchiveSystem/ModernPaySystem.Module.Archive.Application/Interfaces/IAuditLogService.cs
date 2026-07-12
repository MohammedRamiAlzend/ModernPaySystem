using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.Module.Archive.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Application.Interfaces;

public interface IAuditLogService
{
    Task<Result<Success>> LogAsync(
        Guid archiveRecordId,
        string userId,
        AuditAction action,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);
}
