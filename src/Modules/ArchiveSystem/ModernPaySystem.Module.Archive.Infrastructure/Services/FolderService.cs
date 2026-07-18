using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Interfaces;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class FolderService(
    IArchiveUnitOfWork unitOfWork,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveDeletionWorkflowService archiveDeletionWorkflowService,
    IArchiveResourceAuthorizationService resourceAuth,
    IArchiveLeaderService archiveLeaderService,
    IDepartmentService departmentService,
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

            var result = await unitOfWork.Folders.GetAllAsync(
                filter: f => accessibleIds.Contains(f.Id));

            if (result.IsError)
                return result.Errors;

            // Return a flat list of all folders directly to avoid missing subfolders
            // and eliminate reliance on EF Core lazy loading or relationship fix-up for SubFolders.
            var userIdStr = userId.ToString();
            var folders = result.Value!.ToList();

            var departmentIds = folders
                .Where(f => f.DepartmentId.HasValue)
                .Select(f => f.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var deptNames = new Dictionary<Guid, string>();
            foreach (var dId in departmentIds)
            {
                var deptResult = await departmentService.GetByIdAsync(dId);
                if (!deptResult.IsError && deptResult.Value != null)
                    deptNames[dId] = deptResult.Value.Name;
            }

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
                IconId = x.IconId,
                FolderDtos = [], // Frontend handles flat structure
                DepartmentId = x.DepartmentId,
                DepartmentName = x.DepartmentId.HasValue && deptNames.TryGetValue(x.DepartmentId.Value, out var dn) ? dn : null,
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
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> GetByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, id, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.FolderAccessDenied;

            var result = await unitOfWork.Folders.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.Parent).Include(x => x.SubFolders));

            if (result.IsError)
                return result.Errors;

            if (result.Value == null)
                return ArchiveErrors.FolderNotFound;

            var folder = result.Value;
            var canManage = await CanManageFolderPermissionsAsync(userId, id);
            var dto = folder.ToDto();
            dto.CanManagePermissions = !canManage.IsError && canManage.Value;
            return dto;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder by id {FolderId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> CreateAsync(CreateFolderDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return ArchiveErrors.InvalidInput;
            var getCurrentUserId = httpContextServiceManager.GetCurrentUserId();
            var departmentResult = await departmentService.GetByUserIdAsync(getCurrentUserId);
            if (departmentResult.IsError)
                return departmentResult.Errors;
            if (departmentResult.Value is null)
                return ArchiveErrors.UserNotEnrolledInDepartment;
            Guid? departmentId = departmentResult.Value.Id;

            if (dto.ParentId.HasValue && dto.ParentId.Value != Guid.Empty)
            {
                var parent = await unitOfWork.Folders.GetAsync(x => x.Id == dto.ParentId.Value);
                if (parent.IsError)
                    return parent.Errors;

                if (parent.Value == null)
                    return ArchiveErrors.FolderNotFound;

                departmentId = parent.Value.DepartmentId;
            }

            var exists = await unitOfWork.Folders.AnyAsync(
                x => x.Name == dto.Name && x.ParentId == dto.ParentId);
            if (exists)
                return ArchiveErrors.FolderAlreadyExists;

            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                DefaultStoragePath = dto.DefaultStoragePath,
                ParentId = dto.ParentId,
                DepartmentId = departmentId,
                Level = await ResolveFolderLevelAsync(dto.ParentId)
            };

            var addResult = await unitOfWork.Folders.AddAsync(folder);
            if (addResult.IsError)
                return addResult.Errors;

            await unitOfWork.SaveChangesAsync();

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var ownerPermission = new FolderPermission
            {
                Id = Guid.NewGuid(),
                FolderId = folder.Id,
                UserId = currentUserId.ToString(),
                AccessLevel = AccessLevel.FullControl,
                IsInherited = true
            };

            var permResult = await unitOfWork.FolderPermissions.AddAsync(ownerPermission);
            if (permResult.IsError)
                return permResult.Errors;

            if (dto.InitialPermissions.Count != 0)
            {
                foreach (var initial in dto.InitialPermissions)
                {
                    if (initial.UserId == Guid.Empty)
                        continue;

                    var alreadyExists = await unitOfWork.FolderPermissions.AnyAsync(x =>
                        x.FolderId == folder.Id && x.UserId == initial.UserId.ToString());

                    if (!alreadyExists)
                    {
                        var addResult2 = await unitOfWork.FolderPermissions.AddAsync(new FolderPermission
                        {
                            Id = Guid.NewGuid(),
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

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Folder created: {FolderName} by user {UserId}", folder.Name, currentUserId);
            return folder.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating folder");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> UpdateAsync(Guid id, UpdateFolderDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, id, AccessLevel.Write);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.FolderAccessDenied;

            var folderResult = await unitOfWork.Folders.GetAsync(
                x => x.Id == id,
                query => query.Include(x => x.SubFolders));

            if (folderResult.IsError)
                return folderResult.Errors;

            var folder = folderResult.Value;
            if (folder == null)
                return ArchiveErrors.FolderNotFound;

            folder.Name = dto.Name.Trim();

            var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Folder updated: {FolderId} by user {UserId}", id, userId);
            return folder.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating folder {FolderId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderDto>> MoveFolderAsync(Guid folderId, Guid destinationFolderId)
    {
        try
        {
            if (folderId == Guid.Empty || destinationFolderId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();

            var sourceAccess = await resourceAuth.CanAccessFolderAsync(userId, folderId, AccessLevel.FullControl);
            if (sourceAccess.IsError)
                return sourceAccess.Errors;
            if (!sourceAccess.Value)
                return ArchiveErrors.FolderAccessDenied;

            var destAccess = await resourceAuth.CanAccessFolderAsync(userId, destinationFolderId, AccessLevel.Write);
            if (destAccess.IsError)
                return destAccess.Errors;
            if (!destAccess.Value)
                return ArchiveErrors.FolderAccessDenied;

            var folderResult = await unitOfWork.Folders.GetAsync(
                x => x.Id == folderId,
                query => query.Include(x => x.SubFolders).Include(x => x.ArchiveRecords));

            if (folderResult.IsError)
                return folderResult.Errors;

            var folder = folderResult.Value;
            if (folder == null)
                return ArchiveErrors.FolderNotFound;

            var destFolder = await unitOfWork.Folders.GetByIdAsync(destinationFolderId);
            if (destFolder.IsError)
                return destFolder.Errors;
            if (destFolder.Value == null)
                return ArchiveErrors.FolderNotFound;

            if (destFolder.Value.Id == folderId || await WouldCreateCircularReferenceAsync(folderId, destinationFolderId))
                return ArchiveErrors.InvalidInput;

            if (folder.DepartmentId.HasValue && destFolder.Value.DepartmentId.HasValue
                && folder.DepartmentId != destFolder.Value.DepartmentId)
            {
                return ArchiveErrors.InvalidInput;
            }

            var previousLevel = folder.Level;
            folder.ParentId = destinationFolderId;
            folder.Level = await ResolveFolderLevelAsync(destinationFolderId);

            var updateResult = await unitOfWork.Folders.UpdateAsync(folder);
            if (updateResult.IsError)
                return updateResult.Errors;

            if (folder.Level != previousLevel)
                await UpdateDescendantLevelsAsync(folder.Id, folder.Level);

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Folder {FolderId} moved to {DestinationFolderId} by user {UserId}", folderId, destinationFolderId, userId);
            return folder.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error moving folder {FolderId}", folderId);
            return ArchiveErrors.InternalServerError;
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
                return ArchiveErrors.FolderAccessDenied;

            return await archiveDeletionWorkflowService.DeleteFolderAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting folder {FolderId}", id);
            return ArchiveErrors.InternalServerError;
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
                return ArchiveErrors.FolderAccessDenied;

            var result = await unitOfWork.FolderPermissions.GetAllAsync(
                filter: p => p.FolderId == folderId);

            if (result.IsError)
                return result.Errors;

            var permissions = result.Value!.ToList();
            var departmentIds = permissions
                .Where(p => p.DepartmentId.HasValue)
                .Select(p => p.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var departmentNames = new Dictionary<Guid, string>();
            foreach (var deptId in departmentIds)
            {
                var deptResult = await departmentService.GetByIdAsync(deptId);
                if (!deptResult.IsError && deptResult.Value != null)
                    departmentNames[deptId] = deptResult.Value.Name;
            }

            return permissions.Select(p =>
            {
                var dto = p.ToDto();
                if (p.DepartmentId.HasValue && departmentNames.TryGetValue(p.DepartmentId.Value, out var deptName))
                    dto.DepartmentName = deptName;
                return dto;
            }).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching permissions for folder {FolderId}", folderId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderPermissionDto>> GetPermissionByIdAsync(Guid id)
    {
        try
        {
            var permission = await unitOfWork.FolderPermissions.GetByIdAsync(id);
            if (permission.IsError || permission.Value == null)
                return ArchiveErrors.FolderPermissionNotFound;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, permission.Value.FolderId, AccessLevel.Read);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.FolderAccessDenied;

            return permission.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching folder permission {PermissionId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderPermissionDto>> CreatePermissionAsync(CreateFolderPermissionDto dto)
    {
        try
        {
            if (dto == null || dto.FolderId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            if (dto.UserId == null && dto.DepartmentId == null)
                return ArchiveErrors.FolderPermissionDepartmentOrUserRequired;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var canManage = await CanManageFolderPermissionsAsync(currentUserId, dto.FolderId);
            if (canManage.IsError)
                return canManage.Errors;
            if (!canManage.Value)
                return ArchiveErrors.FolderAccessDenied;

            if (dto.UserId.HasValue && dto.UserId.Value != Guid.Empty)
            {
                var exists = await unitOfWork.FolderPermissions.AnyAsync(
                    x => x.FolderId == dto.FolderId && x.UserId == dto.UserId.Value.ToString());
                if (exists)
                    return ArchiveErrors.FolderPermissionAlreadyExists;
            }

            if (dto.DepartmentId.HasValue && dto.DepartmentId.Value != Guid.Empty)
            {
                var deptExists = await unitOfWork.FolderPermissions.AnyAsync(
                    x => x.FolderId == dto.FolderId && x.DepartmentId == dto.DepartmentId.Value);
                if (deptExists)
                    return ArchiveErrors.FolderPermissionDepartmentAlreadyExists;
            }

            var permission = new FolderPermission
            {
                Id = Guid.NewGuid(),
                FolderId = dto.FolderId,
                UserId = dto.UserId?.ToString(),
                DepartmentId = dto.DepartmentId,
                AccessLevel = dto.AccessLevel,
                IsInherited = dto.IsInherited
            };

            var addResult = await unitOfWork.FolderPermissions.AddAsync(permission);
            if (addResult.IsError)
                return addResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Folder permission created for user/dept {UserId}/{DepartmentId} on folder {FolderId}",
                dto.UserId, dto.DepartmentId, dto.FolderId);
            return permission.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating folder permission");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<List<FolderPermissionDto>>> CreateBulkPermissionAsync(BulkCreateFolderPermissionDto dto)
    {
        try
        {
            if (dto == null || dto.FolderIds.Count == 0)
                return ArchiveErrors.InvalidInput;

            if (dto.UserId == null && dto.DepartmentId == null)
                return ArchiveErrors.FolderPermissionDepartmentOrUserRequired;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var createdPermissions = new List<FolderPermissionDto>();

            foreach (var folderId in dto.FolderIds)
            {
                var canManage = await CanManageFolderPermissionsAsync(currentUserId, folderId);
                if (canManage.IsError || !canManage.Value)
                    continue;

                if (dto.UserId.HasValue && dto.UserId.Value != Guid.Empty)
                {
                    var exists = await unitOfWork.FolderPermissions.AnyAsync(
                        x => x.FolderId == folderId && x.UserId == dto.UserId.Value.ToString());
                    if (exists)
                        continue;
                }

                if (dto.DepartmentId.HasValue && dto.DepartmentId.Value != Guid.Empty)
                {
                    var deptExists = await unitOfWork.FolderPermissions.AnyAsync(
                        x => x.FolderId == folderId && x.DepartmentId == dto.DepartmentId.Value);
                    if (deptExists)
                        continue;
                }

                var permission = new FolderPermission
                {
                    Id = Guid.NewGuid(),
                    FolderId = folderId,
                    UserId = dto.UserId?.ToString(),
                    DepartmentId = dto.DepartmentId,
                    AccessLevel = dto.AccessLevel,
                    IsInherited = dto.IsInherited
                };

                var addResult = await unitOfWork.FolderPermissions.AddAsync(permission);
                if (addResult.IsError)
                    continue;

                createdPermissions.Add(permission.ToDto());
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Bulk folder permission created: {Count} folders for user/dept {UserId}/{DepartmentId}",
                createdPermissions.Count, dto.UserId, dto.DepartmentId);
            return createdPermissions;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating bulk folder permissions");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<List<SubFolderTreeNodeDto>>> GetSubFolderTreeAsync(Guid folderId)
    {
        try
        {
            if (folderId == Guid.Empty)
                return ArchiveErrors.InvalidInput;

            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessFolderAsync(userId, folderId, AccessLevel.View);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.FolderAccessDenied;

            var allFoldersResult = await unitOfWork.Folders.GetAllAsync();
            if (allFoldersResult.IsError || allFoldersResult.Value == null)
                return new List<SubFolderTreeNodeDto>();

            var folderList = allFoldersResult.Value.ToList();
            var childrenLookup = folderList
                .Where(f => f.ParentId.HasValue)
                .ToLookup(f => f.ParentId!.Value);

            SubFolderTreeNodeDto MapToTreeNode(Folder folder) => new()
            {
                Id = folder.Id,
                Name = folder.Name,
                Level = folder.Level,
                Children = childrenLookup[folder.Id]
                    .OrderBy(f => f.Name)
                    .Select(MapToTreeNode)
                    .ToList()
            };

            return childrenLookup[folderId]
                .OrderBy(f => f.Name)
                .Select(MapToTreeNode)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subfolder tree for folder {FolderId}", folderId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<FolderPermissionDto>> UpdatePermissionAsync(Guid id, UpdateFolderPermissionDto dto)
    {
        try
        {
            if (dto == null)
                return ArchiveErrors.InvalidInput;

            var permissionResult = await unitOfWork.FolderPermissions.GetByIdAsync(id);
            if (permissionResult.IsError || permissionResult.Value == null)
                return ArchiveErrors.FolderPermissionNotFound;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var canManage = await CanManageFolderPermissionsAsync(currentUserId, permissionResult.Value.FolderId);
            if (canManage.IsError)
                return canManage.Errors;
            if (!canManage.Value)
                return ArchiveErrors.FolderAccessDenied;

            var permission = permissionResult.Value;
            permission.AccessLevel = dto.AccessLevel;
            permission.IsInherited = dto.IsInherited;

            var updateResult = await unitOfWork.FolderPermissions.UpdateAsync(permission);
            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Folder permission {PermissionId} updated by user {UserId}", id, currentUserId);
            return permission.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating folder permission {PermissionId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeletePermissionAsync(Guid id)
    {
        try
        {
            var permissionResult = await unitOfWork.FolderPermissions.GetByIdAsync(id);
            if (permissionResult.IsError || permissionResult.Value == null)
                return ArchiveErrors.FolderPermissionNotFound;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            if (permissionResult.Value.UserId == currentUserId.ToString())
                return ArchiveErrors.CannotRemoveOwnFolderPermission;

            var canManage = await CanManageFolderPermissionsAsync(currentUserId, permissionResult.Value.FolderId);
            if (canManage.IsError)
                return canManage.Errors;
            if (!canManage.Value)
                return ArchiveErrors.FolderAccessDenied;

            var removeResult = await unitOfWork.FolderPermissions.RemoveAsync(x => x.Id == id);
            if (removeResult.IsError)
                return removeResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Folder permission {PermissionId} deleted by user {UserId}", id, currentUserId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting folder permission {PermissionId}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private async Task<Result<bool>> CanManageFolderPermissionsAsync(Guid userId, Guid folderId)
    {
        var folderResult = await unitOfWork.Folders.GetByIdAsync(folderId);
        if (folderResult.IsError || folderResult.Value == null)
            return ArchiveErrors.FolderNotFound;

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

        return ArchiveErrors.FolderAccessDenied;
    }

    private async Task<int> ResolveFolderLevelAsync(Guid? parentId)
    {
        if (!parentId.HasValue || parentId.Value == Guid.Empty)
            return 0;

        var parentResult = await unitOfWork.Folders.GetByIdAsync(parentId.Value);
        if (parentResult.IsError || parentResult.Value == null)
            return 0;

        return parentResult.Value.Level + 1;
    }

    private async Task<bool> WouldCreateCircularReferenceAsync(Guid folderId, Guid newParentId)
    {
        var currentParentId = newParentId;
        var visited = new HashSet<Guid>();

        while (currentParentId != Guid.Empty && visited.Add(currentParentId))
        {
            if (currentParentId == folderId)
                return true;

            var parentResult = await unitOfWork.Folders.GetByIdAsync(currentParentId);
            if (parentResult.IsError || parentResult.Value == null || !parentResult.Value.ParentId.HasValue)
                return false;

            currentParentId = parentResult.Value.ParentId.Value;
        }

        return false;
    }

    private async Task UpdateDescendantLevelsAsync(Guid parentId, int parentLevel)
    {
        var childrenResult = await unitOfWork.Folders.GetAllAsync(
            x => x.ParentId == parentId,
            query => query.Include(x => x.SubFolders));

        if (childrenResult.IsError || childrenResult.Value == null)
            return;

        foreach (var child in childrenResult.Value)
        {
            child.Level = parentLevel + 1;
            var updateResult = await unitOfWork.Folders.UpdateAsync(child);
            if (updateResult.IsError)
                throw new InvalidOperationException("Failed to update folder depth levels.");

            await UpdateDescendantLevelsAsync(child.Id, child.Level);
        }
    }
}
