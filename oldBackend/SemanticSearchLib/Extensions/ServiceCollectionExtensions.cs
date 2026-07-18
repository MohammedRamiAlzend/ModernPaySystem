using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SemanticSearchLib.Abstractions;
using SemanticSearchLib.Models;
using SemanticSearchLib.Services;

namespace SemanticSearchLib.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSemanticSearchLib(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmbeddingOptions>(configuration.GetSection(EmbeddingOptions.SectionName));
        services.Configure<SemanticSearchOptions>(configuration.GetSection(SemanticSearchOptions.SectionName));

        services.AddHttpClient<IEmbeddingProvider, OllamaEmbeddingProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmbeddingOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddScoped<IFileParser, FileParserService>();
        services.AddScoped<ITextChunker, TextChunkerService>();

        return services;
    }
}
