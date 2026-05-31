using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Extensions;
using System.Net;

namespace ModernPaySystem.Controllers.ArchivingControllers;

[ApiController]
[Route("api/archive-records")]
[Authorize]
public class ArchiveRecordsController(IArchiveRecordService archiveRecordService, ILogger<ArchiveRecordsController> logger) : ControllerBase
{
    //[HttpGet]
    //[EndpointPermission("archiving.records.get-all", SubSystem.Archiving, PermissionType.Read)]
    //public async Task<IActionResult> GetAll()
    //{
    //    logger.LogInformation("Getting all archive records");
    //    var result = await archiveRecordService.GetAllAsync();
    //    return result.ToActionResult();
    //}

    //[HttpGet("paged")]
    //[EndpointPermission("archiving.records.get-paged", SubSystem.Archiving, PermissionType.Read)]
    //public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    //{
    //    logger.LogInformation("Getting paged archive records, page: {Page}, size: {PageSize}", page, pageSize);
    //    var result = await archiveRecordService.GetPagedAsync(page, pageSize);
    //    return result.ToActionResult();
    //}

    [HttpGet("{id}")]
    [EndpointPermission("archiving.records.get-by-id", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetById(Guid id)
    {
        logger.LogInformation("Getting archive record by id: {RecordId}", id);
        var result = await archiveRecordService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("folder/{folderId}")]
    [EndpointPermission("archiving.records.get-by-folder", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetByFolderId(Guid folderId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting archive records by folder: {FolderId}, page: {Page}, size: {PageSize}", folderId, page, pageSize);
        var result = await archiveRecordService.GetByFolderIdAsync(folderId, page, pageSize);
        return result.ToActionResult();
    }

    [HttpGet("form/{formId}")]
    [EndpointPermission("archiving.records.get-by-form", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetByFormId(Guid formId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        logger.LogInformation("Getting archive records by form: {FormId}, page: {Page}, size: {PageSize}", formId, page, pageSize);
        var result = await archiveRecordService.GetByFormIdAsync(formId, page, pageSize);
        return result.ToActionResult();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [EndpointPermission("archiving.records.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] CreateArchiveRecordDto dto)
    {
        logger.LogInformation("Creating archive record for folder {FolderId} and form {FormId}", dto?.FolderId, dto?.FormId);
        var result = await archiveRecordService.CreateAsync(dto!);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [EndpointPermission("archiving.records.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateArchiveRecordDto dto)
    {
        logger.LogInformation("Updating archive record: {RecordId}", id);
        var result = await archiveRecordService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpPost("{id}/files")]
    [Consumes("multipart/form-data")]
    [EndpointPermission("archiving.records.add-files", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> AddFiles(Guid id, [FromForm] IFormFileCollection files)
    {
        logger.LogInformation("Adding {FileCount} files to archive record {RecordId}", files?.Count, id);
        if (files == null || files.Count == 0)
        {
            logger.LogWarning("No files provided to add to archive record {RecordId}", id);
            return BadRequest("No files provided");
        }
        var result = await archiveRecordService.AddFilesAsync(id, files);
        return result.ToActionResult();
    }

    [HttpDelete("{id}/files/{fileId}")]
    [EndpointPermission("archiving.records.remove-file", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> RemoveFile(Guid id, Guid fileId)
    {
        logger.LogInformation("Removing file {FileId} from archive record {RecordId}", fileId, id);
        var result = await archiveRecordService.RemoveFileAsync(id, fileId);
        return result.ToActionResult();
    }

    [HttpGet("{recordId}/files/metadata")]
    [EndpointPermission("archiving.records.get-files-metadata", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetFilesMetadata(Guid recordId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeDeleted = false)
    {
        logger.LogInformation("Getting file metadata for archive record {RecordId}, page: {Page}, size: {PageSize}, includeDeleted: {IncludeDeleted}", recordId, page, pageSize, includeDeleted);
        var result = await archiveRecordService.GetFilesMetadataByRecordIdAsync(recordId, page, pageSize, includeDeleted);
        return result.ToActionResult();
    }

    [HttpGet("{recordId}/files/{fileId}")]
    [EndpointPermission("archiving.records.download-file", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> DownloadFile(Guid recordId, Guid fileId, [FromQuery] bool download = false)
    {
        logger.LogInformation("Downloading file {FileId} for archive record {RecordId}. ForcedDownload: {Download}", fileId, recordId, download);
        return await StreamArchiveFileAsync(fileId, download, recordId);
    }

    [HttpGet("files/{fileId}")]
    [EndpointPermission("archiving.records.download-file", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> DownloadFileById(Guid fileId, [FromQuery] bool download = false)
    {
        logger.LogInformation("Downloading file {FileId}. ForcedDownload: {Download}", fileId, download);
        return await StreamArchiveFileAsync(fileId, download);
    }

    [HttpDelete("{id}")]
    [EndpointPermission("archiving.records.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting archive record: {RecordId}", id);
        var result = await archiveRecordService.DeleteAsync(id);
        return result.ToActionResult();
    }

    private async Task<IActionResult> StreamArchiveFileAsync(Guid fileId, bool download, Guid? recordId = null)
    {
        var result = await archiveRecordService.GetPhysicalFileStreamAsync(fileId, recordId);
        if (result.IsError)
        {
            var topError = result.TopError;
            if (topError.HttpStatus == HttpStatusCode.Gone)
            {
                return StatusCode(StatusCodes.Status410Gone, new { errors = result.Errors });
            }

            return result.ToActionResult();
        }

        var file = result.Value!;
        Response.ContentLength = file.ContentLength;

        var contentDisposition = new ContentDispositionHeaderValue(download ? "attachment" : "inline")
        {
            FileNameStar = file.FileName,
            FileName = file.FileName
        };

        Response.Headers[HeaderNames.ContentDisposition] = contentDisposition.ToString();
        Response.Headers[HeaderNames.ContentLength] = file.ContentLength.ToString();

        return File(file.ContentStream, file.ContentType, enableRangeProcessing: true);
    }
}
