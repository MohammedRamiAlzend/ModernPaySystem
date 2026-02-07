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

    public async Task<Result<IEnumerable<Template>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all templates");
            var templates = await _unitOfWork.Templates.GetAllAsync();
            if(templates.IsError)
            {
                return templates.Errors;
            }
            return templates.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all templates");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Template>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching template by id: {TemplateId}", id);
            var template = await _unitOfWork.Templates.GetByIdAsync(id);

            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return ApplicationError.TemplateNotFound;

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching template by id: {TemplateId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Template>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return ApplicationError.InvalidInput;

            _logger.LogInformation("Fetching template by name: {TemplateName}", name);
            var templates = await _unitOfWork.Templates.GetAllAsync();
            if(templates.IsError)
            {
                return templates.Errors;
            }
            var template = templates.Value.FirstOrDefault(t => t.TemplateName == name);

            if (template == null)
                return ApplicationError.TemplateNotFound;

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching template by name: {TemplateName}", name);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Template>> CreateAsync(Template template)
    {
        try
        {
            if (template == null)
                return ApplicationError.InvalidInput;

            if (string.IsNullOrWhiteSpace(template.TemplateName))
                return ApplicationError.MissingRequiredField;

            _logger.LogInformation("Creating new template: {TemplateName}", template.TemplateName);

            await _unitOfWork.Templates.AddAsync(template);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created template: {TemplateName}", template.TemplateName);
            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<Template>> UpdateAsync(Guid id, Template template)
    {
        try
        {
            if (id == Guid.Empty || template == null)
                return ApplicationError.InvalidInput;

            var existingTemplate = await _unitOfWork.Templates.GetByIdAsync(id);
            if (existingTemplate.IsError)
                return existingTemplate.Errors;

            if (existingTemplate.Value == null)
                return ApplicationError.TemplateNotFound;

            _logger.LogInformation("Updating template: {TemplateId}", id);

            existingTemplate.Value.TemplateName = template.TemplateName;
            existingTemplate.Value.ContentAsJson = template.ContentAsJson;
            existingTemplate.Value.TemplateDescription = template.TemplateDescription;

            await _unitOfWork.Templates.UpdateAsync(existingTemplate.Value);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated template: {TemplateId}", id);
            return existingTemplate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template: {TemplateId}", id);
            return ApplicationError.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationError.InvalidInput;

            var template = await _unitOfWork.Templates.GetByIdAsync(id);
            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return ApplicationError.TemplateNotFound;

            _logger.LogInformation("Deleting template: {TemplateId}", id);

            await _unitOfWork.Templates.RemoveAsync(x => x.Id == template.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted template: {TemplateId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template: {TemplateId}", id);
            return ApplicationError.InternalServerError;
        }
    }
}
