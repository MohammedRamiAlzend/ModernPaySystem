using Microsoft.EntityFrameworkCore;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding;

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
