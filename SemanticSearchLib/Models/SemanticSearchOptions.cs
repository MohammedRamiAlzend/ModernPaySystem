namespace SemanticSearchLib.Models;

public class SemanticSearchOptions
{
    public const string SectionName = "SemanticSearch:Processing";

    public int MaxFileSizeMB { get; set; } = 100;
    public int ChunkSize { get; set; } = 512;
    public int ChunkOverlap { get; set; } = 50;
    public string[] AllowedExtensions { get; set; } = [".docx", ".xlsx", ".txt", ".md"];
}
