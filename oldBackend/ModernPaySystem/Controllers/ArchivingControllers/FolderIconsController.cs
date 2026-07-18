using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers.ArchivingControllers;

[ApiController]
[Route("api/ArchiveSystem/[controller]")]
[Authorize]
public class FolderIconsController(
    IFolderIconService folderIconService,
    ILogger<FolderIconsController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("archiving.folder-icons.get-all", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        var result = await folderIconService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("archiving.folder-icons.get-by-id", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await folderIconService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("{id}/svg")]
    [AllowAnonymous]
    public async Task<IActionResult> GetSvg(Guid id)
    {
        var result = await folderIconService.GetIconSvgAsync(id);
        if (result.IsError)
            return result.ToActionResult();

        return Content(result.Value!, "image/svg+xml", System.Text.Encoding.UTF8);
    }

    [HttpPost]
    [EndpointPermission("archiving.folder-icons.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateFolderIconDto dto)
    {
        var result = await folderIconService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("archiving.folder-icons.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFolderIconDto dto)
    {
        var result = await folderIconService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("archiving.folder-icons.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await folderIconService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("assign")]
    [EndpointPermission("archiving.folder-icons.assign", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> AssignIconToFolder([FromBody] AssignFolderIconDto dto)
    {
        var result = await folderIconService.AssignIconToFolderAsync(dto.FolderId, dto.IconId);
        return result.ToActionResult();
    }
}
