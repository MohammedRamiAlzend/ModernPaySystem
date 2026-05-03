using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ModernPaySystem.Infrastructure.Persistence.DesignTime;

/// <summary>
/// Design-time factory for creating <see cref="AppDbContext"/> instances used by EF tools.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        // var configuration = new ConfigurationBuilder()
        //     .SetBasePath(basePath)
        //     .AddJsonFile("appsettings.json", optional: true)
        //     .AddEnvironmentVariables()
        //     .Build();

        var connectionString = "Host=localhost;Database=ModernPaySystemDb;Username=postgres;Password=0000";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
        });

        return new AppDbContext(optionsBuilder.Options);
    }
}
