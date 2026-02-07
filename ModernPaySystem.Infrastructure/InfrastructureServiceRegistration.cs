using Microsoft.AspNetCore.Authorization;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Commons.Auth;
using ModernPaySystem.Infrastructure.Auth.Policies;
using ModernPaySystem.Infrastructure.Auth.Services;
using ModernPaySystem.Infrastructure.Extensions;
using ModernPaySystem.Infrastructure.Services;

namespace ModernPaySystem.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure services
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
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

        // Register Authorization Handlers
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    /// <summary>
    /// Adds authorization policies for the application.
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Transaction System Permissions
            options.AddPolicy(
                Permission.TransactionSystem.ViewTransactions,
                policy => policy.Requirements.Add(
                    new PermissionRequirement(Permission.TransactionSystem.ViewTransactions)));

            options.AddPolicy(
                Permission.TransactionSystem.CreateTransaction,
                policy => policy.Requirements.Add(
                    new PermissionRequirement(Permission.TransactionSystem.CreateTransaction)));

            options.AddPolicy(
                Permission.TransactionSystem.UpdateTransaction,
                policy => policy.Requirements.Add(
                    new PermissionRequirement(Permission.TransactionSystem.UpdateTransaction)));

            options.AddPolicy(
                Permission.TransactionSystem.DeleteTransaction,
                policy => policy.Requirements.Add(
                    new PermissionRequirement(Permission.TransactionSystem.DeleteTransaction)));
        });

        return services;
    }
}
