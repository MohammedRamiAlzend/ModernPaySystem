using FileManager.Models;

namespace FileManager.Abstractions;

/// <summary>
/// Interface for comprehensive file management operations similar to Windows Explorer.
/// </summary>
public interface IFileManager
{
    /// <summary>
    /// Gets the root directory for file operations.
    /// </summary>
    string RootDirectory { get; }

    #region File Operations
    
    /// <summary>
    /// Copies a file from source to destination
    /// </summary>
    /// <param name="sourcePath">Source file path</param>
    /// <param name="destinationPath">Destination file path</param>
    /// <param name="overwrite">Whether to overwrite if destination exists</param>
    /// <returns>Operation result</returns>
    Task<FileOperationResult> CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    /// Moves a file from source to destination
    /// </summary>
    /// <param name="sourcePath">Source file path</param>
    /// <param name="destinationPath">Destination file path</param>
    /// <param name="overwrite">Whether to overwrite if destination exists</param>
    /// <returns>Operation result</returns>
    Task<FileOperationResult> MoveFileAsync(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    /// Renames a file
    /// </summary>
    /// <param name="currentPath">Current file path</param>
    /// <param name="newName">New file name</param>
    /// <returns>Operation result</returns>
    Task<FileOperationResult> RenameFileAsync(string currentPath, string newName);

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="filePath">Path to the file to delete</param>
    /// <returns>Operation result</returns>
    Task<FileOperationResult> DeleteFileAsync(string filePath);

    /// <summary>
    /// Reads file content as bytes
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>File content as bytes</returns>
    Task<FileContentResult> ReadFileAsync(string filePath);

    /// <summary>
    /// Writes content to a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="content">File content as bytes</param>
    /// <param name="createDirectory">Whether to create parent directories if they don't exist</param>
    /// <returns>Operation result</returns>
    Task<FileOperationResult> WriteFileAsync(string filePath, byte[] content, bool createDirectory = true);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="filePath">Path to check</param>
    /// <returns>True if file exists, false otherwise</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Gets detailed information about a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>File information</returns>
    Task<FileInfoResult> GetFileInfoAsync(string filePath);

    #endregion

    #region Directory Operations

    /// <summary>
    /// Creates a directory
    /// </summary>
    /// <param name="directoryPath">Path of the directory to create</param>
    /// <returns>Operation result</returns>
    Task<DirectoryOperationResult> CreateDirectoryAsync(string directoryPath);

    /// <summary>
    /// Copies a directory and its contents recursively
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="destinationPath">Destination directory path</param>
    /// <param name="overwrite">Whether to overwrite if destination exists</param>
    /// <returns>Operation result</returns>
    Task<DirectoryOperationResult> CopyDirectoryAsync(string sourcePath, string destinationPath, bool overwrite = false);

    /// <summary>
    /// Moves a directory and its contents
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="destinationPath">Destination directory path</param>
    /// <returns>Operation result</returns>
    Task<DirectoryOperationResult> MoveDirectoryAsync(string sourcePath, string destinationPath);

    /// <summary>
    /// Renames a directory
    /// </summary>
    /// <param name="currentPath">Current directory path</param>
    /// <param name="newName">New directory name</param>
    /// <returns>Operation result</returns>
    Task<DirectoryOperationResult> RenameDirectoryAsync(string currentPath, string newName);

    /// <summary>
    /// Deletes a directory and optionally its contents
    /// </summary>
    /// <param name="directoryPath">Path to the directory to delete</param>
    /// <param name="recursive">Whether to delete contents recursively</param>
    /// <returns>Operation result</returns>
    Task<DirectoryOperationResult> DeleteDirectoryAsync(string directoryPath, bool recursive = false);

    /// <summary>
    /// Checks if a directory exists
    /// </summary>
    /// <param name="directoryPath">Path to check</param>
    /// <returns>True if directory exists, false otherwise</returns>
    bool DirectoryExists(string directoryPath);

    /// <summary>
    /// Gets detailed information about a directory
    /// </summary>
    /// <param name="directoryPath">Path to the directory</param>
    /// <returns>Directory information</returns>
    Task<DirectoryInfoResult> GetDirectoryInfoAsync(string directoryPath);

    #endregion

    #region Search and Listing

    /// <summary>
    /// Lists files and directories in a directory
    /// </summary>
    /// <param name="directoryPath">Directory path to list</param>
    /// <param name="searchPattern">Search pattern (e.g., "*.txt")</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories</param>
    /// <returns>List of file and directory information</returns>
    Task<ListingResult> ListDirectoryAsync(string directoryPath, string? searchPattern = null, bool includeSubdirectories = false);

    /// <summary>
    /// Searches for files matching criteria
    /// </summary>
    /// <param name="searchPath">Path to search in</param>
    /// <param name="searchCriteria">Search criteria</param>
    /// <param name="searchOption">Search options</param>
    /// <returns>Search results</returns>
    Task<SearchResult> SearchFilesAsync(string searchPath, FileSearchCriteria searchCriteria, SearchOption searchOption = SearchOption.TopDirectoryOnly);

    #endregion

    #region Utilities

    /// <summary>
    /// Gets the content type for a file extension
    /// </summary>
    /// <param name="fileExtension">File extension (with or without dot)</param>
    /// <returns>Content type string</returns>
    string GetContentType(string fileExtension);

    /// <summary>
    /// Gets the file type category from extension
    /// </summary>
    /// <param name="fileExtension">File extension (with or without dot)</param>
    /// <returns>File type category</returns>
    string GetFileTypeFromExtension(string fileExtension);

    /// <summary>
    /// Validates if a file extension is allowed
    /// </summary>
    /// <param name="fileExtension">File extension to validate</param>
    /// <param name="allowedExtensions">List of allowed extensions</param>
    /// <returns>True if allowed, false otherwise</returns>
    bool IsValidFileExtension(string fileExtension, IEnumerable<string>? allowedExtensions = null);

    /// <summary>
    /// Generates a safe filename
    /// </summary>
    /// <param name="originalFileName">Original filename</param>
    /// <returns>Safe filename</returns>
    string GenerateSafeFileName(string originalFileName);

    /// <summary>
    /// Cleans up temporary files older than specified days
    /// </summary>
    /// <param name="olderThanDays">Number of days</param>
    /// <param name="cleanupPattern">Pattern for files to clean up</param>
    /// <returns>Cleanup result</returns>
    Task<CleanupResult> CleanupOldFilesAsync(int olderThanDays = 30, string? cleanupPattern = null);

    #endregion
}