using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers.ArchivingControllers;

[ApiController]
[Route("api/ArchiveSystem/[controller]")]
[Authorize]
public class FoldersController(IFolderService folderService, ILogger<FoldersController> logger) : ControllerBase
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

    [HttpPut("MoveFolder")]
    [EndpointPermission("archiving.folders.MoveFolder", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> MoveFolder([FromBody] MoveFolderDto dto)
    {
        var result = await folderService.MoveFolderAsync(dto.FolderId, dto.DestnationFolderId);
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
}
