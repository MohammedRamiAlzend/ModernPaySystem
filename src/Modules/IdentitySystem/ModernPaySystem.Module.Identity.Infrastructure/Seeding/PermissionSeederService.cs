using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernPaySystem.Module.Identity.Application;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.SharedKernel.Domain.Attrs;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding;

public sealed class PermissionSeederService(
    IIdentityUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ILogger<PermissionSeederService> logger) : IPermissionSeederService
{
    public async Task SeedPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var discoveredPermissions = DiscoverPermissions()
            .GroupBy(permission => permission.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        var existingPermissionsResult = await unitOfWork.Permissions.GetAllAsync(bypassAuth: true);
        if (existingPermissionsResult.IsError)
        {
            throw new InvalidOperationException($"Failed to load existing permissions: {string.Join(", ", existingPermissionsResult.Errors.Select(error => error.Description))}");
        }

        var existingPermissions = existingPermissionsResult.Value ?? [];
        var existingPermissionKeys = existingPermissions
            .Select(permission => permission.Key)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newPermissions = discoveredPermissions
            .Where(permission => !existingPermissionKeys.Contains(permission.Key))
            .ToList();

        var superAdminRole = await EnsureSuperAdminRoleExistsAsync(cancellationToken);

        foreach (var permission in newPermissions)
        {
            permission.Roles = [superAdminRole];
            var addResult = await unitOfWork.Permissions.AddAsync(permission, bypassAuth: true);
            if (addResult.IsError)
            {
                throw new InvalidOperationException($"Failed to create permission '{permission.Key}': {string.Join(", ", addResult.Errors.Select(error => error.Description))}");
            }
        }

        if (newPermissions.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await EnsureSuperAdminOwnsAllPermissionsAsync(superAdminRole, cancellationToken);
        await EnsureDefaultSuperAdminUserAsync(superAdminRole, cancellationToken);

        logger.LogInformation("Seeded {PermissionCount} permissions", discoveredPermissions.Count);
    }

    private IEnumerable<PermissionEntity> DiscoverPermissions()
    {
        var assemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic);

        foreach (var type in assemblies.SelectMany(GetLoadableTypes))
        {
            if (!type.IsClass || type.IsAbstract || !type.Name.EndsWith("Controller", StringComparison.Ordinal))
            {
                continue;
            }

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<EndpointPermissionAttribute>(inherit: true);
                if (attribute is null)
                {
                    continue;
                }

                yield return new PermissionEntity
                {
                    Id = Guid.NewGuid(),
                    Key = attribute.Key,
                    Name = attribute.Name ?? attribute.Key,
                    Description = attribute.Description,
                    Type = attribute.Type,
                    SubSystem = attribute.SubSystem
                };
            }
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null).Cast<Type>();
        }
    }

    private async Task<Role> EnsureSuperAdminRoleExistsAsync(CancellationToken cancellationToken)
    {
        var existingRoleResult = await unitOfWork.Roles.GetAsync(
            role => role.Name == "SuperAdmin",
            query => query.Include(role => role.Permissions).Include(role => role.Users),
            bypassAuth: true);

        if (existingRoleResult.IsError)
        {
            throw new InvalidOperationException($"Failed to load SuperAdmin role: {string.Join(", ", existingRoleResult.Errors.Select(error => error.Description))}");
        }

        if (existingRoleResult.Value is not null)
        {
            return existingRoleResult.Value;
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "SuperAdmin",
            Description = "Role with full access"
        };

        var addResult = await unitOfWork.Roles.AddAsync(role, bypassAuth: true);
        if (addResult.IsError)
        {
            throw new InvalidOperationException($"Failed to create SuperAdmin role: {string.Join(", ", addResult.Errors.Select(error => error.Description))}");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return role;
    }

    private async Task EnsureSuperAdminOwnsAllPermissionsAsync(Role superAdminRole, CancellationToken cancellationToken)
    {
        var allPermissionsResult = await unitOfWork.Permissions.GetAllAsync(bypassAuth: true);
        if (allPermissionsResult.IsError)
        {
            throw new InvalidOperationException($"Failed to load permissions: {string.Join(", ", allPermissionsResult.Errors.Select(error => error.Description))}");
        }

        var allPermissions = allPermissionsResult.Value ?? [];
        var assignedPermissionIds = superAdminRole.Permissions.Select(permission => permission.Id).ToHashSet();
        var missingPermissions = allPermissions.Where(permission => !assignedPermissionIds.Contains(permission.Id)).ToList();

        if (missingPermissions.Count == 0)
        {
            return;
        }

        foreach (var permission in missingPermissions)
        {
            permission.Roles.Add(superAdminRole);
            var updateResult = await unitOfWork.Permissions.UpdateAsync(permission, bypassAuth: true);
            if (updateResult.IsError)
            {
                throw new InvalidOperationException($"Failed to assign permission '{permission.Key}' to SuperAdmin: {string.Join(", ", updateResult.Errors.Select(error => error.Description))}");
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultSuperAdminUserAsync(Role superAdminRole, CancellationToken cancellationToken)
    {
        const string defaultPassword = "1";
        var hashedPassword = passwordHasher.HashPassword(defaultPassword);

        var userResult = await unitOfWork.Users.GetAsync(
            user => user.UserName == "1",
            query => query.Include(user => user.Roles).Include(user => user.SubSystemUser),
            bypassAuth: true);

        if (userResult.IsError)
        {
            throw new InvalidOperationException($"Failed to load the default SuperAdmin user: {string.Join(", ", userResult.Errors.Select(error => error.Description))}");
        }

        var user = userResult.Value;
        if (user is null)
        {
            var newUser = new User
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserName = "1",
                HashedPassword = hashedPassword,
                Roles = [superAdminRole],
                SubSystemUser = new SubSystemUser
                {
                    Id = Guid.NewGuid(),
                    SubSystem = SubSystem.Shared
                }
            };

            var addResult = await unitOfWork.Users.AddAsync(newUser, bypassAuth: true);
            if (addResult.IsError)
            {
                throw new InvalidOperationException($"Failed to create the default SuperAdmin user: {string.Join(", ", addResult.Errors.Select(error => error.Description))}");
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        var passwordChanged = false;
        if (!passwordHasher.VerifyPassword(defaultPassword, user.HashedPassword))
        {
            user.HashedPassword = hashedPassword;
            passwordChanged = true;
        }

        if (!user.Roles.Any(role => role.Id == superAdminRole.Id))
        {
            user.Roles.Add(superAdminRole);
            passwordChanged = true;
        }

        if (!passwordChanged)
        {
            return;
        }

        var updateResult = await unitOfWork.Users.UpdateAsync(user, bypassAuth: true);
        if (updateResult.IsError)
        {
            throw new InvalidOperationException($"Failed to update the default SuperAdmin user: {string.Join(", ", updateResult.Errors.Select(error => error.Description))}");
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}