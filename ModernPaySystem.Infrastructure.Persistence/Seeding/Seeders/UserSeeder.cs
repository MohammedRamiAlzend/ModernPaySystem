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
        int transactionUserCount = count / 2;
        int diwanUserCount = count - transactionUserCount;

        var names = new List<string>
        {
            "أحمد", "محمد", "فاطمة", "علي", "سارة", "خالد", "ليلى", "عمر", "رامي", "يوسف",
            "نور", "مصطفى", "أمل", "حسن", "منى", "زين", "ريم", "طارق", "سلمى", "هاني",
            "عبير", "ايهم", "دينا", "شريف", "ندى", "عمرو", "رنا", "وائل", "كريم", "هند",
            "يسرا", "حسين"
        };

        int nameIndex = 0;

        string GetNextName()
        {
            if (nameIndex < names.Count)
            {
                return names[nameIndex++];
            }
            return $"مستخدم {++nameIndex}";
        }

        // TransactionSystem users: username/password = "123" (except first one which is "1"/"1")
        for (int i = 1; i <= transactionUserCount; i++)
        {
            if (i == 1)
            {
                users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    UserName = "1",
                    HashedPassword = _passwordHasher.HashPassword("1"),
                    Roles = new List<Role>()
                });
            }
            else
            {
                users.Add(new User
                {
                    Id = Guid.NewGuid(),
                    UserName = GetNextName(),
                    HashedPassword = _passwordHasher.HashPassword("123"),
                    Roles = new List<Role>()
                });
            }
        }

        // Diwan users: username/password = "123"
        for (int i = 1; i <= diwanUserCount; i++)
        {
            users.Add(new User
            {
                Id = Guid.NewGuid(),
                UserName = GetNextName(),
                HashedPassword = _passwordHasher.HashPassword("123"),
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
    /// Since there's a one-to-one relationship between User and SubSystemUser,
    /// each user can only be enrolled in one subsystem.
    /// </summary>
    private async Task EnrollUsersInSubsystems(AppDbContext context, List<User> users)
    {
        var subsystemUsers = new List<SubSystemUser>();
        int transactionUserCount = users.Count / 2;

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            SubSystem subSystem;
            if (i < transactionUserCount)
                subSystem = SubSystem.TransactionSystem;
            else
                subSystem = SubSystem.Diwan;

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
