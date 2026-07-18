using ModernPaySystem.Application.Interfaces.TransactionSystemInterfaces;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Persistence.UnitOfWork;

namespace ModernPaySystem.Infrastructure.Services;

public class RequestAuditService(
    IUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<RequestAuditService> logger) : IRequestAuditService
{
    public async Task LogAsync(Guid requestId, RequestAuditAction action, string? details = null)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();

            var auditLog = new RequestAuditLog
            {
                RequestId = requestId,
                UserId = userId,
                Action = action,
                Details = details,
                IpAddress = httpContextServiceManager.GetClientIpAddress(),
                UserAgent = httpContextServiceManager.GetUserAgent(),
                Timestamp = DateTime.UtcNow
            };

            await unitOfWork.RequestAuditLogs.AddAsync(auditLog);
            await unitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to log audit action {Action} for request {RequestId}", action, requestId);
        }
    }
}
