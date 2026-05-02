using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeds the initial department hierarchy structure
/// </summary>
public class DepartmentSeeder : EntitySeederBase<Department>
{
    public override int Order => 5; // Seed after roles and permissions, before users

    public override async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        if (await HasDataAsync(context))
            return;

        // المستوى 1: سوريا (Country)
        var syria = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "الجمهورية العربية السورية",
            Code = "SYR",
            Description = "الدولة الأم",
            Level = 1,
            Type = DepartmentType.Country,
            MaterializedPath = "00000001",
            CreatedAt = DateTime.UtcNow
        };

        // المستوى 2: ريف دمشق (Governorate)
        var rifDimashq = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "محافظة ريف دمشق",
            Code = "RD",
            Description = "محافظة ريف دمشق",
            Level = 2,
            Type = DepartmentType.Governorate,
            ParentDepartmentId = syria.Id,
            MaterializedPath = "00000001/00000002",
            CreatedAt = DateTime.UtcNow
        };

        // المستوى 3: الغوطة الشرقية (District)
        var ghouta = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "منطقة الغوطة الشرقية",
            Code = "GHO",
            Description = "منطقة الغوطة الشرقية",
            Level = 3,
            Type = DepartmentType.District,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000003",
            CreatedAt = DateTime.UtcNow
        };

        // المستوى 4: بلدية دوما (Municipality)
        var doumaMunicipality = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "بلدية دوما",
            Code = "DOU",
            Description = "بلدية دوما",
            Level = 4,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = ghouta.Id,
            MaterializedPath = "00000001/00000002/00000003/00000004",
            CreatedAt = DateTime.UtcNow
        };

        // المستوى 5: مكتب فني ديوان (Office)
        var technicalOffice = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Name = "مكتب فني ديوان",
            Code = "TECH-DIWAN",
            Description = "المكتب الفني في الديوان",
            Level = 5,
            Type = DepartmentType.Office,
            ParentDepartmentId = doumaMunicipality.Id,
            MaterializedPath = "00000001/00000002/00000003/00000004/00000005",
            CreatedAt = DateTime.UtcNow
        };

        await context.Departments.AddRangeAsync(
            syria,
            rifDimashq,
            ghouta,
            doumaMunicipality,
            technicalOffice
        );
        
        await context.SaveChangesAsync();
    }
}
