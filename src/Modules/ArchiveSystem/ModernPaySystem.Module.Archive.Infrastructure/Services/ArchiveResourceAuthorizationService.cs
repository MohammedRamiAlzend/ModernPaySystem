using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveResourceAuthorizationService(
    ArchiveDbContext dbContext,
    IDepartmentService departmentService,
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
            var userIdString = userId.ToString();

            var hasDirectPermission = await dbContext.FolderPermissions
                .AnyAsync(fp => fp.FolderId == folderId && fp.UserId == userIdString && (int)fp.AccessLevel >= (int)minimumLevel);

            if (hasDirectPermission)
                return true;

            var userDeptResult = await departmentService.GetByUserIdAsync(userId);
            Guid? userDepartmentId = userDeptResult.Value?.Id;

            if (userDepartmentId.HasValue)
            {
                var hasDepartmentPermission = await dbContext.FolderPermissions
                    .AnyAsync(fp => fp.FolderId == folderId && fp.DepartmentId == userDepartmentId.Value && (int)fp.AccessLevel >= (int)minimumLevel);

                if (hasDepartmentPermission)
                    return true;
            }

            var currentFolderId = folderId;
            while (currentFolderId != Guid.Empty)
            {
                currentFolderId = await dbContext.Folders
                    .Where(f => f.Id == currentFolderId)
                    .Select(f => f.ParentId ?? Guid.Empty)
                    .FirstOrDefaultAsync();

                if (currentFolderId == Guid.Empty)
                    break;

                var hasInheritedUserPermission = await dbContext.FolderPermissions
                    .AnyAsync(fp => fp.FolderId == currentFolderId && fp.UserId == userIdString && fp.IsInherited && (int)fp.AccessLevel >= (int)minimumLevel);

                if (hasInheritedUserPermission)
                    return true;

                if (userDepartmentId.HasValue)
                {
                    var hasInheritedDeptPermission = await dbContext.FolderPermissions
                        .AnyAsync(fp => fp.FolderId == currentFolderId && fp.DepartmentId == userDepartmentId.Value && fp.IsInherited && (int)fp.AccessLevel >= (int)minimumLevel);

                    if (hasInheritedDeptPermission)
                        return true;
                }
            }

            var folderDeptId = await dbContext.Folders
                .Where(f => f.Id == folderId)
                .Select(f => f.DepartmentId)
                .FirstOrDefaultAsync();

            if (folderDeptId.HasValue)
            {
                var isArchiveLeader = await dbContext.DepartmentArchiveLeaders
                    .AnyAsync(dal => dal.DepartmentId == folderDeptId.Value && dal.UserId == userId);

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

            var userDeptResult = await departmentService.GetByUserIdAsync(userId);
            Guid? userDepartmentId = userDeptResult.Value?.Id;

            while (currentFolderId != Guid.Empty)
            {
                var userLevel = await dbContext.FolderPermissions
                    .Where(fp => fp.FolderId == currentFolderId && fp.UserId == userIdString)
                    .Select(fp => (int)fp.AccessLevel)
                    .DefaultIfEmpty(0)
                    .MaxAsync();

                if (userLevel > (int)maxLevel)
                    maxLevel = (AccessLevel)userLevel;

                if (userDepartmentId.HasValue)
                {
                    var deptLevel = await dbContext.FolderPermissions
                        .Where(fp => fp.FolderId == currentFolderId && fp.DepartmentId == userDepartmentId.Value)
                        .Select(fp => (int)fp.AccessLevel)
                        .DefaultIfEmpty(0)
                        .MaxAsync();

                    if (deptLevel > (int)maxLevel)
                        maxLevel = (AccessLevel)deptLevel;
                }

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

            var folderIdsFromDirectPermissions = await dbContext.FolderPermissions
                .Where(fp => fp.UserId == userIdString && (int)fp.AccessLevel >= (int)minimumLevel)
                .Select(fp => fp.FolderId)
                .ToListAsync();

            var userDeptResult = await departmentService.GetByUserIdAsync(userId);
            Guid? userDepartmentId = userDeptResult.Value?.Id;

            var folderIdsFromDeptPermissions = new List<Guid>();
            if (userDepartmentId.HasValue)
            {
                folderIdsFromDeptPermissions = await dbContext.FolderPermissions
                    .Where(fp => fp.DepartmentId == userDepartmentId.Value && (int)fp.AccessLevel >= (int)minimumLevel)
                    .Select(fp => fp.FolderId)
                    .ToListAsync();
            }

            var inheritedFolderIds = await dbContext.FolderPermissions
                .Where(fp => fp.UserId == userIdString && fp.IsInherited && (int)fp.AccessLevel >= (int)minimumLevel)
                .Select(fp => fp.FolderId)
                .ToListAsync();

            var inheritedDeptFolderIds = new List<Guid>();
            if (userDepartmentId.HasValue)
            {
                inheritedDeptFolderIds = await dbContext.FolderPermissions
                    .Where(fp => fp.DepartmentId == userDepartmentId.Value && fp.IsInherited && (int)fp.AccessLevel >= (int)minimumLevel)
                    .Select(fp => fp.FolderId)
                    .ToListAsync();
            }

            var allInheritedRootIds = inheritedFolderIds
                .Union(inheritedDeptFolderIds)
                .Distinct()
                .ToList();

            var descendantIds = new List<Guid>();
            if (allInheritedRootIds.Count != 0)
            {
                descendantIds = await GetDescendantFolderIdsAsync(allInheritedRootIds);
            }

            var folderIdsFromArchiveLeader = await dbContext.Folders
                .Where(f => f.DepartmentId != null)
                .Where(f => dbContext.DepartmentArchiveLeaders
                    .Any(dal => dal.DepartmentId == f.DepartmentId!.Value && dal.UserId == userId))
                .Select(f => f.Id)
                .ToListAsync();

            var result = folderIdsFromDirectPermissions
                .Union(folderIdsFromDeptPermissions)
                .Union(descendantIds)
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

    public async Task<Result<HashSet<Guid>>> GetAncestorFolderIdsAsync(List<Guid> folderIds)
    {
        if (folderIds.Count == 0)
            return new HashSet<Guid>();

        try
        {
            var ancestors = new HashSet<Guid>();

            var allFolderParents = await dbContext.Folders
                .Select(f => new { f.Id, f.ParentId })
                .ToDictionaryAsync(f => f.Id, f => f.ParentId);

            foreach (var folderId in folderIds)
            {
                var currentId = folderId;
                while (allFolderParents.TryGetValue(currentId, out var parentId)
                       && parentId.HasValue && parentId.Value != Guid.Empty)
                {
                    if (!ancestors.Add(parentId.Value))
                        break;
                    currentId = parentId.Value;
                }
            }

            return ancestors;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching ancestor folder IDs");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> HasDepartmentFolderPermissionAsync(Guid departmentId, Guid folderId, AccessLevel minimumLevel = AccessLevel.View)
    {
        try
        {
            var hasPermission = await dbContext.FolderPermissions
                .AnyAsync(fp => fp.FolderId == folderId && fp.DepartmentId == departmentId && (int)fp.AccessLevel >= (int)minimumLevel);

            return hasPermission;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking department folder permission for dept {DepartmentId} on folder {FolderId}", departmentId, folderId);
            return ArchiveErrors.InternalServerError;
        }
    }

    private async Task<List<Guid>> GetDescendantFolderIdsAsync(List<Guid> rootFolderIds)
    {
        var result = new List<Guid>();
        var queue = new Queue<Guid>(rootFolderIds);

        while (queue.Count > 0)
        {
            var currentId = queue.Dequeue();
            result.Add(currentId);

            var childIds = await dbContext.Folders
                .Where(f => f.ParentId == currentId)
                .Select(f => f.Id)
                .ToListAsync();

            foreach (var childId in childIds)
            {
                queue.Enqueue(childId);
            }
        }

        return result;
    }
}