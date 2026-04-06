using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Template service CRUD operations
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TemplateService> _logger;

    public TemplateService(IUnitOfWork unitOfWork, ILogger<TemplateService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all templates");
            var templates = await _unitOfWork.Templates.GetAllAsync();
            if(templates.IsError)
            {
                return templates.Errors;
            }

            var templateDtos = templates.Value!.Select(t => t.ToDto()).ToList();
            return templateDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all templates");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<TemplateDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching paged templates, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            var pagedTemplates = await _unitOfWork.Templates.GetPagedAsync(page, pageSize);
            if (pagedTemplates.IsError)
                return pagedTemplates.Errors;

            var templateDtos = pagedTemplates.Value!.Items.Select(t => t.ToDto()).ToList();
            var pagedTemplateDtos = new PagedList<TemplateDto>(templateDtos, pagedTemplates.Value.TotalItems, page, pageSize);

            return pagedTemplateDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged templates, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<TemplateDto>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching template by id: {TemplateId}", id);
            var template = await _unitOfWork.Templates.GetByIdAsync(id);

            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return ApplicationErrors.TemplateNotFound;

            return template.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching template by id: {TemplateId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<TemplateDto>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return ApplicationErrors.InvalidInput;

            _logger.LogInformation("Fetching template by name: {TemplateName}", name);
            var templates = await _unitOfWork.Templates.GetAllAsync();
            if(templates.IsError)
            {
                return templates.Errors;
            }

            var template = templates.Value!.FirstOrDefault(t => t.TemplateName == name);

            if (template == null)
                return ApplicationErrors.TemplateNotFound;

            return template.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching template by name: {TemplateName}", name);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<TemplateDto>> CreateAsync(CreateTemplateDto template)
    {
        try
        {
            if (template == null)
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(template.TemplateName))
                return ApplicationErrors.MissingRequiredField;

            _logger.LogInformation("Creating new template: {TemplateName}", template.TemplateName);

            var templateEntity = new Template
            {
                ContentAsJson = template.ContentAsJson,
                TemplateName = template.TemplateName,
                TemplateDescription = template.TemplateDescription
            };

            var addResult = await _unitOfWork.Templates.AddAsync(templateEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await _unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            _logger.LogInformation("Successfully created template: {TemplateName}", template.TemplateName);
            return templateEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<TemplateDto>> UpdateAsync(Guid id, UpdateTemplateDto template)
    {
        try
        {
            if (id == Guid.Empty || template == null)
                return ApplicationErrors.InvalidInput;

            var existingTemplate = await _unitOfWork.Templates.GetByIdAsync(id);
            if (existingTemplate.IsError)
                return existingTemplate.Errors;

            if (existingTemplate.Value == null)
                return ApplicationErrors.TemplateNotFound;

            _logger.LogInformation("Updating template: {TemplateId}", id);

            existingTemplate.Value.ContentAsJson = template.ContentAsJson;
            existingTemplate.Value.TemplateName = template.TemplateName;
            existingTemplate.Value.TemplateDescription = template.TemplateDescription;

            await _unitOfWork.Templates.UpdateAsync(existingTemplate.Value);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated template: {TemplateId}", id);
            return existingTemplate.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template: {TemplateId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var template = await _unitOfWork.Templates.GetByIdAsync(id);
            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return ApplicationErrors.TemplateNotFound;

            _logger.LogInformation("Deleting template: {TemplateId}", id);

            await _unitOfWork.Templates.RemoveAsync(x => x.Id == template.Value.Id);
            int result = await _unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            _logger.LogInformation("Successfully deleted template: {TemplateId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template: {TemplateId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }
}
