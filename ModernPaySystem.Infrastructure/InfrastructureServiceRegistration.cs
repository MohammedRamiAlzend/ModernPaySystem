using FileManager.Extensions;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Infrastructure.Auth.Services;
using ModernPaySystem.Infrastructure.Services;

namespace ModernPaySystem.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Register File Manager Services
        services.AddFileManager();

        // Register HTTP Context Accessor
        services.AddHttpContextAccessor();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Authentication Services
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Register CRUD Services
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<IResponseService, ResponseService>();
        services.AddScoped<IAttachmentService, AttachmentService>();
        services.AddScoped<IWebAttachmentService, WebAttachmentService>();
        services.AddTransient<IHttpContextServiceManager, HttpContextServiceManager>();

        services.AddTransient<IPermissionSeederService>(provider =>
        {
            var applicationPartManager = provider.GetRequiredService<ApplicationPartManager>();
            var uow = provider.GetRequiredService<IUnitOfWork>();
            return new PermissionSeederService(provider, applicationPartManager, uow);
        });

        return services;
    }

    /// <summary>
    /// Adds authorization policies for the application.
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        return services;
    }
}
