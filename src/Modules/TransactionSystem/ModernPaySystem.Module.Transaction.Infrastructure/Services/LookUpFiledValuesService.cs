using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class LookUpFiledValuesService(
    ITransactionUnitOfWork unitOfWork,
    ILogger<LookUpFiledValuesService> logger) : ILookUpFiledValuesService
{
    public async Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all lookup field values");
            var values = await unitOfWork.LookUpFiledValues.GetAllAsync();
            if (values.IsError)
                return values.Errors;

            var dtos = values.Value!.ConvertAll(v => v.ToDto());
            return dtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all lookup field values");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<LookUpFiledValuesDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged lookup field values, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            var pagedValues = await unitOfWork.LookUpFiledValues.GetPagedAsync(page, pageSize);
            if (pagedValues.IsError)
                return pagedValues.Errors;

            var dtos = pagedValues.Value!.Items.Select(v => v.ToDto()).ToList();
            return new PagedList<LookUpFiledValuesDto>(dtos, pagedValues.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged lookup field values, page: {Page}, size: {PageSize}", page, pageSize);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFiledValuesDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching lookup field value by id: {LookUpFiledValueId}", id);

            var value = await unitOfWork.LookUpFiledValues.GetAsync(filter: v => v.Id == id);
            if (value.IsError)
                return value.Errors;

            if (value.Value == null)
                return TransactionErrors.LookUpFiledValueNotFound;

            return value.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lookup field value by id: {LookUpFiledValueId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFiledValuesDto>> CreateAsync(CreateLookUpFiledValuesDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Desc))
                return TransactionErrors.InvalidInput;

            logger.LogInformation("Creating new lookup field value");

            var entity = new LookUpFiledValues
            {
                Id = Guid.NewGuid(),
                LookUpFiledId = dto.LookUpFiledId,
                Desc = dto.Desc
            };

            var addResult = await unitOfWork.LookUpFiledValues.AddAsync(entity);
            if (addResult.IsError)
                return addResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return TransactionErrors.DatabaseError;

            logger.LogInformation("Successfully created lookup field value: {LookUpFiledValueId}", entity.Id);
            return entity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating lookup field value");
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFiledValuesDto>> UpdateAsync(Guid id, UpdateLookUpFiledValuesDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null || string.IsNullOrWhiteSpace(dto.Desc))
                return TransactionErrors.InvalidInput;

            var existing = await unitOfWork.LookUpFiledValues.GetAsync(filter: v => v.Id == id);
            if (existing.IsError)
                return existing.Errors;

            if (existing.Value == null)
                return TransactionErrors.LookUpFiledValueNotFound;

            logger.LogInformation("Updating lookup field value: {LookUpFiledValueId}", id);

            existing.Value.LookUpFiledId = dto.LookUpFiledId;
            existing.Value.Desc = dto.Desc;

            var updateResult = await unitOfWork.LookUpFiledValues.UpdateAsync(existing.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully updated lookup field value: {LookUpFiledValueId}", id);
            return existing.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating lookup field value: {LookUpFiledValueId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var existing = await unitOfWork.LookUpFiledValues.GetAsync(filter: v => v.Id == id);
            if (existing.IsError)
                return existing.Errors;

            if (existing.Value == null)
                return TransactionErrors.LookUpFiledValueNotFound;

            logger.LogInformation("Deleting lookup field value: {LookUpFiledValueId}", id);

            var deleteResult = await unitOfWork.LookUpFiledValues.RemoveAsync(v => v.Id == id);
            if (deleteResult.IsError)
                return deleteResult.Errors;

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
                return TransactionErrors.DatabaseError;

            logger.LogInformation("Successfully deleted lookup field value: {LookUpFiledValueId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting lookup field value: {LookUpFiledValueId}", id);
            return TransactionErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetByLookUpFieldIdAsync(Guid lookUpFieldId)
    {
        try
        {
            logger.LogInformation("Fetching lookup field values for field: {LookUpFieldId}", lookUpFieldId);

            var values = await unitOfWork.LookUpFiledValues.GetAllAsync(filter: v => v.LookUpFiledId == lookUpFieldId);
            if (values.IsError)
                return values.Errors;

            var dtos = values.Value!.Select(v => v.ToDto()).ToList();
            return dtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching lookup field values for field: {LookUpFieldId}", lookUpFieldId);
            return TransactionErrors.InternalServerError;
        }
    }
}
