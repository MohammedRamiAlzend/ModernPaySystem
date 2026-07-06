using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;

namespace ModernPaySystem.Module.Identity.Infrastructure.Persistence.DesignTime;

public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var bootPath = FindBootPath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(bootPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("IdentityConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=ModernPaySystemDb;Username=postgres;Password=0000";

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
        });

        return new IdentityDbContext(optionsBuilder.Options);
    }

    private static string FindBootPath()
    {
        var basePath = Directory.GetCurrentDirectory();
        
        // Try multiple possible paths to find the Boot project
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

        // Fallback to default
        return possiblePaths[0];
    }
}