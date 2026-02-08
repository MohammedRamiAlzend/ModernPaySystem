global using Microsoft.AspNetCore.Mvc.ApplicationParts;
global using System.Reflection;
global using Microsoft.AspNetCore.Mvc;
global using ModernPaySystem.Domain.Attrs;
using ModernPaySystem.Domain.DTOs.AuthDtos;
using Microsoft.EntityFrameworkCore;

namespace ModernPaySystem.Infrastructure.Services;

public class PermissionSeederService(
    IServiceProvider serviceProvider,
    ApplicationPartManager applicationPartManager,
    IUnitOfWork unitOfWork) : IPermissionSeederService
{
    private readonly AppDbContext dbContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
    private readonly ApplicationPartManager applicationPartManager = applicationPartManager;

    public async Task SeedPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var superAdminRole = await EnsureSuperAdminRoleExistsAsync(cancellationToken);

        var controllerTypes = applicationPartManager
            .ApplicationParts
            .OfType<AssemblyPart>()
            .SelectMany(part => part.Types)
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);

        var permissionsToSeed = new List<PermissionEntity>();

        foreach (var controllerType in controllerTypes)
        {
            var methods = controllerType.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(EndpointPermissionAttribute), false).Length > 0);

            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<EndpointPermissionAttribute>();
                if (attribute != null)
                {
                    permissionsToSeed.Add(new PermissionEntity
                    {
                        Key = attribute.Key,
                        SubSystem = attribute.SubSystem,
                        Type = attribute.Type,
                        Name = attribute.Name,
                        Description = attribute.Description
                    });
                }
            }
        }

        var existingPermissionsResult = await unitOfWork.Permissions.GetAllAsync();
        if (existingPermissionsResult.IsError)
        {
            throw new Exception($"Failed to retrieve existing permissions: {string.Join(", ", existingPermissionsResult.Errors.Select(e => e.Description))}");
        }

        var existingPermissions = existingPermissionsResult.Value;
        var existingPermissionKeys = existingPermissions!.Select(p => p.Key).ToHashSet();

        var newPermissions = permissionsToSeed
            .Where(p => !existingPermissionKeys.Contains(p.Key))
            .ToList();

        if (newPermissions.Count != 0)
        {
            dbContext.Permissions.AddRange(newPermissions);
            await dbContext.SaveChangesAsync(cancellationToken);

            await AssignPermissionsToSuperAdminRoleAsync(newPermissions, cancellationToken);
        }

        var ensureAllPermissionsResult = await EnsureAllPermissionsAssignedToSuperAdminAsync(cancellationToken);
        if (ensureAllPermissionsResult.IsError)
        {
            throw new Exception($"Failed to ensure all permissions assigned to SuperAdmin: {string.Join(", ", ensureAllPermissionsResult.Errors.Select(e => e.Description))}");
        }

        var ensureDefaultUserResult = await EnsureDefaultSuperAdminUserExistsAsync(cancellationToken);
        if (ensureDefaultUserResult.IsError)
        {
            throw new Exception($"Failed to ensure default SuperAdmin user exists: {string.Join(", ", ensureDefaultUserResult.Errors.Select(e => e.Description))}");
        }
    }

    private async Task<Result<RoleDto>> EnsureSuperAdminRoleExistsAsync(CancellationToken cancellationToken)
    {
        var existingSuperAdminResult = await unitOfWork.Roles.GetAsync(
            x => x.Name == "SuperAdmin",
            x => x.Include(x => x.Permissions)
            .Include(x => x.Users));

        if (existingSuperAdminResult.IsError)
        {
            return existingSuperAdminResult.Errors;
        }

        var existingSuperAdmin = existingSuperAdminResult.Value;
        if (existingSuperAdmin != null)
        {
            return existingSuperAdmin.DTO;
        }

        var superAdminRole = new Role
        {
            Name = "SuperAdmin",
            Description = "Role with all permissions"
        };
        var result = await unitOfWork.Roles.AddAsync(superAdminRole);

        if (result.IsError)
        {
            return result.Errors;
        }

        return superAdminRole.DTO;
    }

    private async Task<Result<Success>> AssignPermissionsToSuperAdminRoleAsync(List<PermissionEntity> permissions, CancellationToken cancellationToken)
    {
        var getSuperAdminRoleResult = await unitOfWork.Roles.GetAsync(r => r.Name == "SuperAdmin");
        if (getSuperAdminRoleResult.IsError)
            return getSuperAdminRoleResult.Errors;
        foreach (var permission in permissions)
        {

            var dbPermissionResult = await unitOfWork.Permissions.GetAsync(p => p.Key == permission.Key);
            if (dbPermissionResult.IsError)
                return dbPermissionResult.Errors;

            var dbPermission = dbPermissionResult.Value;
            dbPermission.Roles.Add(getSuperAdminRoleResult.Value!);
            var updateResult = await unitOfWork.Permissions.UpdateAsync(dbPermission);
            if (updateResult.IsError)
                return updateResult.Errors;
        }

        int result = await unitOfWork.SaveChangesAsync();
        return result > 0
            ? Result.Success
            : new Error();
    }

    private async Task<Result<Success>> EnsureAllPermissionsAssignedToSuperAdminAsync(CancellationToken cancellationToken)
    {
        var superAdminRoleResult = await unitOfWork.Roles.GetAsync(
            r => r.Name == "SuperAdmin",
            query => query.Include(r => r.Permissions));

        if (superAdminRoleResult.IsError)
        {
            return superAdminRoleResult.Errors;
        }

        var superAdminRole = superAdminRoleResult.Value;
        if (superAdminRole == null)
        {
            return new Error();
        }

        var allPermissionsResult = await unitOfWork.Permissions.GetAllAsync();
        if (allPermissionsResult.IsError)
        {
            return allPermissionsResult.Errors;
        }

        var allPermissions = allPermissionsResult.Value!;
        var assignedPermissionIds = superAdminRole.Permissions.Select(p => p.Id).ToHashSet();

        var unassignedPermissions = allPermissions
            .Where(p => !assignedPermissionIds.Contains(p.Id))
            .ToList();

        foreach (var permission in unassignedPermissions)
        {
            var dbPermissionResult = await unitOfWork.Permissions.GetAsync(p => p.Key == permission.Key);
            if (dbPermissionResult.IsError)
                return dbPermissionResult.Errors;

            var dbPermission = dbPermissionResult.Value;
            dbPermission.Roles.Add(superAdminRole);
            var updateResult = await unitOfWork.Permissions.UpdateAsync(dbPermission);
            if (updateResult.IsError)
                return updateResult.Errors;
        }

        return Result.Success;
    }

    private async Task<Result<Success>> EnsureDefaultSuperAdminUserExistsAsync(
      CancellationToken cancellationToken)
    {
        const string defaultHashedPassword =
            "URiiKlWs+xjqpKmMjtQC8SMG0Oc6nIA/XDbct9TB3/k=";

        var getSuperAdminRoleResult = await unitOfWork.Roles.GetAsync(r => r.Name == "SuperAdmin");
        if (getSuperAdminRoleResult.IsError)
        {
            return getSuperAdminRoleResult.Errors;
        }

        var role = getSuperAdminRoleResult.Value;
        Guid superAdminRoleId = role.Id;

        var userResult = await unitOfWork.Users.GetAsync(
          u => u.Id == Constants.DefaultUserId,
          query => query.Include(u => u.Roles));

        if (userResult.IsError && userResult.Errors.Select(x => x.Code).Any(x => x == "404"))
        {
            User user1 = new User
            {
                Id = Constants.DefaultUserId,
                UserName = "SuperAdmin",
                HashedPassword = defaultHashedPassword,
                Roles = new List<Role> { role }
            };
        }

        var user = userResult.Value;
        if (user == null)
        {
            return Result.Success; // User doesn't exist, nothing to do
        }

        // Ensure password consistency
        bool passwordUpdated = false;
        if (user.HashedPassword != defaultHashedPassword)
        {
            user.HashedPassword = defaultHashedPassword;
            var updateResult = await unitOfWork.Users.UpdateAsync(user);
            if (updateResult.IsError)
            {
                return updateResult.Errors;
            }
            passwordUpdated = true;
        }

        // Ensure SuperAdmin role
        var hasSuperAdminRole = user.Roles
            .Any(r => r.Id == superAdminRoleId);

        if (hasSuperAdminRole)
        {
            if (passwordUpdated)
            {
                var saveResult = await unitOfWork.SaveChangesAsync();
                if (saveResult <= 0)
                {
                    return new Error();
                }
            }

            return Result.Success;
        }

        user.Roles.Add(role);
        var updateUserResult = await unitOfWork.Users.UpdateAsync(user);
        if (passwordUpdated)
        {
            var saveResult = await unitOfWork.SaveChangesAsync();
            if (saveResult <= 0)
            {
                return new Error();
            }
        }

        return Result.Success;
    }

}