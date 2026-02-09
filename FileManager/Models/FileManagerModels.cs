using System.ComponentModel;

namespace FileManager.Models;

/// <summary>
/// Represents the result of a file operation.
/// </summary>
public class FileOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public FileOperationResult(bool success, string? errorMessage = null, string? filePath = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
        FilePath = filePath;
    }
}

/// <summary>
/// Represents the result of a directory operation.
/// </summary>
public class DirectoryOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DirectoryPath { get; set; }
    public int ItemsAffected { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public DirectoryOperationResult(bool success, string? errorMessage = null, string? directoryPath = null, int itemsAffected = 0)
    {
        Success = success;
        ErrorMessage = errorMessage;
        DirectoryPath = directoryPath;
        ItemsAffected = itemsAffected;
    }
}

/// <summary>
/// Represents file content result.
/// </summary>
public class FileContentResult
{
    public bool Success { get; set; }
    public byte[]? Content { get; set; }
    public string? ErrorMessage { get; set; }
    public string? FilePath { get; set; }
    public long FileSize { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public FileContentResult(bool success, byte[]? content = null, string? errorMessage = null, string? filePath = null)
    {
        Success = success;
        Content = content;
        ErrorMessage = errorMessage;
        FilePath = filePath;
        FileSize = content?.Length ?? 0;
    }
}

/// <summary>
/// Represents detailed file information.
/// </summary>
public class FileInfoResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Name { get; set; }
    public string? FullName { get; set; }
    public string? Directory { get; set; }
    public string? Extension { get; set; }
    public long Size { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public bool Exists { get; set; }
    public string? ContentType { get; set; }
    public string? FileType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public FileInfoResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Represents detailed directory information.
/// </summary>
public class DirectoryInfoResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Name { get; set; }
    public string? FullName { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public bool Exists { get; set; }
    public int FileCount { get; set; }
    public int DirectoryCount { get; set; }
    public long TotalSize { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public DirectoryInfoResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Represents the result of a directory listing operation.
/// </summary>
public class ListingResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<FileSystemItem> Items { get; set; } = new();
    public string? DirectoryPath { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ListingResult(bool success, string? errorMessage = null, string? directoryPath = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
        DirectoryPath = directoryPath;
    }
}

/// <summary>
/// Represents a file system item (file or directory).
/// </summary>
public class FileSystemItem
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastAccessTime { get; set; }
    public DateTime LastWriteTime { get; set; }
    public string? ContentType { get; set; }
    public string? FileType { get; set; }
}

/// <summary>
/// Represents the result of a file search operation
/// </summary>
public class SearchResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<FileSystemItem> FoundItems { get; set; } = new();
    public int TotalMatches { get; set; }
    public TimeSpan SearchDuration { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public SearchResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Represents search criteria for file search.
/// </summary>
public class FileSearchCriteria
{
    public string? FileNamePattern { get; set; }
    public string? FileExtension { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
    public DateTime? ModifiedAfter { get; set; }
    public DateTime? ModifiedBefore { get; set; }
    public List<string>? Tags { get; set; } = new();
    public bool CaseSensitive { get; set; } = false;
}

/// <summary>
/// Represents the result of a cleanup operation.
/// </summary>
public class CleanupResult
{
    public int FilesDeleted { get; set; }
    public long BytesFreed { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}