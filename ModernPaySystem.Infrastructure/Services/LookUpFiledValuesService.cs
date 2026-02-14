using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of LookUpFiledValues service CRUD operations.
/// </summary>
public class LookUpFiledValuesService : ILookUpFiledValuesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LookUpFiledValuesService> _logger;

    public LookUpFiledValuesService(IUnitOfWork unitOfWork, ILogger<LookUpFiledValuesService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all lookup field values");
            var lookUpFiledValues = await _unitOfWork.LookUpFiledValues.GetAllAsync();
            if (lookUpFiledValues.IsError)
                return lookUpFiledValues.Errors;

            var lookUpFiledValuesDtos = lookUpFiledValues.Value!.ConvertAll(lfv => lfv.ToDto());
            return lookUpFiledValuesDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all lookup field values");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<LookUpFiledValuesDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching paged lookup field values, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            var pagedLookUpFiledValues = await _unitOfWork.LookUpFiledValues.GetPagedAsync(page, pageSize);
            if (pagedLookUpFiledValues.IsError)
                return pagedLookUpFiledValues.Errors;

            var lookUpFiledValuesDtos = pagedLookUpFiledValues.Value!.Items.Select(lfv => lfv.ToDto()).ToList();
            var pagedLookUpFiledValuesDtos = new PagedList<LookUpFiledValuesDto>(lookUpFiledValuesDtos, pagedLookUpFiledValues.Value.TotalItems, page, pageSize);

            return pagedLookUpFiledValuesDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged lookup field values, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFiledValuesDto>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching lookup field value by id: {LookUpFiledValueId}", id);
            var lookUpFiledValue = await _unitOfWork.LookUpFiledValues.GetByIdAsync(id);

            if (lookUpFiledValue.IsError)
                return lookUpFiledValue.Errors;

            if (lookUpFiledValue.Value == null)
                return ApplicationErrors.OperationFailed;

            return lookUpFiledValue.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lookup field value by id: {LookUpFiledValueId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFiledValuesDto>> CreateAsync(CreateLookUpFiledValuesDto lookUpFiledValue)
    {
        try
        {
            if (lookUpFiledValue == null)
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(lookUpFiledValue.Desc))
                return ApplicationErrors.MissingRequiredField;

            _logger.LogInformation("Creating new lookup field value with ID: {LookUpFiledId}", lookUpFiledValue.LookUpFiledId);

            var lookUpFiledValueEntity = new LookUpFiledValues
            {
                LookUpFiledId = lookUpFiledValue.LookUpFiledId,
                Desc = lookUpFiledValue.Desc
            };

            var addResult = await _unitOfWork.LookUpFiledValues.AddAsync(lookUpFiledValueEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await _unitOfWork.SaveChangesAsync();
            if (result < 0)
                return ApplicationErrors.DatabaseError;

            _logger.LogInformation("Successfully created lookup field value with ID: {LookUpFiledValueId}", lookUpFiledValueEntity.Id);
            return lookUpFiledValueEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lookup field value");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFiledValuesDto>> UpdateAsync(Guid id, UpdateLookUpFiledValuesDto lookUpFiledValue)
    {
        try
        {
            if (id == Guid.Empty || lookUpFiledValue == null)
                return ApplicationErrors.InvalidInput;

            var existingLookUpFiledValue = await _unitOfWork.LookUpFiledValues.GetByIdAsync(id);
            if (existingLookUpFiledValue.IsError)
                return existingLookUpFiledValue.Errors;

            if (existingLookUpFiledValue.Value == null)
                return ApplicationErrors.OperationFailed;

            _logger.LogInformation("Updating lookup field value: {LookUpFiledValueId}", id);

            existingLookUpFiledValue.Value.LookUpFiledId = lookUpFiledValue.LookUpFiledId;
            existingLookUpFiledValue.Value.Desc = lookUpFiledValue.Desc;

            var updateResult = await _unitOfWork.LookUpFiledValues.UpdateAsync(existingLookUpFiledValue.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated lookup field value: {LookUpFiledValueId}", id);
            return existingLookUpFiledValue.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lookup field value: {LookUpFiledValueId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var lookUpFiledValue = await _unitOfWork.LookUpFiledValues.GetByIdAsync(id);
            if (lookUpFiledValue.IsError)
                return lookUpFiledValue.Errors;

            if (lookUpFiledValue.Value == null)
                return ApplicationErrors.OperationFailed;

            _logger.LogInformation("Deleting lookup field value: {LookUpFiledValueId}", id);

            await _unitOfWork.LookUpFiledValues.RemoveAsync(x => x.Id == lookUpFiledValue.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted lookup field value: {LookUpFiledValueId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup field value: {LookUpFiledValueId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<LookUpFiledValuesDto>>> GetByLookUpFieldIdAsync(Guid lookUpFieldId)
    {
        try
        {
            _logger.LogInformation("Fetching lookup field values by lookup field ID: {LookUpFieldId}", lookUpFieldId);
            var lookUpFiledValues = await _unitOfWork.LookUpFiledValues.GetAllAsync(x => x.LookUpFiledId == lookUpFieldId);
            if (lookUpFiledValues.IsError)
                return lookUpFiledValues.Errors;

            return lookUpFiledValues.Value!.ConvertAll(lfv => lfv.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lookup field values by lookup field ID: {LookUpFieldId}", lookUpFieldId);
            return ApplicationErrors.InternalServerError;
        }
    }
}