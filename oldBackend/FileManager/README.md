# Enhanced FileManager Library

The Enhanced FileManager is a comprehensive file management library that provides Windows Explorer-like functionality for .NET applications. It's designed to be reusable across different projects and supports both basic and advanced file operations.

## Features

- **File Operations**: Copy, move, rename, delete, read, write files
- **Directory Operations**: Create, copy, move, rename, delete directories
- **Search Capabilities**: Find files by name, extension, size, date, etc.
- **Listing**: Browse directory contents with filtering options
- **Metadata Management**: Access detailed file and directory information
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Web Compatible**: Includes backward compatibility for web file uploads

## Installation

To use the FileManager in your project, register the services:

```csharp
// In Program.cs or Startup.cs
builder.Services.AddFileManager(); // Basic registration

// Or with custom root directory
builder.Services.AddFileManager(@"C:\MyApp\Data");

// Or with custom configuration
builder.Services.AddFileManager(options =>
{
    options.RootDirectory = @"C:\MyApp\Data";
    options.MaxFileSize = 50 * 1024 * 1024; // 50MB
    options.AllowedExtensions = new List<string> { ".txt", ".pdf", ".jpg" };
});
```

## Usage Examples

### Basic File Operations

```csharp
public class MyService
{
    private readonly IFileManager _fileManager;

    public MyService(IFileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public async Task<FileOperationResult> CopyFile(string source, string destination)
    {
        return await _fileManager.CopyFileAsync(source, destination, overwrite: true);
    }

    public async Task<FileInfoResult> GetFileInfo(string filePath)
    {
        return await _fileManager.GetFileInfoAsync(filePath);
    }
}
```

### Directory Operations

```csharp
public async Task<DirectoryOperationResult> CreateAndPopulateDirectory(string dirPath)
{
    // Create directory
    var createResult = await _fileManager.CreateDirectoryAsync(dirPath);
    if (!createResult.Success) return createResult;

    // Write a file to the directory
    var content = System.Text.Encoding.UTF8.GetBytes("Hello, World!");
    var writeResult = await _fileManager.WriteFileAsync(
        Path.Combine(dirPath, "hello.txt"), 
        content
    );

    return new DirectoryOperationResult(writeResult.Success, writeResult.ErrorMessage, dirPath, 1);
}
```

### File Search

```csharp
public async Task<SearchResult> FindLargeImages(string searchPath)
{
    var criteria = new FileSearchCriteria
    {
        FileExtension = ".jpg",
        MinSize = 1024 * 1024, // 1MB
        ModifiedAfter = DateTime.Now.AddDays(-7) // Last week
    };

    return await _fileManager.SearchFilesAsync(
        searchPath, 
        criteria, 
        SearchOption.AllDirectories
    );
}
```

### Web File Upload (Backward Compatibility)

```csharp
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFilesManagerService _fileManager;

    public FilesController(IFilesManagerService fileManager)
    {
        _fileManager = fileManager;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty");

        var result = await _fileManager.SaveFileAsync(file, "uploads");
        
        if (result.IsError)
            return BadRequest(result.TopError.Description);

        return Ok(result.Value);
    }
}
```

## Architecture

The library consists of:

- `IFileManager`: Main interface with comprehensive file operations
- `EnhancedFileManager`: Full implementation of file explorer-like functionality
- `IFilesManagerService`: Web-focused interface (backward compatible)
- `FilesManagerService`: Implementation for web file uploads
- Various result models for operations
- Extension methods for easy DI registration

## Advanced Features

- **Bulk Operations**: Perform operations on multiple files/directories
- **Progress Tracking**: Monitor long-running operations
- **Custom Filters**: Define custom search and filtering criteria
- **Event Hooks**: Subscribe to file system events
- **Security**: Built-in validation and sanitization
- **Performance**: Optimized for large file operations

## Best Practices

1. Always check operation results before proceeding
2. Use appropriate SearchOptions for performance
3. Validate file extensions and sizes before operations
4. Handle exceptions appropriately
5. Use async methods for I/O operations
6. Clean up temporary files regularly using the cleanup feature

## Migration from Old Version

The new FileManager maintains backward compatibility with the old `IFilesManagerService` interface, so existing code will continue to work. However, you can gradually migrate to the new `IFileManager` interface to access additional functionality.