using ModernPaySystem.Domain.Entities.Abstraction;
using System.Diagnostics.Contracts;
using System.IO;

namespace ModernPaySystem.Domain.Entities.Archiving;

/// <summary>
/// Controls how archive file data is returned by the paginated file endpoint.
/// </summary>
public enum ArchiveFileRetrievalMode
{
    /// <summary>
    /// Returns file metadata only.
    /// </summary>
    MetadataOnly = 0,

    /// <summary>
    /// Returns file metadata and download/view URLs.
    /// </summary>
    WithUrls = 1,

    /// <summary>
    /// Returns file metadata, URLs, and inline Base64 data for small files.
    /// </summary>
    WithData = 2
}

/// <summary>
/// Specifies the field used to sort paginated archive files.
/// </summary>
public enum ArchiveFileSortBy
{
    /// <summary>
    /// Sort by creation time.
    /// </summary>
    CreatedAt = 0,

    /// <summary>
    /// Sort by file name.
    /// </summary>
    FileName = 1,

    /// <summary>
    /// Sort by file size.
    /// </summary>
    FileSize = 2
}

/// <summary>
/// Specifies the ordering direction for paginated archive files.
/// </summary>
public enum ArchiveFileSortOrder
{
    /// <summary>
    /// Sort from lowest to highest.
    /// </summary>
    Asc = 0,

    /// <summary>
    /// Sort from highest to lowest.
    /// </summary>
    Desc = 1
}

public class PhysicalFile : Entity<Guid>, IAuditableEntity
{
    public Guid ArchiveRecordId { get; set; }
    public ArchiveRecord ArchiveRecord { get; set; } = default!;

    public Guid? EditArchiveRequestId { get; set; }
    public EditArchiveRequest? EditArchiveRequest { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool IsQrPage { get; set; } = false;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public PhysicalFileDto ToDto()
    {
        return new PhysicalFileDto
        {
            Id = Id,
            ArchiveRecordId = ArchiveRecordId,
            EditArchiveRequestId = EditArchiveRequestId,
            FileName = FileName,
            FileExtension = FileExtension,
            StoragePath = StoragePath,
            FileSize = FileSize,
            ContentType = ContentType,
            IsDeleted = IsDeleted,
            IsQrPage = IsQrPage,
            DeletedAt = DeletedAt,
            DeletedByUserId = DeletedByUserId,
            CreatedByUserId = CreatedByUserId,
            CreatedAt = CreatedAt,
            UpdatedByUserId = UpdatedByUserId,
            UpdatedAt = UpdatedAt
        };
    }
}

public class PhysicalFileDto
{
    public Guid Id { get; set; }
    public Guid ArchiveRecordId { get; set; }
    public Guid? EditArchiveRequestId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public bool IsQrPage { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedByUserId { get; set; }
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PhysicalFileMetadataDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public required bool IsQrPage { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public Guid ArchiveRecordId { get; set; }
}

public class ArchiveRecordFilesMetadataPageDto
{
    public Guid RecordId { get; set; }
    public int TotalCount { get; set; }
    public List<PhysicalFileMetadataDto> Files { get; set; } = [];
}

public class ArchiveRecordZipBundleDto
{
    public Guid ArchiveRecordId { get; set; }
    public string ZipFilePath { get; set; } = string.Empty;
    public string DownloadFileName { get; set; } = string.Empty;
    public long ContentLength { get; set; }
    public string ContentType { get; set; } = "application/zip";
}

public class ArchivePhysicalFilePageItemDto
{
    public Guid Id { get; set; }
    public Guid ArchiveRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public required bool IsQrPage { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ViewUrl { get; set; }
    public string? Base64Data { get; set; }
}

public class PagedFileResult<TItem>
    where TItem : notnull
{
    public Guid RecordId { get; set; }
    public List<TItem> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
    public long TotalSize { get; set; }
    public double AverageSize { get; set; }
    public Dictionary<string, int> FileTypeBreakdown { get; set; } = [];
}

public class ArchivePhysicalFileDownloadDto
{
    public Guid FileId { get; set; }
    public Guid ArchiveRecordId { get; set; }
    public required bool IsQrPage { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long ContentLength { get; set; }
    public Stream ContentStream { get; set; } = Stream.Null;
}

public class ArchiveFileConsistencyDto
{
    public Guid ArchiveRecordId { get; set; }
    public List<Guid> MissingPhysicalFileIds { get; set; } = [];
    public List<string> MissingStoragePaths { get; set; } = [];
    public List<string> OrphanStoragePaths { get; set; } = [];
    public bool IsConsistent => MissingPhysicalFileIds.Count == 0 && MissingStoragePaths.Count == 0 && OrphanStoragePaths.Count == 0;
}

public class ArchiveFileCleanupDto
{
    public int FilesDeleted { get; set; }
    public List<string> DeletedStoragePaths { get; set; } = [];
    public List<string> FailedStoragePaths { get; set; } = [];
}
