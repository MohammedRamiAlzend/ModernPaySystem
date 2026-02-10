using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Infrastructure.Services;

public class HttpContextServiceManager(IHttpContextAccessor httpContextAccessor,
    IUnitOfWork uow)
    : IHttpContextServiceManager
{
    public HttpContext GetContext()
    {
        return httpContextAccessor.HttpContext!;
    }

    public Result<string?> GetCurrentUserIdAsString()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity.IsAuthenticated == true)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }

        return null;
    }

    public int GetCurrentUserId()
    {
        try
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.Identity.IsAuthenticated == true ? int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value) : throw new Exception("User is not authenticated");
        }
        catch (Exception)
        {
            throw new Exception("User is not authenticated");
        }
    }

    public async Task<IEnumerable<string>> GetCurrentUserRoles()
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity.IsAuthenticated == true)
        {
            var getUser = await uow.Users.GetAsync(
                u => u.Id.ToString() == GetCurrentUserIdAsString().Value,
                transform: u => u.Include(u => u.Roles));
            if (getUser.IsError)
                return [];

            return getUser.Value.Roles.Select(x => x.Name);
        }

        return [];
    }

    public async Task<bool> IsSuperAdmin()
    {
        var getRoles = await GetCurrentUserRoles();
        return getRoles.Contains("SuperAdmin");
    }

}