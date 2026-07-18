using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Services;

public interface IHttpContextServiceManager
{
    public Guid GetCurrentUserId();
    public HttpContext GetContext();
    public string? GetClientIpAddress();
    public string? GetUserAgent();
}
