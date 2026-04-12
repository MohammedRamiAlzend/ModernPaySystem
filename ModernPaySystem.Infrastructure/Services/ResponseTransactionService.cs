using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Infrastructure.Services;

public class ResponseTransactionService(
    IUnitOfWork unitOfWork,
    IWebAttachmentService webAttachmentService,
    IHttpContextServiceManager httpContextServiceManager,
    ILogger<ResponseTransactionService> logger) : IResponseTransactionService
{
    public async Task<Result<PagedList<ResponseTransactionDto>>> GetPagedAsync(int page, int pageSize, TransactionStatus? status = null)
    {
        try
        {
            logger.LogInformation("Fetching paged response transactions, page: {Page}, size: {PageSize}, status: {Status}", page, pageSize, status);

            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            
            var filters = new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanReadByUserId(currentUserId) };
            
            if (status.HasValue)
            {
                filters.Add(rt => rt.Status == status.Value);
            }

            var pagedTransactions = await unitOfWork.ResponseTransactions.GetPagedAsync(
                page,
                pageSize,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.ResponseTransactionAttachments)
                                 .ThenInclude(a => a.Attachment),
                additionalFilters: filters);

            if (pagedTransactions.IsError)
                return pagedTransactions.Errors;

            var transactionDtos = pagedTransactions.Value!.Items.Select(t => t.ToDto()).ToList();
            return new PagedList<ResponseTransactionDto>(transactionDtos, pagedTransactions.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged response transactions, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseTransactionDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching response transaction by id: {TransactionId}", id);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == id,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.ChildTransactions)
                                 .Include(x => x.ResponseTransactionAttachments)
                                 .ThenInclude(a => a.Attachment),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            return transaction.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching response transaction by id: {TransactionId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<ResponseTransactionDto>>> GetByResponseIdAsync(Guid responseId)
    {
        try
        {
            logger.LogInformation("Fetching response transactions for response: {ResponseId}", responseId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transactions = await unitOfWork.ResponseTransactions.GetAllAsync(
                filter: rt => rt.ResponseId == responseId,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.ResponseTransactionAttachments)
                                 .ThenInclude(a => a.Attachment),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transactions.IsError)
                return transactions.Errors;

            var transactionDtos = transactions.Value!.Select(t => t.ToDto()).ToList();
            return transactionDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching response transactions for response: {ResponseId}", responseId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<ResponseTransactionDto>>> GetChildTransactionsAsync(Guid parentTransactionId)
    {
        try
        {
            logger.LogInformation("Fetching child transactions for parent: {ParentTransactionId}", parentTransactionId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transactions = await unitOfWork.ResponseTransactions.GetAllAsync(
                filter: rt => rt.ParentTransactionId == parentTransactionId,
                transform: x => x.Include(x => x.ChildTransactions)
                                 .Include(x => x.ResponseTransactionAttachments)
                                 .ThenInclude(a => a.Attachment),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanReadByUserId(currentUserId) });

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

    public async Task<Result<ResponseTransactionDto>> GetRootTransactionAsync(Guid responseId)
    {
        try
        {
            logger.LogInformation("Fetching root transaction for response: {ResponseId}", responseId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.ResponseId == responseId && !rt.ParentTransactionId.HasValue,
                transform: x => x.Include(x => x.ResponseTransactionAttachments)
                                 .ThenInclude(a => a.Attachment),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            return transaction.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching root transaction for response: {ResponseId}", responseId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<List<ResponseTransactionDto>>> GetTransactionTreeAsync(Guid transactionId)
    {
        try
        {
            logger.LogInformation("Fetching transaction tree for transaction: {TransactionId}", transactionId);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == transactionId,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.ChildTransactions)
                                 .ThenInclude(c => c.ResponseTransactionAttachments)
                                 .ThenInclude(a => a.Attachment),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanReadByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            var tree = new List<ResponseTransactionDto>();
            await BuildTransactionTreeRecursive(transaction.Value, tree, currentUserId, 0);

            return tree;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching transaction tree for transaction: {TransactionId}", transactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseTransactionDto>> CreateAsync(CreateResponseTransactionDto dto)
    {
        try
        {
            if (dto == null)
                return ApplicationErrors.InvalidInput;

            if (dto.ResponseId == Guid.Empty || dto.CurrentUserHolderId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Creating new response transaction for response: {ResponseId}", dto.ResponseId);

            var path = string.Empty;
            var level = 0;

            if (dto.ParentTransactionId.HasValue)
            {
                var parentTransaction = await unitOfWork.ResponseTransactions.GetAsync(
                    filter: rt => rt.Id == dto.ParentTransactionId.Value,
                    transform: x => x.Include(x => x.ParentTransaction));

                if (parentTransaction.IsError)
                    return parentTransaction.Errors;

                if (parentTransaction.Value == null)
                    return ApplicationErrors.ResponseTransactionNotFound;

                level = parentTransaction.Value.Level + 1;
                path = $"{parentTransaction.Value.Path}/{dto.ParentTransactionId.Value}";
            }
            else
            {
                path = $"/{Guid.NewGuid()}";
            }

            var transactionEntity = new ResponseTransaction
            {
                ResponseId = dto.ResponseId,
                Notes = dto.Notes,
                Level = level,
                Path = path,
                ParentTransactionId = dto.ParentTransactionId,
                CurrentUserHolderId = dto.CurrentUserHolderId
            };

            var addResult = await unitOfWork.ResponseTransactions.AddAsync(transactionEntity);
            if (addResult.IsError)
                return addResult.Errors;

            // If this is the first transaction (no parent), update Response status and set FirstTransactionId
            if (!dto.ParentTransactionId.HasValue)
            {
                var response = await unitOfWork.Responses.GetAsync(r => r.Id == dto.ResponseId);
                if (!response.IsError && response.Value != null)
                {
                    response.Value.FirstTransactionId = transactionEntity.Id;
                    response.Value.CurrentTransactionId = transactionEntity.Id;
                    
                    // Change status to InProcess when first transaction is created
                    if (response.Value.Status == ResponseStatus.Delivered || response.Value.Status == ResponseStatus.Pending)
                    {
                        response.Value.Status = ResponseStatus.InProcess;
                        await unitOfWork.Responses.UpdateAsync(response.Value);
                    }
                }
            }
            else
            {
                // Update CurrentTransactionId for child transactions
                var response = await unitOfWork.Responses.GetAsync(r => r.Id == dto.ResponseId);
                if (!response.IsError && response.Value != null)
                {
                    response.Value.CurrentTransactionId = transactionEntity.Id;
                    await unitOfWork.Responses.UpdateAsync(response.Value);
                }
            }

            await unitOfWork.SaveChangesAsync();

            if (dto.Files?.Any() == true)
            {
                logger.LogInformation("Uploading {FileCount} files for transaction: {TransactionId}", dto.Files.Count, transactionEntity.Id);
                foreach (var file in dto.Files)
                {
                    var uploadResult = await webAttachmentService.UploadFileToResponseTransactionAsync(file, transactionEntity.Id);
                    if (uploadResult.IsError)
                        return uploadResult.Errors;
                }
            }

            logger.LogInformation("Successfully created response transaction: {TransactionId}", transactionEntity.Id);
            return transactionEntity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating response transaction");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseTransactionDto>> UpdateAsync(Guid id, UpdateResponseTransactionDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var existingTransaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == id,
                transform: x => x.Include(x => x.ParentTransaction)
                                 .Include(x => x.ResponseTransactionAttachments),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanMakeUpdateByUserId(currentUserId) });

            if (existingTransaction.IsError)
                return existingTransaction.Errors;

            if (existingTransaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            logger.LogInformation("Updating response transaction: {TransactionId}", id);

            var oldParentId = existingTransaction.Value.ParentTransactionId;
            existingTransaction.Value.Notes = dto.Notes;

            if (dto.ParentTransactionId != oldParentId)
            {
                if (dto.ParentTransactionId.HasValue)
                {
                    var parentTransaction = await unitOfWork.ResponseTransactions.GetAsync(
                        filter: rt => rt.Id == dto.ParentTransactionId.Value,
                        transform: x => x.Include(x => x.ParentTransaction));

                    if (parentTransaction.IsError)
                        return parentTransaction.Errors;

                    if (parentTransaction.Value == null)
                        return ApplicationErrors.ResponseTransactionNotFound;

                    existingTransaction.Value.Level = parentTransaction.Value.Level + 1;
                    existingTransaction.Value.Path = $"{parentTransaction.Value.Path}/{dto.ParentTransactionId.Value}";
                }
                else
                {
                    existingTransaction.Value.Level = 0;
                    existingTransaction.Value.Path = $"/{existingTransaction.Value.Id}";
                }
            }

            await unitOfWork.ResponseTransactions.UpdateAsync(existingTransaction.Value);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully updated response transaction: {TransactionId}", id);
            return existingTransaction.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating response transaction: {TransactionId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == id,
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanMakeUpdateByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            logger.LogInformation("Deleting response transaction: {TransactionId}", id);

            await unitOfWork.ResponseTransactions.RemoveAsync(x => x.Id == transaction.Value.Id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully deleted response transaction: {TransactionId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting response transaction: {TransactionId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseTransactionDto>> AddFilesAsync(Guid transactionId, List<IFormFile> files)
    {
        try
        {
            if (transactionId == Guid.Empty || files == null || !files.Any())
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == transactionId,
                transform: x => x.Include(x => x.ResponseTransactionAttachments),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanMakeUpdateByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            logger.LogInformation("Adding {FileCount} files to transaction: {TransactionId}", files.Count, transactionId);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var uploadResult = await webAttachmentService.UploadFileToResponseTransactionAsync(file, transaction.Value.Id);
                    if (uploadResult.IsError)
                        return uploadResult.Errors;
                }
            }

            var updatedTransaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == transactionId,
                transform: x => x.Include(x => x.ResponseTransactionAttachments));

            if (updatedTransaction.IsError)
                return updatedTransaction.Errors;

            return updatedTransaction.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding files to transaction: {TransactionId}", transactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RemoveAttachmentAsync(Guid transactionId, Guid attachmentId)
    {
        try
        {
            if (transactionId == Guid.Empty || attachmentId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var transaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == transactionId,
                transform: x => x.Include(x => x.ResponseTransactionAttachments),
                additionalFilters: new List<Expression<Func<ResponseTransaction, bool>>> { ResponseTransactionExpressions.CanMakeUpdateByUserId(currentUserId) });

            if (transaction.IsError)
                return transaction.Errors;

            if (transaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            var attachment = transaction.Value.ResponseTransactionAttachments
                .FirstOrDefault(a => a.AttachmentId == attachmentId);

            if (attachment == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            logger.LogInformation("Removing attachment: {AttachmentId} from transaction: {TransactionId}", attachmentId, transactionId);

            await unitOfWork.ResponseTransactionAttachments.RemoveAsync(x => x.Id == attachment.Id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            logger.LogInformation("Successfully removed attachment: {AttachmentId}", attachmentId);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing attachment from transaction: {TransactionId}", transactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseTransactionDto>> AddChildTransactionAsync(Guid parentTransactionId, CreateResponseTransactionDto dto)
    {
        try
        {
            if (parentTransactionId == Guid.Empty || dto == null)
                return ApplicationErrors.InvalidInput;

            var parentTransaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == parentTransactionId,
                transform: x => x.Include(x => x.ParentTransaction));

            if (parentTransaction.IsError)
                return parentTransaction.Errors;

            if (parentTransaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            var childDto = new CreateResponseTransactionDto
            {
                ResponseId = dto.ResponseId,
                Notes = dto.Notes,
                ParentTransactionId = parentTransactionId,
                CurrentUserHolderId = dto.CurrentUserHolderId,
                Files = dto.Files
            };

            return await CreateAsync(childDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding child transaction to parent: {ParentTransactionId}", parentTransactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RemoveChildTransactionAsync(Guid parentTransactionId, Guid childTransactionId)
    {
        try
        {
            if (parentTransactionId == Guid.Empty || childTransactionId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var childTransaction = await unitOfWork.ResponseTransactions.GetAsync(
                filter: rt => rt.Id == childTransactionId && rt.ParentTransactionId == parentTransactionId);

            if (childTransaction.IsError)
                return childTransaction.Errors;

            if (childTransaction.Value == null)
                return ApplicationErrors.ResponseTransactionNotFound;

            await unitOfWork.ResponseTransactions.RemoveAsync(x => x.Id == childTransaction.Value.Id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing child transaction: {ChildTransactionId} from parent: {ParentTransactionId}",
                childTransactionId, parentTransactionId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ResponseDto>> MarkAsManagedAsync(Guid responseId)
    {
        try
        {
            if (responseId == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            logger.LogInformation("Marking response as managed: {ResponseId}", responseId);

            var response = await unitOfWork.Responses.GetAsync(r => r.Id == responseId);
            if (response.IsError)
                return response.Errors;

            if (response.Value == null)
                return ApplicationErrors.ResponseNotFound;

            // Change status to Managed when sent to requester
            response.Value.Status = ResponseStatus.Managed;
            await unitOfWork.Responses.UpdateAsync(response.Value);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully marked response as managed: {ResponseId}", responseId);
            return response.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking response as managed: {ResponseId}", responseId);
            return ApplicationErrors.InternalServerError;
        }
    }

    private async Task BuildTransactionTreeRecursive(ResponseTransaction transaction, List<ResponseTransactionDto> tree, Guid currentUserId, int depth)
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
