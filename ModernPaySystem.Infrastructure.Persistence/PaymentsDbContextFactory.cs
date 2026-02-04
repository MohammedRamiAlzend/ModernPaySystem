using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ModernPaySystem.Infrastructure.Persistence
{
    public class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
    {
        public PaymentsDbContext CreateDbContext(string[] args)
        {
            // Build a minimal configuration for design-time context
            var builder = new DbContextOptionsBuilder<PaymentsDbContext>();
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();

            var conn = config.GetConnectionString("PaymentsDb") ??
                       "Server=(localdb)\\mssqllocaldb;Database=ModernPaySystemDb;Trusted_Connection=True;";
            builder.UseSqlServer(conn);
            return new PaymentsDbContext(builder.Options);
        }
    }
}
