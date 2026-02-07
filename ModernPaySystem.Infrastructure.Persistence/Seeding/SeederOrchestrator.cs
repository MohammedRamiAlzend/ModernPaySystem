using Microsoft.Extensions.Logging;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

/// <summary>
/// Orchestrates the seeding process for all entities
/// Ensures entities are seeded in the correct order respecting dependencies
/// Follows the Facade Pattern to simplify complex seeding operations
/// </summary>
public interface ISeederOrchestrator
{
    /// <summary>
    /// Seed the database with all entities
    /// </summary>
    Task SeedDatabaseAsync();
}

/// <summary>
/// Implementation of the seeder orchestrator
/// </summary>
public class SeederOrchestrator : ISeederOrchestrator
{
    private readonly AppDbContext _context;
    private readonly SeedingConfiguration _configuration;
    private readonly IEnumerable<IEntitySeeder> _seeders;
    private readonly ILogger<SeederOrchestrator> _logger;

    public SeederOrchestrator(
        AppDbContext context,
        SeedingConfiguration configuration,
        IEnumerable<IEntitySeeder> seeders,
        ILogger<SeederOrchestrator> logger)
    {
        _context = context;
        _configuration = configuration;
        _seeders = seeders.OrderBy(s => s.Order);
        _logger = logger;
    }

    /// <summary>
    /// Seed the database with all entities in the correct order
    /// </summary>
    public async Task SeedDatabaseAsync()
    {
        try
        {
            if (!_configuration.Enabled)
            {
                _logger.LogInformation("Database seeding is disabled");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // Seed entities in order
            foreach (var seeder in _seeders)
            {
                await SeedEntityAsync(seeder);
            }

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during database seeding");
            // Don't throw - allow app to continue even if seeding fails
        }
    }

    /// <summary>
    /// Seed a single entity type
    /// </summary>
    private async Task SeedEntityAsync(IEntitySeeder seeder)
    {
        try
        {
            var entityName = seeder.GetEntityName();
            var hasData = await seeder.HasDataAsync(_context);

            if (hasData && !_configuration.ClearExistingData)
            {
                _logger.LogInformation("Skipping {Entity} seeding - data already exists", entityName);
                return;
            }

            _logger.LogInformation("Seeding {Entity}...", entityName);
            await seeder.SeedAsync(_context, _configuration);
            _logger.LogInformation("Successfully seeded {Entity}", entityName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding {Entity}", seeder.GetEntityName());
        }
    }
}
