using Microsoft.AspNetCore.Http;
using ModernPaySystem.SharedKernel.Application.Services;
using System.Security.Claims;

namespace ModernPaySystem.SharedKernel.Infrastructure.Services;

public class HttpContextServiceManager(IHttpContextAccessor httpContextAccessor)
    : IHttpContextServiceManager
{
    public HttpContext GetContext()
    {
        return httpContextAccessor.HttpContext!;
    }

    public Guid GetCurrentUserId()
    {
        try
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.Identity!.IsAuthenticated == true ? Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value) : throw new Exception("User is not authenticated");
        }
        catch (Exception)
        {
            throw new Exception("User is not authenticated");
        }
    }

    public string? GetClientIpAddress()
    {
        var context = httpContextAccessor.HttpContext;
        if (context == null) return null;

        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }

    public string? GetUserAgent()
    {
        var context = httpContextAccessor.HttpContext;
        return context?.Request.Headers["User-Agent"].FirstOrDefault();
    }
}
