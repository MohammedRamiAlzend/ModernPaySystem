using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly ILogger<TemplatesController> _logger;

    public TemplatesController(ITemplateService templateService, ILogger<TemplatesController> logger)
    {
        _templateService = templateService;
        _logger = logger;
    }

    [HttpGet]
    [EndpointPermission("templates.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all templates");
        var result = await _templateService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("templates.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting template by id: {TemplateId}", id);
        var result = await _templateService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-name/{name}")]
    [EndpointPermission("templates.get-by-name", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByName(string name)
    {
        _logger.LogInformation("Getting template by name: {Name}", name);
        var result = await _templateService.GetByNameAsync(name);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("templates.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto template)
    {
        _logger.LogInformation("Creating new template: {TemplateName}", template?.TemplateName);
        var result = await _templateService.CreateAsync(template);
        return result.ToActionResult();
    }


    [HttpPut("{id}")]
    [EndpointPermission("templates.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateDto template)
    {
        _logger.LogInformation("Updating template: {TemplateId}", id);
        var result = await _templateService.UpdateAsync(id, template);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("templates.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting template: {TemplateId}", id);
        var result = await _templateService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
