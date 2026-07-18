using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace ModernPaySystem.Infrastructure.Services.Qdrant;

public class QdrantVectorStore(
    IOptions<QdrantOptions> options,
    ILogger<QdrantVectorStore> logger) : IQdrantVectorStore
{
    private const int VectorSize = 1024;

    private readonly QdrantOptions _options = options.Value;
    private QdrantClient? _client;
    private bool _initialized;

    private QdrantClient Client => _client ??= CreateClient();

    private QdrantClient CreateClient()
    {
        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            var channel = QdrantChannel.ForAddress(
                $"{( _options.UseTls ? "https" : "http" )}://{_options.Host}:{_options.Port}",
                new ClientConfiguration
                {
                    ApiKey = _options.ApiKey,
                    CertificateThumbprint = null
                });
            var grpcClient = new QdrantGrpcClient(channel);
            return new QdrantClient(grpcClient);
        }

        return !_options.UseTls
            ? new QdrantClient(_options.Host, _options.Port)
            : new QdrantClient($"https://{_options.Host}:{_options.Port}");
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized) return;

        var exists = await Client.CollectionExistsAsync(_options.CollectionName, ct);
        if (!exists)
        {
            logger.LogInformation("Creating Qdrant collection '{Collection}' with {Size} dimensions",
                _options.CollectionName, VectorSize);

            await Client.CreateCollectionAsync(_options.CollectionName,
                new VectorParams { Size = VectorSize, Distance = Distance.Cosine }, cancellationToken: ct);
        }

        _initialized = true;
    }

    public async Task UpsertChunksAsync(
        Guid documentId,
        IReadOnlyList<DocumentChunk> chunks,
        float[][] embeddings,
        SearchSourceType sourceType,
        string fileName,
        Guid? physicalFileId,
        Guid? archiveRecordId,
        string? archiveRecordNumber = null,
        CancellationToken ct = default)
    {
        await InitializeAsync(ct);

        var points = new List<PointStruct>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var embedding = embeddings[i];

            var point = new PointStruct
            {
                Id = chunk.Id,
                Vectors = embedding,
                Payload =
                {
                    ["document_id"] = documentId.ToString(),
                    ["chunk_index"] = (ulong)chunk.ChunkIndex,
                    ["content"] = chunk.Content,
                    ["token_count"] = (ulong)chunk.TokenCount,
                    ["file_name"] = fileName,
                    ["source_type"] = ((int)sourceType).ToString(),
                    ["physical_file_id"] = physicalFileId?.ToString() ?? "",
                    ["archive_record_id"] = archiveRecordId?.ToString() ?? "",
                    ["archive_record_number"] = archiveRecordNumber ?? ""
                }
            };

            points.Add(point);
        }

        await Client.UpsertAsync(_options.CollectionName, points, cancellationToken: ct);
        logger.LogDebug("Upserted {Count} vectors for document {DocId}", chunks.Count, documentId);
    }

    public async Task<IReadOnlyList<SearchHit>> SearchAsync(
        float[] queryVector,
        int topK,
        double minScore,
        SearchFilter? filter = null,
        CancellationToken ct = default)
    {
        await InitializeAsync(ct);

        var qdrantFilter = BuildFilter(filter);

        var results = await Client.SearchAsync(
            _options.CollectionName,
            new ReadOnlyMemory<float>(queryVector),
            filter: qdrantFilter,
            limit: (ulong)topK,
            offset: 0,
            scoreThreshold: (float)minScore,
            cancellationToken: ct);

        var hits = new List<SearchHit>();
        foreach (var result in results)
        {
            var payload = result.Payload;
            var documentIdStr = payload["document_id"]?.StringValue ?? "";

            hits.Add(new SearchHit
            {
                ChunkId = Guid.Parse(result.Id.Uuid),
                DocumentId = Guid.TryParse(documentIdStr, out var docId) ? docId : Guid.Empty,
                Score = result.Score,
                ChunkIndex = (int)(payload["chunk_index"]?.IntegerValue ?? 0),
                Content = payload["content"]?.StringValue ?? "",
                FileName = payload["file_name"]?.StringValue ?? "",
                SourceType = payload["source_type"]?.StringValue is { } st && Enum.TryParse<SearchSourceType>(st, out var parsed) ? parsed : SearchSourceType.PhysicalFile,
                PhysicalFileId = TryParseGuid(payload["physical_file_id"]?.StringValue),
                ArchiveRecordId = TryParseGuid(payload["archive_record_id"]?.StringValue),
                ArchiveRecordNumber = payload["archive_record_number"]?.StringValue
            });
        }

        return hits;
    }

    public async Task DeleteDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        await InitializeAsync(ct);

        var filter = new Filter();
        filter.Must.Add(MatchKeyword("document_id", documentId.ToString()));

        await Client.DeleteAsync(_options.CollectionName, filter, cancellationToken: ct);
        logger.LogDebug("Deleted vectors for document {DocId}", documentId);
    }

    public async Task<bool> CollectionExistsAsync(CancellationToken ct = default)
    {
        return await Client.CollectionExistsAsync(_options.CollectionName, ct);
    }

    private static Filter? BuildFilter(SearchFilter? filter)
    {
        if (filter is null) return null;

        var qdrantFilter = new Filter();

        if (filter.SourceType.HasValue)
            qdrantFilter.Must.Add(MatchKeyword("source_type", ((int)filter.SourceType.Value).ToString()));

        if (filter.ArchiveRecordIds?.Count > 0)
        {
            foreach (var id in filter.ArchiveRecordIds)
                qdrantFilter.Should.Add(MatchKeyword("archive_record_id", id.ToString()));
        }

        return qdrantFilter.Must.Count > 0 || qdrantFilter.Should.Count > 0 ? qdrantFilter : null;
    }

    private static Guid? TryParseGuid(string? value)
    {
        return value is not null && Guid.TryParse(value, out var guid) ? guid : null;
    }
}
