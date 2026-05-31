using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers.ArchivingControllers;

[ApiController]
[Route("api/ArchiveSystem/[controller]")]
[Authorize]
public class DynamicFormsController(IArchiveFormTemplateService dynamicFormService, ILogger<DynamicFormsController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("archiving.forms.get-all", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all dynamic forms");
        var result = await dynamicFormService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("paged")]
    [EndpointPermission("archiving.forms.get-paged", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged dynamic forms, page: {Page}, size: {PageSize}", page, pageSize);
        var result = await dynamicFormService.GetPagedAsync(page, pageSize);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("archiving.forms.get-by-id", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting dynamic form by id: {FormId}", id);
        var result = await dynamicFormService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-name/{name}")]
    [EndpointPermission("archiving.forms.get-by-name", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetByName(string name)
    {
        logger.LogInformation("Getting dynamic form by name: {FormName}", name);
        var result = await dynamicFormService.GetByNameAsync(name);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("archiving.forms.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateDynamicFormTemplateDto dto)
    {
        logger.LogInformation("Creating dynamic form: {FormName}", dto?.TemplateFormName);
        var result = await dynamicFormService.CreateAsync(dto!);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("archiving.forms.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicFormTemplateDto dto)
    {
        logger.LogInformation("Updating dynamic form: {FormId}", id);
        var result = await dynamicFormService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("archiving.forms.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting dynamic form: {FormId}", id);
        var result = await dynamicFormService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
