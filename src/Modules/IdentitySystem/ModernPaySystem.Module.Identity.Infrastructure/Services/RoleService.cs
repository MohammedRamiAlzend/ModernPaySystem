using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Identity.Application;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Commons;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Services;

public class RoleService(
    IIdentityUnitOfWork unitOfWork,
    ILogger<RoleService> logger) : IRoleService
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
            logger.LogError(ex, "Error fetching all roles");
            return Error.Failure("InternalServerError", "An error occurred while fetching roles.");
        }
    }

    public async Task<Result<PagedList<RoleDto>>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            logger.LogInformation("Fetching paged roles, page: {Page}, size: {PageSize}", page, pageSize);

            if (page <= 0)
                return new Error("InvalidInput", "Page must be greater than 0.", ErrorKind.Validation);
            if (pageSize <= 0 || pageSize > 100)
                return new Error("InvalidInput", "Page size must be between 1 and 100.", ErrorKind.Validation);

            var pagedRoles = await unitOfWork.Roles.GetPagedAsync(page, pageSize);
            if (pagedRoles.IsError)
                return pagedRoles.Errors;

            var roleDtos = pagedRoles.Value!.Items.Select(r => r.ToDto()).ToList();
            var pagedRoleDtos = new PagedList<RoleDto>(roleDtos, pagedRoles.Value.TotalItems, page, pageSize);

            return pagedRoleDtos;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paged roles");
            return Error.Failure("InternalServerError", "An error occurred while fetching roles.");
        }
    }

    public async Task<Result<RoleDto>> GetByIdAsync(Guid id)
    {
        try
        {
            logger.LogInformation("Fetching role by id: {RoleId}", id);
            var role = await unitOfWork.Roles.GetAsync(
                filter: x => x.Id == id,
                transform: x => x.Include(r => r.Permissions).Include(r => r.Users)
            );

            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return new Error("NotFound", "Role not found.", ErrorKind.NotFound);

            return role.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching role by id: {RoleId}", id);
            return Error.Failure("InternalServerError", "An error occurred while fetching the role.");
        }
    }

    public async Task<Result<RoleDto>> GetByNameAsync(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                return new Error("InvalidInput", "Role name is required.", ErrorKind.Validation);

            logger.LogInformation("Fetching role by name: {RoleName}", name);
            var role = await unitOfWork.Roles.GetAsync(
                filter: x => x.Name == name,
                transform: x => x.Include(r => r.Permissions).Include(r => r.Users)
            );

            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return new Error("NotFound", "Role not found.", ErrorKind.NotFound);

            return role.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching role by name: {RoleName}", name);
            return Error.Failure("InternalServerError", "An error occurred while fetching the role.");
        }
    }

    public async Task<Result<RoleDto>> CreateAsync(CreateRoleDto role)
    {
        try
        {
            if (role == null)
                return new Error("InvalidInput", "Role data is required.", ErrorKind.Validation);

            if (string.IsNullOrWhiteSpace(role.Name))
                return new Error("MissingRequiredField", "Role name is required.", ErrorKind.Validation);

            logger.LogInformation("Creating new role: {RoleName}", role.Name);

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
                return new Error("DatabaseError", "Failed to save role to database.", ErrorKind.Failure);

            logger.LogInformation("Successfully created role: {RoleName}", role.Name);
            return roleEntity.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating role");
            return Error.Failure("InternalServerError", "An error occurred while creating the role.");
        }
    }

    public async Task<Result<RoleDto>> UpdateAsync(Guid id, UpdateRoleDto role)
    {
        try
        {
            if (id == Guid.Empty || role == null)
                return new Error("InvalidInput", "Role ID and data are required.", ErrorKind.Validation);

            var existingRole = await unitOfWork.Roles.GetAsync(
                filter: x => x.Id == id,
                transform: x => x.Include(r => r.Permissions).Include(r => r.Users));

            if (existingRole.IsError)
                return existingRole.Errors;

            if (existingRole.Value == null)
                return new Error("NotFound", "Role not found.", ErrorKind.NotFound);

            logger.LogInformation("Updating role: {RoleId}", id);

            existingRole.Value.Name = role.Name;
            existingRole.Value.Description = role.Description;

            var updateResult = await unitOfWork.Roles.UpdateAsync(existingRole.Value);
            if (updateResult.IsError)
                return updateResult.Errors;

            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return new Error("DatabaseError", "Failed to update role.", ErrorKind.Failure);

            logger.LogInformation("Successfully updated role: {RoleId}", id);
            return existingRole.Value.ToDto();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating role: {RoleId}", id);
            return Error.Failure("InternalServerError", "An error occurred while updating the role.");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
                return new Error("InvalidInput", "Role ID is required.", ErrorKind.Validation);

            var role = await unitOfWork.Roles.GetAsync(filter: x => x.Id == id);

            if (role.IsError)
                return role.Errors;

            if (role.Value == null)
                return new Error("NotFound", "Role not found.", ErrorKind.NotFound);

            logger.LogInformation("Deleting role: {RoleId}", id);

            await unitOfWork.Roles.RemoveAsync(x => x.Id == id);
            int result = await unitOfWork.SaveChangesAsync();
            if (result <= 0)
                return new Error("DatabaseError", "Failed to delete role.", ErrorKind.Failure);

            logger.LogInformation("Successfully deleted role: {RoleId}", id);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting role: {RoleId}", id);
            return Error.Failure("InternalServerError", "An error occurred while deleting the role.");
        }
    }
}
