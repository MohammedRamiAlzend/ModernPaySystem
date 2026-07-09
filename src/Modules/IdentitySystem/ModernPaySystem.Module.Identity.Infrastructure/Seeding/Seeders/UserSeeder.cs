using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Application.Interfaces;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;

public sealed class UserSeeder(IPasswordHasher passwordHasher) : EntitySeederBase<User>
{
    private static readonly IReadOnlyList<(Guid Id, string UserName, string Password, string RoleName, SubSystem SubSystem)> SeedUsers =
    [
        (Guid.Parse("11111111-1111-1111-1111-111111111111"), "1", "1", "SuperAdmin", SubSystem.Shared),
        (Guid.Parse("22222222-2222-2222-2222-222222222222"), "محافظة ريف دمشق", "123456", "Admin", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333303"), "مركز خدمة المواطن الكسوة", "123456", "NormalUser", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333304"), "مركز خدمة المواطن حرستا", "123456", "NormalUser", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333305"), "مركز خدمة المواطن النبك", "123456", "NormalUser", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333306"), "مركز خدمة المواطن قطنا", "123456", "NormalUser", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333307"), "مركز خدمة المواطن يبرود", "123456", "NormalUser", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333308"), "مركز خدمة المواطن صحنايا", "123456", "NormalUser", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333309"), "مركز خدمة المواطن جرمانا", "123456", "NormalUser", SubSystem.TransactionSystem),
        (Guid.Parse("33333333-3333-3333-3333-333333333310"), "ماجد", "123456", "Admin", SubSystem.TransactionSystem)
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
        var existingUsersByName = existingUsers.ToDictionary(user => user.UserName, StringComparer.Ordinal);

        var newUsers = new List<User>();

        foreach (var seedUser in SeedUsers)
        {
            if (existingUsersByName.ContainsKey(seedUser.UserName))
                continue;

            if (!rolesByName.TryGetValue(seedUser.RoleName, out var role))
                continue;

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
            return;

        await context.Users.AddRangeAsync(newUsers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
