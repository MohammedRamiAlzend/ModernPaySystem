using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SemanticSearchLib.Abstractions;
using SemanticSearchLib.Exceptions;
using SemanticSearchLib.Models;

namespace SemanticSearchLib.Services;

public class OllamaEmbeddingProvider(
    HttpClient httpClient,
    IOptions<EmbeddingOptions> options,
    ILogger<OllamaEmbeddingProvider> logger) : IEmbeddingProvider
{
    private readonly EmbeddingOptions _options = options.Value;

    private const string TaskPrefix = "Represent this sentence for searching relevant passages: ";

    public int VectorDimensions => 1024;

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        try
        {
            var request = new OllamaEmbedRequest
            {
                Model = _options.ModelName,
                Prompt = $"{TaskPrefix}{text}"
            };

            var response = await httpClient.PostAsJsonAsync("/api/embeddings", request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken: ct);

            if (result?.Embedding is { Length: > 0 })
            {
                return result.Embedding;
            }

            if (result?.Embeddings is { Count: > 0 } && result.Embeddings[0].Length > 0)
            {
                return result.Embeddings[0];
            }

            throw new SemanticSearchException("Ollama returned an empty embedding.");
        }
        catch (Exception ex) when (ex is not SemanticSearchException)
        {
            logger.LogError(ex, "Failed to generate embedding for text of length {Length}", text.Length);
            throw new SemanticSearchException("Failed to generate embedding via Ollama.", ex);
        }
    }

    public async Task<List<float[]>> GenerateEmbeddingsBatchAsync(List<string> texts, CancellationToken ct = default)
    {
        var embeddings = new List<float[]>();
        const int batchSize = 10;

        for (int i = 0; i < texts.Count; i += batchSize)
        {
            var batch = texts.Skip(i).Take(batchSize).ToList();
            foreach (var text in batch)
            {
                var embedding = await GenerateEmbeddingAsync(text, ct);
                embeddings.Add(embedding);
            }
        }

        return embeddings;
    }

    private sealed class OllamaEmbedRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;
    }

    private sealed class OllamaEmbedResponse
    {
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }

        [JsonPropertyName("embeddings")]
        public List<float[]>? Embeddings { get; set; }
    }
}
