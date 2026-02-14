using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for LookUpFiledValues management
/// Provides CRUD operations for lookup field values.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookUpFiledValuesController : ControllerBase
{
    private readonly ILookUpFiledValuesService _lookUpFiledValuesService;
    private readonly ILogger<LookUpFiledValuesController> _logger;

    public LookUpFiledValuesController(ILookUpFiledValuesService lookUpFiledValuesService, ILogger<LookUpFiledValuesController> logger)
    {
        _lookUpFiledValuesService = lookUpFiledValuesService;
        _logger = logger;
    }

    /// <summary>
    /// Get all lookup field values.
    /// </summary>
    [HttpGet]
    [EndpointPermission("lookupfieldvalues.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all lookup field values");
        var result = await _lookUpFiledValuesService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get lookup field value by id.
    /// </summary>
    [HttpGet("{id}")]
    [EndpointPermission("lookupfieldvalues.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting lookup field value by id: {LookUpFiledValueId}", id);
        var result = await _lookUpFiledValuesService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new lookup field value.
    /// </summary>
    [HttpPost]
    [EndpointPermission("lookupfieldvalues.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateLookUpFiledValuesDto lookUpFiledValue)
    {
        _logger.LogInformation("Creating new lookup field value with ID: {LookUpFiledId}", lookUpFiledValue?.LookUpFiledId);
        var result = await _lookUpFiledValuesService.CreateAsync(lookUpFiledValue);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update lookup field value.
    /// </summary>
    [HttpPut("{id}")]
    [EndpointPermission("lookupfieldvalues.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLookUpFiledValuesDto lookUpFiledValue)
    {
        _logger.LogInformation("Updating lookup field value: {LookUpFiledValueId}", id);
        var result = await _lookUpFiledValuesService.UpdateAsync(id, lookUpFiledValue);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete lookup field value.
    /// </summary>
    [HttpDelete("{id}")]
    [EndpointPermission("lookupfieldvalues.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting lookup field value: {LookUpFiledValueId}", id);
        var result = await _lookUpFiledValuesService.DeleteAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get lookup field values by lookup field ID.
    /// </summary>
    [HttpGet("by-lookup-field/{lookUpFieldId}")]
    [EndpointPermission("lookupfieldvalues.get-by-lookup-field", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByLookUpFieldId(Guid lookUpFieldId)
    {
        _logger.LogInformation("Getting lookup field values by lookup field ID: {LookUpFieldId}", lookUpFieldId);
        var result = await _lookUpFiledValuesService.GetByLookUpFieldIdAsync(lookUpFieldId);
        return result.ToActionResult();
    }
}