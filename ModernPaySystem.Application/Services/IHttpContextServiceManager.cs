using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Services;

public interface IHttpContextServiceManager
{
    public Result<string> GetCurrentUserIdAsString();
    public int GetCurrentUserId();
    public HttpContext GetContext();
    public Task<IEnumerable<string>> GetCurrentUserRoles();
    public Task<bool> IsSuperAdmin();
}
