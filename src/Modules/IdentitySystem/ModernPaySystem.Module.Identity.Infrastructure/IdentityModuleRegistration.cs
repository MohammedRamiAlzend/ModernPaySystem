using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Module.Identity.Application;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.Module.Identity.Application.Validators;
using ModernPaySystem.Module.Identity.Infrastructure.Auth;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.Module.Identity.Infrastructure.Seeding;
using ModernPaySystem.Module.Identity.Infrastructure.Services;
using ModernPaySystem.SharedKernel.Infrastructure;

namespace ModernPaySystem.Module.Identity.Infrastructure;

public static class IdentityModuleRegistration
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSharedKernel();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                connectionString,
                npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                });
        });

        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddIdentitySeeding(configuration);

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // FluentValidation validators
        services.AddIdentityValidators();

        return services;
    }
}
