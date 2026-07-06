using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class RequestAuditService(
    ITransactionUnitOfWork unitOfWork,
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
                Id = Guid.NewGuid(),
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
