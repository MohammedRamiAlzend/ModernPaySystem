using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;

public sealed class UserSeeder(IPasswordHasher passwordHasher) : EntitySeederBase<User>
{
    private static readonly IReadOnlyList<(Guid Id, string UserName, string Password, string RoleName, SubSystem SubSystem)> SeedUsers =
    [
        (Guid.Parse("00000000-0000-0000-0000-000000000001"), "1", "1", "SuperAdmin", SubSystem.Shared),
        (Guid.Parse("00000000-0000-0000-0000-000000000002"), "manager1", "123", "Manager", SubSystem.TransactionSystem),
        (Guid.Parse("00000000-0000-0000-0000-000000000003"), "employee1", "123", "Employee", SubSystem.TransactionSystem),
        (Guid.Parse("00000000-0000-0000-0000-000000000004"), "employee2", "123", "Employee", SubSystem.TransactionSystem),
        (Guid.Parse("00000000-0000-0000-0000-000000000005"), "viewer1", "123", "Viewer", SubSystem.TransactionSystem)
    ];

    public override int Order => 2;

    public override async Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var existingUsers = await context.Users
            .Include(user => user.Roles)
            .Include(user => user.SubSystemUser)
            .ToListAsync(cancellationToken);

        var rolesByName = await context.Roles.ToDictionaryAsync(role => role.Name, cancellationToken);
        var existingUsersByName = existingUsers.ToDictionary(user => user.UserName, StringComparer.OrdinalIgnoreCase);

        var newUsers = new List<User>();

        foreach (var seedUser in SeedUsers)
        {
            if (existingUsersByName.ContainsKey(seedUser.UserName))
            {
                continue;
            }

            if (!rolesByName.TryGetValue(seedUser.RoleName, out var role))
            {
                continue;
            }

            newUsers.Add(new User
            {
                Id = seedUser.Id,
                UserName = seedUser.UserName,
                HashedPassword = passwordHasher.HashPassword(seedUser.Password),
                Roles = [role],
                SubSystemUser = new SubSystemUser
                {
                    Id = Guid.NewGuid(),
                    SubSystem = seedUser.SubSystem
                }
            });
        }

        if (newUsers.Count == 0)
        {
            return;
        }

        await context.Users.AddRangeAsync(newUsers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}