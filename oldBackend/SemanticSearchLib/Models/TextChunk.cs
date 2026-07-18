namespace SemanticSearchLib.Models;

public class TextChunk
{
    public string Content { get; set; } = string.Empty;
    public int Index { get; set; }
    public int TokenCount { get; set; }
}
