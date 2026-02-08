global using Microsoft.AspNetCore.Mvc.ApplicationParts;
global using System.Reflection;
global using Microsoft.AspNetCore.Mvc;
global using ModernPaySystem.Application.Repos;
global using ModernPaySystem.Domain.Attrs;
using ModernPaySystem.Domain.DTOs.AuthDtos;
using Microsoft.EntityFrameworkCore;

namespace ModernPaySystem.Infrastructure.Services;

public class PermissionSeederService(
    IServiceProvider serviceProvider,
    ApplicationPartManager applicationPartManager,
    IRoleService roleService,
    IUnitOfWork unitOfWork) : IPermissionSeederService
{
    private readonly AppDbContext dbContext = serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
    private readonly ApplicationPartManager applicationPartManager = applicationPartManager;
    private readonly IRoleService roleService = roleService;
    private readonly IRepositoryBase<User, Guid> userRepo = unitOfWork.Users;

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

            await AssignPermissionsToSuperAdminRoleAsync(superAdminRole.Id, newPermissions, cancellationToken);
        }

        await EnsureAllPermissionsAssignedToSuperAdminAsync(superAdminRole.Id, cancellationToken);

        await EnsureDefaultSuperAdminUserExistsAsync(superAdminRole.Id, cancellationToken);
    }

    private async Task<Result<RoleDto>> EnsureSuperAdminRoleExistsAsync(CancellationToken cancellationToken)
    {
        var existingSuperAdminResult = await unitOfWork.Roles.GetAsync(
            x => x.Name == "SuperAdmin",
            x => x.Include(x => x.RolePermissions)
            .Include(x => x.UserRoles));

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
            dbPermission.RolePermissions.Add(new RolePermission
            {
                RoleId = getSuperAdminRoleResult.Value.Id,
                PermissionId = dbPermission.Id
            });
            var updateResult = await unitOfWork.Permissions.UpdateAsync(dbPermission);
            if (updateResult.IsError)
                return updateResult.Errors;
        }

        int result = await unitOfWork.SaveChangesAsync();
        return result > 0
            ? Result.Success
            : new Error();
    }

    private async Task EnsureAllPermissionsAssignedToSuperAdminAsync(int superAdminRoleId, CancellationToken cancellationToken)
    {
        var role = await dbContext.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == superAdminRoleId, cancellationToken);

        if (role == null)
        {
            throw new Exception($"SuperAdmin role with ID {superAdminRoleId} not found.");
        }

        var allPermissions = await dbContext.Permissions
            .Select(p => new { p.Id, p.Key })
            .ToListAsync(cancellationToken);

        var assignedPermissionIds = role.Permissions.Select(p => p.Id).ToHashSet();

        var unassignedPermissions = allPermissions
            .Where(p => !assignedPermissionIds.Contains(p.Id))
            .ToList();

        foreach (var permission in unassignedPermissions)
        {
            var assignRequest = new AssignPermissionToRoleRequest(permission.Id, superAdminRoleId);

            await roleService.AssignPermissionToRoleAsync(assignRequest, cancellationToken);
        }
    }

    private async Task EnsureDefaultSuperAdminUserExistsAsync(
      int superAdminRoleId,
      CancellationToken cancellationToken)
    {
        try
        {
            const string defaultHashedPassword =
                "URiiKlWs+xjqpKmMjtQC8SMG0Oc6nIA/XDbct9TB3/k=";

            var user = await dbContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(
                    u => u.Id == Constants.DefaultUserId,
                    cancellationToken);

            if (user == null)
            {
                return;
            }

            // Ensure password consistency
            if (user.HashedPassword != defaultHashedPassword)
            {
                user.HashedPassword = defaultHashedPassword;
                dbContext.Users.Update(user);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // Ensure SuperAdmin role
            var hasSuperAdminRole = user.Roles
                .Any(r => r.Id == superAdminRoleId);

            if (hasSuperAdminRole)
            {
                return;
            }

            var assignRoleRequest =
                new AssignRoleToUserRequest(superAdminRoleId, user.Id);

            var roleAssignmentResult =
                await roleService.AssignRoleToUserAsync(
                    assignRoleRequest,
                    cancellationToken);

            if (roleAssignmentResult.IsError)
            {
                throw new Exception(
                    $"Failed to assign SuperAdmin role to user {user.Id}: " +
                    string.Join(
                        ", ",
                        roleAssignmentResult.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error in EnsureDefaultSuperAdminUserExistsAsync: {ex.Message}",
                ex);
        }
    }

}