namespace ModernPaySystem.Infrastructure.Services;

public class RoleService(IUnitOfWork unitOfWork, ILogger<RoleService> logger) : IRoleService
{
    public async Task<Result<IEnumerable<RoleDto>>> GetAllAsync()
    {
        try
        {
            logger.LogInformation("Fetching all roles");
            var roles = await unitOfWork.Roles.GetAllAsync();
            if (roles.IsError)
                return roles.Errors;

            var roleDtos = roles.Value!.ConvertAll(r => r.ToDto());
            return roleDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all roles");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<PagedList<RoleDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            _logger.LogInformation("Fetching paged roles, page: {Page}, size: {PageSize}", page, pageSize);

            // Validate parameters
            if (page <= 0)
                return ApplicationErrors.InvalidInput;
            if (pageSize <= 0 || pageSize > 100) // Limit max page size to prevent abuse
                return ApplicationErrors.InvalidInput;

            var pagedRoles = await unitOfWork.Roles.GetPagedAsync(page, pageSize);
            if (pagedRoles.IsError)
                return pagedRoles.Errors;

            var roleDtos = pagedRoles.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedRoleDtos = new PagedList<RoleDto>(roleDtos, pagedRoles.Value.TotalItems, page, pageSize);

            return pagedRoleDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged roles, page: {Page}, size: {PageSize}", page, pageSize);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RoleDto>> GetByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Fetching role by id: {RoleId}", id);
            var role = await unitOfWork.Roles.GetAsync(
                filter: RoleExpressions.ById(id),
                transform: x => x.Include(r => r.Permissions)
                                 .Include(r => r.Users));

            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return ApplicationErrors.RoleNotFound;

            return role.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role by id: {RoleId}", id);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RoleDto>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return ApplicationErrors.InvalidInput;

            _logger.LogInformation("Fetching role by name: {RoleName}", name);
            var role = await unitOfWork.Roles.GetAsync(
                filter: RoleExpressions.ByName(name),
                transform: x => x.Include(r => r.Permissions)
                                 .Include(r => r.Users));

            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return ApplicationErrors.RoleNotFound;

            return role.Value.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching role by name: {RoleName}", name);
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleDto role)
    {
        try
        {
            if (role == null)
                return ApplicationErrors.InvalidInput;

            if (string.IsNullOrWhiteSpace(role.Name))
                return ApplicationErrors.MissingRequiredField;

            _logger.LogInformation("Creating new role: {RoleName}", role.Name);

            var roleEntity = new Role
            {
                Name = role.Name,
                Description = role.Description
            };

            var addResult = await unitOfWork.Roles.AddAsync(roleEntity);
            if (addResult.IsError)
                return addResult.Errors;

            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            _logger.LogInformation("Successfully created role: {RoleName}", role.Name);
            return roleEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return ApplicationErrors.InternalServerError;
        }
    }

    public async Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto role)
    {
        try
        {
            if (id == Guid.Empty || role == null)
                return ApplicationErrors.InvalidInput;

            var existingRole = await unitOfWork.Roles.GetAsync(
                filter: RoleExpressions.ById(id),
                transform: x => x.Include(r => r.Permissions)
                                 .Include(r => r.Users));

            if (existingRole.IsError)
                return existingRole.Errors;

            if (existingRole.Value == null)
                return ApplicationErrors.RoleNotFound;

            _logger.LogInformation("Updating role: {RoleId}", id);

            existingRole.Value.Name = role.Name;
            existingRole.Value.Description = role.Description;

            var updateResult = await unitOfWork.Roles.UpdateAsync(existingRole.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

            _logger.LogInformation("Successfully updated role: {RoleId}", id);
            return existingRole.Value.ToDto();
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

            var role = await unitOfWork.Roles.GetAsync(
                filter: RoleExpressions.ById(id));

            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return ApplicationErrors.RoleNotFound;

            _logger.LogInformation("Deleting role: {RoleId}", id);

            await unitOfWork.Roles.RemoveAsync(RoleExpressions.ById(id));
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return ApplicationErrors.DatabaseError;

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
