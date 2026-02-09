using FileManager.Abstractions;
using FileManager.Core;
using FileManager.Services;
using FileManager.Services.Abstraction;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Extensions;

/// <summary>
/// Extension methods for registering FileManager services
/// </summary>
public static class FileManagerServiceCollectionExtensions
{
    /// <summary>
    /// Adds FileManager services to the service collection
    /// </summary>
    /// <param name="services">IServiceCollection instance</param>
    /// <param name="rootDirectory">Root directory for file operations (optional, defaults to My Documents)</param>
    /// <returns>IServiceCollection for chaining</returns>
    public static IServiceCollection AddFileManager(this IServiceCollection services, string? rootDirectory = null)
    {
        services.AddSingleton<IFileManager>(provider => new EnhancedFileManager(rootDirectory));
        services.AddScoped<IFilesManagerService, FilesManagerService>();
        return services;
    }

    /// <summary>
    /// Adds FileManager services with custom configuration
    /// </summary>
    /// <param name="services">IServiceCollection instance</param>
    /// <param name="configureOptions">Action to configure FileManager options</param>
    /// <returns>IServiceCollection for chaining</returns>
    public static IServiceCollection AddFileManager(this IServiceCollection services, Action<FileManagerOptions> configureOptions)
    {
        var options = new FileManagerOptions();
        configureOptions(options);
        
        services.AddSingleton<IFileManager>(provider => new EnhancedFileManager(options.RootDirectory));
        services.AddScoped<IFilesManagerService, FilesManagerService>();
        return services;
    }
}

/// <summary>
/// Options for configuring the FileManager.
/// </summary>
public class FileManagerOptions
{
    /// <summary>
    /// Root directory for file operations.
    /// </summary>
    public string? RootDirectory { get; set; }

    /// <summary>
    /// Allowed file extensions.
    /// </summary>
    public List<string> AllowedExtensions { get; set; } = new();

    /// <summary>
    /// Maximum file size in bytes (default: 100MB).
    /// </summary>
    public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB

    /// <summary>
    /// Whether to enable automatic cleanup.
    /// </summary>
    public bool EnableAutoCleanup { get; set; } = false;

    /// <summary>
    /// Days after which files are considered old for cleanup.
    /// </summary>
    public int CleanupOlderThanDays { get; set; } = 30;
}