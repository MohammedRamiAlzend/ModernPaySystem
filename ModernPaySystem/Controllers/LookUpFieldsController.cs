namespace ModernPaySystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LookUpFieldsController : ControllerBase
{
    private readonly ILookUpFieldService _lookUpFieldService;
    private readonly ILogger<LookUpFieldsController> _logger;

    public LookUpFieldsController(ILookUpFieldService lookUpFieldService, ILogger<LookUpFieldsController> logger)
    {
        _lookUpFieldService = lookUpFieldService;
        _logger = logger;
    }

    [HttpGet]
    [EndpointPermission("lookupfields.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all lookup fields");
        var result = await _lookUpFieldService.GetAllAsync();
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    [EndpointPermission("lookupfields.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting lookup field by id: {LookUpFieldId}", id);
        var result = await _lookUpFieldService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpPost]
    [EndpointPermission("lookupfields.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] CreateLookUpFieldDto lookUpField)
    {
        _logger.LogInformation("Creating new lookup field: {FieldName}", lookUpField?.FiledName);
        ArgumentNullException.ThrowIfNull(lookUpField);
        var result = await _lookUpFieldService.CreateAsync(lookUpField);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [EndpointPermission("lookupfields.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLookUpFieldDto lookUpField)
    {
        _logger.LogInformation("Updating lookup field: {LookUpFieldId}", id);
        var result = await _lookUpFieldService.UpdateAsync(id, lookUpField);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("lookupfields.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting lookup field: {LookUpFieldId}", id);
        var result = await _lookUpFieldService.DeleteAsync(id);
        return result.ToActionResult();
    }

}
