namespace ModernPaySystem.Module.Archive.Infrastructure.Options;

public sealed class ArchiveRecordZipOptions
{
    public int GenerationTimeoutSeconds { get; set; } = 120;

    public long MaxTotalSizeBytes { get; set; } = 1L * 1024 * 1024 * 1024;

    public long MaxInlineDataSizeBytes { get; set; } = 5L * 1024 * 1024;

    public int CacheExpirationMinutes { get; set; } = 15;

    public string CacheDirectoryName { get; set; } = "ModernPaySystem-ArchiveZipCache";
}