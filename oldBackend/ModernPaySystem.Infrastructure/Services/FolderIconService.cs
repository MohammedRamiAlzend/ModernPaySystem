using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Persistence.UnitOfWork;

namespace ModernPaySystem.Infrastructure.Services;

public class FolderIconService(
    IUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveResourceAuthorizationService resourceAuth,
    ILogger<FolderIconService> logger) : IFolderIconService
{
    public async Task<Result<List<FolderIconDto>>> GetAllAsync()
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetAllAsync(orderBy: q => q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name));
            if (result.IsError)
                return result.Errors;

            return result.Value!.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder icons");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderIconDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ApplicationErrors.FolderIconNotFound;

            return MapToDto(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder icon {IconId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderIconDto>> CreateAsync(CreateFolderIconDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(dto.SvgContent))
                return ApplicationErrors.InvalidInput;

            var icon = new FolderIcon
            {
                Name = dto.Name.Trim(),
                SvgContent = dto.SvgContent,
                IsDefault = dto.IsDefault
            };

            if (icon.IsDefault)
                await ClearDefaultFlagAsync();

            var addResult = await unitOfWork.FolderIcons.AddAsync(icon);
            if (addResult.IsError)
                return addResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ApplicationErrors.DatabaseError;

            return MapToDto(icon);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating folder icon");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderIconDto>> UpdateAsync(Guid id, UpdateFolderIconDto dto)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ApplicationErrors.FolderIconNotFound;

            var icon = result.Value;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                icon.Name = dto.Name.Trim();

            if (!string.IsNullOrWhiteSpace(dto.SvgContent))
                icon.SvgContent = dto.SvgContent;

            if (dto.IsDefault.HasValue && dto.IsDefault.Value && !icon.IsDefault)
            {
                await ClearDefaultFlagAsync();
                icon.IsDefault = true;
            }
            else if (dto.IsDefault.HasValue && !dto.IsDefault.Value)
            {
                icon.IsDefault = false;
            }

            var updateResult = await unitOfWork.FolderIcons.UpdateAsync(icon);
            if (updateResult.IsError)
                return updateResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ApplicationErrors.DatabaseError;

            return MapToDto(icon);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating folder icon {IconId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ApplicationErrors.FolderIconNotFound;

            var icon = result.Value;
            if (icon.IsDefault)
                return ApplicationErrors.CannotDeleteDefaultFolderIcon;

            var removeResult = await unitOfWork.FolderIcons.RemoveAsync(x => x.Id == id);
            if (removeResult.IsError)
                return removeResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            return saveResult > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting folder icon {IconId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<string>> GetIconSvgAsync(Guid id)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ApplicationErrors.FolderIconNotFound;

            return result.Value.SvgContent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder icon SVG {IconId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> AssignIconToFolderAsync(Guid folderId, Guid? iconId)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, folderId, AccessLevel.Write);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.FolderAccessDenied;

            var folderResult = await unitOfWork.Folders.GetAsync(x => x.Id == folderId, query => query.Include(x => x.Icon));
            if (folderResult.IsError)
                return folderResult.Errors;

            var folder = folderResult.Value;
            if (folder == null)
                return ApplicationErrors.FolderNotFound;

            if (iconId.HasValue)
            {
                var iconResult = await unitOfWork.FolderIcons.GetByIdAsync(iconId.Value);
                if (iconResult.IsError || iconResult.Value == null)
                    return ApplicationErrors.FolderIconNotFound;
            }

            folder.IconId = iconId;

            var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
            if (updateResult.IsError)
                return updateResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ApplicationErrors.DatabaseError;

            var dto = folder.ToDto();
            dto.IconId = folder.IconId;
            return dto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning icon to folder {FolderId}", folderId);
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task ClearDefaultFlagAsync()
    {
        var existingDefaults = await unitOfWork.FolderIcons.GetAllAsync(x => x.IsDefault);
        if (existingDefaults.IsSuccess && existingDefaults.Value != null)
        {
            foreach (var icon in existingDefaults.Value)
            {
                icon.IsDefault = false;
                await unitOfWork.FolderIcons.UpdateAsync(icon);
            }
        }
    }

    private static FolderIconDto MapToDto(FolderIcon icon) => new()
    {
        Id = icon.Id,
        Name = icon.Name,
        SvgContent = icon.SvgContent,
        IsDefault = icon.IsDefault,
        CreatedByUserId = icon.CreatedByUserId,
        CreatedAt = icon.CreatedAt
    };
}
