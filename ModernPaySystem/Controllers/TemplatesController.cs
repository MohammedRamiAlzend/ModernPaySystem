using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Template management
/// Provides CRUD operations for templates.
/// </summary>
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

    /// <summary>
    /// Get all templates.
    /// </summary>
    [HttpGet]
    [EndpointPermission("templates.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all templates");
        var result = await _templateService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get template by id.
    /// </summary>
    [HttpGet("{id}")]
    [EndpointPermission("templates.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting template by id: {TemplateId}", id);
        var result = await _templateService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get template by name.
    /// </summary>
    [HttpGet("by-name/{name}")]
    [EndpointPermission("templates.get-by-name", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByName(string name)
    {
        _logger.LogInformation("Getting template by name: {Name}", name);
        var result = await _templateService.GetByNameAsync(name);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new template.
    /// </summary>
    [HttpPost]
    [EndpointPermission("templates.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateDto template)
    {
        _logger.LogInformation("Creating new template: {TemplateName}", template?.TemplateName);
        var result = await _templateService.CreateAsync(template);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update template.
    /// </summary>
    [HttpPut("{id}")]
    [EndpointPermission("templates.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTemplateDto template)
    {
        _logger.LogInformation("Updating template: {TemplateId}", id);
        var result = await _templateService.UpdateAsync(id, template);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete template.
    /// </summary>
    [HttpDelete("{id}")]
    [EndpointPermission("templates.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting template: {TemplateId}", id);
        var result = await _templateService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
