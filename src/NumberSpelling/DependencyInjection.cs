using Microsoft.Extensions.DependencyInjection;

namespace NumberSpelling;

public static class NumberSpellingServiceCollectionExtensions
{
    public static void AddNumberSpelling(this IServiceCollection services)
    {
        services.AddScoped<INumberSpellingService, NumberSpellingService>();
    }
}
