using ModernPaySystem.Module.Identity.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding;

public interface IEntitySeeder
{
    int Order { get; }

    string GetEntityName();

    Task<bool> HasDataAsync(IdentityDbContext context, CancellationToken cancellationToken = default);

    Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default);
}