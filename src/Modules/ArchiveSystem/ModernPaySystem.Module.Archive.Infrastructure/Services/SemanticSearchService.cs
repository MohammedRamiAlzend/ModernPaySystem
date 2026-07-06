using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.Module.Archive.Infrastructure.Services.Qdrant;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using OcrReader;
using SemanticSearchLib.Abstractions;
using SemanticSearchLib.Models;
using FileManager.Abstractions;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class SemanticSearchService(
    IArchiveUnitOfWork unitOfWork,
    IEmbeddingProvider embeddingProvider,
    IFileParser fileParser,
    ITextChunker textChunker,
    IFileManager fileManager,
    IOcrGenerator ocrGenerator,
    IQdrantVectorStore qdrantVectorStore,
    IOptions<SemanticSearchOptions> options,
    ILogger<SemanticSearchService> logger,
    IHttpContextServiceManager httpContextServiceManager,
    IArchiveResourceAuthorizationService resourceAuth) : ISemanticSearchService
{
    private readonly SemanticSearchOptions _options = options.Value;

    public async Task<Result<DocumentDto>> IndexPhysicalFileAsync(Guid physicalFileId, CancellationToken ct = default)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessPhysicalFileAsync(userId, physicalFileId, AccessLevel.Read);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.PhysicalFileAccessDenied;

            var physicalFileResult = await unitOfWork.PhysicalFiles.GetByIdAsync(physicalFileId);
            if (physicalFileResult.IsError)
                return ArchiveErrors.ArchiveRecordNotFound;

            var physicalFile = physicalFileResult.Value!;
            var fileExt = physicalFile.FileExtension.ToLowerInvariant();

            if (!fileParser.SupportsFileType(fileExt))
                return Error.Validation("UnsupportedFileType", $"File type '{fileExt}' is not supported for indexing.");

            var filePath = NormalizePath(physicalFile.StoragePath);
            if (!System.IO.File.Exists(filePath))
                return ArchiveErrors.InvalidInput;

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

            await qdrantVectorStore.UpsertChunksAsync(document.Id, docChunks, embeddings.ToArray(),
                document.SourceType, document.FileName, document.PhysicalFileId, document.ArchiveRecordId,
                archiveRecordNumber: null, ct);

            logger.LogInformation("Indexed physical file {FileId} as document {DocId} with {ChunkCount} chunks",
                physicalFileId, document.Id, chunks.Count);

            return document.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to index physical file {FileId}", physicalFileId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<DocumentDto>> IndexArchiveRecordAsync(Guid archiveRecordId, CancellationToken ct = default)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, archiveRecordId, AccessLevel.Read);
            if (access.IsError)
                return access.Errors;
            if (!access.Value)
                return ArchiveErrors.ArchiveRecordAccessDenied;

            var archiveResult = await unitOfWork.ArchiveRecords.GetAsync(
                filter: ar => ar.Id == archiveRecordId,
                transform: q => q.Include(ar => ar.ArchiveRecordTemplateValuesId!)
                    .ThenInclude(artv => artv.ArchiveRecordFormInputValues));

            if (archiveResult.IsError)
                return ArchiveErrors.ArchiveRecordNotFound;

            var archiveRecord = archiveResult.Value!;

            var textParts = new List<string>();

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
                FileName = $"ArchiveRecord_{archiveRecord.Id}",
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

            await qdrantVectorStore.UpsertChunksAsync(document.Id, docChunks, embeddings.ToArray(),
                document.SourceType, document.FileName, null, document.ArchiveRecordId,
                archiveRecordNumber: string.Empty, ct);

            logger.LogInformation("Indexed archive record {RecordId} as document {DocId} with {ChunkCount} chunks",
                archiveRecordId, document.Id, chunks.Count);

            return document.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to index archive record {RecordId}", archiveRecordId);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<List<SearchResultDto>>> SearchAsync(SearchQueryDto query, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query.Query))
                return Error.Validation("EmptyQuery", "Search query cannot be empty.");

            var userId = httpContextServiceManager.GetCurrentUserId();

            var queryEmbedding = await embeddingProvider.GenerateEmbeddingAsync(query.Query, ct);

            var topK = Math.Clamp(query.TopK, 1, 100);
            var minScore = Math.Clamp(query.MinScore, 0.0, 1.0);

            List<Guid>? archiveRecordIds = null;

            if (query.ArchiveRecordId.HasValue)
            {
                var recordAccess = await resourceAuth.CanAccessArchiveRecordAsync(userId, query.ArchiveRecordId.Value, AccessLevel.View);
                if (recordAccess.IsError)
                    return recordAccess.Errors;
                if (!recordAccess.Value)
                    return ArchiveErrors.ArchiveRecordAccessDenied;

                archiveRecordIds = [query.ArchiveRecordId.Value];
            }
            else if (query.FolderId.HasValue)
            {
                var folderAccess = await resourceAuth.CanAccessFolderAsync(userId, query.FolderId.Value, AccessLevel.View);
                if (folderAccess.IsError)
                    return folderAccess.Errors;
                if (!folderAccess.Value)
                    return ArchiveErrors.FolderAccessDenied;

                var folderResult = await unitOfWork.ArchiveRecords.FindAsync(
                    ar => ar.FolderId == query.FolderId.Value);
                if (folderResult.IsSuccess)
                    archiveRecordIds = folderResult.Value!.Select(ar => ar.Id).ToList();
            }

            var filter = new SearchFilter
            {
                SourceType = query.SourceType,
                ArchiveRecordIds = archiveRecordIds
            };

            var results = await qdrantVectorStore.SearchAsync(queryEmbedding, topK, minScore, filter, ct);

            // Filter by accessible folders
            var accessibleFolderIdsResult = await resourceAuth.GetAccessibleFolderIdsAsync(userId);
            if (accessibleFolderIdsResult.IsError)
                return accessibleFolderIdsResult.Errors;
            var accessibleFolderIds = accessibleFolderIdsResult.Value!;

            var accessibleRecordIds = new HashSet<Guid>();
            var accessiblePhysicalFileIds = new HashSet<Guid>();

            var recordIdsToCheck = results
                .Where(r => r.ArchiveRecordId.HasValue)
                .Select(r => r.ArchiveRecordId!.Value)
                .Distinct()
                .ToList();

            if (recordIdsToCheck.Count != 0)
            {
                var records = await unitOfWork.ArchiveRecords.GetAllAsync(
                    filter: ar => recordIdsToCheck.Contains(ar.Id));

                if (records.IsSuccess && records.Value != null)
                {
                    foreach (var record in records.Value)
                    {
                        if (record.CreatedByUserId == userId.ToString() || accessibleFolderIds.Contains(record.FolderId))
                            accessibleRecordIds.Add(record.Id);
                    }
                }
            }

            var dtos = results
                .Where(r => !r.ArchiveRecordId.HasValue || accessibleRecordIds.Contains(r.ArchiveRecordId.Value))
                .Select(r => new SearchResultDto
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
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<DocumentDto>>> GetDocumentsPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            var result = await unitOfWork.Documents.GetPagedAsync(page, pageSize);
            if (result.IsError) return result.Errors;

            var items = result.Value!.Items.Select(d => d.ToDto()).ToList();
            return PagedList<DocumentDto>.Create(items, items.Count, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get paged documents");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var docResult = await unitOfWork.Documents.GetByIdAsync(documentId);
            if (docResult.IsError || docResult.Value == null)
                return ArchiveErrors.InvalidInput;

            var doc = docResult.Value;

            if (doc.CreatedByUserId != userId.ToString())
            {
                if (doc.ArchiveRecordId.HasValue)
                {
                    var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, doc.ArchiveRecordId.Value, AccessLevel.FullControl);
                    if (access.IsError)
                        return access.Errors;
                    if (!access.Value)
                        return ArchiveErrors.ArchiveRecordAccessDenied;
                }
                else if (doc.PhysicalFileId.HasValue)
                {
                    var access = await resourceAuth.CanAccessPhysicalFileAsync(userId, doc.PhysicalFileId.Value, AccessLevel.FullControl);
                    if (access.IsError)
                        return access.Errors;
                    if (!access.Value)
                        return ArchiveErrors.PhysicalFileAccessDenied;
                }
            }

            await qdrantVectorStore.DeleteDocumentAsync(documentId, ct);

            var removeResult = await unitOfWork.Documents.RemoveAsync(d => d.Id == documentId);
            if (removeResult.IsError) return removeResult.Errors;

            await unitOfWork.SaveChangesAsync();
            logger.LogInformation("Deleted document {DocId}", documentId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete document {DocId}", documentId);
            return ArchiveErrors.InternalServerError;
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
            return ArchiveErrors.InternalServerError;
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
