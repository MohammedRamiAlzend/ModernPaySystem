using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModernPaySystem.Module.Archive.Api.Extensions;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Api.Controllers;

[ApiController]
[Route("api/document-indexing")]
[Authorize]
public class DocumentIndexingController(
    ISemanticSearchService semanticSearchService,
    ILogger<DocumentIndexingController> logger) : ControllerBase
{
    [HttpPost("physical-file/{id:guid}")]
    [EndpointPermission("semantic-search.index.physical-file", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> IndexPhysicalFile(Guid id)
    {
        logger.LogInformation("Indexing physical file: {FileId}", id);
        var result = await semanticSearchService.IndexPhysicalFileAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("archive-record/{id:guid}")]
    [EndpointPermission("semantic-search.index.archive-record", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> IndexArchiveRecord(Guid id)
    {
        logger.LogInformation("Indexing archive record: {RecordId}", id);
        var result = await semanticSearchService.IndexArchiveRecordAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("physical-file/{id:guid}/reindex")]
    [EndpointPermission("semantic-search.reindex.physical-file", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> ReIndexPhysicalFile(Guid id)
    {
        logger.LogInformation("Re-indexing physical file: {FileId}", id);
        var result = await semanticSearchService.ReIndexPhysicalFileAsync(id);
        return result.ToActionResult();
    }
}
