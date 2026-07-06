using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Module.Identity.Infrastructure.Persistence;
using ModernPaySystem.SharedKernel.Domain.Entities;

namespace ModernPaySystem.Module.Identity.Infrastructure.Seeding.Seeders;

public sealed class DepartmentSeeder : EntitySeederBase<Department>
{
    public override int Order => 3;

    public override async Task SeedAsync(
        IdentityDbContext context,
        SeedingConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var existingDepartmentNames = await context.Departments
            .Select(department => department.Name)
            .ToListAsync(cancellationToken);

        var departments = new List<Department>
        {
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000101"),
                Name = "Head Office",
                Code = "HQ",
                Description = "Root department",
                Level = 1,
                Type = DepartmentType.Country,
                MaterializedPath = "000000000101",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000102"),
                Name = "Operations",
                Code = "OPS",
                Description = "Operations division",
                ParentDepartmentId = Guid.Parse("00000000-0000-0000-0000-000000000101"),
                Level = 2,
                Type = DepartmentType.Governorate,
                MaterializedPath = "000000000101/000000000102",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000103"),
                Name = "Finance",
                Code = "FIN",
                Description = "Finance division",
                ParentDepartmentId = Guid.Parse("00000000-0000-0000-0000-000000000102"),
                Level = 3,
                Type = DepartmentType.District,
                MaterializedPath = "000000000101/000000000102/000000000103",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000104"),
                Name = "Human Resources",
                Code = "HR",
                Description = "HR division",
                ParentDepartmentId = Guid.Parse("00000000-0000-0000-0000-000000000103"),
                Level = 4,
                Type = DepartmentType.Municipality,
                MaterializedPath = "000000000101/000000000102/000000000103/000000000104",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("00000000-0000-0000-0000-000000000105"),
                Name = "Support",
                Code = "SUP",
                Description = "Support office",
                ParentDepartmentId = Guid.Parse("00000000-0000-0000-0000-000000000104"),
                Level = 5,
                Type = DepartmentType.Office,
                MaterializedPath = "000000000101/000000000102/000000000103/000000000104/000000000105",
                CreatedAt = DateTime.UtcNow
            }
        };

        var newDepartments = departments
            .Where(department => !existingDepartmentNames.Contains(department.Name, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (newDepartments.Count == 0)
        {
            return;
        }

        await context.Departments.AddRangeAsync(newDepartments, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}