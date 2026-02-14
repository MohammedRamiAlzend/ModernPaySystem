using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

/// <summary>
/// Extension methods for registering seeding services
/// Follows the Dependency Injection pattern for loose coupling
/// </summary>
public static class SeedingServiceRegistration
{
    /// <summary>
    /// Add seeding services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddSeeding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Get seeding configuration from appsettings
        var seedingConfig = new SeedingConfiguration();
        var seedingSection = configuration.GetSection("Seeding");
        if (seedingSection.Exists())
        {
            string env = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
            var seedingEnv = env.Equals("Production", StringComparison.OrdinalIgnoreCase)
                ? SeedingEnvironment.Production
                : SeedingEnvironment.Development;

            seedingConfig.Environment = seedingEnv;
            seedingConfig.Quantities.ApplyEnvironmentDefaults(seedingEnv);
        }

        // Register configuration
        services.AddSingleton(seedingConfig);

        // Register all seeders
        RegisterSeeders(services);

        // Register orchestrator
        services.AddScoped<ISeederOrchestrator, SeederOrchestrator>();

        return services;
    }

    /// <summary>
    /// Register all entity seeders
    /// Each seeder is registered separately to support dependency injection
    /// </summary>
    private static void RegisterSeeders(IServiceCollection services)
    {
        services.AddScoped<IEntitySeeder, DefaultDataSeeder>();
        services.AddScoped<IEntitySeeder, PermissionSeeder>();
        services.AddScoped<IEntitySeeder, RoleSeeder>();
        services.AddScoped<IEntitySeeder, UserSeeder>();
    }
}
