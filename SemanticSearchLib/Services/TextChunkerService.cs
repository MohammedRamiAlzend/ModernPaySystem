using System.Text.RegularExpressions;
using SemanticSearchLib.Abstractions;
using SemanticSearchLib.Models;

namespace SemanticSearchLib.Services;

public partial class TextChunkerService : ITextChunker
{
    private static readonly Regex SentenceSplitter = SentenceSplitterRegex();

    public List<TextChunk> ChunkText(string text, int chunkSize = 512, int overlap = 50)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var sentences = SentenceSplitter.Split(text)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        var chunks = new List<TextChunk>();
        var currentChunk = new List<string>();
        var currentLength = 0;

        foreach (var sentence in sentences)
        {
            if (currentLength + sentence.Length > chunkSize && currentChunk.Count > 0)
            {
                chunks.Add(new TextChunk
                {
                    Content = string.Join(" ", currentChunk),
                    Index = chunks.Count,
                    TokenCount = EstimateTokenCount(string.Join(" ", currentChunk))
                });

                // Apply overlap: keep last N words
                if (overlap > 0 && currentChunk.Count > 0)
                {
                    var overlapWords = string.Join(" ", currentChunk)
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                        .TakeLast(overlap)
                        .ToList();

                    currentChunk = overlapWords;
                    currentLength = string.Join(" ", overlapWords).Length;
                }
                else
                {
                    currentChunk = [];
                    currentLength = 0;
                }
            }

            currentChunk.Add(sentence);
            currentLength += sentence.Length;
        }

        if (currentChunk.Count > 0)
        {
            chunks.Add(new TextChunk
            {
                Content = string.Join(" ", currentChunk),
                Index = chunks.Count,
                TokenCount = EstimateTokenCount(string.Join(" ", currentChunk))
            });
        }

        return chunks;
    }

    private static int EstimateTokenCount(string text)
    {
        return (int)Math.Ceiling(text.Length / 3.5);
    }

    [GeneratedRegex(@"(?<=[.!?\u060C\u061F\u0621-\u064A])\s+", RegexOptions.Compiled)]
    private static partial Regex SentenceSplitterRegex();
}
