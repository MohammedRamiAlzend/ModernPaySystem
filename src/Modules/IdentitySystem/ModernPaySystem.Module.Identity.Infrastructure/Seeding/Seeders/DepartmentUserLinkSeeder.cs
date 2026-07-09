using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;

public sealed class DepartmentUserLinkSeeder : IEntitySeeder
{
    public int Order => 4;

    public string GetEntityName() => "DepartmentUserLinks";

    public async Task<bool> HasDataAsync(IdentityDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Users.AnyAsync(u => u.DepartmentId.HasValue && !u.IsDepartmentHead, cancellationToken);
    }

    public async Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var departments = await context.Departments
            .OrderBy(d => d.Level)
            .ThenBy(d => d.Id)
            .ToListAsync(cancellationToken);

        if (departments.Count == 0)
            return;

        var users = await context.Users
            .Include(u => u.SubSystemUser)
            .OrderBy(u => u.IsDepartmentHead ? 0 : 1)
            .ThenBy(u => u.UserName)
            .ToListAsync(cancellationToken);

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

        await context.SaveChangesAsync(cancellationToken);
    }
}
