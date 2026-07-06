using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Archive.Application;
using ModernPaySystem.Module.Archive.Application.Interfaces;
using ModernPaySystem.Module.Archive.Domain;
using ModernPaySystem.Module.Archive.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;
using System.Text.Json;

namespace ModernPaySystem.Module.Archive.Infrastructure.Services;

public class ArchiveFormTemplateService(
    IArchiveUnitOfWork unitOfWork,
    ILogger<ArchiveFormTemplateService> logger,
    IHttpContextServiceManager httpContextServiceManager) : IArchiveFormTemplateService
{
    public async Task<Result<IEnumerable<ArchiveFormTemplateDto>>> GetAllAsync()
    {
        try
        {
            var result = await unitOfWork.DynamicForms.GetAllAsync();

            if (result.IsError)
                return result.Errors;

            return result.Value.Select(e => e.ToDto()).ToList();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all form templates");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<ArchiveFormTemplateDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            var result = await unitOfWork.DynamicForms.GetPagedAsync(page, pageSize);

            if (result.IsError)
                return result.Errors;

            var dtos = result.Value.Items.Select(e => e.ToDto()).ToList();
            return new PagedList<ArchiveFormTemplateDto>(dtos, result.Value.TotalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged form templates");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await unitOfWork.DynamicForms.GetAsync(
                filter: x => x.Id == id,
                transform: q => q.Include(x => x.ArchiveRecords));

            if (result.IsError)
                return result.Errors;

            if (result.Value == null)
                return ArchiveErrors.DynamicFormNotFound;

            return result.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching form template by id: {Id}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> GetByNameAsync(string name)
    {
        try
        {
            var result = await unitOfWork.DynamicForms.GetAsync(
                filter: x => x.FormName == name.Trim());

            if (result.IsError)
                return result.Errors;

            if (result.Value == null)
                return ArchiveErrors.DynamicFormNotFound;

            return result.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching form template by name: {Name}", name);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> CreateAsync(CreateDynamicFormTemplateDto dto)
    {
        try
        {
            try
            {
                JsonDocument.Parse(dto.ContentAsJson);
            }
            catch (JsonException)
            {
                return ArchiveErrors.InvalidJsonDefinition;
            }

            var existing = await unitOfWork.DynamicForms.GetAsync(
                filter: x => x.FormName == dto.TemplateFormName.Trim());

            if (existing.IsError)
                return existing.Errors;

            if (existing.Value != null)
                return ArchiveErrors.DynamicFormAlreadyExists;

            var entity = new ArchiveFormTemplate
            {
                Id = Guid.NewGuid(),
                FormName = dto.TemplateFormName.Trim(),
                ContentAsJson = dto.ContentAsJson
            };

            var addResult = await unitOfWork.DynamicForms.AddAsync(entity);

            if (addResult.IsError)
                return addResult.Errors;

            await unitOfWork.SaveChangesAsync();

            var created = await unitOfWork.DynamicForms.GetAsync(
                filter: x => x.Id == entity.Id);

            if (created.IsError)
                return created.Errors;

            var userId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Form template created: {FormName} by user {UserId}", entity.FormName, userId);

            return created.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating form template");
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<ArchiveFormTemplateDto>> UpdateAsync(Guid id, UpdateDynamicFormTemplateDto dto)
    {
        try
        {
            try
            {
                JsonDocument.Parse(dto.ContentAsJson);
            }
            catch (JsonException)
            {
                return ArchiveErrors.InvalidJsonDefinition;
            }

            var existing = await unitOfWork.DynamicForms.GetAsync(
                filter: x => x.Id == id);

            if (existing.IsError)
                return existing.Errors;

            if (existing.Value == null)
                return ArchiveErrors.DynamicFormNotFound;

            var duplicate = await unitOfWork.DynamicForms.AnyAsync(
                x => x.FormName == dto.TemplateFormName.Trim() && x.Id != id);

            if (duplicate)
                return ArchiveErrors.DynamicFormAlreadyExists;

            existing.Value.FormName = dto.TemplateFormName.Trim();
            existing.Value.ContentAsJson = dto.ContentAsJson;

            var updateResult = await unitOfWork.DynamicForms.UpdateAsync(existing.Value);

            if (updateResult.IsError)
                return updateResult.Errors;

            await unitOfWork.SaveChangesAsync();

            var updated = await unitOfWork.DynamicForms.GetAsync(
                filter: x => x.Id == id,
                transform: q => q.Include(x => x.ArchiveRecords));

            if (updated.IsError)
                return updated.Errors;

            var userId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Form template updated: {FormName} by user {UserId}", existing.Value.FormName, userId);

            return updated.Value!.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating form template: {Id}", id);
            return ArchiveErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var existing = await unitOfWork.DynamicForms.GetAsync(
                filter: x => x.Id == id);

            if (existing.IsError)
                return existing.Errors;

            if (existing.Value == null)
                return ArchiveErrors.DynamicFormNotFound;

            var hasRecords = await unitOfWork.ArchiveRecords.AnyAsync(
                x => x.FormId == id);

            if (hasRecords)
                return ArchiveErrors.DynamicFormInUse;

            await unitOfWork.DynamicForms.RemoveAsync(x => x.Id == id);
            await unitOfWork.SaveChangesAsync();

            var userId = httpContextServiceManager.GetCurrentUserId();
            logger.LogInformation("Form template deleted: {FormName} by user {UserId}", existing.Value.FormName, userId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting form template: {Id}", id);
            return ArchiveErrors.InternalServerError;
        }
    }
}
