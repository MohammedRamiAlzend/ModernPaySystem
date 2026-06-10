namespace SemanticSearchLib.Abstractions;

public interface IEmbeddingProvider
{
    int VectorDimensions { get; }

    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);

    Task<List<float[]>> GenerateEmbeddingsBatchAsync(List<string> texts, CancellationToken ct = default);
}
