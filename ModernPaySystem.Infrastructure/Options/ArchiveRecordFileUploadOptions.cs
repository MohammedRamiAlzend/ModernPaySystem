namespace ModernPaySystem.Infrastructure.Options;

public sealed class ArchiveRecordFileUploadOptions
{
    public string[] AllowedExtensions { get; set; } =
    [
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif",
        ".zip", ".rar", ".7z"
    ];

    public long MaxFileSize { get; set; } = 100 * 1024 * 1024;

    public int RetryCount { get; set; } = 3;

    public int RetryDelayMilliseconds { get; set; } = 250;
}
