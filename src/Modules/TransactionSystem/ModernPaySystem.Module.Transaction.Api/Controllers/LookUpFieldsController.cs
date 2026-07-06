using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Transaction.Api.Extensions;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Api.Controllers;

[ApiController]
[Route("api/transaction/[controller]")]
[Authorize]
public class LookUpFieldsController(ILookUpFieldService lookUpFieldService, ILogger<LookUpFieldsController> logger) : ControllerBase
{
    [HttpGet]
    [EndpointPermission("lookupfields.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        logger.LogInformation("Getting all lookup fields");
        var result = await lookUpFieldService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("lookupfields.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting lookup field by id: {LookUpFieldId}", id);
        var result = await lookUpFieldService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("lookupfields.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateLookUpFieldDto lookUpField)
    {
        logger.LogInformation("Creating new lookup field: {FieldName}", lookUpField?.FiledName);
        ArgumentNullException.ThrowIfNull(lookUpField);
        var result = await lookUpFieldService.CreateAsync(lookUpField);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("lookupfields.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLookUpFieldDto lookUpField)
    {
        logger.LogInformation("Updating lookup field: {LookUpFieldId}", id);
        var result = await lookUpFieldService.UpdateAsync(id, lookUpField);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("lookupfields.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting lookup field: {LookUpFieldId}", id);
        var result = await lookUpFieldService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
