using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Response management
/// Provides CRUD operations and response-specific queries
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResponsesController : ControllerBase
{
    private readonly IResponseService _responseService;
    private readonly ILogger<ResponsesController> _logger;

    public ResponsesController(IResponseService responseService, ILogger<ResponsesController> logger)
    {
        _responseService = responseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all responses
    /// </summary>
    [HttpGet]
    [EndpointPermission("responses.get-all", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all responses");
        var result = await _responseService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get response by id
    /// </summary>
    [HttpGet("{id}")]
    [EndpointPermission("responses.get-by-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting response by id: {ResponseId}", id);
        var result = await _responseService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get responses by request id
    /// </summary>
    [HttpGet("by-request/{requestId}")]
    [EndpointPermission("responses.get-by-request-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByRequestId(Guid requestId)
    {
        _logger.LogInformation("Getting responses for request: {RequestId}", requestId);
        var result = await _responseService.GetByRequestIdAsync(requestId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get responses by responder id
    /// </summary>
    [HttpGet("by-responder/{responderId}")]
    [EndpointPermission("responses.get-by-responder-id", SubSystem.TransactionSystem, PermissionType.Read)]
    public async Task<IActionResult> GetByResponderId(Guid responderId)
    {
        _logger.LogInformation("Getting responses for responder: {ResponderId}", responderId);
        var result = await _responseService.GetByResponderIdAsync(responderId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new response
    /// </summary>
    [HttpPost]
    [EndpointPermission("responses.create", SubSystem.TransactionSystem, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromBody] Response response)
    {
        _logger.LogInformation("Creating new response for request: {RequestId}", response?.RequestId);
        var result = await _responseService.CreateAsync(response);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update response
    /// </summary>
    [HttpPut("{id}")]
    [EndpointPermission("responses.update", SubSystem.TransactionSystem, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromBody] Response response)
    {
        _logger.LogInformation("Updating response: {ResponseId}", id);
        var result = await _responseService.UpdateAsync(id, response);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete response
    /// </summary>
    [HttpDelete("{id}")]
    [EndpointPermission("responses.delete", SubSystem.TransactionSystem, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting response: {ResponseId}", id);
        var result = await _responseService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
