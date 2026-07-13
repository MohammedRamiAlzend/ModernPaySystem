using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class LookUpFieldService(
    ITransactionUnitOfWork unitOfWork,
    ILogger<LookUpFieldService> logger) : ILookUpFieldService
{
    public async Task<Result<IEnumerable<LookUpFieldDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all lookup fields");
            var lookUpFields = await unitOfWork.LookUpFields.GetAllAsync();
            if (lookUpFields.IsError)
                return lookUpFields.Errors;

            var lookUpFieldDtos = lookUpFields.Value!.ConvertAll(lf => lf.ToDto());
            return lookUpFieldDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all lookup fields");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<LookUpFieldDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged lookup fields, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            var pagedLookUpFields = await unitOfWork.LookUpFields.GetPagedAsync(page, pageSize);
            if (pagedLookUpFields.IsError)
                return pagedLookUpFields.Errors;

            var lookUpFieldDtos = pagedLookUpFields.Value!.Items.Select(lf => lf.ToDto()).ToList();
            return new PagedList<LookUpFieldDto>(lookUpFieldDtos, pagedLookUpFields.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged lookup fields, page: {Page}, size: {PageSize}", page, pageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFieldDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching lookup field by id: {LookUpFieldId}", id);

            var lookUpField = await unitOfWork.LookUpFields.GetAsync(filter: lf => lf.Id == id, transform: q => q.Include(lf => lf.LookUpFiledValues));
            if (lookUpField.IsError)
                return lookUpField.Errors;

            if (lookUpField.Value == null)
                return TransactionErrors.LookUpFieldNotFound;

            return lookUpField.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lookup field by id: {LookUpFieldId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFieldDto>> CreateAsync(CreateLookUpFieldDto lookUpField)
    {
        try
        {
            if (lookUpField == null || string.IsNullOrWhiteSpace(lookUpField.FiledName))
                return TransactionErrors.InvalidInput;

            logger.LogInformation("Creating new lookup field: {FiledName}", lookUpField.FiledName);

            var entity = new LookUpField
            {
                Id = Guid.NewGuid(),
                FiledName = lookUpField.FiledName
            };

            var addResult = await unitOfWork.LookUpFields.AddAsync(entity);
            if (addResult.IsError)
                return addResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return TransactionErrors.DatabaseError;

            logger.LogInformation("Successfully created lookup field: {LookUpFieldId}", entity.Id);
            return entity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lookup field");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFieldDto>> UpdateAsync(Guid id, UpdateLookUpFieldDto lookUpField)
    {
        try
        {
            if (id == Guid.Empty || lookUpField == null || string.IsNullOrWhiteSpace(lookUpField.FiledName))
                return TransactionErrors.InvalidInput;

            var existing = await unitOfWork.LookUpFields.GetAsync(filter: lf => lf.Id == id);
            if (existing.IsError)
                return existing.Errors;

            if (existing.Value == null)
                return TransactionErrors.LookUpFieldNotFound;

            logger.LogInformation("Updating lookup field: {LookUpFieldId}", id);

            existing.Value.FiledName = lookUpField.FiledName;

            var updateResult = await unitOfWork.LookUpFields.UpdateAsync(existing.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully updated lookup field: {LookUpFieldId}", id);
            return existing.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lookup field: {LookUpFieldId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var existing = await unitOfWork.LookUpFields.GetAsync(filter: lf => lf.Id == id);
            if (existing.IsError)
                return existing.Errors;

            if (existing.Value == null)
                return TransactionErrors.LookUpFieldNotFound;

            logger.LogInformation("Deleting lookup field: {LookUpFieldId}", id);

            var deleteResult = await unitOfWork.LookUpFields.RemoveAsync(lf => lf.Id == id);
            if (deleteResult.IsError)
                return deleteResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return TransactionErrors.DatabaseError;

            logger.LogInformation("Successfully deleted lookup field: {LookUpFieldId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lookup field: {LookUpFieldId}", id);
            return TransactionErrors.InternalServerError;
        }
    }
}
