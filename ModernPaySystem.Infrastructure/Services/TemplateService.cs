using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.DTOs;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Application.Repos;
using ModernPaySystem.Application.Services;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

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
            var templates = await _unitOfWork.Templates.GetAllAsync(filter: TemplateExpressions.CanReadByUserId(getCurrentUserId),
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department).ThenInclude(d => d!.Users)
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
                filter: TemplateExpressions.CanReadByUserId(getCurrentUserId),
                page: page,
                pageSize: pageSize,
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department).ThenInclude(d => d!.Users)
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
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department).ThenInclude(d => d!.Users)
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
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department).ThenInclude(d => d!.Users)
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

            var currentUserId = httpContextServiceManager.GetCurrentUserId();
            var currentUserResult = await _unitOfWork.Users.GetByIdAsync(currentUserId);

            if (currentUserResult.IsError)
                return currentUserResult.Errors;

            var currentUser = currentUserResult.Value;
            if (currentUser == null)
                return ApplicationErrors.UserNotFound;

            var templateEntity = new Template
            {
                ContentAsJson = template.ContentAsJson,
                TemplateName = template.TemplateName,
                TemplateDescription = template.TemplateDescription,
                IsRequireAttachments = template.IsRequireAttachments,
                DefaultReceiverDepartmentId = template.DefaultReceiverDepartmentId
            };

            var addResult = await _unitOfWork.Templates.AddAsync(templateEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await _unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            if (currentUser.DepartmentId.HasValue)
            {
                var departmentOwnership = new TemplateDepartmentOwnership
                {
                    TemplateId = templateEntity.Id,
                    DepartmentId = currentUser.DepartmentId.Value
                };

                var addDepartmentOwnershipResult = await _unitOfWork.TemplateOwnerships.AddAsync(departmentOwnership);
                if (addDepartmentOwnershipResult.IsError)
                    return addDepartmentOwnershipResult.Errors;
            }
            var userOwnership = new UserTemplateOwnership
            {
                TemplateId = templateEntity.Id,
                UserId = currentUser.Id
            };

            var addUserOwnershipResult = await _unitOfWork.UserTemplateOwnerships.AddAsync(userOwnership);
            if (addUserOwnershipResult.IsError)
                return addUserOwnershipResult.Errors;

            await _unitOfWork.SaveChangesAsync();
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
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department).ThenInclude(d => d!.Users)
                                 .Include(t => t.LookUpFields));

            if (existingTemplate.IsError)
                return existingTemplate.Errors;

            if (existingTemplate.Value == null)
                return ApplicationErrors.TemplateNotFound;

            _logger.LogInformation("Updating template: {TemplateId}", id);

            existingTemplate.Value.ContentAsJson = template.ContentAsJson;
            existingTemplate.Value.TemplateName = template.TemplateName;
            existingTemplate.Value.TemplateDescription = template.TemplateDescription;
            existingTemplate.Value.IsRequireAttachments = template.IsRequireAttachments;
            existingTemplate.Value.DefaultReceiverDepartmentId = template.DefaultReceiverDepartmentId;


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

            var template = await _unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ById(id),
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department).ThenInclude(d => d!.Users));

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

            var ownership = new TemplateDepartmentOwnership
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

    public async Task<Result<IEnumerable<UserTemplateOwnershipDto>>> GetUserOwnershipsAsync(Guid templateId)
    {
        try
        {
            if (templateId == Guid.Empty) return ApplicationErrors.InvalidInput;

            var ownerships = await _unitOfWork.UserTemplateOwnerships.FindAsync(
                filter: uto => uto.TemplateId == templateId,
                transform: q => q.Include(uto => uto.User)
            );

            if (ownerships.IsError) return ownerships.Errors;

            var dtos = ownerships.Value!.Select(uto => new UserTemplateOwnershipDto
            {
                Id = uto.Id,
                TemplateId = uto.TemplateId,
                UserId = uto.UserId,
                UserName = uto.User?.UserName
            }).ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user template ownerships for template: {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserTemplateOwnershipDto>> AddUserOwnershipAsync(Guid templateId, Guid userId)
    {
        try
        {
            if (templateId == Guid.Empty || userId == Guid.Empty) return ApplicationErrors.InvalidInput;

            var template = await _unitOfWork.Templates.GetByIdAsync(templateId);
            if (template.IsError) return template.Errors;
            if (template.Value == null) return ApplicationErrors.TemplateNotFound;

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user.IsError) return user.Errors;
            if (user.Value == null) return ApplicationErrors.UserNotFound;

            var exists = await _unitOfWork.UserTemplateOwnerships.AnyAsync(uto => uto.TemplateId == templateId && uto.UserId == userId);
            if (exists) return ApplicationErrors.DuplicateEntry;

            var ownership = new UserTemplateOwnership
            {
                TemplateId = templateId,
                UserId = userId
            };

            var addRes = await _unitOfWork.UserTemplateOwnerships.AddAsync(ownership);
            if (addRes.IsError) return addRes.Errors;

            int saved = await _unitOfWork.SaveChangesAsync();
            if (saved <= 0) return ApplicationErrors.DatabaseError;

            return new UserTemplateOwnershipDto
            {
                Id = ownership.Id,
                TemplateId = ownership.TemplateId,
                UserId = ownership.UserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding user template ownership for template {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<UserTemplateOwnershipDto>> RemoveUserOwnershipAsync(Guid templateId, Guid userId)
    {
        try
        {
            if (templateId == Guid.Empty || userId == Guid.Empty) return ApplicationErrors.InvalidInput;

            var ownership = await _unitOfWork.UserTemplateOwnerships.GetAsync(
                filter: uto => uto.TemplateId == templateId && uto.UserId == userId
            );

            if (ownership.IsError) return ownership.Errors;
            if (ownership.Value == null) return ApplicationErrors.UserNotFound;

            var removeRes = await _unitOfWork.UserTemplateOwnerships.RemoveAsync(
                uto => uto.TemplateId == templateId && uto.UserId == userId);
            if (removeRes.IsError) return removeRes.Errors;

            int saved = await _unitOfWork.SaveChangesAsync();
            if (saved <= 0) return ApplicationErrors.DatabaseError;

            return new UserTemplateOwnershipDto
            {
                Id = ownership.Value.Id,
                TemplateId = ownership.Value.TemplateId,
                UserId = ownership.Value.UserId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user template ownership for template {TemplateId}", templateId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetByDepartmentAsync(Guid departmentId)
    {
        try
        {
            if (departmentId == Guid.Empty) return ApplicationErrors.InvalidInput;

            _logger.LogInformation("Fetching templates for department: {DepartmentId}", departmentId);

            // We need to fetch TemplateDepartmentOwnerships for this department and include the Template
            var ownerships = await _unitOfWork.TemplateOwnerships.FindAsync(
                filter: to => to.DepartmentId == departmentId,
                transform: q => q.Include(to => to.Template)
                                 .ThenInclude(t => t!.DepartmentOwnerships)
                                 .Include(to => to.Template)
                                 .ThenInclude(t => t!.LookUpFields)
            );

            if (ownerships.IsError) return ownerships.Errors;

            // Map the fetched templates to TemplateDto
            var dtos = ownerships.Value!
                .Where(o => o.Template != null)
                .Select(o => o.Template!.ToDto())
                .ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching templates for department: {DepartmentId}", departmentId);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetUserDirectAsync(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty) return ApplicationErrors.InvalidInput;

            _logger.LogInformation("Fetching direct templates for user: {UserId}", userId);

            // We need to fetch UserTemplateOwnerships for this user and include the Template
            var ownerships = await _unitOfWork.UserTemplateOwnerships.FindAsync(
                filter: uto => uto.UserId == userId,
                transform: q => q.Include(uto => uto.Template)
                                 .ThenInclude(t => t!.DepartmentOwnerships)
                                 .Include(uto => uto.Template)
                                 .ThenInclude(t => t!.LookUpFields)
            );

            if (ownerships.IsError) return ownerships.Errors;

            // Map the fetched templates to TemplateDto
            var dtos = ownerships.Value!
                .Where(o => o.Template != null)
                .Select(o => o.Template!.ToDto())
                .ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching direct templates for user: {UserId}", userId);
            return ApplicationErrors.InternalServerError;
        }
    }
}