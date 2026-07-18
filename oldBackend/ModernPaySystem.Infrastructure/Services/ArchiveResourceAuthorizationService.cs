using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class ArchiveResourceAuthorizationService(
    IUnitOfWork unitOfWork,
    IArchiveAuthorizationService archiveAuthorizationService,
    ILogger<ArchiveResourceAuthorizationService> logger) : IArchiveResourceAuthorizationService
{
    public async Task<Result<bool>> CanAccessFolderAsync(Guid userId, Guid folderId, AccessLevel minimumLevel = AccessLevel.View)
    {
        var level = await GetFolderAccessLevelAsync(userId, folderId);
        if (level.IsError)
            return level.Errors;

        return level.Value >= minimumLevel;
    }

    public async Task<Result<bool>> CanAccessArchiveRecordAsync(Guid userId, Guid recordId, AccessLevel minimumLevel = AccessLevel.View)
    {
        var record = await unitOfWork.Context.ArchiveRecords
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == recordId && !x.IsDeleted)
            .Select(x => new { x.FolderId, x.CreatedByUserId })
            .SingleOrDefaultAsync();

        if (record == null)
            return ApplicationErrors.ArchiveRecordNotFound;

        if (record.CreatedByUserId == userId.ToString())
            return true;

        return await CanAccessFolderAsync(userId, record.FolderId, minimumLevel);
    }

    public async Task<Result<bool>> CanAccessPhysicalFileAsync(Guid userId, Guid fileId, AccessLevel minimumLevel = AccessLevel.View)
    {
        var file = await unitOfWork.Context.PhysicalFiles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.Id == fileId && !x.IsDeleted)
            .Select(x => new { x.ArchiveRecordId, x.CreatedByUserId })
            .SingleOrDefaultAsync();

        if (file == null)
            return ApplicationErrors.PhysicalFileAccessDenied;

        if (file.CreatedByUserId == userId.ToString())
            return true;

        return await CanAccessArchiveRecordAsync(userId, file.ArchiveRecordId, minimumLevel);
    }

    public async Task<Result<AccessLevel>> GetFolderAccessLevelAsync(Guid userId, Guid folderId)
    {
        if (folderId == Guid.Empty)
            return ApplicationErrors.InvalidInput;

        var userIdStr = userId.ToString();

        // Walk up the folder hierarchy collecting access levels
        var currentId = folderId;
        var visited = new HashSet<Guid>();
        var maxLevel = AccessLevel.View;

        while (currentId != Guid.Empty && visited.Add(currentId))
        {
            var folder = await unitOfWork.Context.Folders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.Id == currentId)
                .Select(x => new
                {
                    x.Id,
                    x.CreatedByUserId,
                    x.ParentId,
                    x.DepartmentId,
                    Permissions = x.Permissions
                        .Where(p => p.UserId == userIdStr)
                        .Select(p => p.AccessLevel)
                        .ToList()
                })
                .SingleOrDefaultAsync();

            if (folder == null)
                break;

            // Creator has full control at the folder they created
            if (folder.CreatedByUserId == userIdStr)
            {
                maxLevel = AccessLevel.FullControl;
                break;
            }

            // Check department leader bypass
            if (folder.DepartmentId.HasValue)
            {
                var isLeader = await archiveAuthorizationService.IsArchiveLeaderAsync(userId, folder.DepartmentId.Value);
                if (!isLeader.IsError && isLeader.Value)
                {
                    maxLevel = AccessLevel.FullControl;
                    break;
                }
            }

            // Check direct permissions on this folder
            foreach (var level in folder.Permissions)
            {
                if (level > maxLevel)
                    maxLevel = level;
            }

            // If we're at the requested folder (first iteration), check inherited permissions
            // from ancestor folders too
            currentId = folder.ParentId ?? Guid.Empty;
        }

        return maxLevel;
    }

    public async Task<Result<List<Guid>>> GetAccessibleFolderIdsAsync(Guid userId, AccessLevel minimumLevel = AccessLevel.View)
    {
        var userIdStr = userId.ToString();

        // 1. Folders the user created
        var createdIds = await unitOfWork.Context.Folders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.CreatedByUserId == userIdStr && !x.IsDeleted)
            .Select(x => x.Id)
            .ToListAsync();

        // 2. Folders with direct FolderPermission at or above minimum level
        var permissionFolderIds = await unitOfWork.Context.FolderPermissions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.UserId == userIdStr && p.AccessLevel >= minimumLevel)
            .Select(p => p.FolderId)
            .ToListAsync();

        // 3. Folders in departments where user is archive leader
        var leaderDeptIds = await unitOfWork.Context.DepartmentArchiveLeaders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(dal => dal.UserId == userId && !dal.IsDeleted)
            .Select(dal => dal.DepartmentId)
            .ToListAsync();

        var leaderFolderIds = new List<Guid>();
        if (leaderDeptIds.Count != 0)
        {
            leaderFolderIds = await unitOfWork.Context.Folders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(f => f.DepartmentId.HasValue && leaderDeptIds.Contains(f.DepartmentId.Value) && !f.IsDeleted)
                .Select(f => f.Id)
                .ToListAsync();
        }

        // 4. Inherited permissions: for any folder with IsInherited = true permission,
        //    include all descendants
        var inheritedRootIds = await unitOfWork.Context.FolderPermissions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(p => p.UserId == userIdStr && p.IsInherited && p.AccessLevel >= minimumLevel)
            .Select(p => p.FolderId)
            .ToListAsync();

        var inheritedDescendantIds = new List<Guid>();
        if (inheritedRootIds.Count != 0)
        {
            inheritedDescendantIds = await GetDescendantFolderIdsAsync(inheritedRootIds);
        }

        var allIds = createdIds
            .Concat(permissionFolderIds)
            .Concat(leaderFolderIds)
            .Concat(inheritedDescendantIds)
            .Distinct()
            .ToList();

        return allIds;
    }

    private async Task<List<Guid>> GetDescendantFolderIdsAsync(List<Guid> rootIds)
    {
        var result = new List<Guid>();
        var toProcess = new Queue<Guid>(rootIds);

        while (toProcess.Count > 0)
        {
            var parentId = toProcess.Dequeue();

            var children = await unitOfWork.Context.Folders
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(f => f.ParentId == parentId && !f.IsDeleted)
                .Select(f => f.Id)
                .ToListAsync();

            foreach (var childId in children)
            {
                result.Add(childId);
                toProcess.Enqueue(childId);
            }
        }

        return result;
    }
}
