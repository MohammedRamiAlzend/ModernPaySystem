using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class FolderService(IUnitOfWork unitOfWork, ILogger<FolderService> logger) : IFolderService
{
    public async Task<Result<IEnumerable<FolderDto>>> GetAllAsync()
    {
        try
        {
            var result = await unitOfWork.Folders.GetAllAsync();
            if (result.IsError)
            {
                return result.Errors;
            }

            // Return a flat list of all folders directly to avoid missing subfolders 
            // and eliminate reliance on EF Core lazy loading or relationship fix-up for SubFolders.
            return result.Value!.Select(x => new FolderDto
            {
                Id = x.Id,
                Name = x.Name,
                Level = x.Level,
                ParentId = x.ParentId,
                FolderDtos = [], // Frontend will handle flat structure
                CreatedByUserId = x.CreatedByUserId,
                CreatedAt = x.CreatedAt,
                UpdatedByUserId = x.UpdatedByUserId,
                UpdatedAt = x.UpdatedAt
            }).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folders");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> GetByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var result = await unitOfWork.Folders.GetAsync(x => x.Id == id, query => query.Include(x => x.Parent).Include(x => x.SubFolders));
            if (result.IsError)
            {
                return result.Errors;
            }

            if (result.Value == null)
            {
                return ApplicationErrors.FolderNotFound;
            }

            return result.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder by id {FolderId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> CreateAsync(CreateFolderDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return ApplicationErrors.InvalidInput;
            }

            if (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty)
            {
                var parent = await unitOfWork.Folders.GetByIdAsync(dto.ParentId.Value);
                if (parent.IsError)
                {
                    return parent.Errors;
                }

                if (parent.Value == null)
                {
                    return ApplicationErrors.FolderNotFound;
                }
            }

            var exists = await unitOfWork.Folders.AnyAsync(x => x.Name == dto.Name && x.ParentId == dto.ParentId);
            if (exists)
            {
                return ApplicationErrors.FolderAlreadyExists;
            }

            var folder = new Folder
            {
                Name = dto.Name.Trim(),
                ParentId = dto.ParentId,
                Level = await ResolveFolderLevelAsync(dto.ParentId)
            };

            var addResult = await unitOfWork.Folders.AddAsync(folder);
            if (addResult.IsError)
            {
                return addResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            return folder.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating folder");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> UpdateAsync(Guid id, UpdateFolderDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return ApplicationErrors.InvalidInput;
            }

            var folderResult = await unitOfWork.Folders.GetAsync(x => x.Id == id, query => query.Include(x => x.SubFolders));
            if (folderResult.IsError)
            {
                return folderResult.Errors;
            }

            var folder = folderResult.Value;
            if (folder == null)
            {
                return ApplicationErrors.FolderNotFound;
            }

            folder.Name = dto.Name.Trim();

            var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            return folder.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating folder {FolderId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }
    public async Task<Result<FolderDto>> MoveFolderAsync(Guid folderId, Guid destinationFolderId)
    {
        try
        {
            if (folderId == Guid.Empty || destinationFolderId == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }
            var folderResult = await unitOfWork.Folders.GetAsync(x => x.Id == folderId, query => query.Include(x => x.SubFolders).Include(x => x.ArchiveRecords));
            if (folderResult.IsError)
            {
                return folderResult.Errors;
            }
            var folder = folderResult.Value;
            if (folder == null)
            {
                return ApplicationErrors.FolderNotFound;
            }

            var destFolder = await unitOfWork.Folders.GetByIdAsync(destinationFolderId);
            if (destFolder.IsError)
            {
                return destFolder.Errors;
            }
            if (destFolder.Value == null)
            {
                return ApplicationErrors.FolderNotFound;
            }
            if (destFolder.Value.Id == folderId || await WouldCreateCircularReferenceAsync(folderId, destinationFolderId))
            {
                return ApplicationErrors.InvalidInput;
            }

            if (await AreArchiveNumbersUniqueBetweenFolderTrees(folderId, destinationFolderId) == false)
            {
                return ApplicationErrors.ArchiveRecordArchivalNumberAlreadyInUse;
            }


            var previousLevel = folder.Level;
            folder.ParentId = destinationFolderId;
            folder.Level = await ResolveFolderLevelAsync(destinationFolderId);
            var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }
            if (folder.Level != previousLevel)
            {
                await UpdateDescendantLevelsAsync(folder.Id, folder.Level);
            }
            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }
            return folder.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error moving folder {FolderId}", folderId);
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task<int> ResolveFolderLevelAsync(Guid? parentId)
    {
        if (!parentId.HasValue || parentId.Value == Guid.Empty)
        {
            return 0;
        }

        var parentResult = await unitOfWork.Folders.GetByIdAsync(parentId.Value);
        if (parentResult.IsError || parentResult.Value == null)
        {
            return 0;
        }

        return parentResult.Value.Level + 1;
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(Guid folderId, Guid newParentId)
    {
        var currentParentId = newParentId;
        var visited = new HashSet<Guid>();

        while (currentParentId != Guid.Empty && visited.Add(currentParentId))
        {
            if (currentParentId == folderId)
            {
                return true;
            }

            var parentResult = await unitOfWork.Folders.GetByIdAsync(currentParentId);
            if (parentResult.IsError || parentResult.Value == null || !parentResult.Value.ParentId.HasValue)
            {
                return false;
            }

            currentParentId = parentResult.Value.ParentId.Value;
        }

        return false;
    }


    private async Task<bool> AreArchiveNumbersUniqueBetweenFolderTrees(Guid sourceFolderId, Guid destinationFolderId)
    {
        var sourceArchiveNumbersResult = await GetArchiveNumbersInFolderTreeAsync(sourceFolderId);
        if (sourceArchiveNumbersResult == null)
        {
            return false;
        }

        var destinationArchiveNumbersResult = await GetArchiveNumbersInFolderTreeAsync(destinationFolderId);
        if (destinationArchiveNumbersResult == null)
        {
            return false;
        }

        return sourceArchiveNumbersResult.Overlaps(destinationArchiveNumbersResult) == false;
    }

    private async Task<HashSet<string>?> GetArchiveNumbersInFolderTreeAsync(Guid folderId)
    {
        var archiveNumbers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visitedFolders = new HashSet<Guid>();

        var collected = await CollectArchiveNumbersInFolderTreeAsync(folderId, archiveNumbers, visitedFolders);
        return collected ? archiveNumbers : null;
    }

    private async Task<bool> CollectArchiveNumbersInFolderTreeAsync(Guid folderId, ISet<string> archiveNumbers, HashSet<Guid> visitedFolders)
    {
        if (!visitedFolders.Add(folderId))
        {
            return true;
        }

        var folderResult = await unitOfWork.Folders.GetAsync(x => x.Id == folderId, query => query.Include(x => x.ArchiveRecords).Include(x => x.SubFolders));
        if (folderResult.IsError || folderResult.Value == null)
        {
            return false;
        }

        foreach (var archiveRecord in folderResult.Value.ArchiveRecords)
        {
            if (!string.IsNullOrWhiteSpace(archiveRecord.ArchivalNumber))
            {
                archiveNumbers.Add(archiveRecord.ArchivalNumber.Trim());
            }
        }

        foreach (var childFolder in folderResult.Value.SubFolders)
        {
            if (!await CollectArchiveNumbersInFolderTreeAsync(childFolder.Id, archiveNumbers, visitedFolders))
            {
                return false;
            }
        }

        return true;
    }

    private async Task UpdateDescendantLevelsAsync(Guid parentId, int parentLevel)
    {
        var childrenResult = await unitOfWork.Folders.GetAllAsync(x => x.ParentId == parentId, query => query.Include(x => x.SubFolders));
        if (childrenResult.IsError || childrenResult.Value == null)
        {
            return;
        }

        foreach (var child in childrenResult.Value)
        {
            child.Level = parentLevel + 1;
            var updateResult = await unitOfWork.Folders.UpdateAsync(child);
            if (updateResult.IsError)
            {
                throw new InvalidOperationException("Failed to update folder depth levels.");
            }

            await UpdateDescendantLevelsAsync(child.Id, child.Level);
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var hasChildren = await unitOfWork.Folders.AnyAsync(x => x.ParentId == id);
            var hasRecords = await unitOfWork.ArchiveRecords.AnyAsync(x => x.FolderId == id);
            var hasPermissions = await unitOfWork.FolderPermissions.AnyAsync(x => x.FolderId == id);

            if (hasChildren || hasRecords || hasPermissions)
            {
                return ApplicationErrors.FolderHasChildren;
            }

            var removeResult = await unitOfWork.Folders.RemoveAsync(x => x.Id == id);
            if (removeResult.IsError)
            {
                return removeResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting folder {FolderId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }
}
