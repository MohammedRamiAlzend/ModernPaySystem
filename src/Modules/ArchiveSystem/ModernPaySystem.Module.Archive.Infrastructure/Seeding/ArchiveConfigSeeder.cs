using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Archive.Infrastructure.Seeding;

public class ArchiveConfigSeeder(
    ArchiveDbContext dbContext,
    ILogger<ArchiveConfigSeeder> logger) : IArchiveConfigSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var hasConfig = await dbContext.ArchiveConfigs.AnyAsync(cancellationToken);
        if (hasConfig)
        {
            logger.LogInformation("Archive configuration already exists, skipping seed");
            return;
        }

        var config = new ArchiveConfig
        {
            Id = Guid.NewGuid(),
            DefaultPath = "Uploads",
            IsActive = true
        };

        dbContext.ArchiveConfigs.Add(config);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded default archive configuration with Id {ConfigId}", config.Id);
    }
}
