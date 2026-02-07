using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Extensions;

namespace ModernPaySystem.Controllers;

/// <summary>
/// API controller for Request management
/// Provides CRUD operations and request-specific queries
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(IRequestService requestService, ILogger<RequestsController> logger)
    {
        _requestService = requestService;
        _logger = logger;
    }

    /// <summary>
    /// Get all requests
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Getting all requests");
        var result = await _requestService.GetAllAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Get request by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("Getting request by id: {RequestId}", id);
        var result = await _requestService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get requests by requester id
    /// </summary>
    [HttpGet("by-requester/{requesterId}")]
    public async Task<IActionResult> GetByRequesterId(Guid requesterId)
    {
        _logger.LogInformation("Getting requests for requester: {RequesterId}", requesterId);
        var result = await _requestService.GetByRequesterIdAsync(requesterId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get requests by approver id
    /// </summary>
    [HttpGet("by-approver/{approverId}")]
    public async Task<IActionResult> GetByApproverId(Guid approverId)
    {
        _logger.LogInformation("Getting requests for approver: {ApproverId}", approverId);
        var result = await _requestService.GetByApproverIdAsync(approverId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Get requests by template id
    /// </summary>
    [HttpGet("by-template/{templateId}")]
    public async Task<IActionResult> GetByTemplateId(Guid templateId)
    {
        _logger.LogInformation("Getting requests for template: {TemplateId}", templateId);
        var result = await _requestService.GetByTemplateIdAsync(templateId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Create new request
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Request request)
    {
        _logger.LogInformation("Creating new request for requester: {RequesterId}", request?.RequesterId);
        var result = await _requestService.CreateAsync(request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Update request
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Request request)
    {
        _logger.LogInformation("Updating request: {RequestId}", id);
        var result = await _requestService.UpdateAsync(id, request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Delete request
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting request: {RequestId}", id);
        var result = await _requestService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
