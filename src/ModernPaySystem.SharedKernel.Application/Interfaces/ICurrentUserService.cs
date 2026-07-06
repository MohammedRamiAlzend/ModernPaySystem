using ModernPaySystem.SharedKernel.Domain.Identity;

namespace ModernPaySystem.SharedKernel.Application.Interfaces;

public interface ICurrentUserService
{
    CurrentUser GetCurrentUser();
    Guid GetCurrentUserId();
    bool IsAuthenticated();
    bool HasPermission(string permission);
}
