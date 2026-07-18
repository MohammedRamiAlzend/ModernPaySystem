using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Archive.Api.Extensions;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Api.Controllers;

[ApiController]
[Route("api/ArchiveSystem/[controller]")]
[Authorize]
public class FoldersController(
    IFolderService folderService,
    ILogger<FoldersController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("archiving.folders.get-all", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all folders");
        var result = await folderService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("archiving.folders.get-by-id", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting folder by id: {FolderId}", id);
        var result = await folderService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("archiving.folders.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateFolderDto dto)
    {
        logger.LogInformation("Creating folder: {FolderName}", dto?.Name);
        var result = await folderService.CreateAsync(dto!);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("archiving.folders.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFolderDto dto)
    {
        logger.LogInformation("Updating folder: {FolderId}", id);
        var result = await folderService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpPut("move")]
    [EndpointPermission("archiving.folders.move", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> MoveFolder([FromBody] MoveFolderDto dto)
    {
        logger.LogInformation("Moving folder {FolderId} to {DestinationFolderId}", dto?.FolderId, dto?.DestnationFolderId);
        var result = await folderService.MoveFolderAsync(dto!.FolderId, dto.DestnationFolderId);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("archiving.folders.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting folder: {FolderId}", id);
        var result = await folderService.DeleteAsync(id);
        return result.ToActionResult();
    }

    // ── Folder Permission endpoints ────────────────────────────────────────────

    [HttpGet("{folderId}/permissions")]
    [EndpointPermission("archiving.folders.permissions.get", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetPermissions(Guid folderId)
    {
        logger.LogInformation("Getting permissions for folder {FolderId}", folderId);
        var result = await folderService.GetPermissionsByFolderAsync(folderId);
        return result.ToActionResult();
    }

    [HttpGet("permissions/{id}")]
    [EndpointPermission("archiving.folders.permissions.get-by-id", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetPermissionById(Guid id)
    {
        logger.LogInformation("Getting folder permission {PermissionId}", id);
        var result = await folderService.GetPermissionByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{folderId}/permissions")]
    [EndpointPermission("archiving.folders.permissions.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> CreatePermission(Guid folderId, [FromBody] CreateFolderPermissionDto dto)
    {
        dto.FolderId = folderId;
        logger.LogInformation("Creating folder permission for user/dept {UserId}/{DepartmentId} on folder {FolderId}",
            dto.UserId, dto.DepartmentId, folderId);
        var result = await folderService.CreatePermissionAsync(dto);
        return result.ToActionResult();
    }

    [HttpPost("permissions/bulk")]
    [EndpointPermission("archiving.folders.permissions.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> CreateBulkPermission([FromBody] BulkCreateFolderPermissionDto dto)
    {
        logger.LogInformation("Creating bulk permissions for {FolderCount} folders for user/dept {UserId}/{DepartmentId}",
            dto.FolderIds.Count, dto.UserId, dto.DepartmentId);
        var result = await folderService.CreateBulkPermissionAsync(dto);
        return result.ToActionResult();
    }

    [HttpGet("{folderId}/subfolders")]
    [EndpointPermission("archiving.folders.get-by-id", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetSubFolderTree(Guid folderId)
    {
        logger.LogInformation("Getting subfolder tree for folder {FolderId}", folderId);
        var result = await folderService.GetSubFolderTreeAsync(folderId);
        return result.ToActionResult();
    }

    [HttpPut("permissions/{id}")]
    [EndpointPermission("archiving.folders.permissions.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] UpdateFolderPermissionDto dto)
    {
        logger.LogInformation("Updating folder permission {PermissionId}", id);
        var result = await folderService.UpdatePermissionAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("permissions/{id}")]
    [EndpointPermission("archiving.folders.permissions.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> DeletePermission(Guid id)
    {
        logger.LogInformation("Deleting folder permission {PermissionId}", id);
        var result = await folderService.DeletePermissionAsync(id);
        return result.ToActionResult();
    }
}
