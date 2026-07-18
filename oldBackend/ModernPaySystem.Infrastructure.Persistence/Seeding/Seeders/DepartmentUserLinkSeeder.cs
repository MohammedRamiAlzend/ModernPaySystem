using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Links departments with users after both seeders have run.
/// Ensures every department has at least one normal user in addition to its head.
/// </summary>
public sealed class DepartmentUserLinkSeeder : IEntitySeeder
{
    public int Order => 5;

    public async Task<bool> HasDataAsync(AppDbContext context)
    {
        return await context.Users.AnyAsync(u => u.DepartmentId.HasValue && !u.IsDepartmentHead);
    }

    public async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        var departments = await context.Departments
            .OrderBy(d => d.Level)
            .ThenBy(d => d.Id)
            .ToListAsync();

        if (!departments.Any())
            return;

        var users = await context.Users
            .Include(u => u.SubSystemUser)
            .OrderBy(u => u.IsDepartmentHead ? 0 : 1)
            .ThenBy(u => u.UserName)
            .ToListAsync();

        var departmentsNeedingNormalUsers = departments
            .Where(d => !users.Any(u => u.DepartmentId == d.Id && !u.IsDepartmentHead))
            .ToList();

        var availableNormalUsers = users
            .Where(u => !u.IsDepartmentHead && !u.DepartmentId.HasValue)
            .OrderBy(u => u.SubSystemUser?.SubSystem == SubSystem.Diwan ? 0 : 1)
            .ThenBy(u => int.TryParse(u.UserName, out var numericUserName) ? numericUserName : int.MaxValue)
            .ThenBy(u => u.UserName)
            .ToList();

        var assignmentIndex = 0;
        foreach (var user in availableNormalUsers)
        {
            if (assignmentIndex < departmentsNeedingNormalUsers.Count)
            {
                user.DepartmentId = departmentsNeedingNormalUsers[assignmentIndex++].Id;
            }
            else
            {
                user.DepartmentId = departments[0].Id;
            }
        }

        await context.SaveChangesAsync();
    }

    public string GetEntityName() => "DepartmentUserLinks";
}
