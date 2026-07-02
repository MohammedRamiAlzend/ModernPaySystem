using ModernPaySystem.Domain.Entities.SharedEntities;
using Microsoft.EntityFrameworkCore;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeds the initial department hierarchy structure
/// </summary>
public class DepartmentSeeder : EntitySeederBase<Department>
{
    public override int Order => 4; // Seed after users so department heads can be assigned

    public override async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        if (await HasDataAsync(context))
            return;

        // المستوى 1: الجمهورية العربية السورية (Country)
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

        // المستوى 2: محافظة ريف دمشق (Governorate)
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

        // المستوى 3: المراكز الخدمية (الأبناء)
        var kiswah = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "كسوة",
            Code = "KIS",
            Description = "مركز خدمة المواطن كسوة",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000003",
            CreatedAt = DateTime.UtcNow
        };

        var harasta = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "حرستا",
            Code = "HAR",
            Description = "مركز خدمة المواطن حرستا",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000004",
            CreatedAt = DateTime.UtcNow
        };

        var nabek = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Name = "النبك",
            Code = "NAB",
            Description = "مركز خدمة المواطن النبك",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000005",
            CreatedAt = DateTime.UtcNow
        };

        var qatana = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
            Name = "قطنا",
            Code = "QAT",
            Description = "مركز خدمة المواطن قطنا",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000006",
            CreatedAt = DateTime.UtcNow
        };

        var yabroud = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
            Name = "يبرود",
            Code = "YAB",
            Description = "مركز خدمة المواطن يبرود",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000007",
            CreatedAt = DateTime.UtcNow
        };

        var sahnaya = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
            Name = "صحنايا",
            Code = "SAH",
            Description = "مركز خدمة المواطن صحنايا",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000008",
            CreatedAt = DateTime.UtcNow
        };

        var jaramana = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000009"),
            Name = "جرمانا",
            Code = "JAR",
            Description = "مركز خدمة المواطن جرمانا",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000009",
            CreatedAt = DateTime.UtcNow
        };

        await context.Departments.AddRangeAsync(
            syria,
            rifDimashq,
            kiswah,
            harasta,
            nabek,
            qatana,
            yabroud,
            sahnaya,
            jaramana
        );

        var departments = new[] { syria, rifDimashq, kiswah, harasta, nabek, qatana, yabroud, sahnaya, jaramana };
        var headAssignments = new Dictionary<Guid, Guid>
        {
            { syria.Id, Guid.Parse("11111111-1111-1111-1111-111111111111") },
            { rifDimashq.Id, Guid.Parse("22222222-2222-2222-2222-222222222222") },
            { kiswah.Id, Guid.Parse("33333333-3333-3333-3333-333333333303") },
            { harasta.Id, Guid.Parse("33333333-3333-3333-3333-333333333304") },
            { nabek.Id, Guid.Parse("33333333-3333-3333-3333-333333333305") },
            { qatana.Id, Guid.Parse("33333333-3333-3333-3333-333333333306") },
            { yabroud.Id, Guid.Parse("33333333-3333-3333-3333-333333333307") },
            { sahnaya.Id, Guid.Parse("33333333-3333-3333-3333-333333333308") },
            { jaramana.Id, Guid.Parse("33333333-3333-3333-3333-333333333309") }
        };

        foreach (var dept in departments)
        {
            if (headAssignments.TryGetValue(dept.Id, out var headUserId))
            {
                var user = await context.Users.FindAsync(headUserId);
                if (user != null)
                {
                    dept.DepartmentHeadId = user.Id;
                    user.IsDepartmentHead = true;
                    user.HeadedDepartmentId = dept.Id;
                    user.DepartmentId = dept.Id;
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
