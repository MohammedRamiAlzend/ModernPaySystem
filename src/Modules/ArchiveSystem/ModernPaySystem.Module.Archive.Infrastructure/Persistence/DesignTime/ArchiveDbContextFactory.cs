using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Archive.Infrastructure.Persistence.DesignTime;

public class ArchiveDbContextFactory : IDesignTimeDbContextFactory<ArchiveDbContext>
{
    public ArchiveDbContext CreateDbContext(string[] args)
    {
        var bootPath = FindBootPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(bootPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("ArchiveConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=ModernPaySystemDb;Username=postgres;Password=0000";

        var optionsBuilder = new DbContextOptionsBuilder<ArchiveDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(ArchiveDbContext).Assembly.FullName);
        });

        return new ArchiveDbContext(optionsBuilder.Options);
    }

    private static string FindBootPath()
    {
        var basePath = Directory.GetCurrentDirectory();
        
        var possiblePaths = new[]
        {
            Path.GetFullPath(Path.Combine(basePath, "..", "..", "..", "..", "ModernPaySystem.Boot")),
            Path.GetFullPath(Path.Combine(basePath, "..", "..", "ModernPaySystem.Boot")),
            Path.GetFullPath(Path.Combine(basePath, "..", "ModernPaySystem.Boot")),
            Path.Combine(basePath, "ModernPaySystem.Boot"),
        };

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path) && File.Exists(Path.Combine(path, "appsettings.json")))
            {
                return path;
            }
        }

        return possiblePaths[0];
    }
}