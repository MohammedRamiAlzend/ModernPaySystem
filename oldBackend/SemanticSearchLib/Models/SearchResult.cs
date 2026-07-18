namespace SemanticSearchLib.Models;

public class SearchResult
{
    public Guid DocumentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
}
