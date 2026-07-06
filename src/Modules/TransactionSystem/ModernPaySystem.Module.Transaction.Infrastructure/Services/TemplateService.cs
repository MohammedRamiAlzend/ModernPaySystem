using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Transaction.Application;
using ModernPaySystem.Module.Transaction.Application.Interfaces;
using ModernPaySystem.Module.Transaction.Domain;
using ModernPaySystem.Module.Transaction.Domain.DTOs;
using ModernPaySystem.Module.Transaction.Domain.Entities;
using ModernPaySystem.SharedKernel.Application.Services;
using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Transaction.Infrastructure.Services;

public class TemplateService(
    ITransactionUnitOfWork unitOfWork,
    ILogger<TemplateService> logger,
    IHttpContextServiceManager httpContextServiceManager) : ITemplateService
{
    public async Task<Result<IEnumerable<TemplateDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all templates");
            var getCurrentUserId = httpContextServiceManager.GetCurrentUserId();
            var templates = await unitOfWork.Templates.GetAllAsync(
                filter: TemplateExpressions.CanReadByUserId(getCurrentUserId),
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department)
            );
            if (templates.IsError)
                return templates.Errors;

            var templateDtos = templates.Value!.Select(t => t.ToDto()).ToList();
            return templateDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all templates");
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<PagedList<TemplateDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged templates, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return TransactionErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100)
                return TransactionErrors.InvalidInput;

            var getCurrentUserId = httpContextServiceManager.GetCurrentUserId();

            var pagedTemplates = await unitOfWork.Templates.GetPagedAsync(
                filter: TemplateExpressions.CanReadByUserId(getCurrentUserId),
                page: page,
                pageSize: pageSize,
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department)
            );
            if (pagedTemplates.IsError)
                return pagedTemplates.Errors;

            var templateDtos = pagedTemplates.Value!.Items.Select(t => t.ToDto()).ToList();
            var pagedTemplateDtos = new PagedList<TemplateDto>(templateDtos, pagedTemplates.Value.TotalItems, page, pageSize);

            return pagedTemplateDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged templates, page: {Page}, size: {PageSize}", page, pageSize);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<TemplateDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching template by id: {TemplateId}", id);
            var template = await unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ById(id),
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department)
                                 .Include(t => t.LookUpFields));

            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return TransactionErrors.TemplateNotFound;

            return template.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching template by id: {TemplateId}", id);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<TemplateDto>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return TransactionErrors.InvalidInput;

            logger.LogInformation("Fetching template by name: {TemplateName}", name);
            var template = await unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ByName(name),
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department)
                                 .Include(t => t.LookUpFields));

            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return TransactionErrors.TemplateNotFound;

            return template.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching template by name: {TemplateName}", name);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<TemplateDto>> CreateAsync(CreateTemplateDto template)
    {
        try
        {
            if (template == null)
                return TransactionErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(template.TemplateName))
                return Error.Validation("MissingRequiredField", "Template name is required.");

            logger.LogInformation("Creating new template: {TemplateName}", template.TemplateName);

            var currentUserId = httpContextServiceManager.GetCurrentUserId();

            var templateEntity = new Template
            {
                ContentAsJson = template.ContentAsJson,
                TemplateName = template.TemplateName,
                TemplateDescription = template.TemplateDescription,
                IsRequireAttachments = template.IsRequireAttachments,
                DefaultReceiverDepartmentId = template.DefaultReceiverDepartmentId
            };

            var addResult = await unitOfWork.Templates.AddAsync(templateEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return TransactionErrors.DatabaseError;

            if (template.DepartmentId.HasValue)
            {
                var departmentOwnership = new TemplateDepartmentOwnership
                {
                    TemplateId = templateEntity.Id,
                    DepartmentId = template.DepartmentId.Value
                };

                var addDepartmentOwnershipResult = await unitOfWork.TemplateDepartmentOwnerships.AddAsync(departmentOwnership);
                if (addDepartmentOwnershipResult.IsError)
                    return addDepartmentOwnershipResult.Errors;
            }

            var ownerId = template.OwnerId ?? currentUserId;
            var userOwnership = new UserTemplateOwnership
            {
                TemplateId = templateEntity.Id,
                UserId = ownerId
            };

            var addUserOwnershipResult = await unitOfWork.UserTemplateOwnerships.AddAsync(userOwnership);
            if (addUserOwnershipResult.IsError)
                return addUserOwnershipResult.Errors;

            await unitOfWork.SaveChangesAsync();
            return templateEntity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating template");
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<TemplateDto>> UpdateAsync(Guid id, UpdateTemplateDto template)
    {
        try
        {
            if (id == Guid.Empty || template == null)
                return TransactionErrors.InvalidInput;

            var existingTemplate = await unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ById(id),
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department)
                                 .Include(t => t.LookUpFields));

            if (existingTemplate.IsError)
                return existingTemplate.Errors;

            if (existingTemplate.Value == null)
                return TransactionErrors.TemplateNotFound;

            logger.LogInformation("Updating template: {TemplateId}", id);

            existingTemplate.Value.ContentAsJson = template.ContentAsJson;
            existingTemplate.Value.TemplateName = template.TemplateName;
            existingTemplate.Value.TemplateDescription = template.TemplateDescription;
            existingTemplate.Value.IsRequireAttachments = template.IsRequireAttachments;
            existingTemplate.Value.DefaultReceiverDepartmentId = template.DefaultReceiverDepartmentId;

            await unitOfWork.Templates.UpdateAsync(existingTemplate.Value);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Successfully updated template: {TemplateId}", id);
            return existingTemplate.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating template: {TemplateId}", id);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var template = await unitOfWork.Templates.GetAsync(
                filter: TemplateExpressions.ById(id),
                transform: x => x.Include(t => t.DepartmentOwnerships)!.ThenInclude(o => o.Department));

            if (template.IsError)
                return template.Errors;

            if (template.Value == null)
                return TransactionErrors.TemplateNotFound;

            logger.LogInformation("Deleting template: {TemplateId}", id);

            await unitOfWork.Templates.RemoveAsync(TemplateExpressions.ById(id));
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return TransactionErrors.DatabaseError;

            logger.LogInformation("Successfully deleted template: {TemplateId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting template: {TemplateId}", id);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<IEnumerable<TemplateOwnershipDto>>> GetOwnershipsAsync(Guid templateId)
    {
        try
        {
            if (templateId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var ownerships = await unitOfWork.TemplateDepartmentOwnerships.FindAsync(
                filter: to => to.TemplateId == templateId,
                transform: q => q.Include(to => to.Department));

            if (ownerships.IsError)
                return ownerships.Errors;

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
            logger.LogError(ex, "Error fetching template ownerships for template: {TemplateId}", templateId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<TemplateOwnershipDto>> AddOwnershipAsync(Guid templateId, Guid departmentId)
    {
        try
        {
            if (templateId == Guid.Empty || departmentId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var template = await unitOfWork.Templates.GetByIdAsync(templateId);
            if (template.IsError)
                return template.Errors;
            if (template.Value == null)
                return TransactionErrors.TemplateNotFound;

            var exists = await unitOfWork.TemplateDepartmentOwnerships.AnyAsync(to => to.TemplateId == templateId && to.DepartmentId == departmentId);
            if (exists)
                return Error.Validation("DuplicateEntry", "Department is already an owner of this template.");

            var ownership = new TemplateDepartmentOwnership
            {
                TemplateId = templateId,
                DepartmentId = departmentId
            };

            var addRes = await unitOfWork.TemplateDepartmentOwnerships.AddAsync(ownership);
            if (addRes.IsError)
                return addRes.Errors;

            int saved = await unitOfWork.SaveChangesAsync();
            if (saved <= 0)
                return TransactionErrors.DatabaseError;

            return new TemplateOwnershipDto
            {
                Id = ownership.Id,
                TemplateId = ownership.TemplateId,
                DepartmentId = ownership.DepartmentId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding template ownership for template {TemplateId}", templateId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<bool>> RemoveOwnershipAsync(Guid templateId, Guid departmentId)
    {
        try
        {
            if (templateId == Guid.Empty || departmentId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var removeRes = await unitOfWork.TemplateDepartmentOwnerships.RemoveAsync(to => to.TemplateId == templateId && to.DepartmentId == departmentId);
            if (removeRes.IsError)
                return removeRes.Errors;

            int saved = await unitOfWork.SaveChangesAsync();
            if (saved <= 0)
                return TransactionErrors.DatabaseError;

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing template ownership for template {TemplateId}", templateId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<IEnumerable<UserTemplateOwnershipDto>>> GetUserOwnershipsAsync(Guid templateId)
    {
        try
        {
            if (templateId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var ownerships = await unitOfWork.UserTemplateOwnerships.FindAsync(
                filter: uto => uto.TemplateId == templateId);

            if (ownerships.IsError)
                return ownerships.Errors;

            var dtos = ownerships.Value!.Select(uto => new UserTemplateOwnershipDto
            {
                Id = uto.Id,
                TemplateId = uto.TemplateId,
                UserId = uto.UserId
            }).ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user template ownerships for template: {TemplateId}", templateId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<UserTemplateOwnershipDto>> AddUserOwnershipAsync(Guid templateId, Guid userId)
    {
        try
        {
            if (templateId == Guid.Empty || userId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var template = await unitOfWork.Templates.GetByIdAsync(templateId);
            if (template.IsError)
                return template.Errors;
            if (template.Value == null)
                return TransactionErrors.TemplateNotFound;

            var exists = await unitOfWork.UserTemplateOwnerships.AnyAsync(uto => uto.TemplateId == templateId && uto.UserId == userId);
            if (exists)
                return Error.Validation("DuplicateEntry", "User is already an owner of this template.");

            var ownership = new UserTemplateOwnership
            {
                TemplateId = templateId,
                UserId = userId
            };

            var addRes = await unitOfWork.UserTemplateOwnerships.AddAsync(ownership);
            if (addRes.IsError)
                return addRes.Errors;

            int saved = await unitOfWork.SaveChangesAsync();
            if (saved <= 0)
                return TransactionErrors.DatabaseError;

            return new UserTemplateOwnershipDto
            {
                Id = ownership.Id,
                TemplateId = ownership.TemplateId,
                UserId = ownership.UserId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding user template ownership for template {TemplateId}", templateId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<UserTemplateOwnershipDto>> RemoveUserOwnershipAsync(Guid templateId, Guid userId)
    {
        try
        {
            if (templateId == Guid.Empty || userId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            var ownership = await unitOfWork.UserTemplateOwnerships.GetAsync(
                filter: uto => uto.TemplateId == templateId && uto.UserId == userId);

            if (ownership.IsError)
                return ownership.Errors;
            if (ownership.Value == null)
                return TransactionErrors.TemplateNotFound;

            var removeRes = await unitOfWork.UserTemplateOwnerships.RemoveAsync(
                uto => uto.TemplateId == templateId && uto.UserId == userId);
            if (removeRes.IsError)
                return removeRes.Errors;

            int saved = await unitOfWork.SaveChangesAsync();
            if (saved <= 0)
                return TransactionErrors.DatabaseError;

            return new UserTemplateOwnershipDto
            {
                Id = ownership.Value.Id,
                TemplateId = ownership.Value.TemplateId,
                UserId = ownership.Value.UserId
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing user template ownership for template {TemplateId}", templateId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetByDepartmentAsync(Guid departmentId)
    {
        try
        {
            if (departmentId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            logger.LogInformation("Fetching templates for department: {DepartmentId}", departmentId);

            var ownerships = await unitOfWork.TemplateDepartmentOwnerships.FindAsync(
                filter: to => to.DepartmentId == departmentId,
                transform: q => q.Include(to => to.Template)
                                 .ThenInclude(t => t!.DepartmentOwnerships)
                                 .Include(to => to.Template)
                                 .ThenInclude(t => t!.LookUpFields));

            if (ownerships.IsError)
                return ownerships.Errors;

            var dtos = ownerships.Value!
                .Where(o => o.Template != null)
                .Select(o => o.Template!.ToDto())
                .ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching templates for department: {DepartmentId}", departmentId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }

    public async Task<Result<IEnumerable<TemplateDto>>> GetUserDirectAsync(Guid userId)
    {
        try
        {
            if (userId == Guid.Empty)
                return TransactionErrors.InvalidInput;

            logger.LogInformation("Fetching direct templates for user: {UserId}", userId);

            var ownerships = await unitOfWork.UserTemplateOwnerships.FindAsync(
                filter: uto => uto.UserId == userId,
                transform: q => q.Include(uto => uto.Template)
                                 .ThenInclude(t => t!.DepartmentOwnerships)
                                 .Include(uto => uto.Template)
                                 .ThenInclude(t => t!.LookUpFields));

            if (ownerships.IsError)
                return ownerships.Errors;

            var dtos = ownerships.Value!
                .Where(o => o.Template != null)
                .Select(o => o.Template!.ToDto())
                .ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching direct templates for user: {UserId}", userId);
            return Error.Failure("InternalServerError", "An unexpected error occurred.");
        }
    }
}
