using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain;

namespace ModernPaySystem.Infrastructure.Persistence
{
    public class PaymentsDbContext : DbContext
    {
        public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

        public DbSet<Payment> Payments { get; set; } = default!;
    }
}
