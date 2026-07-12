using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class AuditLogService(ArchiveDbContext dbContext) : IAuditLogService
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
                return ArchiveErrors.InvalidInput;
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

            await dbContext.ArchiveAuditLogs.AddAsync(auditLog);
            await dbContext.SaveChangesAsync();

            return Result.Success;
        }
        catch (Exception)
        {
            return ArchiveErrors.InternalServerError;
        }
    }
}
