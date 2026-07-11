namespace ModernPaySystem.Module.Archive.Infrastructure.Options;

public class ServerSettings
{
    public const string SectionName = "ServerSettings";

    public bool ActivateSemanticSearch { get; set; } = false;
}
