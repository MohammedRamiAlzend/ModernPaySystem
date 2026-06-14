using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class FolderService(
    IUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveDeletionWorkflowService archiveDeletionWorkflowService,
    IArchiveResourceAuthorizationService resourceAuth,
    IArchiveLeaderService archiveLeaderService,
    ILogger<FolderService> logger) : IFolderService
{
    public async Task<Result<IEnumerable<FolderDto>>> GetAllAsync()
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var accessibleIdsResult = await resourceAuth.GetAccessibleFolderIdsAsync(userId);
            if (accessibleIdsResult.IsError)
                return accessibleIdsResult.Errors;

            var accessibleIds = accessibleIdsResult.Value!;
            if (accessibleIds.Count == 0)
                return new List<FolderDto>();

            var result = await unitOfWork.Folders.GetAllAsync(filter: f => accessibleIds.Contains(f.Id));
            if (result.IsError)
            {
                return result.Errors;
            }

            // Return a flat list of all folders directly to avoid missing subfolders 
            // and eliminate reliance on EF Core lazy loading or relationship fix-up for SubFolders.
            var userIdStr = userId.ToString();
            var folders = result.Value!.ToList();

            var departmentIds = folders.Where(f => f.DepartmentId.HasValue).Select(f => f.DepartmentId!.Value).Distinct().ToList();
            var isLeaderTasks = departmentIds.Select(d => archiveLeaderService.IsArchiveLeaderAsync(userId, d));
            var leaderResults = await Task.WhenAll(isLeaderTasks);
            var leaderDepartments = new HashSet<Guid>();
            for (int i = 0; i < departmentIds.Count; i++)
            {
                if (!leaderResults[i].IsError && leaderResults[i].Value)
                    leaderDepartments.Add(departmentIds[i]);
            }

            return folders.Select(x => new FolderDto
            {
                Id = x.Id,
                Name = x.Name,
                Level = x.Level,
                ParentId = x.ParentId,
                FolderDtos = [], // Frontend will handle flat structure
                DepartmentId = x.DepartmentId,
                CreatedByUserId = x.CreatedByUserId,
                CreatedAt = x.CreatedAt,
                UpdatedByUserId = x.UpdatedByUserId,
                UpdatedAt = x.UpdatedAt,
                CanManagePermissions = x.CreatedByUserId == userIdStr
                    || (x.DepartmentId.HasValue && leaderDepartments.Contains(x.DepartmentId.Value))
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

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.FolderAccessDenied;

            var result = await unitOfWork.Folders.GetAsync(x => x.Id == id, query => query.Include(x => x.Parent).Include(x => x.SubFolders));
            if (result.IsError)
            {
                return result.Errors;
            }

            if (result.Value == null)
            {
                return ApplicationErrors.FolderNotFound;
            }

            var folder = result.Value;
            var canManage = await CanManageFolderPermissionsAsync(userId, id);
            var dto = folder.ToDto();
            dto.CanManagePermissions = !canManage.IsError && canManage.Value;
            return dto;
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

            Guid? departmentId = dto.DepartmentId;

            if (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty)
            {
                var parent = await unitOfWork.Folders.GetAsync(x => x.Id == dto.ParentId.Value);
                if (parent.IsError)
                {
                    return parent.Errors;
                }

                if (parent.Value == null)
                {
                    return ApplicationErrors.FolderNotFound;
                }

                departmentId = parent.Value.DepartmentId;
            }

            if (!departmentId.HasValue || departmentId == Guid.Empty)
            {
                var currentUser = await unitOfWork.Users.GetByIdAsync(httpContextServiceManager.GetCurrentUserId());
                if (currentUser.IsError || currentUser.Value == null || !currentUser.Value.DepartmentId.HasValue)
                {
                    return ApplicationErrors.FolderDepartmentNotConfigured;
                }

                departmentId = currentUser.Value.DepartmentId;
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
                DepartmentId = departmentId,
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

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var ownerPermission = new FolderPermission
            {
                FolderId = folder.Id,
                UserId = currentUserId.ToString(),
                AccessLevel = AccessLevel.FullControl,
                IsInherited = true
            };

            var permResult = await unitOfWork.FolderPermissions.AddAsync(ownerPermission);
            if (permResult.IsError)
            {
                return permResult.Errors;
            }

            if (dto.InitialPermissions.Count != 0)
            {
                foreach (var initial in dto.InitialPermissions)
                {
                    if (initial.UserId == Guid.Empty)
                        continue;

                    var existing = await unitOfWork.FolderPermissions.AnyAsync(x =>
                        x.FolderId == folder.Id && x.UserId == initial.UserId.ToString());

                    if (!existing)
                    {
                        var addResult2 = await unitOfWork.FolderPermissions.AddAsync(new FolderPermission
                        {
                            FolderId = folder.Id,
                            UserId = initial.UserId.ToString(),
                            AccessLevel = initial.AccessLevel,
                            IsInherited = true
                        });

                        if (addResult2.IsError)
                            return addResult2.Errors;
                    }
                }
            }

            var permSaveResult = await unitOfWork.SaveChangesAsync();
            if (permSaveResult <= 0)
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

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, id, AccessLevel.Write);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.FolderAccessDenied;

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

            var userId = httpContextServiceManager.GetCurrentUserId();

            var sourceAccess = await resourceAuth.CanAccessFolderAsync(userId, folderId, AccessLevel.FullControl);
            if (sourceAccess.IsError)
                return sourceAccess.Errors;
            if (!sourceAccess.Value)
                return ApplicationErrors.FolderAccessDenied;

            var destAccess = await resourceAuth.CanAccessFolderAsync(userId, destinationFolderId, AccessLevel.Write);
            if (destAccess.IsError)
                return destAccess.Errors;
            if (!destAccess.Value)
                return ApplicationErrors.FolderAccessDenied;

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

            if (folder.DepartmentId.HasValue && destFolder.Value.DepartmentId.HasValue && folder.DepartmentId != destFolder.Value.DepartmentId)
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
            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, id, AccessLevel.FullControl);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.FolderAccessDenied;

            return await archiveDeletionWorkflowService.DeleteFolderAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting folder {FolderId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<FolderPermissionDto>>> GetPermissionsByFolderAsync(Guid folderId)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, folderId, AccessLevel.Read);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.FolderAccessDenied;

            var permissions = await unitOfWork.Context.FolderPermissions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.FolderId == folderId)
                .Select(p => new FolderPermissionDto
                {
                    Id = p.Id,
                    FolderId = p.FolderId,
                    UserId = p.UserId,
                    AccessLevel = p.AccessLevel,
                    IsInherited = p.IsInherited,
                    CreatedByUserId = p.CreatedByUserId,
                    CreatedAt = p.CreatedAt,
                    UpdatedByUserId = p.UpdatedByUserId,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            var userIds = permissions.Select(p => Guid.Parse(p.UserId)).Distinct().ToList();
            var userMap = await unitOfWork.Context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id.ToString(), u => u.UserName);

            foreach (var p in permissions)
            {
                if (userMap.TryGetValue(p.UserId, out var name))
                    p.UserName = name;
            }

            return permissions;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching permissions for folder {FolderId}", folderId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderPermissionDto>> GetPermissionByIdAsync(Guid id)
    {
        try
        {
            var permission = await unitOfWork.FolderPermissions.GetByIdAsync(id);
            if (permission.IsError || permission.Value == null)
                return ApplicationErrors.FolderPermissionNotFound;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, permission.Value.FolderId, AccessLevel.Read);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ApplicationErrors.FolderAccessDenied;

            return permission.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder permission {PermissionId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderPermissionDto>> CreatePermissionAsync(CreateFolderPermissionDto dto)
    {
        try
        {
            if (dto == null || dto.FolderId == Guid.Empty || dto.UserId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var canManage = await CanManageFolderPermissionsAsync(currentUserId, dto.FolderId);
            if (canManage.IsError)
                return canManage.Errors;
            if (!canManage.Value)
                return ApplicationErrors.FolderAccessDenied;

            var exists = await unitOfWork.FolderPermissions.AnyAsync(x =>
                x.FolderId == dto.FolderId && x.UserId == dto.UserId.ToString());
            if (exists)
                return ApplicationErrors.FolderPermissionAlreadyExists;

            var permission = new FolderPermission
            {
                FolderId = dto.FolderId,
                UserId = dto.UserId.ToString(),
                AccessLevel = dto.AccessLevel,
                IsInherited = dto.IsInherited
            };

            var addResult = await unitOfWork.FolderPermissions.AddAsync(permission);
            if (addResult.IsError)
                return addResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ApplicationErrors.DatabaseError;

            return permission.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating folder permission");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderPermissionDto>> UpdatePermissionAsync(Guid id, UpdateFolderPermissionDto dto)
    {
        try
        {
            if (dto == null)
                return ApplicationErrors.InvalidInput;

            var permissionResult = await unitOfWork.FolderPermissions.GetByIdAsync(id);
            if (permissionResult.IsError || permissionResult.Value == null)
                return ApplicationErrors.FolderPermissionNotFound;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var canManage = await CanManageFolderPermissionsAsync(currentUserId, permissionResult.Value.FolderId);
            if (canManage.IsError)
                return canManage.Errors;
            if (!canManage.Value)
                return ApplicationErrors.FolderAccessDenied;

            var permission = permissionResult.Value;
            permission.AccessLevel = dto.AccessLevel;
            permission.IsInherited = dto.IsInherited;

            var updateResult = await unitOfWork.FolderPermissions.UpdateAsync(permission);
            if (updateResult.IsError)
                return updateResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return ApplicationErrors.DatabaseError;

            return permission.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating folder permission {PermissionId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeletePermissionAsync(Guid id)
    {
        try
        {
            var permissionResult = await unitOfWork.FolderPermissions.GetByIdAsync(id);
            if (permissionResult.IsError || permissionResult.Value == null)
                return ApplicationErrors.FolderPermissionNotFound;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            if (permissionResult.Value.UserId == currentUserId.ToString())
                return ApplicationErrors.CannotRemoveOwnFolderPermission;

            var canManage = await CanManageFolderPermissionsAsync(currentUserId, permissionResult.Value.FolderId);
            if (canManage.IsError)
                return canManage.Errors;
            if (!canManage.Value)
                return ApplicationErrors.FolderAccessDenied;

            var removeResult = await unitOfWork.FolderPermissions.RemoveAsync(x => x.Id == id);
            if (removeResult.IsError)
                return removeResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            return saveResult > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting folder permission {PermissionId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task<Result<bool>> CanManageFolderPermissionsAsync(Guid userId, Guid folderId)
    {
        var folderResult = await unitOfWork.Folders.GetByIdAsync(folderId);
        if (folderResult.IsError || folderResult.Value == null)
            return ApplicationErrors.FolderNotFound;

        var folder = folderResult.Value;

        if (folder.CreatedByUserId == userId.ToString())
            return true;

        if (folder.DepartmentId.HasValue)
        {
            var isLeader = await archiveLeaderService.IsArchiveLeaderAsync(userId, folder.DepartmentId.Value);
            if (isLeader.IsError)
                return isLeader.Errors;
            if (isLeader.Value)
                return true;
        }

        return ApplicationErrors.FolderAccessDenied;
    }
}
