using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class FolderIconService(
    IArchiveUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveResourceAuthorizationService resourceAuth,
    ILogger<FolderIconService> logger) : IFolderIconService
{
    public async Task<Result<List<FolderIconDto>>> GetAllAsync()
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetAllAsync(
                orderBy: q => q.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name));

            if (result.IsError)
                return result.Errors;

            return result.Value!.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder icons");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderIconDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ArchiveErrors.FolderIconNotFound;

            return MapToDto(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder icon {IconId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderIconDto>> CreateAsync(CreateFolderIconDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return ArchiveErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(dto.SvgContent))
                return ArchiveErrors.InvalidInput;

            var icon = new FolderIcon
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                SvgContent = dto.SvgContent,
                IsDefault = dto.IsDefault
            };

            if (icon.IsDefault)
                await ClearDefaultFlagAsync();

            var addResult = await unitOfWork.FolderIcons.AddAsync(icon);
            if (addResult.IsError)
                return addResult.Errors;

            await unitOfWork.SaveChangesAsync();

            var userId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Folder icon created: {IconName} by user {UserId}", icon.Name, userId);

            return MapToDto(icon);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating folder icon");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderIconDto>> UpdateAsync(Guid id, UpdateFolderIconDto dto)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ArchiveErrors.FolderIconNotFound;

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

            await unitOfWork.SaveChangesAsync();

            var userId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Folder icon updated: {IconId} by user {UserId}", id, userId);

            return MapToDto(icon);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating folder icon {IconId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ArchiveErrors.FolderIconNotFound;

            var icon = result.Value;
            if (icon.IsDefault)
                return ArchiveErrors.CannotDeleteDefaultFolderIcon;

            var removeResult = await unitOfWork.FolderIcons.RemoveAsync(x => x.Id == id);
            if (removeResult.IsError)
                return removeResult.Errors;

            await unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting folder icon {IconId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<string>> GetIconSvgAsync(Guid id)
    {
        try
        {
            var result = await unitOfWork.FolderIcons.GetByIdAsync(id);
            if (result.IsError || result.Value == null)
                return ArchiveErrors.FolderIconNotFound;

            return result.Value.SvgContent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder icon SVG {IconId}", id);
            return ArchiveErrors.InternalServerError;
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
                return ArchiveErrors.FolderAccessDenied;

            var folderResult = await unitOfWork.Folders.GetAsync(
                x => x.Id == folderId,
                query => query.Include(x => x.Icon));

            if (folderResult.IsError)
                return folderResult.Errors;

            var folder = folderResult.Value;
            if (folder == null)
                return ArchiveErrors.FolderNotFound;

            if (iconId.HasValue)
            {
                var iconResult = await unitOfWork.FolderIcons.GetByIdAsync(iconId.Value);
                if (iconResult.IsError || iconResult.Value == null)
                    return ArchiveErrors.FolderIconNotFound;
            }

            folder.IconId = iconId;

            var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Icon {IconId} assigned to folder {FolderId} by user {UserId}", iconId, folderId, userId);

            var dto = folder.ToDto();
            dto.IconId = folder.IconId;
            return dto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning icon to folder {FolderId}", folderId);
            return ArchiveErrors.InternalServerError;
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
