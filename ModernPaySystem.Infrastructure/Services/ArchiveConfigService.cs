using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveConfigService(
    IUnitOfWork unitOfWork,
    IArchiveAuthorizationService archiveAuthorizationService,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ArchiveConfigService> logger) : IArchiveConfigService
{
    public async Task<Result<ArchiveConfigDto>> GetAsync()
    {
        try
        {
            var config = await unitOfWork.Context.ArchiveConfigs
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (config == null)
                return ApplicationErrors.ArchiveConfigNotFound;

            return config.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching archive config");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveConfigDto>> UpdateAsync(UpdateArchiveConfigDto dto)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var leaderDepartmentsResult = await archiveAuthorizationService.GetUserArchiveLeaderDepartmentsAsync(userId);
            if (leaderDepartmentsResult.IsError)
                return leaderDepartmentsResult.Errors;

            var isArchiveLeader = leaderDepartmentsResult.Value?.Count > 0;
            if (!isArchiveLeader)
                return ApplicationErrors.ArchiveConfigUpdateNotAuthorized;

            var config = await unitOfWork.Context.ArchiveConfigs
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (config == null)
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
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Archive config updated by user {UserId}", userId);
            return config.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating archive config");
            return ApplicationErrors.InternalServerError;
        }
    }
}