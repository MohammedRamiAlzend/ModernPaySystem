using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;
using ModernPaySystem.Module.Identity.Infrastructure.Services;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding;

public static class SeedingServiceRegistration
{
    public static IServiceCollection AddIdentitySeeding(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var seedingConfiguration = new SeedingConfiguration();
        var seedingSection = configuration.GetSection("Seeding");

        if (seedingSection.Exists())
        {
            if (bool.TryParse(seedingSection["Enabled"], out var enabled))
            {
                seedingConfiguration.Enabled = enabled;
            }

            if (bool.TryParse(seedingSection["ClearExistingData"], out var clearExistingData))
            {
                seedingConfiguration.ClearExistingData = clearExistingData;
            }

            seedingConfiguration.Environment = seedingSection["Environment"] ?? seedingConfiguration.Environment;
        }

        services.AddSingleton(seedingConfiguration);

        services.AddScoped<IEntitySeeder, RoleSeeder>();
        services.AddScoped<IEntitySeeder, UserSeeder>();
        services.AddScoped<IEntitySeeder, DepartmentSeeder>();
        services.AddScoped<IEntitySeeder, DepartmentUserLinkSeeder>();
        services.AddScoped<ISeederOrchestrator, IdentitySeederOrchestrator>();
        services.AddScoped<IPermissionSeederService, PermissionSeederService>();

        return services;
    }
}