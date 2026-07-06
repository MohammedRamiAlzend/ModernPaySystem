using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Archive.Api.Extensions;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Api.Controllers;

[ApiController]
[Route("api/archive/folder-icons")]
[Authorize]
public class FolderIconsController(
    IFolderIconService folderIconService,
    ILogger<FolderIconsController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("archiving.folder-icons.get-all", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all folder icons");
        var result = await folderIconService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("archiving.folder-icons.get-by-id", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting folder icon by id: {IconId}", id);
        var result = await folderIconService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id}/svg")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSvg(Guid id)
    {
        logger.LogInformation("Getting SVG for folder icon: {IconId}", id);
        var result = await folderIconService.GetIconSvgAsync(id);
        if (result.IsError)
            return result.ToActionResult();

        return Content(result.Value!, "image/svg+xml", System.Text.Encoding.UTF8);
    }

    [HttpPost]
    [EndpointPermission("archiving.folder-icons.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateFolderIconDto dto)
    {
        logger.LogInformation("Creating folder icon: {IconName}", dto?.Name);
        var result = await folderIconService.CreateAsync(dto!);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("archiving.folder-icons.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFolderIconDto dto)
    {
        logger.LogInformation("Updating folder icon: {IconId}", id);
        var result = await folderIconService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("archiving.folder-icons.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting folder icon: {IconId}", id);
        var result = await folderIconService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("assign")]
    [EndpointPermission("archiving.folder-icons.assign", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> AssignIconToFolder([FromBody] AssignFolderIconDto dto)
    {
        logger.LogInformation("Assigning icon {IconId} to folder {FolderId}", dto?.IconId, dto?.FolderId);
        var result = await folderIconService.AssignIconToFolderAsync(dto!.FolderId, dto.IconId);
        return result.ToActionResult();
    }
}
