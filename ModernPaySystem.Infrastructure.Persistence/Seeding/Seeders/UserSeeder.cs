using Bogus;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Application.Interfaces;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeder for User entities
/// Order: 3 (depends on roles being created for relationships).
/// </summary>
public class UserSeeder : EntitySeederBase<User>
{
    private readonly IPasswordHasher _passwordHasher;

    public UserSeeder(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public override int Order => 3;

    public override async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        // Get existing roles
        var roles = await context.Roles.ToListAsync();
        if (!roles.Any())
            throw new InvalidOperationException("Roles must be seeded before users");

        var users = GenerateUsers(configuration.Quantities.UserCount, roles);
        await AddEntitiesAsync(context, users);

        // Assign roles to users
        await AssignRolesToUsers(context, users, roles);

        // Ensure user "1" is SuperAdmin
        await EnsureSuperAdminRole(context, users, roles);

        // Enroll users in subsystems
        await EnrollUsersInSubsystems(context, users);
    }

    /// <summary>
    /// Generate random user data using Bogus.
    /// </summary>
    private List<User> GenerateUsers(int count, List<Role> roles)
    {
        var users = new List<User>();

        // 1. Add special users with fixed GUIDs
        var specialUsers = new List<(Guid Id, string UserName, string Password)>
        {
            (Guid.Parse("11111111-1111-1111-1111-111111111111"), "1", "1"),
            (Guid.Parse("22222222-2222-2222-2222-222222222222"), "محافظة ريف دمشق", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333303"), "مركز خدمة المواطن الكسوة", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333304"), "مركز خدمة المواطن حرستا", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333305"), "مركز خدمة المواطن النبك", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333306"), "مركز خدمة المواطن قطنا", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333307"), "مركز خدمة المواطن يبرود", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333308"), "مركز خدمة المواطن صحنايا", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333309"), "مركز خدمة المواطن جرمانا", "123456"),
            (Guid.Parse("33333333-3333-3333-3333-333333333310"), "ماجد", "123456")
        };

        foreach (var spec in specialUsers)
        {
            users.Add(new User
            {
                Id = spec.Id,
                UserName = spec.UserName,
                HashedPassword = _passwordHasher.HashPassword(spec.Password),
                Roles = new List<Role>()
            });
        }

        return users;
    }

    // Ensure SuperAdmin role is assigned to user "1"
    private async Task EnsureSuperAdminRole(AppDbContext context, List<User> users, List<Role> roles)
    {
        var superAdminRole = roles.FirstOrDefault(r => r.Name == "SuperAdmin");
        if (superAdminRole == null) return;
        var user1 = users.FirstOrDefault(u => u.UserName == "1");
        if (user1 != null && !user1.Roles.Any(r => r.Name == "SuperAdmin"))
        {
            user1.Roles.Add(superAdminRole);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Assign roles to users.
    /// </summary>
    private async Task AssignRolesToUsers(AppDbContext context, List<User> users, List<Role> roles)
    {
        var random = new Random();

        foreach (var user in users)
        {
            // Assign 1-3 random roles to each user
            var assignedRoles = roles
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, Math.Min(4, roles.Count + 1)))
                .ToList();

            foreach (var role in assignedRoles)
            {
                user.Roles.Add(role);
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Enroll users in subsystems
    /// </summary>
    private async Task EnrollUsersInSubsystems(AppDbContext context, List<User> users)
    {
        var subsystemUsers = new List<SubSystemUser>();
        var transactionUserNames = new HashSet<string>
        {
            "1",
            "محافظة ريف دمشق",
            "مركز خدمة المواطن الكسوة",
            "مركز خدمة المواطن حرستا",
            "مركز خدمة المواطن النبك",
            "مركز خدمة المواطن قطنا",
            "مركز خدمة المواطن يبرود",
            "مركز خدمة المواطن صحنايا",
            "مركز خدمة المواطن جرمانا",
            "ماجد"
        };

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            SubSystem subSystem;

            if (transactionUserNames.Contains(user.UserName))
            {
                subSystem = SubSystem.TransactionSystem;
            }
            else
            {
                // Alternate remaining users between systems
                subSystem = (i % 2 == 0) ? SubSystem.TransactionSystem : SubSystem.Diwan;
            }

            var subsystemUser = new SubSystemUser
            {
                Id = Guid.NewGuid(),
                SubSystem = subSystem,
                UserId = user.Id
            };

            subsystemUsers.Add(subsystemUser);
        }

        await context.SubSystemUsers.AddRangeAsync(subsystemUsers);
        await context.SaveChangesAsync();
    }
}
