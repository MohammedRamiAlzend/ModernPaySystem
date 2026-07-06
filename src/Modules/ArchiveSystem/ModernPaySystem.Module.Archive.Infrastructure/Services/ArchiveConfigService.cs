using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using System.IO;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveConfigService(
    IArchiveUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ArchiveConfigService> logger) : IArchiveConfigService
{
    public async Task<Result<ArchiveConfigDto>> GetAsync()
    {
        try
        {
            var configResult = await unitOfWork.ArchiveConfigs.GetAsync(
                filter: x => x.IsActive,
                transform: q => q.OrderBy(x => x.Id));

            if (configResult.IsError || configResult.Value is null)
                return Error.NotFound("ArchiveConfigNotFound", "Archive configuration not found.");

            return configResult.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive config");
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<ArchiveConfigDto>> UpdateAsync(UpdateArchiveConfigDto dto)
    {
        try
        {
            var configResult = await unitOfWork.ArchiveConfigs.GetAsync(
                filter: x => x.IsActive,
                transform: q => q.OrderBy(x => x.Id));

            ArchiveConfig config;
            if (configResult.IsError || configResult.Value is null)
            {
                config = new ArchiveConfig
                {
                    Id = Guid.NewGuid(),
                    DefaultPath = dto.DefaultPath,
                    Description = dto.Description,
                    IsActive = dto.IsActive,
                    AllowedFileExtensions = dto.AllowedFileExtensions
                };
                var addResult = await unitOfWork.ArchiveConfigs.AddAsync(config);
                if (addResult.IsError)
                    return addResult.Errors;
            }
            else
            {
                config = configResult.Value;
                config.DefaultPath = dto.DefaultPath;
                config.Description = dto.Description;
                config.IsActive = dto.IsActive;
                config.AllowedFileExtensions = dto.AllowedFileExtensions;

                var updateResult = await unitOfWork.ArchiveConfigs.UpdateAsync(config);
                if (updateResult.IsError)
                    return updateResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return Error.Failure("DatabaseError", "A database error occurred.");

            var userId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Archive config updated by user {UserId}", userId);
            return config.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating archive config");
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<string[]>> GetSystemDrivesAsync()
    {
        try
        {
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady)
                .Select(d => d.Name)
                .ToArray();

            return drives;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving system drives");
            return Error.Failure("InternalServerError", "Failed to retrieve system drives.");
        }
    }

    public async Task<Result<string[]>> GetSubdirectoriesAsync(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return Error.Validation("InvalidPath", "Path cannot be empty.");

            var directories = Directory.GetDirectories(path)
                .OrderBy(d => d)
                .ToArray();

            return directories;
        }
        catch (UnauthorizedAccessException)
        {
            return Error.Forbidden("AccessDenied", "Access to the specified path is denied.");
        }
        catch (DirectoryNotFoundException)
        {
            return Error.NotFound("DirectoryNotFound", "The specified directory was not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving subdirectories for path {Path}", path);
            return Error.Failure("InternalServerError", "Failed to retrieve subdirectories.");
        }
    }
}
