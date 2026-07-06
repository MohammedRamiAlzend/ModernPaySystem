using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding;

public sealed class IdentitySeederOrchestrator(
    IdentityDbContext context,
    SeedingConfiguration configuration,
    IEnumerable<IEntitySeeder> seeders,
    ILogger<IdentitySeederOrchestrator> logger) : ISeederOrchestrator
{
    private readonly IdentityDbContext _context = context;
    private readonly SeedingConfiguration _configuration = configuration;
    private readonly IReadOnlyList<IEntitySeeder> _seeders = seeders.OrderBy(seeder => seeder.Order).ToList();
    private readonly ILogger<IdentitySeederOrchestrator> _logger = logger;

    public async Task SeedDatabaseAsync(CancellationToken cancellationToken = default)
    {
        if (!_configuration.Enabled)
        {
            _logger.LogInformation("Identity seeding is disabled");
            return;
        }

        _logger.LogInformation("Starting identity seeding");

        foreach (var seeder in _seeders)
        {
            await SeedEntityAsync(seeder, cancellationToken);
        }

        _logger.LogInformation("Identity seeding completed successfully");
    }

    private async Task SeedEntityAsync(IEntitySeeder seeder, CancellationToken cancellationToken)
    {
        var entityName = seeder.GetEntityName();
        var hasData = await seeder.HasDataAsync(_context, cancellationToken);

        if (hasData && !_configuration.ClearExistingData)
        {
            _logger.LogInformation("Skipping {Entity} seeding because data already exists", entityName);
            return;
        }

        _logger.LogInformation("Seeding {Entity}", entityName);
        await seeder.SeedAsync(_context, _configuration, cancellationToken);
    }
}