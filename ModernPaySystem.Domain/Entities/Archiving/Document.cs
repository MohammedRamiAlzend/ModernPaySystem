using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.Archiving;

public enum SearchSourceType
{
    PhysicalFile = 1,
    ArchiveRecord = 2,
    FormValue = 3
}

public class Document : Entity<Guid>, IAuditableEntity
{
    public SearchSourceType SourceType { get; set; }

    public Guid? PhysicalFileId { get; set; }
    public PhysicalFile? PhysicalFile { get; set; }

    public Guid? ArchiveRecordId { get; set; }
    public ArchiveRecord? ArchiveRecord { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int TotalChunks { get; set; }
    public string? ExtractedText { get; set; }

    public ICollection<DocumentChunk> Chunks { get; set; } = [];

    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public DocumentDto ToDto()
    {
        return new DocumentDto
        {
            Id = Id,
            SourceType = SourceType,
            PhysicalFileId = PhysicalFileId,
            ArchiveRecordId = ArchiveRecordId,
            FileName = FileName,
            FileType = FileType,
            FileSizeBytes = FileSizeBytes,
            TotalChunks = TotalChunks,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public SearchSourceType SourceType { get; set; }
    public Guid? PhysicalFileId { get; set; }
    public Guid? ArchiveRecordId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int TotalChunks { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DocumentChunkDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; }
}

public class SearchQueryDto
{
    public string Query { get; set; } = string.Empty;
    public int TopK { get; set; } = 10;
    public double MinScore { get; set; } = 0.7;
    public SearchSourceType? SourceType { get; set; }
    public Guid? ArchiveRecordId { get; set; }
    public Guid? FolderId { get; set; }
}

public class SearchResultDto
{
    public Guid DocumentId { get; set; }
    public Guid ChunkId { get; set; }
    public SearchSourceType SourceType { get; set; }
    public Guid? PhysicalFileId { get; set; }
    public Guid? ArchiveRecordId { get; set; }
    public string? ArchiveRecordNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public double Score { get; set; }
}
