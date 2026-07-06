namespace ModernPaySystem.Module.Archive.Application.Interfaces;

public interface IArchiveConfigSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
