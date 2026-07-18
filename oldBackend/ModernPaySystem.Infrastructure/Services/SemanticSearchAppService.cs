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
    ILogger<SemanticSearchAppService> logger,
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
                return ApplicationErrors.PhysicalFileAccessDenied;

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
            return ApplicationErrors.InternalServerError;
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
                return ApplicationErrors.ArchiveRecordAccessDenied;

            var archiveResult = await unitOfWork.ArchiveRecords.GetAsync(
                filter: ar => ar.Id == archiveRecordId,
                transform: q => q.Include(ar => ar.ArchiveRecordTemplateValuesId!)
                    .ThenInclude(artv => artv.ArchiveRecordFormInputValues));

            if (archiveResult.IsError)
                return ApplicationErrors.ArchiveRecordNotFound;

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
            return ApplicationErrors.InternalServerError;
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
                    return ApplicationErrors.ArchiveRecordAccessDenied;

                archiveRecordIds = [query.ArchiveRecordId.Value];
            }
            else if (query.FolderId.HasValue)
            {
                var folderAccess = await resourceAuth.CanAccessFolderAsync(userId, query.FolderId.Value, AccessLevel.View);
                if (folderAccess.IsError)
                    return folderAccess.Errors;
                if (!folderAccess.Value)
                    return ApplicationErrors.FolderAccessDenied;

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
                var records = await unitOfWork.Context.ArchiveRecords
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(ar => recordIdsToCheck.Contains(ar.Id))
                    .Select(ar => new { ar.Id, ar.FolderId, ar.CreatedByUserId })
                    .ToListAsync();

                foreach (var record in records)
                {
                    if (record.CreatedByUserId == userId.ToString() || accessibleFolderIds.Contains(record.FolderId))
                        accessibleRecordIds.Add(record.Id);
                }
            }

            var fileIdsToCheck = results
                .Where(r => r.PhysicalFileId.HasValue)
                .Select(r => r.PhysicalFileId!.Value)
                .Distinct()
                .ToList();

            if (fileIdsToCheck.Count != 0)
            {
                var files = await unitOfWork.Context.PhysicalFiles
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(pf => fileIdsToCheck.Contains(pf.Id))
                    .Select(pf => new { pf.Id, pf.ArchiveRecordId, pf.CreatedByUserId })
                    .ToListAsync();

                var fileRecordIds = files
                    .Where(f => !accessibleRecordIds.Contains(f.ArchiveRecordId))
                    .Select(f => f.ArchiveRecordId)
                    .Distinct()
                    .ToList();

                if (fileRecordIds.Count != 0)
                {
                    var fileRecords = await unitOfWork.Context.ArchiveRecords
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .Where(ar => fileRecordIds.Contains(ar.Id))
                        .Select(ar => new { ar.Id, ar.FolderId, ar.CreatedByUserId })
                        .ToListAsync();

                    foreach (var fileRecord in fileRecords)
                    {
                        if (fileRecord.CreatedByUserId == userId.ToString() || accessibleFolderIds.Contains(fileRecord.FolderId))
                            accessibleRecordIds.Add(fileRecord.Id);
                    }
                }

                foreach (var file in files)
                {
                    if (file.CreatedByUserId == userId.ToString() || accessibleRecordIds.Contains(file.ArchiveRecordId))
                        accessiblePhysicalFileIds.Add(file.Id);
                }
            }

            var dtos = results
                .Where(r =>
                    (!r.ArchiveRecordId.HasValue || accessibleRecordIds.Contains(r.ArchiveRecordId.Value)) &&
                    (!r.PhysicalFileId.HasValue || accessiblePhysicalFileIds.Contains(r.PhysicalFileId.Value)))
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
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<DocumentDto>>> GetDocumentsPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        try
        {
            var userId = httpContextServiceManager.GetCurrentUserId();
            var accessibleFolderIdsResult = await resourceAuth.GetAccessibleFolderIdsAsync(userId);
            if (accessibleFolderIdsResult.IsError)
                return accessibleFolderIdsResult.Errors;
            var accessibleFolderIds = accessibleFolderIdsResult.Value!;

            var result = await unitOfWork.Documents.GetPagedAsync(page, pageSize);
            if (result.IsError) return result.Errors;

            var items = result.Value!.Items.Select(d => d.ToDto()).ToList();

            var filteredItems = await FilterDocumentsByAccessAsync(items, userId, accessibleFolderIds.ToHashSet(), ct);

            return PagedList<DocumentDto>.Create(filteredItems, filteredItems.Count, page, pageSize);
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
            var userId = httpContextServiceManager.GetCurrentUserId();
            var docResult = await unitOfWork.Documents.GetByIdAsync(documentId);
            if (docResult.IsError || docResult.Value == null)
                return ApplicationErrors.DocumentNotFound;

            var doc = docResult.Value;

            if (doc.CreatedByUserId != userId.ToString())
            {
                if (doc.ArchiveRecordId.HasValue)
                {
                    var access = await resourceAuth.CanAccessArchiveRecordAsync(userId, doc.ArchiveRecordId.Value, AccessLevel.FullControl);
                    if (access.IsError)
                        return access.Errors;
                    if (!access.Value)
                        return ApplicationErrors.ArchiveRecordAccessDenied;
                }
                else if (doc.PhysicalFileId.HasValue)
                {
                    var access = await resourceAuth.CanAccessPhysicalFileAsync(userId, doc.PhysicalFileId.Value, AccessLevel.FullControl);
                    if (access.IsError)
                        return access.Errors;
                    if (!access.Value)
                        return ApplicationErrors.PhysicalFileAccessDenied;
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

    private async Task<List<DocumentDto>> FilterDocumentsByAccessAsync(List<DocumentDto> documents, Guid userId, HashSet<Guid> accessibleFolderIds, CancellationToken ct)
    {
        if (documents.Count == 0)
            return documents;

        var recordIds = documents
            .Where(d => d.ArchiveRecordId.HasValue)
            .Select(d => d.ArchiveRecordId!.Value)
            .Distinct()
            .ToList();

        var fileIds = documents
            .Where(d => d.PhysicalFileId.HasValue)
            .Select(d => d.PhysicalFileId!.Value)
            .Distinct()
            .ToList();

        var accessibleRecordIds = new HashSet<Guid>();
        var accessibleFileIds = new HashSet<Guid>();

        if (recordIds.Count != 0)
        {
            var records = await unitOfWork.Context.ArchiveRecords
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(ar => recordIds.Contains(ar.Id))
                .Select(ar => new { ar.Id, ar.FolderId, ar.CreatedByUserId })
                .ToListAsync();

            foreach (var record in records)
            {
                if (record.CreatedByUserId == userId.ToString() || accessibleFolderIds.Contains(record.FolderId))
                    accessibleRecordIds.Add(record.Id);
            }
        }

        if (fileIds.Count != 0)
        {
            var files = await unitOfWork.Context.PhysicalFiles
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(pf => fileIds.Contains(pf.Id))
                .Select(pf => new { pf.Id, pf.ArchiveRecordId, pf.CreatedByUserId })
                .ToListAsync();

            var fileRecordIds = files
                .Where(f => !accessibleRecordIds.Contains(f.ArchiveRecordId))
                .Select(f => f.ArchiveRecordId)
                .Distinct()
                .ToList();

            if (fileRecordIds.Count != 0)
            {
                var fileRecords = await unitOfWork.Context.ArchiveRecords
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(ar => fileRecordIds.Contains(ar.Id))
                    .Select(ar => new { ar.Id, ar.FolderId, ar.CreatedByUserId })
                    .ToListAsync();

                foreach (var fileRecord in fileRecords)
                {
                    if (fileRecord.CreatedByUserId == userId.ToString() || accessibleFolderIds.Contains(fileRecord.FolderId))
                        accessibleRecordIds.Add(fileRecord.Id);
                }
            }

            foreach (var file in files)
            {
                if (file.CreatedByUserId == userId.ToString() || accessibleRecordIds.Contains(file.ArchiveRecordId))
                    accessibleFileIds.Add(file.Id);
            }
        }

        return documents.Where(d =>
            (!d.ArchiveRecordId.HasValue || accessibleRecordIds.Contains(d.ArchiveRecordId.Value)) &&
            (!d.PhysicalFileId.HasValue || accessibleFileIds.Contains(d.PhysicalFileId.Value))
        ).ToList();
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
