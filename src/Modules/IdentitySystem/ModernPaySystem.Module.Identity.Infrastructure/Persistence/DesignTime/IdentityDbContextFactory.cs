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
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (dir.Name == "ModernPaySystem.Boot" && File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
            {
                return dir.FullName;
            }
            
            var srcBoot = Path.Combine(dir.FullName, "src", "ModernPaySystem.Boot");
            if (Directory.Exists(srcBoot) && File.Exists(Path.Combine(srcBoot, "appsettings.json")))
            {
                return srcBoot;
            }

            var directBoot = Path.Combine(dir.FullName, "ModernPaySystem.Boot");
            if (Directory.Exists(directBoot) && File.Exists(Path.Combine(directBoot, "appsettings.json")))
            {
                return directBoot;
            }

            dir = dir.Parent;
        }

        return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "ModernPaySystem.Boot"));
    }
}