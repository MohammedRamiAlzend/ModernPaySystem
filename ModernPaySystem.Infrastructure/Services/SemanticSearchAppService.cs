using System.IO;
using FileManager.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.Archiving;
using ModernPaySystem.Infrastructure.Persistence.UnitOfWork;
using ModernPaySystem.Infrastructure.Services.Qdrant;
using OcrReader;
using SemanticSearchLib.Abstractions;
using SemanticSearchLib.Models;

namespace ModernPaySystem.Infrastructure.Services;

public class SemanticSearchAppService(
    IUnitOfWork unitOfWork,
    IEmbeddingProvider embeddingProvider,
    IFileParser fileParser,
    ITextChunker textChunker,
    IFileManager fileManager,
    IOcrGenerator ocrGenerator,
    IQdrantVectorStore qdrantVectorStore,
    IOptions<SemanticSearchOptions> options,
    ILogger<SemanticSearchAppService> logger) : ISemanticSearchService
{
    private readonly SemanticSearchOptions _options = options.Value;

    public async Task<Result<DocumentDto>> IndexPhysicalFileAsync(Guid physicalFileId, CancellationToken ct = default)
    {
        try
        {
            var physicalFileResult = await unitOfWork.PhysicalFiles.GetByIdAsync(physicalFileId);
            if (physicalFileResult.IsError)
                return ApplicationErrors.ArchiveRecordNotFound;

            var physicalFile = physicalFileResult.Value!;
            var fileExt = physicalFile.FileExtension.ToLowerInvariant();

            if (!fileParser.SupportsFileType(fileExt))
                return Error.Validation("UnsupportedFileType", $"File type '{fileExt}' is not supported for indexing.");

            var filePath = NormalizePath(physicalFile.StoragePath);
            if (!System.IO.File.Exists(filePath))
                return ApplicationErrors.FileNotFound(filePath);

            string text;
            if (IsOcrImage(fileExt))
            {
                text = await ocrGenerator.ExtractTextFromImageAsync(filePath, "eng+ara");
            }
            else if (IsOcrPdf(fileExt))
            {
                text = await ocrGenerator.ExtractTextFromPdfAsync(filePath, "eng+ara");
            }
            else
            {
                await using (var stream = System.IO.File.OpenRead(filePath))
                {
                    text = await fileParser.ParseAsync(stream, fileExt, ct);
                }
            }

            if (string.IsNullOrWhiteSpace(text))
                return Error.Validation("EmptyContent", "No extractable text found in the file.");

            await RemoveExistingDocumentByPhysicalFileAsync(physicalFileId, ct);

            var chunks = textChunker.ChunkText(text, _options.ChunkSize, _options.ChunkOverlap);
            if (chunks.Count == 0)
                return Error.Validation("NoChunks", "Text could not be split into chunks.");

            var embeddings = await embeddingProvider.GenerateEmbeddingsBatchAsync(
                chunks.Select(c => c.Content).ToList(), ct);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                SourceType = SearchSourceType.PhysicalFile,
                PhysicalFileId = physicalFileId,
                ArchiveRecordId = physicalFile.ArchiveRecordId,
                FileName = physicalFile.FileName,
                FileType = fileExt,
                FileSizeBytes = physicalFile.FileSize,
                TotalChunks = chunks.Count,
                ExtractedText = text
            };

            var addDocResult = await unitOfWork.Documents.AddAsync(document);
            if (addDocResult.IsError) return addDocResult.Errors;

            var docChunks = new List<DocumentChunk>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    ChunkIndex = chunks[i].Index,
                    Content = chunks[i].Content,
                    TokenCount = chunks[i].TokenCount
                };

                var addChunkResult = await unitOfWork.DocumentChunks.AddAsync(chunk);
                if (addChunkResult.IsError) return addChunkResult.Errors;
                docChunks.Add(chunk);
            }

            await unitOfWork.SaveChangesAsync();

            await qdrantVectorStore.UpsertChunksAsync(document.Id, docChunks, embeddings.ToArray(), ct);

            logger.LogInformation("Indexed physical file {FileId} as document {DocId} with {ChunkCount} chunks",
                physicalFileId, document.Id, chunks.Count);

            return document.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to index physical file {FileId}", physicalFileId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<DocumentDto>> IndexArchiveRecordAsync(Guid archiveRecordId, CancellationToken ct = default)
    {
        try
        {
            var archiveResult = await unitOfWork.ArchiveRecords.GetAsync(
                filter: ar => ar.Id == archiveRecordId,
                transform: q => q.Include(ar => ar.ArchiveRecordTemplateValuesId!)
                    .ThenInclude(artv => artv.ArchiveRecordFormInputValues));

            if (archiveResult.IsError)
                return ApplicationErrors.ArchiveRecordNotFound;

            var archiveRecord = archiveResult.Value!;

            var textParts = new List<string> { archiveRecord.ArchivalNumber };

            if (archiveRecord.ArchiveRecordTemplateValuesId?.ArchiveRecordFormInputValues is not null)
            {
                foreach (var input in archiveRecord.ArchiveRecordTemplateValuesId.ArchiveRecordFormInputValues)
                {
                    if (!string.IsNullOrWhiteSpace(input.Value))
                    {
                        textParts.Add($"{input.Key}: {input.Value}");
                    }
                }
            }

            var text = string.Join("\n", textParts);
            if (string.IsNullOrWhiteSpace(text))
                return Error.Validation("EmptyContent", "Archive record has no indexable content.");

            await RemoveExistingDocumentByArchiveRecordAsync(archiveRecordId, ct);

            var chunks = textChunker.ChunkText(text, _options.ChunkSize, _options.ChunkOverlap);
            if (chunks.Count == 0)
                return Error.Validation("NoChunks", "Text could not be split into chunks.");

            var embeddings = await embeddingProvider.GenerateEmbeddingsBatchAsync(
                chunks.Select(c => c.Content).ToList(), ct);

            var document = new Document
            {
                Id = Guid.NewGuid(),
                SourceType = SearchSourceType.ArchiveRecord,
                ArchiveRecordId = archiveRecordId,
                FileName = $"ArchiveRecord_{archiveRecord.ArchivalNumber}",
                FileType = ".metadata",
                FileSizeBytes = text.Length,
                TotalChunks = chunks.Count,
                ExtractedText = text
            };

            var addDocResult = await unitOfWork.Documents.AddAsync(document);
            if (addDocResult.IsError) return addDocResult.Errors;

            var docChunks = new List<DocumentChunk>();
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = new DocumentChunk
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document.Id,
                    ChunkIndex = chunks[i].Index,
                    Content = chunks[i].Content,
                    TokenCount = chunks[i].TokenCount
                };

                var addChunkResult = await unitOfWork.DocumentChunks.AddAsync(chunk);
                if (addChunkResult.IsError) return addChunkResult.Errors;
                docChunks.Add(chunk);
            }

            await unitOfWork.SaveChangesAsync();

            await qdrantVectorStore.UpsertChunksAsync(document.Id, docChunks, embeddings.ToArray(), ct);

            logger.LogInformation("Indexed archive record {RecordId} as document {DocId} with {ChunkCount} chunks",
                archiveRecordId, document.Id, chunks.Count);

            return document.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to index archive record {RecordId}", archiveRecordId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<SearchResultDto>>> SearchAsync(SearchQueryDto query, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query.Query))
                return Error.Validation("EmptyQuery", "Search query cannot be empty.");

            var queryEmbedding = await embeddingProvider.GenerateEmbeddingAsync(query.Query, ct);

            var topK = Math.Clamp(query.TopK, 1, 100);
            var minScore = Math.Clamp(query.MinScore, 0.0, 1.0);

            var filter = new SearchFilter
            {
                SourceType = query.SourceType,
                ArchiveRecordId = query.ArchiveRecordId,
                PhysicalFileId = query.PhysicalFileId
            };

            var results = await qdrantVectorStore.SearchAsync(queryEmbedding, topK, minScore, filter, ct);

            var dtos = results.Select(r => new SearchResultDto
            {
                DocumentId = r.DocumentId,
                ChunkId = r.ChunkId,
                SourceType = r.SourceType,
                PhysicalFileId = r.PhysicalFileId,
                ArchiveRecordId = r.ArchiveRecordId,
                ArchiveRecordNumber = r.ArchiveRecordNumber,
                FileName = r.FileName,
                ChunkIndex = r.ChunkIndex,
                Content = r.Content,
                Score = r.Score
            }).ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Semantic search failed for query: {Query}", query.Query);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<DocumentDto>>> GetDocumentsPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            var result = await unitOfWork.Documents.GetPagedAsync(page, pageSize);
            if (result.IsError) return result.Errors;

            var items = result.Value!.Items.Select(d => d.ToDto()).ToList();
            return PagedList<DocumentDto>.Create(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get paged documents");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        try
        {
            await qdrantVectorStore.DeleteDocumentAsync(documentId, ct);

            var result = await unitOfWork.Documents.RemoveAsync(d => d.Id == documentId);
            if (result.IsError) return result.Errors;

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Deleted document {DocId}", documentId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete document {DocId}", documentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> ReIndexPhysicalFileAsync(Guid physicalFileId, CancellationToken ct = default)
    {
        try
        {
            var indexResult = await IndexPhysicalFileAsync(physicalFileId, ct);
            if (indexResult.IsError) return indexResult.Errors;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to re-index physical file {FileId}", physicalFileId);
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task RemoveExistingDocumentByPhysicalFileAsync(Guid physicalFileId, CancellationToken ct)
    {
        var existing = await unitOfWork.Documents.FindAsync(
            DocumentExpressions.ByPhysicalFileId(physicalFileId));
        if (existing.IsSuccess && existing.Value!.Count > 0)
        {
            foreach (var doc in existing.Value)
            {
                await qdrantVectorStore.DeleteDocumentAsync(doc.Id, ct);
                await unitOfWork.Documents.RemoveAsync(d => d.Id == doc.Id);
            }
            await unitOfWork.SaveChangesAsync();
        }
    }

    private async Task RemoveExistingDocumentByArchiveRecordAsync(Guid archiveRecordId, CancellationToken ct)
    {
        var existing = await unitOfWork.Documents.FindAsync(
            DocumentExpressions.ByArchiveRecordId(archiveRecordId));
        if (existing.IsSuccess && existing.Value!.Count > 0)
        {
            foreach (var doc in existing.Value)
            {
                await qdrantVectorStore.DeleteDocumentAsync(doc.Id, ct);
                await unitOfWork.Documents.RemoveAsync(d => d.Id == doc.Id);
            }
            await unitOfWork.SaveChangesAsync();
        }
    }

    private string NormalizePath(string path)
    {
        if (Path.IsPathRooted(path))
            return Path.GetFullPath(path);

        return Path.GetFullPath(Path.Combine(fileManager.RootDirectory, path));
    }

    private static bool IsOcrImage(string extension)
    {
        return extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".tif";
    }

    private static bool IsOcrPdf(string extension)
    {
        return extension == ".pdf";
    }
}
