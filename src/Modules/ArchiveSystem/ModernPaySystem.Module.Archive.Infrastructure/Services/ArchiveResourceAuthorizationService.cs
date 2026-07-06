using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveResourceAuthorizationService(
    ArchiveDbContext dbContext,
    ILogger<ArchiveResourceAuthorizationService> logger)
    : IArchiveResourceAuthorizationService
{
    public async Task<Result<bool>> CanAccessFolderAsync(Guid userId, Guid folderId, AccessLevel minimumLevel = AccessLevel.View)
    {
        if (userId == Guid.Empty)
            return Error.Validation("InvalidInput", "User ID is required.");

        if (folderId == Guid.Empty)
            return Error.Validation("InvalidInput", "Folder ID is required.");

        try
        {
            var folderExists = await dbContext.Folders.AnyAsync(f => f.Id == folderId);
            if (!folderExists)
                return ArchiveErrors.FolderNotFound;

            var userIdString = userId.ToString();

            var hasDirectPermission = await dbContext.FolderPermissions
                .AnyAsync(fp => fp.FolderId == folderId && fp.UserId == userIdString && (int)fp.AccessLevel >= (int)minimumLevel);

            if (hasDirectPermission)
                return true;

            var departmentId = await dbContext.Folders
                .Where(f => f.Id == folderId)
                .Select(f => f.DepartmentId)
                .FirstOrDefaultAsync();

            if (departmentId.HasValue)
            {
                var isArchiveLeader = await dbContext.DepartmentArchiveLeaders
                    .AnyAsync(dal => dal.DepartmentId == departmentId.Value && dal.UserId == userId);

                if (isArchiveLeader)
                    return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking folder access for user {UserId} on folder {FolderId}", userId, folderId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> CanAccessArchiveRecordAsync(Guid userId, Guid recordId, AccessLevel minimumLevel = AccessLevel.View)
    {
        if (userId == Guid.Empty)
            return Error.Validation("InvalidInput", "User ID is required.");

        if (recordId == Guid.Empty)
            return Error.Validation("InvalidInput", "Record ID is required.");

        try
        {
            var folderId = await dbContext.ArchiveRecords
                .Where(ar => ar.Id == recordId)
                .Select(ar => ar.FolderId)
                .FirstOrDefaultAsync();

            if (folderId == Guid.Empty)
                return ArchiveErrors.ArchiveRecordNotFound;

            return await CanAccessFolderAsync(userId, folderId, minimumLevel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking archive record access for user {UserId} on record {RecordId}", userId, recordId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> CanAccessPhysicalFileAsync(Guid userId, Guid fileId, AccessLevel minimumLevel = AccessLevel.View)
    {
        if (userId == Guid.Empty)
            return Error.Validation("InvalidInput", "User ID is required.");

        if (fileId == Guid.Empty)
            return Error.Validation("InvalidInput", "File ID is required.");

        try
        {
            var recordId = await dbContext.PhysicalFiles
                .Where(pf => pf.Id == fileId)
                .Select(pf => pf.ArchiveRecordId)
                .FirstOrDefaultAsync();

            if (recordId == Guid.Empty)
                return ArchiveErrors.AttachmentNotFound;

            var folderId = await dbContext.ArchiveRecords
                .Where(ar => ar.Id == recordId)
                .Select(ar => ar.FolderId)
                .FirstOrDefaultAsync();

            if (folderId == Guid.Empty)
                return ArchiveErrors.ArchiveRecordNotFound;

            return await CanAccessFolderAsync(userId, folderId, minimumLevel);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking physical file access for user {UserId} on file {FileId}", userId, fileId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<AccessLevel>> GetFolderAccessLevelAsync(Guid userId, Guid folderId)
    {
        if (userId == Guid.Empty)
            return Error.Validation("InvalidInput", "User ID is required.");

        if (folderId == Guid.Empty)
            return Error.Validation("InvalidInput", "Folder ID is required.");

        try
        {
            var userIdString = userId.ToString();
            var maxLevel = (AccessLevel)0;
            var currentFolderId = folderId;

            while (currentFolderId != Guid.Empty)
            {
                var folderLevel = await dbContext.FolderPermissions
                    .Where(fp => fp.FolderId == currentFolderId && fp.UserId == userIdString)
                    .Select(fp => (int)fp.AccessLevel)
                    .DefaultIfEmpty(0)
                    .MaxAsync();

                if (folderLevel > (int)maxLevel)
                    maxLevel = (AccessLevel)folderLevel;

                currentFolderId = await dbContext.Folders
                    .Where(f => f.Id == currentFolderId)
                    .Select(f => f.ParentId ?? Guid.Empty)
                    .FirstOrDefaultAsync();
            }

            return maxLevel;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting folder access level for user {UserId} on folder {FolderId}", userId, folderId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<List<Guid>>> GetAccessibleFolderIdsAsync(Guid userId, AccessLevel minimumLevel = AccessLevel.View)
    {
        if (userId == Guid.Empty)
            return Error.Validation("InvalidInput", "User ID is required.");

        try
        {
            var userIdString = userId.ToString();

            var folderIdsFromPermissions = await dbContext.FolderPermissions
                .Where(fp => fp.UserId == userIdString && (int)fp.AccessLevel >= (int)minimumLevel)
                .Select(fp => fp.FolderId)
                .ToListAsync();

            var folderIdsFromArchiveLeader = await dbContext.Folders
                .Where(f => f.DepartmentId != null)
                .Where(f => dbContext.DepartmentArchiveLeaders
                    .Any(dal => dal.DepartmentId == f.DepartmentId!.Value && dal.UserId == userId))
                .Select(f => f.Id)
                .ToListAsync();

            var result = folderIdsFromPermissions
                .Union(folderIdsFromArchiveLeader)
                .Distinct()
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting accessible folder IDs for user {UserId}", userId);
            return ArchiveErrors.InternalServerError;
        }
    }
}
