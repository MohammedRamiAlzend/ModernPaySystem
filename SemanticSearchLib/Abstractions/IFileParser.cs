namespace SemanticSearchLib.Abstractions;

public interface IFileParser
{
    Task<string> ParseAsync(Stream fileStream, string fileType, CancellationToken ct = default);

    bool SupportsFileType(string fileType);
}
