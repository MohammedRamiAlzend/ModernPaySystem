using ModernPaySystem.Domain.Entities.Abstraction;

namespace ModernPaySystem.Domain.Entities.Archiving;

public class DocumentChunk : Entity<Guid>
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = default!;

    public int ChunkIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; }

    public DocumentChunkDto ToDto()
    {
        return new DocumentChunkDto
        {
            Id = Id,
            DocumentId = DocumentId,
            ChunkIndex = ChunkIndex,
            Content = Content,
            TokenCount = TokenCount
        };
    }
}
