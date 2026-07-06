using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding;

public abstract class EntitySeederBase<TEntity> : IEntitySeeder
    where TEntity : class
{
    public abstract int Order { get; }

    public string GetEntityName() => typeof(TEntity).Name;

    public Task<bool> HasDataAsync(IdentityDbContext context, CancellationToken cancellationToken = default)
    {
        return context.Set<TEntity>().AnyAsync(cancellationToken);
    }

    public abstract Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default);
}