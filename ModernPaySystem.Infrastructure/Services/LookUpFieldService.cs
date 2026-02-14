using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of LookUpField service CRUD operations.
/// </summary>
public class LookUpFieldService : ILookUpFieldService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LookUpFieldService> _logger;

    public LookUpFieldService(IUnitOfWork unitOfWork, ILogger<LookUpFieldService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<LookUpFieldDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all lookup fields");
            var lookUpFields = await _unitOfWork.LookUpFields.GetAllAsync();
            if (lookUpFields.IsError)
                return lookUpFields.Errors;

            var lookUpFieldDtos = lookUpFields.Value!.ConvertAll(lf => lf.ToDto());
            return lookUpFieldDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all lookup fields");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<LookUpFieldDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching paged lookup fields, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            var pagedLookUpFields = await _unitOfWork.LookUpFields.GetPagedAsync(page, pageSize);
            if (pagedLookUpFields.IsError)
                return pagedLookUpFields.Errors;

            var lookUpFieldDtos = pagedLookUpFields.Value!.Items.Select(lf => lf.ToDto()).ToList();
            var pagedLookUpFieldDtos = new PagedList<LookUpFieldDto>(lookUpFieldDtos, pagedLookUpFields.Value.TotalItems, page, pageSize);

            return pagedLookUpFieldDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged lookup fields, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFieldDto>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching lookup field by id: {LookUpFieldId}", id);
            var lookUpField = await _unitOfWork.LookUpFields.GetByIdAsync(id);

            if (lookUpField.IsError)
                return lookUpField.Errors;

            if (lookUpField.Value == null)
                return ApplicationErrors.OperationFailed;

            return lookUpField.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lookup field by id: {LookUpFieldId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFieldDto>> CreateAsync(CreateLookUpFieldDto lookUpField)
    {
        try
        {
            if (lookUpField == null)
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(lookUpField.FiledName))
                return ApplicationErrors.MissingRequiredField;

            _logger.LogInformation("Creating new lookup field: {FieldName}", lookUpField.FiledName);

            var lookUpFieldEntity = new LookUpField
            {
                FiledName = lookUpField.FiledName,
                TemplateId = lookUpField.TemplateId
            };

            var addResult = await _unitOfWork.LookUpFields.AddAsync(lookUpFieldEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await _unitOfWork.SaveChangesAsync();
            if (result < 0)
                return ApplicationErrors.DatabaseError;

            _logger.LogInformation("Successfully created lookup field: {FieldName}", lookUpField.FiledName);
            return lookUpFieldEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lookup field");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<LookUpFieldDto>> UpdateAsync(Guid id, UpdateLookUpFieldDto lookUpField)
    {
        try
        {
            if (id == Guid.Empty || lookUpField == null)
                return ApplicationErrors.InvalidInput;

            var existingLookUpField = await _unitOfWork.LookUpFields.GetByIdAsync(id);
            if (existingLookUpField.IsError)
                return existingLookUpField.Errors;

            if (existingLookUpField.Value == null)
                return ApplicationErrors.OperationFailed;

            _logger.LogInformation("Updating lookup field: {LookUpFieldId}", id);

            existingLookUpField.Value.FiledName = lookUpField.FiledName;
            existingLookUpField.Value.TemplateId = lookUpField.TemplateId;

            var updateResult = await _unitOfWork.LookUpFields.UpdateAsync(existingLookUpField.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated lookup field: {LookUpFieldId}", id);
            return existingLookUpField.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lookup field: {LookUpFieldId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var lookUpField = await _unitOfWork.LookUpFields.GetByIdAsync(id);
            if (lookUpField.IsError)
                return lookUpField.Errors;

            if (lookUpField.Value == null)
                return ApplicationErrors.OperationFailed;

            _logger.LogInformation("Deleting lookup field: {LookUpFieldId}", id);

            await _unitOfWork.LookUpFields.RemoveAsync(x => x.Id == lookUpField.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted lookup field: {LookUpFieldId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup field: {LookUpFieldId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<LookUpFieldDto>>> GetByTemplateIdAsync(Guid templateId)
    {
        try
        {
            _logger.LogInformation("Fetching lookup fields by template ID: {TemplateId}", templateId);
            
            var lookUpFields = await _unitOfWork.LookUpFields.GetAllAsync(x => x.TemplateId == templateId);
            if (lookUpFields.IsError)
                return lookUpFields.Errors;

            var lookUpFieldDtos = lookUpFields.Value!.Select(lf => lf.ToDto()).ToList();
            return lookUpFieldDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lookup fields by template ID: {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }
}