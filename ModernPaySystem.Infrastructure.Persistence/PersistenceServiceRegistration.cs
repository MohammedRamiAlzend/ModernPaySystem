using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Infrastructure.Persistence.Interceptors;

namespace ModernPaySystem.Infrastructure.Persistence;

/// <summary>
/// Extension methods for registering persistence services.
/// </summary>
public static class PersistenceServiceRegistration
{
    /// <summary>
    /// Adds persistence services and DbContext to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptionsAction: sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
        });

        return services;
    }
}
