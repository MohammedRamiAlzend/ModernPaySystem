using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Archive.Api.Extensions;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Api.Controllers;

[ApiController]
[Route("api/semantic-search")]
[Authorize]
public class SemanticSearchController(
    ISemanticSearchService semanticSearchService,
    ILogger<SemanticSearchController> logger) : ControllerBase
{
    [HttpPost("search")]
    [EndpointPermission("semantic-search.query", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> Search([FromBody] SearchQueryDto query)
    {
        logger.LogInformation("Semantic search: {Query}", query.Query);
        var result = await semanticSearchService.SearchAsync(query);
        return result.ToActionResult();
    }

    [HttpGet("documents")]
    [EndpointPermission("semantic-search.documents.list", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetDocuments([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting paged indexed documents: page {Page}, size {PageSize}", page, pageSize);
        var result = await semanticSearchService.GetDocumentsPagedAsync(page, pageSize);
        return result.ToActionResult();
    }

    [HttpDelete("documents/{id:guid}")]
    [EndpointPermission("semantic-search.documents.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        logger.LogInformation("Deleting indexed document: {DocumentId}", id);
        var result = await semanticSearchService.DeleteDocumentAsync(id);
        return result.ToActionResult();
    }
}
