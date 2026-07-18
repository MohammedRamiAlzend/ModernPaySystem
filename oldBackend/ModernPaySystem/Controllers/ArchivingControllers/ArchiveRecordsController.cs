using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Auth;
using ModernPaySystem.Infrastructure.Extensions;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Net;
using ModernPaySystem.Domain.Commons;
using System.Text.Json;

namespace ModernPaySystem.Controllers.ArchivingControllers;

[ApiController]
[Route("api/archive-records")]
[Authorize]
public class ArchiveRecordsController(
    IArchiveRecordService archiveRecordService,
    IAuditLogService auditLogService,
    IArchiveAuthorizationService archiveAuthorizationService,
    IAuthorizationService authorizationService,
    IMemoryCache memoryCache,
    IDepartmentService departmentService,
    ILogger<ArchiveRecordsController> logger) : ControllerBase
{
    private static readonly ConcurrentDictionary<string, object> RateLimitLocks = new();

    //[HttpGet]
    //[EndpointPermission("archiving.records.get-all", SubSystem.Archiving, PermissionType.Read)]
    //public async Task<IActionResult> GetAll()
    //{
    //    logger.LogInformation("Getting all archive records");
    //    var result = await archiveRecordService.GetAllAsync();
    //    return result.ToActionResult();
    //}

    [HttpGet("paged")]
    [EndpointPermission("archiving.records.get-paged", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetPaged([FromQuery] ArchiveRecordPagedFilterDto filterDto)
    {
        logger.LogInformation("Getting paged archive records with filters");
        var result = await archiveRecordService.GetPagedAsync(filterDto);
        return result.ToActionResult();
    }

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
    [DisableRequestSizeLimit]
    [EndpointPermission("archiving.records.create", SubSystem.Archiving, PermissionType.Insert)]
    public async Task<IActionResult> Create([FromForm] CreateArchiveRecordDto dto)
    {
        logger.LogInformation("Creating archive record for folder {FolderId} and form {FormId}", dto?.FolderId, dto?.FormId);
        var result = await archiveRecordService.CreateAsync(dto!);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    [EndpointPermission("archiving.records.update", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateArchiveRecordDto dto)
    {
        logger.LogInformation("Updating archive record: {RecordId}", id);
        var result = await archiveRecordService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpPost("{id}/files")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
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

    /// <summary>
    /// Downloads all files for an archive record as a ZIP bundle.
    /// </summary>
    /// <param name="recordId">The archive record identifier.</param>
    /// <param name="flatten">When true, folders inside the archive are flattened into a single ZIP structure.</param>
    /// <param name="password">Optional ZIP password used to encrypt the bundle.</param>
    /// <param name="compression">The ZIP compression level. Allowed values are Optimal, Fastest, NoCompression, and SmallestSize.</param>
    /// <param name="includeMetadata">When true, includes archive metadata in the ZIP bundle.</param>
    [EndpointSummary("Download an archive record as a ZIP bundle.")]
    [EndpointDescription("Creates a ZIP archive containing the files for the selected archive record. Use flatten to ignore folder nesting, password to encrypt the ZIP, compression to control ZIP size versus speed, and includeMetadata to embed archive metadata in the bundle.")]
    [HttpGet("{recordId}/files/zip")]
    [EndpointPermission("archiving.records.download-zip", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> DownloadZip(Guid recordId, [FromQuery] bool flatten = false, [FromQuery] string? password = null, [FromQuery] CompressionLevel compression = CompressionLevel.Optimal, [FromQuery] bool includeMetadata = false)
    {
        logger.LogInformation("Downloading ZIP bundle for archive record {RecordId}. Flatten: {Flatten}, Compression: {Compression}, IncludeMetadata: {IncludeMetadata}", recordId, flatten, compression, includeMetadata);
        return await StreamArchiveZipAsync(recordId, flatten, password, compression, includeMetadata);
    }

    /// <summary>
    /// Returns paginated archive file results for a record.
    /// </summary>
    /// <param name="recordId">The archive record identifier.</param>
    /// <param name="pageNumber">The 1-based page number to retrieve.</param>
    /// <param name="pageSize">The number of files per page. The service accepts values from 1 to 100.</param>
    /// <param name="mode">Controls the payload shape. MetadataOnly returns file metadata only, WithUrls adds download and view URLs, and WithData also includes inline Base64 content for small files.</param>
    /// <param name="sortBy">The field used to sort the file results. Allowed values are CreatedAt, FileName, and FileSize.</param>
    /// <param name="sortOrder">The sort direction. Allowed values are Asc and Desc.</param>
    /// <param name="searchTerm">Optional case-insensitive search text applied to file names.</param>
    /// <param name="fileTypes">Optional file content-type filters, such as application/pdf or image/png.</param>
    [EndpointSummary("Get paginated archive file results.")]
    [EndpointDescription("Retrieves archive files in pages and supports three retrieval modes. MetadataOnly returns file metadata, WithUrls adds download and view links, and WithData additionally includes inline Base64 data for small files. Sorting is available by CreatedAt, FileName, or FileSize, and the sort order can be Asc or Desc. The fileTypes filter expects content types, for example application/pdf or image/png.")]
    [HttpGet("{recordId}/files/paginated")]
    [EndpointPermission("archiving.records.get-files-paginated", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetPaginatedFiles(
        Guid recordId,
        [FromQuery(Name = "pageNumber")] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ArchiveFileRetrievalMode mode = ArchiveFileRetrievalMode.MetadataOnly,
        [FromQuery] ArchiveFileSortBy sortBy = ArchiveFileSortBy.CreatedAt,
        [FromQuery] ArchiveFileSortOrder sortOrder = ArchiveFileSortOrder.Desc,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string[]? fileTypes = null)
    {
        return await GetPaginatedFilesInternal(recordId, pageNumber, pageSize, mode, sortBy, sortOrder, searchTerm, fileTypes, headOnly: false);
    }

    [HttpHead("{recordId}/files/paginated")]
    [EndpointPermission("archiving.records.get-files-paginated", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> HeadPaginatedFiles(
        Guid recordId,
        [FromQuery(Name = "pageNumber")] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ArchiveFileRetrievalMode mode = ArchiveFileRetrievalMode.MetadataOnly,
        [FromQuery] ArchiveFileSortBy sortBy = ArchiveFileSortBy.CreatedAt,
        [FromQuery] ArchiveFileSortOrder sortOrder = ArchiveFileSortOrder.Desc,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string[]? fileTypes = null)
    {
        if (mode != ArchiveFileRetrievalMode.MetadataOnly)
        {
            return BadRequest("HEAD requests are supported for MetadataOnly mode only.");
        }

        return await GetPaginatedFilesInternal(recordId, pageNumber, pageSize, mode, sortBy, sortOrder, searchTerm, fileTypes, headOnly: true);
    }

    [HttpGet("files/{fileId}")]
    [EndpointPermission("archiving.records.download-file", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> DownloadFileById(Guid fileId, [FromQuery] bool download = false, [FromQuery] bool includeDeleted = false)
    {
        logger.LogInformation("Downloading file {FileId}. ForcedDownload: {Download}, IncludeDeleted: {IncludeDeleted}", fileId, download, includeDeleted);
        return await StreamArchiveFileAsync(fileId, download, includeDeleted: includeDeleted);
    }

    [HttpPut("{id}/move")]
    [EndpointPermission("archiving.records.move", SubSystem.Archiving, PermissionType.Update)]
    public async Task<IActionResult> MoveRecord(Guid id, [FromBody] MoveArchiveRecordDto dto)
    {
        logger.LogInformation("Moving archive record {RecordId} to destination folder {DestinationFolderId}", id, dto.DestinationFolderId);
        var result = await archiveRecordService.MoveRecordAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [EndpointPermission("archiving.records.delete", SubSystem.Archiving, PermissionType.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        logger.LogInformation("Deleting archive record: {RecordId}", id);
        var recordResult = await archiveRecordService.GetByIdAsync(id);
        if (recordResult.IsError)
        {
            return recordResult.ToActionResult();
        }

        var record = recordResult.Value!;
        if (!record.DepartmentId.HasValue)
        {
            return BadRequest("Archive record is not scoped to a department.");
        }

        var authResult = await authorizationService.AuthorizeAsync(User, new ArchiveDepartmentScope(record.DepartmentId.Value), ArchiveAuthorizationPolicyExtensions.RequireDepartmentArchiveLeader);
        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var result = await archiveRecordService.DeleteAsync(id);
        return result.ToActionResult();
    }

    [HttpPost("{recordId}/print")]
    [EndpointPermission("archiving.records.print", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> LogPrint(Guid recordId)
    {
        logger.LogInformation("Logging print action for archive record: {RecordId}", recordId);
        var result = await archiveRecordService.LogPrintAsync(recordId);
        return result.ToActionResult();
    }

    [HttpGet("audit-logs")]
    [EndpointPermission("archiving.records.get-audit-logs", SubSystem.Archiving, PermissionType.Read)]
    public async Task<IActionResult> GetAuditLogsByDepartment(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] AuditAction? action = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? departmentId = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var leaderDepartmentsResult = await archiveAuthorizationService.GetUserArchiveLeaderDepartmentsAsync(userGuid);
        if (leaderDepartmentsResult.IsError)
        {
            return leaderDepartmentsResult.ToActionResult();
        }

        var leaderDepartments = leaderDepartmentsResult.Value!;
        if (leaderDepartments.Count == 0)
        {
            return Forbid();
        }

        Guid targetDepartmentId;
        if (departmentId.HasValue)
        {
            if (!leaderDepartments.Contains(departmentId.Value))
            {
                return Forbid();
            }
            targetDepartmentId = departmentId.Value;
        }
        else
        {
            targetDepartmentId = leaderDepartments[0];
        }

        logger.LogInformation("Getting audit logs for department: {DepartmentId}, page: {Page}, size: {PageSize}", targetDepartmentId, page, pageSize);

        var result = await auditLogService.GetAuditLogsByDepartmentAsync(targetDepartmentId, page, pageSize, action, fromDate, toDate);
        return result.ToActionResult();
    }

    [HttpGet("departments/led")]
    public async Task<IActionResult> GetLedDepartments()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var leaderDepartmentsResult = await archiveAuthorizationService.GetUserArchiveLeaderDepartmentsAsync(userGuid);
        if (leaderDepartmentsResult.IsError)
        {
            return leaderDepartmentsResult.ToActionResult();
        }

        var leaderDepartments = leaderDepartmentsResult.Value!;
        var departmentsList = new List<DepartmentDto>();

        foreach (var depId in leaderDepartments)
        {
            var depResult = await departmentService.GetByIdAsync(depId);
            if (!depResult.IsError && depResult.Value != null)
            {
                departmentsList.Add(depResult.Value);
            }
        }

        Result<List<DepartmentDto>> result = departmentsList;
        return result.ToActionResult();
    }

    private async Task<IActionResult> StreamArchiveFileAsync(Guid fileId, bool download, Guid? recordId = null, bool includeDeleted = false)
    {
        var result = await archiveRecordService.GetPhysicalFileStreamAsync(fileId, recordId, includeDeleted, isDownload: download);
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

    private async Task<IActionResult> StreamArchiveZipAsync(Guid recordId, bool flatten, string? password, CompressionLevel compression, bool includeMetadata)
    {
        var result = await archiveRecordService.GetZipBundleAsync(recordId, flatten, password, compression, includeMetadata, HttpContext.RequestAborted);
        if (result.IsError)
        {
            var topError = result.TopError;
            if (topError.HttpStatus == HttpStatusCode.Gone)
            {
                return StatusCode(StatusCodes.Status410Gone, new { errors = result.Errors });
            }

            if (topError.HttpStatus == HttpStatusCode.RequestTimeout)
            {
                return StatusCode(StatusCodes.Status408RequestTimeout, new { errors = result.Errors });
            }

            return result.ToActionResult();
        }

        var bundle = result.Value!;
        Response.ContentType = bundle.ContentType;
        Response.ContentLength = bundle.ContentLength;
        Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("attachment")
        {
            FileName = bundle.DownloadFileName,
            FileNameStar = bundle.DownloadFileName
        }.ToString();

        var stream = new FileStream(
            bundle.ZipFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return File(stream, bundle.ContentType, bundle.DownloadFileName, enableRangeProcessing: true);
    }

    private async Task<IActionResult> GetPaginatedFilesInternal(
        Guid recordId,
        int pageNumber,
        int pageSize,
        ArchiveFileRetrievalMode mode,
        ArchiveFileSortBy sortBy,
        ArchiveFileSortOrder sortOrder,
        string? searchTerm,
        string[]? fileTypes,
        bool headOnly)
    {
        var rateLimitResult = ApplyPaginatedFileRateLimit(recordId, mode);
        if (rateLimitResult is not null)
        {
            return rateLimitResult;
        }

        var result = await archiveRecordService.GetPaginatedFilesAsync(recordId, pageNumber, pageSize, mode, sortBy, sortOrder, searchTerm, fileTypes, HttpContext.RequestAborted);
        if (result.IsError)
        {
            return result.ToActionResult();
        }

        var payload = result.Value!;
        var responseBytes = JsonSerializer.SerializeToUtf8Bytes(payload);
        var etag = $"W/\"{Convert.ToHexString(SHA256.HashData(responseBytes))}\"";

        Response.Headers[HeaderNames.ETag] = etag;
        Response.Headers[HeaderNames.AcceptRanges] = "bytes";
        Response.Headers[HeaderNames.CacheControl] = "private, max-age=0, must-revalidate";

        if (Request.Headers.IfNoneMatch.Any(x => string.Equals(x, etag, StringComparison.Ordinal)))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        if (headOnly)
        {
            Response.ContentLength = responseBytes.Length;
            Response.Headers[HeaderNames.ContentType] = "application/json; charset=utf-8";
            Response.Headers[HeaderNames.ContentLength] = responseBytes.Length.ToString();
            return StatusCode(StatusCodes.Status200OK);
        }

        if (Request.Headers.Range.Count > 0)
        {
            if (!TryGetSingleRange(Request.Headers.Range.ToString(), responseBytes.Length, out var start, out var end))
            {
                return StatusCode(StatusCodes.Status416RangeNotSatisfiable);
            }

            var length = end - start + 1;
            var partialBytes = responseBytes.AsSpan(start, length).ToArray();
            Response.StatusCode = StatusCodes.Status206PartialContent;
            Response.Headers[HeaderNames.ContentRange] = $"bytes {start}-{end}/{responseBytes.Length}";
            Response.Headers[HeaderNames.ContentLength] = partialBytes.Length.ToString();
            Response.Headers[HeaderNames.ContentType] = "application/json; charset=utf-8";
            return File(partialBytes, "application/json; charset=utf-8");
        }

        Response.Headers[HeaderNames.ContentType] = "application/json; charset=utf-8";
        Response.Headers[HeaderNames.ContentLength] = responseBytes.Length.ToString();
        return File(responseBytes, "application/json; charset=utf-8");
    }

    private IActionResult? ApplyPaginatedFileRateLimit(Guid recordId, ArchiveFileRetrievalMode mode)
    {
        var clientKey = User?.Identity?.Name ?? HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        var threshold = mode == ArchiveFileRetrievalMode.WithData ? 6 : 30;
        var window = TimeSpan.FromMinutes(1);
        var cacheKey = $"archive-files-rate:{recordId:N}:{mode}:{clientKey}";
        var gate = RateLimitLocks.GetOrAdd(cacheKey, _ => new object());

        lock (gate)
        {
            var bucket = memoryCache.GetOrCreate(cacheKey, entry =>
            {
                entry.SlidingExpiration = window;
                return new RateLimitBucket(DateTime.UtcNow, 0);
            });

            bucket = DateTime.UtcNow - bucket.WindowStartUtc >= window
                ? new RateLimitBucket(DateTime.UtcNow, 1)
                : bucket with { Count = bucket.Count + 1 };
            memoryCache.Set(cacheKey, bucket, new MemoryCacheEntryOptions { SlidingExpiration = window });

            if (bucket.Count > threshold)
            {
                var retryAfter = (int)Math.Ceiling(Math.Max(1, (bucket.WindowStartUtc.Add(window) - DateTime.UtcNow).TotalSeconds));
                Response.Headers[HeaderNames.RetryAfter] = retryAfter.ToString();
                return StatusCode(StatusCodes.Status429TooManyRequests, new { errors = new[] { "Rate limit exceeded for this archive file query mode." } });
            }
        }

        return null;
    }

    private static bool TryGetSingleRange(string rangeHeader, int totalLength, out int start, out int end)
    {
        start = 0;
        end = totalLength - 1;

        if (!RangeHeaderValue.TryParse(rangeHeader, out var rangeValue) || rangeValue.Ranges.Count != 1)
        {
            return false;
        }

        var range = rangeValue.Ranges.First();
        if (!range.From.HasValue && !range.To.HasValue)
        {
            return false;
        }

        start = (int)(range.From ?? 0);
        end = (int)(range.To ?? (totalLength - 1));

        if (start < 0 || end < start || end >= totalLength)
        {
            return false;
        }

        return true;
    }

    private readonly record struct RateLimitBucket(DateTime WindowStartUtc, int Count);
}
