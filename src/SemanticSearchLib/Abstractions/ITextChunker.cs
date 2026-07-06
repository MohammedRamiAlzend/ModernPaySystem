using SemanticSearchLib.Models;

namespace SemanticSearchLib.Abstractions;

public interface ITextChunker
{
    List<TextChunk> ChunkText(string text, int chunkSize = 512, int overlap = 50);
}
