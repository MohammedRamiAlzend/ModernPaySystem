using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.Archiving;

namespace ModernPaySystem.Infrastructure.Services;

public class DynamicFormService(IUnitOfWork unitOfWork, ILogger<DynamicFormService> logger) : IDynamicFormService
{
    public async Task<Result<IEnumerable<ArchiveFormTemplateDto>>> GetAllAsync()
    {
        try
        {
            var result = await unitOfWork.DynamicForms.GetAllAsync();
            if (result.IsError)
            {
                return result.Errors;
            }

            return result.Value!.Select(x => x.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching dynamic forms");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveFormTemplateDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            if (page <= 0 || pageSize <= 0 || pageSize > 100)
            {
                return ApplicationErrors.InvalidInput;
            }

            var result = await unitOfWork.DynamicForms.GetPagedAsync(page, pageSize);
            if (result.IsError)
            {
                return result.Errors;
            }

            var items = result.Value!.Items.Select(x => x.ToDto()).ToList();
            return new PagedList<ArchiveFormTemplateDto>(items, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged dynamic forms, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> GetByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var result = await unitOfWork.DynamicForms.GetAsync(x => x.Id == id, query => query.Include(x => x.ArchiveRecords));
            if (result.IsError)
            {
                return result.Errors;
            }

            if (result.Value == null)
            {
                return ApplicationErrors.DynamicFormNotFound;
            }

            return result.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching dynamic form {FormId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ApplicationErrors.InvalidInput;
            }

            var result = await unitOfWork.DynamicForms.GetAsync(x => x.FormName == name.Trim(), query => query.Include(x => x.ArchiveRecords));
            if (result.IsError)
            {
                return result.Errors;
            }

            if (result.Value == null)
            {
                return ApplicationErrors.DynamicFormNotFound;
            }

            return result.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching dynamic form by name {FormName}", name);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> CreateAsync(CreateDynamicFormTemplateDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.TemplateFormName) || string.IsNullOrWhiteSpace(dto.ContentAsJson))
            {
                return ApplicationErrors.InvalidInput;
            }

            JsonDocument fieldsDefinition;
            try
            {
                fieldsDefinition = JsonDocument.Parse(dto.ContentAsJson);
            }
            catch (JsonException)
            {
                return ApplicationErrors.InvalidJsonDefinition;
            }

            var exists = await unitOfWork.DynamicForms.AnyAsync(x => x.FormName == dto.TemplateFormName.Trim());
            if (exists)
            {
                return ApplicationErrors.DynamicFormAlreadyExists;
            }

            var form = new ArchiveFormTemplate
            {
                FormName = dto.TemplateFormName.Trim(),
                ContentAsJson = fieldsDefinition.RootElement.GetRawText()
            };

            var addResult = await unitOfWork.DynamicForms.AddAsync(form);
            if (addResult.IsError)
            {
                return addResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            return form.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating dynamic form");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> UpdateAsync(Guid id, UpdateDynamicFormTemplateDto dto)
    {
        try
        {
            if (id == Guid.Empty || dto == null || string.IsNullOrWhiteSpace(dto.TemplateFormName) || string.IsNullOrWhiteSpace(dto.ContentAsJson))
            {
                return ApplicationErrors.InvalidInput;
            }

            JsonDocument fieldsDefinition;
            try
            {
                fieldsDefinition = JsonDocument.Parse(dto.ContentAsJson);
            }
            catch (JsonException)
            {
                return ApplicationErrors.InvalidJsonDefinition;
            }

            var formResult = await unitOfWork.DynamicForms.GetByIdAsync(id);
            if (formResult.IsError)
            {
                return formResult.Errors;
            }

            var form = formResult.Value;
            if (form == null)
            {
                return ApplicationErrors.DynamicFormNotFound;
            }

            var duplicateExists = await unitOfWork.DynamicForms.AnyAsync(x => x.FormName == dto.TemplateFormName.Trim() && x.Id != id);
            if (duplicateExists)
            {
                return ApplicationErrors.DynamicFormAlreadyExists;
            }

            form.FormName = dto.TemplateFormName.Trim();
            form.ContentAsJson = fieldsDefinition.RootElement.GetRawText();

            var updateResult = await unitOfWork.DynamicForms.UpdateAsync(form);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            return form.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating dynamic form {FormId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                return ApplicationErrors.InvalidInput;
            }

            var hasRecords = await unitOfWork.ArchiveRecords.AnyAsync(x => x.FormId == id);
            if (hasRecords)
            {
                return ApplicationErrors.DynamicFormInUse;
            }

            var removeResult = await unitOfWork.DynamicForms.RemoveAsync(x => x.Id == id);
            if (removeResult.IsError)
            {
                return removeResult.Errors;
            }

            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return ApplicationErrors.DatabaseError;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting dynamic form {FormId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }
}
