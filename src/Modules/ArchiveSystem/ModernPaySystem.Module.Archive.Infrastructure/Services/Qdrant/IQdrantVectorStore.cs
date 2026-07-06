using ModernPaySystem.Module.Archive.Domain.Entities;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services.Qdrant;

public sealed class SearchHit
{
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public double Score { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public SearchSourceType SourceType { get; set; }
    public Guid? PhysicalFileId { get; set; }
    public Guid? ArchiveRecordId { get; set; }
    public string? ArchiveRecordNumber { get; set; }
}

public sealed class SearchFilter
{
    public SearchSourceType? SourceType { get; set; }
    public List<Guid>? ArchiveRecordIds { get; set; }
}

public interface IQdrantVectorStore
{
    Task InitializeAsync(CancellationToken ct = default);
    Task UpsertChunksAsync(Guid documentId, IReadOnlyList<DocumentChunk> chunks, float[][] embeddings, SearchSourceType sourceType, string fileName, Guid? physicalFileId, Guid? archiveRecordId, string? archiveRecordNumber = null, CancellationToken ct = default);
    Task<IReadOnlyList<SearchHit>> SearchAsync(float[] queryVector, int topK, double minScore, SearchFilter? filter = null, CancellationToken ct = default);
    Task DeleteDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<bool> CollectionExistsAsync(CancellationToken ct = default);
}
