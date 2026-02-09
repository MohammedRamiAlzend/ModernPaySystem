using FileManager.Abstractions;
using FileManager.Models;

namespace FileManager.Core;

/// <summary>
/// Enhanced file manager implementation with Windows Explorer-like functionality.
/// </summary>
public class EnhancedFileManager : IFileManager
{
    private readonly string[] _defaultAllowedExtensions = {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".rtf", ".csv",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".svg", ".webp",
        ".mp3", ".wav", ".flac", ".aac", ".ogg",
        ".mp4", ".avi", ".mov", ".wmv", ".mkv", ".webm",
        ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2",
        ".exe", ".dll", ".bat", ".sh",
        ".json", ".xml", ".html", ".htm", ".css", ".js", ".ts",
        ".sql", ".db", ".mdb", ".accdb"
    };

    public string RootDirectory { get; }

    public EnhancedFileManager(string? rootDirectory = null)
    {
        RootDirectory = rootDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (!Directory.Exists(RootDirectory))
        {
            Directory.CreateDirectory(RootDirectory);
        }
    }

    #region File Operations

    public async Task<FileOperationResult> CopyFileAsync(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                return new FileOperationResult(false, $"Source file does not exist: {sourcePath}", destinationPath);
            }

            var destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(sourcePath, destinationPath, overwrite);

            var fileInfo = new FileInfo(destinationPath);
            return new FileOperationResult(true, null, destinationPath)
            {
                FileSize = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            return new FileOperationResult(false, ex.Message, destinationPath);
        }
    }

    public async Task<FileOperationResult> MoveFileAsync(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (!File.Exists(sourcePath))
            {
                return new FileOperationResult(false, $"Source file does not exist: {sourcePath}", destinationPath);
            }

            var destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            if (File.Exists(destinationPath) && !overwrite)
            {
                return new FileOperationResult(false, $"Destination file already exists: {destinationPath}", destinationPath);
            }

            if (File.Exists(destinationPath) && overwrite)
            {
                File.Delete(destinationPath);
            }

            File.Move(sourcePath, destinationPath);

            var fileInfo = new FileInfo(destinationPath);
            return new FileOperationResult(true, null, destinationPath)
            {
                FileSize = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            return new FileOperationResult(false, ex.Message, destinationPath);
        }
    }

    public async Task<FileOperationResult> RenameFileAsync(string currentPath, string newName)
    {
        try
        {
            if (!File.Exists(currentPath))
            {
                return new FileOperationResult(false, $"File does not exist: {currentPath}", currentPath);
            }

            var directory = Path.GetDirectoryName(currentPath);
            var newPath = Path.Combine(directory ?? string.Empty, newName);

            if (File.Exists(newPath))
            {
                return new FileOperationResult(false, $"File with name already exists: {newPath}", newPath);
            }

            File.Move(currentPath, newPath);

            var fileInfo = new FileInfo(newPath);
            return new FileOperationResult(true, null, newPath)
            {
                FileSize = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            return new FileOperationResult(false, ex.Message, currentPath);
        }
    }

    public async Task<FileOperationResult> DeleteFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new FileOperationResult(true, null, filePath); // Already deleted
            }

            File.Delete(filePath);
            return new FileOperationResult(true, null, filePath);
        }
        catch (Exception ex)
        {
            return new FileOperationResult(false, ex.Message, filePath);
        }
    }

    public async Task<FileContentResult> ReadFileAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new FileContentResult(false, null, $"File does not exist: {filePath}", filePath);
            }

            var bytes = await File.ReadAllBytesAsync(filePath);
            return new FileContentResult(true, bytes, null, filePath);
        }
        catch (Exception ex)
        {
            return new FileContentResult(false, null, ex.Message, filePath);
        }
    }

    public async Task<FileOperationResult> WriteFileAsync(string filePath, byte[] content, bool createDirectory = true)
    {
        try
        {
            if (createDirectory)
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            await File.WriteAllBytesAsync(filePath, content);

            var fileInfo = new FileInfo(filePath);
            return new FileOperationResult(true, null, filePath)
            {
                FileSize = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            return new FileOperationResult(false, ex.Message, filePath);
        }
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public async Task<FileInfoResult> GetFileInfoAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new FileInfoResult(false, $"File does not exist: {filePath}")
                {
                    Exists = false
                };
            }

            var fileInfo = new FileInfo(filePath);
            var extension = fileInfo.Extension.ToLower();

            return new FileInfoResult(true)
            {
                Name = fileInfo.Name,
                FullName = fileInfo.FullName,
                Directory = fileInfo.DirectoryName,
                Extension = fileInfo.Extension,
                Size = fileInfo.Length,
                CreationTime = fileInfo.CreationTime,
                LastAccessTime = fileInfo.LastAccessTime,
                LastWriteTime = fileInfo.LastWriteTime,
                Exists = true,
                ContentType = GetContentType(fileInfo.Extension),
                FileType = GetFileTypeFromExtension(fileInfo.Extension)
            };
        }
        catch (Exception ex)
        {
            return new FileInfoResult(false, ex.Message);
        }
    }

    #endregion

    #region Directory Operations

    public async Task<DirectoryOperationResult> CreateDirectoryAsync(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                return new DirectoryOperationResult(true, null, directoryPath);
            }

            Directory.CreateDirectory(directoryPath);
            return new DirectoryOperationResult(true, null, directoryPath);
        }
        catch (Exception ex)
        {
            return new DirectoryOperationResult(false, ex.Message, directoryPath);
        }
    }

    public async Task<DirectoryOperationResult> CopyDirectoryAsync(string sourcePath, string destinationPath, bool overwrite = false)
    {
        try
        {
            if (!Directory.Exists(sourcePath))
            {
                return new DirectoryOperationResult(false, $"Source directory does not exist: {sourcePath}", destinationPath);
            }

            if (!overwrite && Directory.Exists(destinationPath))
            {
                return new DirectoryOperationResult(false, $"Destination directory already exists: {destinationPath}", destinationPath);
            }

            if (Directory.Exists(destinationPath) && overwrite)
            {
                await DeleteDirectoryAsync(destinationPath, true);
            }

            Directory.CreateDirectory(destinationPath);

            var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            var copiedCount = 0;

            foreach (var file in files)
            {
                var relativePath = Path.GetRelativePath(sourcePath, file);
                var destFile = Path.Combine(destinationPath, relativePath);

                var destDir = Path.GetDirectoryName(destFile);
                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                File.Copy(file, destFile, overwrite);
                copiedCount++;
            }

            return new DirectoryOperationResult(true, null, destinationPath, copiedCount);
        }
        catch (Exception ex)
        {
            return new DirectoryOperationResult(false, ex.Message, destinationPath);
        }
    }

    public async Task<DirectoryOperationResult> MoveDirectoryAsync(string sourcePath, string destinationPath)
    {
        try
        {
            if (!Directory.Exists(sourcePath))
            {
                return new DirectoryOperationResult(false, $"Source directory does not exist: {sourcePath}", destinationPath);
            }

            if (Directory.Exists(destinationPath))
            {
                return new DirectoryOperationResult(false, $"Destination directory already exists: {destinationPath}", destinationPath);
            }

            Directory.Move(sourcePath, destinationPath);
            return new DirectoryOperationResult(true, null, destinationPath, 0);
        }
        catch (Exception ex)
        {
            return new DirectoryOperationResult(false, ex.Message, destinationPath);
        }
    }

    public async Task<DirectoryOperationResult> RenameDirectoryAsync(string currentPath, string newName)
    {
        try
        {
            if (!Directory.Exists(currentPath))
            {
                return new DirectoryOperationResult(false, $"Directory does not exist: {currentPath}", currentPath);
            }

            var parentDir = Path.GetDirectoryName(currentPath);
            var newPath = Path.Combine(parentDir ?? string.Empty, newName);

            if (Directory.Exists(newPath))
            {
                return new DirectoryOperationResult(false, $"Directory with name already exists: {newPath}", newPath);
            }

            Directory.Move(currentPath, newPath);
            return new DirectoryOperationResult(true, null, newPath, 0);
        }
        catch (Exception ex)
        {
            return new DirectoryOperationResult(false, ex.Message, currentPath);
        }
    }

    public async Task<DirectoryOperationResult> DeleteDirectoryAsync(string directoryPath, bool recursive = false)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return new DirectoryOperationResult(true, null, directoryPath); // Already deleted
            }

            Directory.Delete(directoryPath, recursive);
            return new DirectoryOperationResult(true, null, directoryPath);
        }
        catch (Exception ex)
        {
            return new DirectoryOperationResult(false, ex.Message, directoryPath);
        }
    }

    public bool DirectoryExists(string directoryPath)
    {
        return Directory.Exists(directoryPath);
    }

    public async Task<DirectoryInfoResult> GetDirectoryInfoAsync(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return new DirectoryInfoResult(false, $"Directory does not exist: {directoryPath}")
                {
                    Exists = false
                };
            }

            var dirInfo = new DirectoryInfo(directoryPath);
            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            var subdirs = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories);

            long totalSize = 0;
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
            }

            return new DirectoryInfoResult(true)
            {
                Name = dirInfo.Name,
                FullName = dirInfo.FullName,
                CreationTime = dirInfo.CreationTime,
                LastAccessTime = dirInfo.LastAccessTime,
                LastWriteTime = dirInfo.LastWriteTime,
                Exists = true,
                FileCount = files.Length,
                DirectoryCount = subdirs.Length,
                TotalSize = totalSize
            };
        }
        catch (Exception ex)
        {
            return new DirectoryInfoResult(false, ex.Message);
        }
    }

    #endregion

    #region Search and Listing

    public async Task<ListingResult> ListDirectoryAsync(string directoryPath, string? searchPattern = null, bool includeSubdirectories = false)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return new ListingResult(false, $"Directory does not exist: {directoryPath}", directoryPath);
            }

            var result = new ListingResult(true, null, directoryPath);
            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            searchPattern = string.IsNullOrEmpty(searchPattern) ? "*" : searchPattern;

            // Get files
            var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                result.Items.Add(new FileSystemItem
                {
                    Name = fileInfo.Name,
                    FullName = fileInfo.FullName,
                    Extension = fileInfo.Extension,
                    Size = fileInfo.Length,
                    IsDirectory = false,
                    CreationTime = fileInfo.CreationTime,
                    LastAccessTime = fileInfo.LastAccessTime,
                    LastWriteTime = fileInfo.LastWriteTime,
                    ContentType = GetContentType(fileInfo.Extension),
                    FileType = GetFileTypeFromExtension(fileInfo.Extension)
                });
            }

            // Get directories
            var directories = Directory.GetDirectories(directoryPath, searchPattern, searchOption);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                result.Items.Add(new FileSystemItem
                {
                    Name = dirInfo.Name,
                    FullName = dirInfo.FullName,
                    IsDirectory = true,
                    CreationTime = dirInfo.CreationTime,
                    LastAccessTime = dirInfo.LastAccessTime,
                    LastWriteTime = dirInfo.LastWriteTime
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            return new ListingResult(false, ex.Message, directoryPath);
        }
    }

    public async Task<SearchResult> SearchFilesAsync(string searchPath, FileSearchCriteria searchCriteria, SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        try
        {
            if (!Directory.Exists(searchPath))
            {
                return new SearchResult(false, $"Search path does not exist: {searchPath}");
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = new SearchResult(true);

            var searchPattern = string.IsNullOrEmpty(searchCriteria.FileNamePattern) ? "*" : searchCriteria.FileNamePattern;
            if (!searchPattern.Contains("*") && !searchPattern.Contains("?"))
            {
                searchPattern = $"*{searchPattern}*";
            }

            var files = Directory.GetFiles(searchPath, searchPattern, searchOption);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                // Apply filters
                if (!string.IsNullOrEmpty(searchCriteria.FileExtension) &&
                    !fileInfo.Extension.Equals(searchCriteria.FileExtension, searchCriteria.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (searchCriteria.MinSize.HasValue && fileInfo.Length < searchCriteria.MinSize.Value)
                {
                    continue;
                }

                if (searchCriteria.MaxSize.HasValue && fileInfo.Length > searchCriteria.MaxSize.Value)
                {
                    continue;
                }

                if (searchCriteria.ModifiedAfter.HasValue && fileInfo.LastWriteTime < searchCriteria.ModifiedAfter.Value)
                {
                    continue;
                }

                if (searchCriteria.ModifiedBefore.HasValue && fileInfo.LastWriteTime > searchCriteria.ModifiedBefore.Value)
                {
                    continue;
                }

                result.FoundItems.Add(new FileSystemItem
                {
                    Name = fileInfo.Name,
                    FullName = fileInfo.FullName,
                    Extension = fileInfo.Extension,
                    Size = fileInfo.Length,
                    IsDirectory = false,
                    CreationTime = fileInfo.CreationTime,
                    LastAccessTime = fileInfo.LastAccessTime,
                    LastWriteTime = fileInfo.LastWriteTime,
                    ContentType = GetContentType(fileInfo.Extension),
                    FileType = GetFileTypeFromExtension(fileInfo.Extension)
                });
            }

            stopwatch.Stop();
            result.TotalMatches = result.FoundItems.Count;
            result.SearchDuration = stopwatch.Elapsed;

            return result;
        }
        catch (Exception ex)
        {
            return new SearchResult(false, ex.Message);
        }
    }

    #endregion

    #region Utilities

    public string GetContentType(string fileExtension)
    {
        var extension = fileExtension.ToLower().TrimStart('.');
        return extension switch
        {
            "pdf" => "application/pdf",
            "doc" => "application/msword",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xls" => "application/vnd.ms-excel",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ppt" => "application/vnd.ms-powerpoint",
            "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "txt" => "text/plain",
            "rtf" => "application/rtf",
            "csv" => "text/csv",
            "json" => "application/json",
            "xml" => "application/xml",
            "html" or "htm" => "text/html",
            "css" => "text/css",
            "js" => "application/javascript",
            "ts" => "application/typescript",
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "bmp" => "image/bmp",
            "tiff" or "tif" => "image/tiff",
            "svg" => "image/svg+xml",
            "webp" => "image/webp",
            "mp3" => "audio/mpeg",
            "wav" => "audio/wav",
            "flac" => "audio/flac",
            "aac" => "audio/aac",
            "ogg" => "audio/ogg",
            "mp4" => "video/mp4",
            "avi" => "video/x-msvideo",
            "mov" => "video/quicktime",
            "wmv" => "video/x-ms-wmv",
            "mkv" => "video/x-matroska",
            "webm" => "video/webm",
            "zip" => "application/zip",
            "rar" => "application/x-rar-compressed",
            "7z" => "application/x-7z-compressed",
            "tar" => "application/x-tar",
            "gz" => "application/gzip",
            "bz2" => "application/x-bzip2",
            "exe" => "application/x-executable",
            "dll" => "application/x-msdownload",
            "bat" => "application/x-batch",
            "sh" => "application/x-shellscript",
            "sql" => "application/sql",
            "db" => "application/vnd.sqlite3",
            _ => "application/octet-stream"
        };
    }

    public string GetFileTypeFromExtension(string fileExtension)
    {
        var extension = fileExtension.ToLower().TrimStart('.');
        return extension switch
        {
            "pdf" => "Document",
            "doc" or "docx" => "Document",
            "xls" or "xlsx" => "Spreadsheet",
            "ppt" or "pptx" => "Presentation",
            "txt" or "rtf" or "csv" => "Text",
            "json" or "xml" or "html" or "htm" or "css" or "js" or "ts" or "sql" => "Code/Data",
            "jpg" or "jpeg" or "png" or "gif" or "bmp" or "tiff" or "tif" or "svg" or "webp" => "Image",
            "mp3" or "wav" or "flac" or "aac" or "ogg" => "Audio",
            "mp4" or "avi" or "mov" or "wmv" or "mkv" or "webm" => "Video",
            "zip" or "rar" or "7z" or "tar" or "gz" or "bz2" => "Archive",
            "exe" or "dll" or "bat" or "sh" => "Executable",
            "db" or "mdb" or "accdb" => "Database",
            _ => "Other"
        };
    }

    public bool IsValidFileExtension(string fileExtension, IEnumerable<string>? allowedExtensions = null)
    {
        var extensions = allowedExtensions?.ToList() ?? _defaultAllowedExtensions.ToList();
        var extension = fileExtension.ToLower().TrimStart('.');
        return extensions.Contains($".{extension}");
    }

    public string GenerateSafeFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

        var safeName = string.Join("_", fileNameWithoutExtension.Split(Path.GetInvalidFileNameChars()))
            .Replace(" ", "_")
            .Replace("__", "_")
            .Trim('_');

        if (safeName.Length > 100)
        {
            safeName = safeName.Substring(0, 100);
        }

        var guid = Guid.NewGuid().ToString("N")[..8]; // First 8 characters of GUID
        return $"{safeName}_{guid}{extension}";
    }

    public async Task<CleanupResult> CleanupOldFilesAsync(int olderThanDays = 30, string? cleanupPattern = null)
    {
        var cleanupResult = new CleanupResult();
        var cutoffDate = DateTime.Now.AddDays(-olderThanDays);

        try
        {
            if (!Directory.Exists(RootDirectory))
            {
                return cleanupResult;
            }

            var pattern = string.IsNullOrEmpty(cleanupPattern) ? "*.*" : cleanupPattern;
            var files = Directory.GetFiles(RootDirectory, pattern, SearchOption.AllDirectories)
                .Where(f => File.GetLastWriteTime(f) < cutoffDate);

            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    File.Delete(file);
                    cleanupResult.FilesDeleted++;
                    cleanupResult.BytesFreed += fileInfo.Length;
                }
                catch (Exception ex)
                {
                    cleanupResult.Errors.Add($"Failed to delete {file}: {ex.Message}");
                }
            }

            return cleanupResult;
        }
        catch (Exception ex)
        {
            cleanupResult.Errors.Add($"Cleanup failed: {ex.Message}");
            return cleanupResult;
        }
    }

    #endregion
}