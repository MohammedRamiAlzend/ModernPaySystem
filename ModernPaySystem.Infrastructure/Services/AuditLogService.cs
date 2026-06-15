using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Persistence.UnitOfWork;

namespace ModernPaySystem.Infrastructure.Services;

public class AuditLogService(IUnitOfWork unitOfWork) : IAuditLogService
{
    public async Task<Result<Success>> LogAsync(
        Guid archiveRecordId,
        string userId,
        AuditAction action,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            if (archiveRecordId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
            {
                return ApplicationErrors.InvalidInput;
            }

            var auditLog = new ArchiveAuditLog
            {
                Id = Guid.NewGuid(),
                ArchiveRecordId = archiveRecordId,
                UserId = userId,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            var addResult = await unitOfWork.Context.Set<ArchiveAuditLog>().AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();

            return Result.Success;
        }
        catch (Exception ex)
        {
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveAuditLog>>> GetAuditLogsAsync(
        Guid archiveRecordId,
        int page = 1,
        int pageSize = 50)
    {
        try
        {
            if (archiveRecordId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var query = unitOfWork.Context.Set<ArchiveAuditLog>()
                .Where(x => x.ArchiveRecordId == archiveRecordId)
                .OrderByDescending(x => x.Timestamp);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<ArchiveAuditLog>(items, totalCount, page, pageSize);
        }
        catch (Exception ex)
        {
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveAuditLog>>> GetAllAuditLogsAsync(
        int page = 1,
        int pageSize = 50,
        Guid? archiveRecordId = null,
        AuditAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var query = unitOfWork.Context.Set<ArchiveAuditLog>().AsQueryable();

            if (archiveRecordId.HasValue && archiveRecordId.Value != Guid.Empty)
            {
                query = query.Where(x => x.ArchiveRecordId == archiveRecordId.Value);
            }

            if (action.HasValue)
            {
                query = query.Where(x => x.Action == action.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.Timestamp <= toDate.Value);
            }

            query = query.OrderByDescending(x => x.Timestamp);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<ArchiveAuditLog>(items, totalCount, page, pageSize);
        }
        catch (Exception ex)
        {
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveAuditLog>>> GetAuditLogsByDepartmentAsync(
        Guid departmentId,
        int page = 1,
        int pageSize = 50,
        AuditAction? action = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        try
        {
            if (departmentId == Guid.Empty || page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var query = from auditLog in unitOfWork.Context.Set<ArchiveAuditLog>()
                        join archiveRecord in unitOfWork.Context.Set<ArchiveRecord>()
                            on auditLog.ArchiveRecordId equals archiveRecord.Id
                        where archiveRecord.DepartmentId == departmentId
                        select auditLog;

            if (action.HasValue)
            {
                query = query.Where(x => x.Action == action.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.Timestamp >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.Timestamp <= toDate.Value);
            }

            query = query.OrderByDescending(x => x.Timestamp);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<ArchiveAuditLog>(items, totalCount, page, pageSize);
        }
        catch (Exception ex)
        {
            return ApplicationErrors.InternalServerError;
        }
    }
}