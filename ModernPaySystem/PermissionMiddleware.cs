using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using ModernPaySystem.Domain.Attrs;
using ModernPaySystem.Infrastructure.Auth.Policies;

namespace ModernPaySystem;

public class PermissionMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<ControllerActionDescriptor>() is ControllerActionDescriptor actionDescriptor)
        {
            var methodInfo = actionDescriptor.MethodInfo;
            var permissionAttribute = methodInfo.GetCustomAttribute<EndpointPermissionAttribute>();

            if (permissionAttribute != null)
            {
                var requirement = new PermissionRequirement(permissionAttribute.Key);
                var authResult = await authorizationService.AuthorizeAsync(context.User, null, requirement);

                if (!authResult.Succeeded)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Access denied: Insufficient permissions.");
                    return;
                }
            }
        }

        await _next(context);
    }
}