using FileManager.Abstractions;
using FileManager.Models;
using FileManager.Services.Abstraction;
using Microsoft.AspNetCore.Http;
using ModernPaySystem.Domain.Commons;

namespace FileManager.Services;

/// <summary>
/// Backward compatible file manager service that wraps the enhanced file manager
/// for use with web applications
/// </summary>
public class FilesManagerService(IFileManager? fileManager = null) : IFilesManagerService
{
    private readonly IFileManager _fileManager = fileManager ?? new Core.EnhancedFileManager();
    private readonly string[] _defaultAllowedExtensions = {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif",
        ".zip", ".rar", ".7z"
    };

    public string UploadsDirectory => Path.Combine("Diwan", "Uploads");

    public string RootDirectory => _fileManager.RootDirectory;

    public async Task<Result<FileMetadata>> SaveFileAsync(IFormFile file, string? subDirectory = null, string? customFileName = null)
    {
        if (file == null || file.Length == 0)
        {
            return ApplicationErrors.FileOperationFailed("File is null or empty");
        }

        try
        {
            var uploadPath = UploadsDirectory;
            if (!string.IsNullOrEmpty(subDirectory))
            {
                uploadPath = Path.Combine(uploadPath, subDirectory);
            }

            if (!_fileManager.DirectoryExists(uploadPath))
            {
                await _fileManager.CreateDirectoryAsync(uploadPath);
            }

            var safeFileName = customFileName ?? GenerateSafeFileName(file.FileName);
            var fullFilePath = Path.Combine(uploadPath, safeFileName);

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                var fileContent = stream.ToArray();
                var result = await _fileManager.WriteFileAsync(fullFilePath, fileContent);
                
                if (!result.Success)
                {
                    return ApplicationErrors.FileOperationFailed(result.ErrorMessage ?? "Unknown error");
                }
            }

            var fileInfo = await _fileManager.GetFileInfoAsync(fullFilePath);

            var resultMetadata = new FileMetadata
            {
                FilePath = fullFilePath,
                OriginalFileName = file.FileName,
                StoredFileName = safeFileName,
                FileExtension = Path.GetExtension(file.FileName).ToLower(),
                FileSize = fileInfo.Size,
                ContentType = GetContentType(Path.GetExtension(file.FileName)),
                FileType = GetFileTypeFromExtension(Path.GetExtension(file.FileName)),
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime
            };

            return new Result<FileMetadata>(resultMetadata, null, true);
        }
        catch (Exception ex)
        {
            return ApplicationErrors.FileOperationFailed($"Failed to save file: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> GetFileBytesAsync(string filePath)
    {
        if (!_fileManager.FileExists(filePath))
        {
            return ApplicationErrors.FileNotFound(filePath);
        }

        var result = await _fileManager.ReadFileAsync(filePath);
        if (!result.Success)
        {
            return ApplicationErrors.FileOperationFailed(result.ErrorMessage ?? "Unknown error");
        }

        return result.Content!;
    }

    public async Task<Result<Stream>> GetFileStreamAsync(string filePath)
    {
        if (!_fileManager.FileExists(filePath))
        {
            return ApplicationErrors.FileNotFound(filePath);
        }

        try
        {
            var fileContentResult = await _fileManager.ReadFileAsync(filePath);
            if (!fileContentResult.Success)
            {
                return ApplicationErrors.FileOperationFailed(fileContentResult.ErrorMessage ?? "Unknown error");
            }

            var stream = new MemoryStream(fileContentResult.Content!);
            return stream;
        }
        catch (Exception ex)
        {
            return ApplicationErrors.FileOperationFailed($"Failed to open file stream: {ex.Message}");
        }
    }

    public async Task<Result<Success>> DeleteFileAsync(string filePath)
    {
        if (!_fileManager.FileExists(filePath))
        {
            return Result.Success;
        }

        var result = await _fileManager.DeleteFileAsync(filePath);
        if (!result.Success)
        {
            return ApplicationErrors.FileOperationFailed(result.ErrorMessage ?? "Unknown error");
        }

        return Result.Success;
    }

    public bool FileExists(string filePath)
    {
        return _fileManager.FileExists(filePath);
    }

    public Result<FileMetadata> GetFileInfo(string filePath)
    {
        if (!_fileManager.FileExists(filePath))
        {
            return ApplicationErrors.FileNotFound(filePath);
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            var extension = fileInfo.Extension.ToLower();

            var result = new FileMetadata
            {
                FilePath = filePath,
                OriginalFileName = fileInfo.Name,
                StoredFileName = fileInfo.Name,
                FileExtension = extension,
                FileSize = fileInfo.Length,
                ContentType = GetContentType(extension),
                FileType = GetFileTypeFromExtension(extension),
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime
            };

            return new Result<FileMetadata>(result, null, true);
        }
        catch (Exception ex)
        {
            return ApplicationErrors.FileOperationFailed($"Failed to get file info: {ex.Message}");
        }
    }

    public string GetContentType(string fileExtension)
    {
        return _fileManager.GetContentType(fileExtension);
    }

    public string GetFileTypeFromExtension(string fileExtension)
    {
        return _fileManager.GetFileTypeFromExtension(fileExtension);
    }

    public bool IsValidFileExtension(string fileExtension, string[]? allowedExtensions = null)
    {
        var extensions = allowedExtensions ?? _defaultAllowedExtensions;
        var extension = fileExtension.ToLower().TrimStart('.');
        return extensions.Contains($".{extension}");
    }

    public string GenerateSafeFileName(string originalFileName)
    {
        return _fileManager.GenerateSafeFileName(originalFileName);
    }

    public async Task<Result<Services.Abstraction.CleanupResult>> CleanupOldFilesAsync(int olderThanDays = 30)
    {
        var result = await _fileManager.CleanupOldFilesAsync(olderThanDays);
        var cleanupResult = new Services.Abstraction.CleanupResult
        {
            FilesDeleted = result.FilesDeleted,
            BytesFreed = result.BytesFreed,
            Errors = result.Errors
        };
        return cleanupResult;
    }
}