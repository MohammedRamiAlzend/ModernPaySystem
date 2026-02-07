using Microsoft.EntityFrameworkCore;

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

/// <summary>
/// Abstract base class for entity seeders
/// Implements common functionality to reduce code duplication.
/// </summary>
public abstract class EntitySeederBase<TEntity> : IEntitySeeder
    where TEntity : class
{
    /// <summary>
    /// Define seeding order in derived classes.
    /// </summary>
    public abstract int Order { get; }

    /// <summary>
    /// Get entity name for logging.
    /// </summary>
    public virtual string GetEntityName() => typeof(TEntity).Name;

    /// <summary>
    /// Check if entity has data.
    /// </summary>
    public virtual async Task<bool> HasDataAsync(AppDbContext context)
    {
        var set = context.Set<TEntity>();
        return await set.AnyAsync();
    }

    /// <summary>
    /// Seed the entity.
    /// </summary>
    public abstract Task SeedAsync(AppDbContext context, SeedingConfiguration configuration);

    /// <summary>
    /// Add entities to context and save.
    /// </summary>
    protected async Task AddEntitiesAsync(AppDbContext context, IEnumerable<TEntity> entities)
    {
        await context.Set<TEntity>().AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}
