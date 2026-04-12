using Microsoft.AspNetCore.Http;

namespace ModernPaySystem.Application.Interfaces;

public interface IResponseTransactionService
{
    Task<Result<PagedList<ResponseTransactionDto>>> GetPagedAsync(int page, int pageSize, TransactionStatus? status = null);
    Task<Result<ResponseTransactionDto>> GetByIdAsync(Guid id);
    Task<Result<List<ResponseTransactionDto>>> GetByResponseIdAsync(Guid responseId);
    Task<Result<List<ResponseTransactionDto>>> GetChildTransactionsAsync(Guid parentTransactionId);
    Task<Result<ResponseTransactionDto>> GetRootTransactionAsync(Guid responseId);
    Task<Result<List<ResponseTransactionDto>>> GetTransactionTreeAsync(Guid transactionId);
    Task<Result<ResponseTransactionDto>> CreateAsync(CreateResponseTransactionDto dto);
    Task<Result<ResponseTransactionDto>> AddChildTransactionAsync(Guid parentTransactionId, CreateResponseTransactionDto dto);
    Task<Result<ResponseDto>> MarkAsManagedAsync(Guid responseId);
}
