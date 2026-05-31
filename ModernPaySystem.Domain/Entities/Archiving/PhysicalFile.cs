using ModernPaySystem.Domain.Entities.Abstraction;
using System.IO;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class PhysicalFile : Entity<Guid>, IAuditableEntity
{
    public Guid ArchiveRecordId { get; set; }
    public ArchiveRecord ArchiveRecord { get; set; } = default!;

    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

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
            FileName = FileName,
            FileExtension = FileExtension,
            StoragePath = StoragePath,
            FileSize = FileSize,
            ContentType = ContentType,
            IsDeleted = IsDeleted,
            DeletedAt = DeletedAt,
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
    public string FileName { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
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

public class ArchivePhysicalFileDownloadDto
{
    public Guid FileId { get; set; }
    public Guid ArchiveRecordId { get; set; }
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
