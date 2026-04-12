using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Interfaces;

public interface IResponseTransactionService
{
    // Query methods
    Task<Result<PagedList<ResponseTransactionDto>>> GetPagedAsync(int page, int pageSize);
    Task<Result<ResponseTransactionDto>> GetByIdAsync(Guid id);
    Task<Result<List<ResponseTransactionDto>>> GetByResponseIdAsync(Guid responseId);
    Task<Result<List<ResponseTransactionDto>>> GetChildTransactionsAsync(Guid parentTransactionId);
    Task<Result<ResponseTransactionDto>> GetRootTransactionAsync(Guid responseId);
    Task<Result<List<ResponseTransactionDto>>> GetTransactionTreeAsync(Guid transactionId);

    // CRUD methods
    Task<Result<ResponseTransactionDto>> CreateAsync(CreateResponseTransactionDto dto);
    Task<Result<ResponseTransactionDto>> UpdateAsync(Guid id, UpdateResponseTransactionDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);

    // Attachment methods
    Task<Result<ResponseTransactionDto>> AddFilesAsync(Guid transactionId, List<IFormFile> files);
    Task<Result<bool>> RemoveAttachmentAsync(Guid transactionId, Guid attachmentId);

    // Self-reference methods
    Task<Result<ResponseTransactionDto>> AddChildTransactionAsync(Guid parentTransactionId, CreateResponseTransactionDto dto);
    Task<Result<bool>> RemoveChildTransactionAsync(Guid parentTransactionId, Guid childTransactionId);
}
