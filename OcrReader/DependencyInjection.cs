using Microsoft.Extensions.DependencyInjection;

namespace OcrReader;

public static class DependencyInjection
{
    public static void AddOcrReader(this IServiceCollection services)
    {
        services.AddSingleton<IOcrGenerator, OcrGenerator>();
    }
}
