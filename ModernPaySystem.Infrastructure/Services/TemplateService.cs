namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Template service CRUD operations
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TemplateService> _logger;

    private readonly IHttpContextServiceManager httpContextServiceManager;

    public TemplateService(IUnitOfWork unitOfWork, ILogger<TemplateService> logger, IHttpContextServiceManager httpContextServiceManager)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        this.httpContextServiceManager = httpContextServiceManager;
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all templates");
            var getCurrentUserId = httpContextServiceManager.GetCurrentUserId();
            var templates = await _unitOfWork.Templates.GetAllAsync(filter: TemplateExpressions.CanReadByUserId(getCurrentUserId.ToString()),
                transform: x => x.Include(t => t.Ownerships).ThenInclude(o => o.Department).ThenInclude(d => d.Users)
            );
            if (templates.IsError)
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

            var getCurrentUserId = httpContextServiceManager.GetCurrentUserId();

            var pagedTemplates = await _unitOfWork.Templates.GetPagedAsync(
                filter: TemplateExpressions.CanReadByUserId(getCurrentUserId.ToString()),
                page: page,
                pageSize: pageSize,
                transform: x => x.Include(t => t.Ownerships).ThenInclude(o => o.Department).ThenInclude(d => d.Users)
            );
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
            var template = await _unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ById(id),
                transform: x => x.Include(t => t.Ownerships).ThenInclude(o => o.Department).ThenInclude(d => d.Users)
                                 .Include(t => t.LookUpFields));

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
            var template = await _unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ByName(name),
                transform: x => x.Include(t => t.Ownerships).ThenInclude(o => o.Department).ThenInclude(d => d.Users)
                                 .Include(t => t.LookUpFields));

            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return ApplicationErrors.TemplateNotFound;

            return template.Value.ToDto();
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

            var existingTemplate = await _unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ById(id),
                transform: x => x.Include(t => t.Ownerships).ThenInclude(o => o.Department).ThenInclude(d => d.Users)
                                 .Include(t => t.LookUpFields));

            if (existingTemplate.IsError)
                return existingTemplate.Errors;

            if (existingTemplate.Value == null)
                return ApplicationErrors.TemplateNotFound;

            _logger.LogInformation("Updating template: {TemplateId}", id);

            existingTemplate.Value.ContentAsJson = template.ContentAsJson;
            existingTemplate.Value.TemplateName = template.TemplateName;
            existingTemplate.Value.TemplateDescription = template.TemplateDescription;

            await _unitOfWork.Templates.UpdateAsync(existingTemplate.Value);
            int result = await _unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

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

            var template = await _unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ById(id),
                transform: x => x.Include(t => t.Ownerships).ThenInclude(o => o.Department).ThenInclude(d => d.Users));

            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return ApplicationErrors.TemplateNotFound;

            _logger.LogInformation("Deleting template: {TemplateId}", id);

            await _unitOfWork.Templates.RemoveAsync(TemplateExpressions.ById(id));
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

    public async Task<Result<IEnumerable<TemplateOwnershipDto>>> GetOwnershipsAsync(Guid templateId)
    {
        try
        {
            if (templateId == Guid.Empty) return ApplicationErrors.InvalidInput;

            var ownerships = await _unitOfWork.TemplateOwnerships.FindAsync(
                filter: to => to.TemplateId == templateId,
                transform: q => q.Include(to => to.Department));

            if (ownerships.IsError) return ownerships.Errors;

            var dtos = ownerships.Value!.Select(o => new TemplateOwnershipDto
            {
                Id = o.Id,
                TemplateId = o.TemplateId,
                DepartmentId = o.DepartmentId,
                DepartmentName = o.Department?.Name
            }).ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching template ownerships for template: {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<TemplateOwnershipDto>> AddOwnershipAsync(Guid templateId, Guid departmentId)
    {
        try
        {
            if (templateId == Guid.Empty || departmentId == Guid.Empty) return ApplicationErrors.InvalidInput;

            var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
            if (template.IsError) return template.Errors;
            if (template.Value == null) return ApplicationErrors.TemplateNotFound;

            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department.IsError) return department.Errors;
            if (department.Value == null) return ApplicationErrors.DepartmentNotFound;

            var exists = await _unitOfWork.TemplateOwnerships.AnyAsync(to => to.TemplateId == templateId && to.DepartmentId == departmentId);
            if (exists) return ApplicationErrors.DuplicateEntry;

            var ownership = new TemplateOwnership
            {
                TemplateId = templateId,
                DepartmentId = departmentId
            };

            var addRes = await _unitOfWork.TemplateOwnerships.AddAsync(ownership);
            if (addRes.IsError) return addRes.Errors;

            int saved = await _unitOfWork.SaveChangesAsync();
            if (saved <= 0) return ApplicationErrors.DatabaseError;

            return new TemplateOwnershipDto
            {
                Id = ownership.Id,
                TemplateId = ownership.TemplateId,
                DepartmentId = ownership.DepartmentId,
                DepartmentName = department.Value!.Name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding template ownership for template {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> RemoveOwnershipAsync(Guid templateId, Guid departmentId)
    {
        try
        {
            if (templateId == Guid.Empty || departmentId == Guid.Empty) return ApplicationErrors.InvalidInput;

            var removeRes = await _unitOfWork.TemplateOwnerships.RemoveAsync(to => to.TemplateId == templateId && to.DepartmentId == departmentId);
            if (removeRes.IsError) return removeRes.Errors;

            int saved = await _unitOfWork.SaveChangesAsync();
            if (saved <= 0) return ApplicationErrors.DatabaseError;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing template ownership for template {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }
}
