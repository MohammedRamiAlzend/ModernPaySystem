using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

public static class SeedingServiceRegistration
{
    public static IServiceCollection AddSeeding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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

        services.AddSingleton(seedingConfig);

        RegisterSeeders(services);

        services.AddScoped<ISeederOrchestrator, SeederOrchestrator>();

        return services;
    }

    private static void RegisterSeeders(IServiceCollection services)
    {
        services.AddScoped<IEntitySeeder, DefaultDataSeeder>();
        services.AddScoped<IEntitySeeder, PermissionSeeder>();
        services.AddScoped<IEntitySeeder, RoleSeeder>();
        services.AddScoped<IEntitySeeder, UserSeeder>();
        services.AddScoped<IEntitySeeder, DepartmentSeeder>();
        services.AddScoped<IEntitySeeder, DepartmentUserLinkSeeder>();
    }
}
