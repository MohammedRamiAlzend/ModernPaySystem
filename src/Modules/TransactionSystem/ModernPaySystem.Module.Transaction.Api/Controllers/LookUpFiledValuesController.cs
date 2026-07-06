using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Transaction.Api.Extensions;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookUpFiledValuesController(ILookUpFiledValuesService lookUpFiledValuesService, ILogger<LookUpFiledValuesController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("lookupfieldvalues.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all lookup field values");
        var result = await lookUpFiledValuesService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("lookupfieldvalues.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting lookup field value by id: {LookUpFiledValueId}", id);
        var result = await lookUpFiledValuesService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("lookupfieldvalues.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateLookUpFiledValuesDto lookUpFiledValue)
    {
        logger.LogInformation("Creating new lookup field value with ID: {LookUpFiledId}", lookUpFiledValue?.LookUpFiledId);
        ArgumentNullException.ThrowIfNull(lookUpFiledValue);
        var result = await lookUpFiledValuesService.CreateAsync(lookUpFiledValue);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("lookupfieldvalues.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLookUpFiledValuesDto lookUpFiledValue)
    {
        logger.LogInformation("Updating lookup field value: {LookUpFiledValueId}", id);
        var result = await lookUpFiledValuesService.UpdateAsync(id, lookUpFiledValue);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("lookupfieldvalues.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting lookup field value: {LookUpFiledValueId}", id);
        var result = await lookUpFiledValuesService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("by-lookup-field/{lookUpFieldId}")]
    [EndpointPermission("lookupfieldvalues.get-by-lookup-field", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByLookUpFieldId(Guid lookUpFieldId)
    {
        logger.LogInformation("Getting lookup field values by lookup field ID: {LookUpFieldId}", lookUpFieldId);
        var result = await lookUpFiledValuesService.GetByLookUpFieldIdAsync(lookUpFieldId);
        return result.ToActionResult();
    }
}
