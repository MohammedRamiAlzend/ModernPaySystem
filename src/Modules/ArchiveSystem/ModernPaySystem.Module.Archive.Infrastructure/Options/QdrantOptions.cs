namespace ModernPaySystem.Module.Archive.Infrastructure.Options;

public class QdrantOptions
{
    public const string SectionName = "Qdrant";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6334;
    public string CollectionName { get; set; } = "document_chunks";
    public string ApiKey { get; set; } = "";
    public bool UseTls { get; set; } = false;
}
