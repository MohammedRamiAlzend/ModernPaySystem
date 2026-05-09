using Microsoft.Extensions.DependencyInjection;

namespace ModernPaySystem.Modules.Transactions.Infrastructure;

public static class TransactionsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionsModule(this IServiceCollection services)
    {
        return services;
    }
}
