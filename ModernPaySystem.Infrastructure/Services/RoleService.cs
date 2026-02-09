using Microsoft.Extensions.Logging;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Commons;
using ModernPaySystem.Domain.Entities.SharedEntities;
using ModernPaySystem.Infrastructure.Persistence;

namespace ModernPaySystem.Infrastructure.Services;

/// <summary>
/// Implementation of Role service CRUD operations
/// </summary>
public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IUnitOfWork unitOfWork, ILogger<RoleService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Role>>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all roles");
            var roles = await _unitOfWork.Roles.GetAllAsync();
            if (roles.IsError)
                return roles.Errors;

            return roles.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all roles");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<Role>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching paged roles, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            var pagedRoles = await _unitOfWork.Roles.GetPagedAsync(page, pageSize);
            if (pagedRoles.IsError)
                return pagedRoles.Errors;

            return pagedRoles.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged roles, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Role>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching role by id: {RoleId}", id);
            var role = await _unitOfWork.Roles.GetByIdAsync(id);

            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return ApplicationErrors.RoleNotFound;

            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role by id: {RoleId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Role>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return ApplicationErrors.InvalidInput;

            _logger.LogInformation("Fetching role by name: {RoleName}", name);
            var roles = await _unitOfWork.Roles.GetAllAsync();
            if (roles.IsError)
            {
                return roles.Errors;
            }
            var role = roles.Value.FirstOrDefault(r => r.Name == name);

            if (role == null)
                return ApplicationErrors.RoleNotFound;

            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role by name: {RoleName}", name);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Role>> CreateAsync(Role role)
    {
        try
        {
            if (role == null)
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(role.Name))
                return ApplicationErrors.MissingRequiredField;

            _logger.LogInformation("Creating new role: {RoleName}", role.Name);

            await _unitOfWork.Roles.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully created role: {RoleName}", role.Name);
            return role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<Role>> UpdateAsync(Guid id, Role role)
    {
        try
        {
            if (id == Guid.Empty || role == null)
                return ApplicationErrors.InvalidInput;

            var existingRole = await _unitOfWork.Roles.GetByIdAsync(id);
            if (existingRole.IsError)
                return existingRole.Errors;

            if (existingRole.Value == null)
                return ApplicationErrors.RoleNotFound;

            _logger.LogInformation("Updating role: {RoleId}", id);

            existingRole.Value.Name = role.Name;
            existingRole.Value.Description = role.Description;

            await _unitOfWork.Roles.UpdateAsync(existingRole.Value);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully updated role: {RoleId}", id);
            return existingRole;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role: {RoleId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return ApplicationErrors.InvalidInput;

            var role = await _unitOfWork.Roles.GetByIdAsync(id);
            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return ApplicationErrors.RoleNotFound;

            _logger.LogInformation("Deleting role: {RoleId}", id);

            await _unitOfWork.Roles.RemoveAsync(x => x.Id == role.Value.Id);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Successfully deleted role: {RoleId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role: {RoleId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }
}
