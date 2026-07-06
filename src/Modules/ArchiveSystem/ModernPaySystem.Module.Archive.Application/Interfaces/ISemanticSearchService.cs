using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Application.Interfaces;

public interface ISemanticSearchService
{
    Task<Result<DocumentDto>> IndexPhysicalFileAsync(Guid physicalFileId, CancellationToken ct = default);
    Task<Result<DocumentDto>> IndexArchiveRecordAsync(Guid archiveRecordId, CancellationToken ct = default);
    Task<Result<List<SearchResultDto>>> SearchAsync(SearchQueryDto query, CancellationToken ct = default);
    Task<Result<PagedList<DocumentDto>>> GetDocumentsPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Result<bool>> DeleteDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<Result<bool>> ReIndexPhysicalFileAsync(Guid physicalFileId, CancellationToken ct = default);
}
