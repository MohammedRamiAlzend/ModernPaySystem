using Bogus;
using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeder for User entities
/// Order: 3 (depends on roles being created for relationships).
/// </summary>
public class UserSeeder : EntitySeederBase<User>
{
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

        // Enroll users in subsystems
        await EnrollUsersInSubsystems(context, users);
    }

    /// <summary>
    /// Generate random user data using Bogus.
    /// </summary>
    private List<User> GenerateUsers(int count, List<Role> roles)
    {
        var faker = new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.UserName, f => f.Internet.UserName())
            .RuleFor(u => u.HashedPassword, f => HashPassword("123456"));

        return faker.Generate(count);
    }

    /// <summary>
    /// Assign roles to users.
    /// </summary>
    private async Task AssignRolesToUsers(AppDbContext context, List<User> users, List<Role> roles)
    {
        var random = new Random();
        var userRoles = new List<UserRole>();

        foreach (var user in users)
        {
            // Assign 1-3 random roles to each user
            var assignedRoles = roles
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, Math.Min(4, roles.Count + 1)))
                .ToList();

            foreach (var role in assignedRoles)
            {
                userRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }
        }

        await context.UserRoles.AddRangeAsync(userRoles);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Simple password hashing (use same as in AuthenticationService).
    /// </summary>
    private string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Enroll users in subsystems
    /// Since there's a one-to-one relationship between User and SubSystemUser,
    /// each user can only be enrolled in one subsystem.
    /// </summary>
    private async Task EnrollUsersInSubsystems(AppDbContext context, List<User> users)
    {
        var random = new Random();
        var subsystemUsers = new List<SubSystemUser>();

        foreach (var user in users)
        {
            // For each user, assign them to one subsystem
            // In this example, we'll randomly assign each user to one of the available subsystems
            // You could modify this logic to assign users to specific subsystems based on your business rules

            var availableSubsystems = Enum.GetValues(typeof(SubSystem)).Cast<SubSystem>().ToList();

            // Randomly pick one subsystem for this user (since it's a one-to-one relationship)
            var selectedSubsystem = availableSubsystems[random.Next(availableSubsystems.Count)];

            var subsystemUser = new SubSystemUser
            {
                Id = Guid.NewGuid(),
                SubSystem = selectedSubsystem,
                UserId = user.Id
            };

            subsystemUsers.Add(subsystemUser);
        }

        await context.SubSystemUsers.AddRangeAsync(subsystemUsers);
        await context.SaveChangesAsync();
    }
}
