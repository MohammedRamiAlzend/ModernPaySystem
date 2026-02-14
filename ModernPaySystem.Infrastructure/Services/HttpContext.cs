using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Infrastructure.Services;

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
            return user?.Identity.IsAuthenticated == true ? Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value) : throw new Exception("User is not authenticated");
        }
        catch (Exception)
        {
            throw new Exception("User is not authenticated");
        }
    }
}