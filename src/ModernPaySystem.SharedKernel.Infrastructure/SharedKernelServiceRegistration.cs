using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.SharedKernel.Application.Interfaces;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Infrastructure.Services;

namespace ModernPaySystem.SharedKernel.Infrastructure;

public static class SharedKernelServiceRegistration
{
    public static IServiceCollection AddSharedKernel(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<IHttpContextServiceManager, HttpContextServiceManager>();
        services.AddTransient<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
