using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using ModernPaySystem.Domain.Commons;
using FileManager.Abstractions;

namespace FileManager.Services.Abstraction;

/// <summary>
/// File information returned by file operations.
/// </summary>
public class FileMetadata
{
    public string FilePath { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// Result of cleanup operation.
/// </summary>
public class CleanupResult
{
    public int FilesDeleted { get; set; }
    public long BytesFreed { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Interface for web-focused file management operations (backward compatibility)
/// </summary>
public interface IFilesManagerService
{
    /// <summary>
    /// Saves an uploaded file to the specified directory with a generated safe filename.
    /// </summary>
    /// <param name="file">The uploaded file.</param>
    /// <param name="subDirectory">Optional subdirectory within the uploads folder.</param>
    /// <param name="customFileName">Optional custom filename (will be made safe).</param>
    /// <returns>Result containing file information.</returns>
    Task<Result<FileMetadata>> SaveFileAsync(IFormFile file, string? subDirectory = null, string? customFileName = null);

    /// <summary>
    /// Retrieves a file as a byte array.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns>Result containing file bytes.</returns>
    Task<Result<byte[]>> GetFileBytesAsync(string filePath);

    /// <summary>
    /// Retrieves a file as a stream.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns>Result containing file stream.</returns>
    Task<Result<Stream>> GetFileStreamAsync(string filePath);

    /// <summary>
    /// Deletes a file from the filesystem.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<Success>> DeleteFileAsync(string filePath);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns>True if file exists, false otherwise.</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Gets file information.
    /// </summary>
    /// <param name="filePath">The full path to the file.</param>
    /// <returns>Result containing file information.</returns>
    Result<FileMetadata> GetFileInfo(string filePath);

    /// <summary>
    /// Gets the content type for a file extension.
    /// </summary>
    /// <param name="fileExtension">File extension (with or without dot).</param>
    /// <returns>Content type string.</returns>
    string GetContentType(string fileExtension);

    /// <summary>
    /// Gets the file type category from extension.
    /// </summary>
    /// <param name="fileExtension">File extension (with or without dot).</param>
    /// <returns>File type category.</returns>
    string GetFileTypeFromExtension(string fileExtension);

    /// <summary>
    /// Validates if a file extension is allowed.
    /// </summary>
    /// <param name="fileExtension">File extension to validate.</param>
    /// <param name="allowedExtensions">List of allowed extensions.</param>
    /// <returns>True if allowed, false otherwise.</returns>
    bool IsValidFileExtension(string fileExtension, string[]? allowedExtensions = null);

    /// <summary>
    /// Generates a safe filename.
    /// </summary>
    /// <param name="originalFileName">Original filename.</param>
    /// <returns>Safe filename.</returns>
    string GenerateSafeFileName(string originalFileName);

    /// <summary>
    /// Gets the uploads directory path.
    /// </summary>
    string UploadsDirectory { get; }

    /// <summary>
    /// Cleans up temporary files older than specified days.
    /// </summary>
    /// <param name="olderThanDays">Number of days.</param>
    /// <returns>Result with cleanup summary.</returns>
    Task<Result<CleanupResult>> CleanupOldFilesAsync(int olderThanDays = 30);
}
