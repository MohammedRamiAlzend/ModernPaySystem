using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Application.Interfaces;

public interface IAuditLogService
{
    Task<Result<Success>> LogAsync(
        Guid archiveRecordId,
        string userId,
        AuditAction action,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null);

    Task<Result<PagedList<ArchiveAuditLog>>> GetAuditLogsAsync(
        Guid archiveRecordId,
        int page = 1,
        int pageSize = 50);

    Task<Result<PagedList<ArchiveAuditLog>>> GetAllAuditLogsAsync(
        int page = 1,
        int pageSize = 50,
        Guid? archiveRecordId = null,
        AuditAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    Task<Result<PagedList<ArchiveAuditLog>>> GetAuditLogsByDepartmentAsync(
        Guid departmentId,
        int page = 1,
        int pageSize = 50,
        AuditAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
}