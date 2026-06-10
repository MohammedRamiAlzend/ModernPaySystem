namespace SemanticSearchLib.Models;

public class EmbeddingOptions
{
    public const string SectionName = "SemanticSearch:Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ModelName { get; set; } = "mxbai-embed-large";
    public int TimeoutSeconds { get; set; } = 30;
}
