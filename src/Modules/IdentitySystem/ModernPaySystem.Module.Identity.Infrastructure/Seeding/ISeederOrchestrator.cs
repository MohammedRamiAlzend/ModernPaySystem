namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding;

public interface ISeederOrchestrator
{
    Task SeedDatabaseAsync(CancellationToken cancellationToken = default);
}