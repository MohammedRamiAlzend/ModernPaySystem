using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;

public sealed class DepartmentUserLinkSeeder : EntitySeederBase<User>
{
    public override int Order => 4;

    public override async Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var users = await context.Users
            .Include(user => user.Roles)
            .Include(user => user.SubSystemUser)
            .ToListAsync(cancellationToken);

        var departments = await context.Departments
            .OrderBy(department => department.Level)
            .ToListAsync(cancellationToken);

        if (users.Count == 0 || departments.Count == 0)
        {
            return;
        }

        var userByName = users.ToDictionary(user => user.UserName, StringComparer.OrdinalIgnoreCase);
        var departmentByName = departments.ToDictionary(department => department.Name, StringComparer.OrdinalIgnoreCase);

        var assignments = new (string UserName, string DepartmentName)[]
        {
            ("1", "Head Office"),
            ("manager1", "Operations"),
            ("employee1", "Finance"),
            ("employee2", "Human Resources"),
            ("viewer1", "Support")
        };

        foreach (var (userName, departmentName) in assignments)
        {
            if (!userByName.TryGetValue(userName, out var user) ||
                !departmentByName.TryGetValue(departmentName, out var department))
            {
                continue;
            }

            user.DepartmentId = department.Id;
            department.DepartmentHeadId = user.Id;
            user.IsDepartmentHead = true;
            user.HeadedDepartmentId = department.Id;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}