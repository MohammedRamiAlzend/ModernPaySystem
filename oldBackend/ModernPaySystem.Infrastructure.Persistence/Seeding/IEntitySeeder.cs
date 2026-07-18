namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

/// <summary>
/// Base interface for entity seeders
/// Follows the Single Responsibility Principle - each seeder has one job.
/// </summary>
public interface IEntitySeeder
{
    /// <summary>
    /// Get the order priority for seeding (lower numbers seed first)
    /// This ensures dependencies are seeded before dependents.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Check if the entity already has data.
    /// </summary>
    Task<bool> HasDataAsync(AppDbContext context);

    /// <summary>
    /// Seed entities into the database.
    /// </summary>
    Task SeedAsync(AppDbContext context, SeedingConfiguration configuration);

    /// <summary>
    /// Get the name of the entity being seeded.
    /// </summary>
    string GetEntityName();
}
