using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Infrastructure.Services;

public class RequestTransactionService(
    IUnitOfWork unitOfWork,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<RequestTransactionService> logger) : IRequestTransactionService
{
    public async Task<Result<PagedList<RequestTransactionDto>>> GetPagedAsync(int page, int pageSize, TransactionStatus? status = null)
    {
        try
        {
            logger.LogInformation("Fetching paged request transactions, page: {Page}, size: {PageSize}, status: {Status}", page, pageSize, status);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var filters = new List<Expression<Func<RequestTransaction, bool>>> { RequestTransactionExpressions.CanReadByUserId(currentUserId) };

            if (status.HasValue)
            {
                filters.Add(rt => rt.Status == status.Value);
            }

            var pagedTransactions = await unitOfWork.RequestTransactions.GetPagedAsync(
                page,
                pageSize,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.RequestTransactionAttachments)
                                 .ThenInclude(a => a.Attachment)
                                 .Include(x => x.Request),
                additionalFilters: filters);

            if (pagedTransactions.IsError)
                return pagedTransactions.Errors;

            var transactionDtos = pagedTransactions.Value!.Items.Select(t => t.ToDto()).ToList();
            return new PagedList<RequestTransactionDto>(transactionDtos, pagedTransactions.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged request transactions, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestTransactionDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching request transaction by id: {TransactionId}", id);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.RequestTransactions.GetAsync(
                filter: rt => rt.Id == id,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.ChildTransactions)
                                 .Include(x => x.RequestTransactionAttachments)
                                 .ThenInclude(a => a.Attachment)
                                 .Include(x => x.Request),
                additionalFilters: new List<Expression<Func<RequestTransaction, bool>>> { RequestTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.RequestTransactionNotFound;

            return transaction.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request transaction by id: {TransactionId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<RequestTransactionDto>>> GetByRequestIdAsync(Guid requestId)
    {
        try
        {
            logger.LogInformation("Fetching request transactions for request: {RequestId}", requestId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transactions = await unitOfWork.RequestTransactions.GetAllAsync(
                filter: rt => rt.RequestId == requestId,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.RequestTransactionAttachments)
                                 .ThenInclude(a => a.Attachment)
                                 .Include(x => x.Request),
                additionalFilters: new List<Expression<Func<RequestTransaction, bool>>> { RequestTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transactions.IsError)
                return transactions.Errors;

            var transactionDtos = transactions.Value!.Select(t => t.ToDto()).ToList();
            return transactionDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching request transactions for request: {RequestId}", requestId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<RequestTransactionDto>>> GetChildTransactionsAsync(Guid parentTransactionId)
    {
        try
        {
            logger.LogInformation("Fetching child transactions for parent: {ParentTransactionId}", parentTransactionId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transactions = await unitOfWork.RequestTransactions.GetAllAsync(
                filter: rt => rt.ParentTransactionId == parentTransactionId,
                transform: x => x.Include(x => x.ChildTransactions)
                                 .Include(x => x.RequestTransactionAttachments)
                                 .ThenInclude(a => a.Attachment)
                                 .Include(x => x.Request),
                additionalFilters: new List<Expression<Func<RequestTransaction, bool>>> { RequestTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transactions.IsError)
                return transactions.Errors;

            var transactionDtos = transactions.Value!.Select(t => t.ToDto()).ToList();
            return transactionDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching child transactions for parent: {ParentTransactionId}", parentTransactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RequestTransactionDto>> GetRootTransactionAsync(Guid requestId)
    {
        try
        {
            logger.LogInformation("Fetching root transaction for request: {RequestId}", requestId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.RequestTransactions.GetAsync(
                filter: rt => rt.RequestId == requestId && !rt.ParentTransactionId.HasValue,
                transform: x => x.Include(x => x.RequestTransactionAttachments)
                                 .ThenInclude(a => a.Attachment)
                                 .Include(x => x.Request),
                additionalFilters: new List<Expression<Func<RequestTransaction, bool>>> { RequestTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.RequestTransactionNotFound;

            return transaction.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching root transaction for request: {RequestId}", requestId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<RequestTransactionDto>>> GetTransactionTreeAsync(Guid transactionId)
    {
        try
        {
            logger.LogInformation("Fetching transaction tree for transaction: {TransactionId}", transactionId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.RequestTransactions.GetAsync(
                filter: rt => rt.Id == transactionId,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.ChildTransactions)
                                 .ThenInclude(c => c.RequestTransactionAttachments)
                                 .ThenInclude(a => a.Attachment)
                                 .Include(x => x.Request),
                additionalFilters: new List<Expression<Func<RequestTransaction, bool>>> { RequestTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.RequestTransactionNotFound;

            var tree = new List<RequestTransactionDto>();
            await BuildTransactionTreeRecursive(transaction.Value, tree, currentUserId, 0);

            return tree;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching transaction tree for transaction: {TransactionId}", transactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Success>> AddInitialRequestTransaction(AddInitialRequestTransactionDto dto)
    {
        try
        {
            if (dto == null)
                return ApplicationErrors.InvalidInput;

            if (dto.RequestId == Guid.Empty || dto.CurrentUserHolderId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Creating new request transaction for request: {RequestId}", dto.RequestId);

            var level = 0;
            var newId = Guid.NewGuid();
            var path = $"{newId}";
            var getRequestResult = await unitOfWork.Requests.GetAsync(r => r.Id == dto.RequestId, transform:
                x => x.Include(i => i.CurrentTransaction).Include(i => i.FirstTransaction));

            if (getRequestResult.IsError)
                return getRequestResult.Errors;

            if (getRequestResult.Value.FirstTransaction != null || getRequestResult.Value.CurrentTransaction != null)
                return ApplicationErrors.RequestAlreadyHasTransaction;

            var transactionEntity = new RequestTransaction
            {
                RequestId = dto.RequestId,
                Notes = dto.Notes,
                Level = level,
                Path = path,
                ParentTransactionId = null,
                CurrentUserHolderId = dto.CurrentUserHolderId
            };

            var addResult = await unitOfWork.RequestTransactions.AddAsync(transactionEntity);
            if (addResult.IsError)
                return addResult.Errors;

            getRequestResult.Value.CurrentTransactionId = transactionEntity.Id;
            getRequestResult.Value.FirstTransactionId = transactionEntity.Id;

            // Change Request status to InProcess when first transaction is created
            if (getRequestResult.Value.Status == RequestStatus.Pending || getRequestResult.Value.Status == RequestStatus.Delivered)
            {
                getRequestResult.Value.Status = RequestStatus.InProcess;
            }

            await unitOfWork.Requests.UpdateAsync(getRequestResult.Value);
            await unitOfWork.SaveChangesAsync();

            if (dto.Files?.Any() == true)
            {
                logger.LogInformation("Uploading {FileCount} files for transaction: {TransactionId}", dto.Files.Count, transactionEntity.Id);
                foreach (var file in dto.Files)
                {
                    var uploadResult = await webAttachmentService.UploadFileToRequestTransactionAsync(file, transactionEntity.Id);
                    if (uploadResult.IsError)
                        return uploadResult.Errors;
                }
            }

            logger.LogInformation("Successfully created request transaction: {TransactionId}", transactionEntity.Id);
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating request transaction");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Success>> AddChildTransactionAsync(CreateRequestTransactionDto dto)
    {
        try
        {
            if (dto.ParentTransactionId == Guid.Empty || dto == null)
                return ApplicationErrors.InvalidInput;

            var parentTransaction = await unitOfWork.RequestTransactions.GetAsync(
                filter: rt => rt.Id == dto.ParentTransactionId,
                transform: x =>
                x
                .Include(x => x.ParentTransaction).ThenInclude(i => i.Request).ThenInclude(i => i.CurrentTransaction)
                .Include(x => x.ParentTransaction).ThenInclude(i => i.Request).ThenInclude(i => i.FirstTransaction)
                .Include(x => x.Request));

            if (parentTransaction.IsError)
                return parentTransaction.Errors;

            var request = parentTransaction.Value?.Request;
            var newId = Guid.NewGuid();
            var childTransaction = new RequestTransaction
            {
                Id = newId,
                RequestId = dto.RequestId,
                Notes = dto.Notes,
                ParentTransactionId = dto.ParentTransactionId,
                CurrentUserHolderId = dto.CurrentUserHolderId,
                Level = parentTransaction.Value.Level + 1,
                Path = $"{parentTransaction.Value.Path}/{newId}"
            };
            var addResult = await unitOfWork.RequestTransactions.AddAsync(childTransaction);
            if (addResult.IsError)
                return addResult.Errors;

            request.CurrentTransactionId = childTransaction.Id;

            await unitOfWork.Requests.UpdateAsync(request);
            await unitOfWork.SaveChangesAsync();

            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding child transaction to parent: {ParentTransactionId}", dto.ParentTransactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Success>> MarkAsManagedAsync(Guid requestId)
    {
        try
        {
            if (requestId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Marking request transaction as managed: {RequestId}", requestId);

            var request = await unitOfWork.Requests.GetAsync(r => r.Id == requestId);
            if (request.IsError)
                return request.Errors;

            if (request.Value == null)
                return ApplicationErrors.RequestNotFound;

            logger.LogInformation("Successfully marked request transaction as managed: {RequestId}", requestId);
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking request transaction as managed: {RequestId}", requestId);
            return ApplicationErrors.InternalServerError;
        }
    }

    private static async Task BuildTransactionTreeRecursive(RequestTransaction transaction, List<RequestTransactionDto> tree, Guid currentUserId, int depth)
    {
        if (depth > 10)
            return;

        tree.Add(transaction.ToDto());

        if (transaction.ChildTransactions != null && transaction.ChildTransactions.Any())
        {
            foreach (var child in transaction.ChildTransactions)
            {
                await BuildTransactionTreeRecursive(child, tree, currentUserId, depth + 1);
            }
        }
    }
}
