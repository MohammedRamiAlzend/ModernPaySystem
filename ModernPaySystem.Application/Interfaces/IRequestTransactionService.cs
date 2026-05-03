namespace ModernPaySystem.Application.Interfaces;

public interface IRequestTransactionService
{
    Task<Result<PagedList<RequestTransactionDto>>> GetPagedAsync(int page, int pageSize, TransactionStatus status);
    Task<Result<RequestTransactionDto>> GetByIdAsync(Guid id);
    Task<Result<List<RequestTransactionDto>>> GetByRequestIdAsync(Guid requestId);
    Task<Result<List<RequestTransactionDto>>> GetChildTransactionsAsync(Guid parentTransactionId);
    Task<Result<RequestTransactionDto>> GetRootTransactionAsync(Guid requestId);
    Task<Result<List<RequestTransactionDto>>> GetTransactionTreeAsync(Guid transactionId);

    Task<Result<Success>> AddInitialRequestTransaction(AddInitialRequestTransactionDto dto);
    Task<Result<Success>> AddChildTransactionAsync(CreateRequestTransactionDto dto);
    Task<Result<Success>> MarkAsManagedAsync(Guid requestId);
}
