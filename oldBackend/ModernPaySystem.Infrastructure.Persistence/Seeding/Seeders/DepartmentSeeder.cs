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

        // المستوى 3: البلديات / المناطق (Municipality)
        var kiswah = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "الكسوة",
            Code = "KIS",
            Description = "بلدية الكسوة",
            Level = 3,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000003",
            CreatedAt = DateTime.UtcNow
        };

        var harasta = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "حرستا",
            Code = "HAR",
            Description = "بلدية حرستا",
            Level = 3,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000004",
            CreatedAt = DateTime.UtcNow
        };

        var nabek = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Name = "النبك",
            Code = "NAB",
            Description = "بلدية النبك",
            Level = 3,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000005",
            CreatedAt = DateTime.UtcNow
        };

        var qatana = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
            Name = "قطنا",
            Code = "QAT",
            Description = "بلدية قطنا",
            Level = 3,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000006",
            CreatedAt = DateTime.UtcNow
        };

        var yabroud = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
            Name = "يبرود",
            Code = "YAB",
            Description = "بلدية يبرود",
            Level = 3,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000007",
            CreatedAt = DateTime.UtcNow
        };

        var sahnaya = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
            Name = "صحنايا",
            Code = "SAH",
            Description = "بلدية صحنايا",
            Level = 3,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000008",
            CreatedAt = DateTime.UtcNow
        };

        var jaramana = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000009"),
            Name = "جرمانا",
            Code = "JAR",
            Description = "بلدية جرمانا",
            Level = 3,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000009",
            CreatedAt = DateTime.UtcNow
        };

        // المستوى 4: مراكز خدمة المواطن (Office)
        var kiswahCenter = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000103"),
            Name = "مركز خدمة المواطن الكسوة",
            Code = "KIS_CSC",
            Description = "مركز خدمة المواطن الكسوة",
            Level = 4,
            Type = DepartmentType.Office,
            ParentDepartmentId = kiswah.Id,
            MaterializedPath = "00000001/00000002/00000003/00000103",
            CreatedAt = DateTime.UtcNow
        };

        var harastaCenter = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000104"),
            Name = "مركز خدمة المواطن حرستا",
            Code = "HAR_CSC",
            Description = "مركز خدمة المواطن حرستا",
            Level = 4,
            Type = DepartmentType.Office,
            ParentDepartmentId = harasta.Id,
            MaterializedPath = "00000001/00000002/00000004/00000104",
            CreatedAt = DateTime.UtcNow
        };

        var nabekCenter = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000105"),
            Name = "مركز خدمة المواطن النبك",
            Code = "NAB_CSC",
            Description = "مركز خدمة المواطن النبك",
            Level = 4,
            Type = DepartmentType.Office,
            ParentDepartmentId = nabek.Id,
            MaterializedPath = "00000001/00000002/00000005/00000105",
            CreatedAt = DateTime.UtcNow
        };

        var qatanaCenter = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000106"),
            Name = "مركز خدمة المواطن قطنا",
            Code = "QAT_CSC",
            Description = "مركز خدمة المواطن قطنا",
            Level = 4,
            Type = DepartmentType.Office,
            ParentDepartmentId = qatana.Id,
            MaterializedPath = "00000001/00000002/00000006/00000106",
            CreatedAt = DateTime.UtcNow
        };

        var yabroudCenter = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000107"),
            Name = "مركز خدمة المواطن يبرود",
            Code = "YAB_CSC",
            Description = "مركز خدمة المواطن يبرود",
            Level = 4,
            Type = DepartmentType.Office,
            ParentDepartmentId = yabroud.Id,
            MaterializedPath = "00000001/00000002/00000007/00000107",
            CreatedAt = DateTime.UtcNow
        };

        var sahnayaCenter = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000108"),
            Name = "مركز خدمة المواطن صحنايا",
            Code = "SAH_CSC",
            Description = "مركز خدمة المواطن صحنايا",
            Level = 4,
            Type = DepartmentType.Office,
            ParentDepartmentId = sahnaya.Id,
            MaterializedPath = "00000001/00000002/00000008/00000108",
            CreatedAt = DateTime.UtcNow
        };

        var jaramanaCenter = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000109"),
            Name = "مركز خدمة المواطن جرمانا",
            Code = "JAR_CSC",
            Description = "مركز خدمة المواطن جرمانا",
            Level = 4,
            Type = DepartmentType.Office,
            ParentDepartmentId = jaramana.Id,
            MaterializedPath = "00000001/00000002/00000009/00000109",
            CreatedAt = DateTime.UtcNow
        };

        // قسم المصالح العقارية
        var realEstate = new Department
        {
            Id = Guid.Parse("019f2467-2fd0-7eae-aa59-eb2714f9cee7"),
            Name = "المصالح العقارية",
            Code = "RE",
            Description = "مديرية المصالح العقارية",
            Level = 3,
            Type = DepartmentType.Office,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/019f2467",
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
            jaramana,
            realEstate,
            kiswahCenter,
            harastaCenter,
            nabekCenter,
            qatanaCenter,
            yabroudCenter,
            sahnayaCenter,
            jaramanaCenter
        );

        var departments = new[] { 
            syria, 
            rifDimashq, 
            kiswah, 
            harasta, 
            nabek, 
            qatana, 
            yabroud, 
            sahnaya, 
            jaramana,
            realEstate,
            kiswahCenter,
            harastaCenter,
            nabekCenter,
            qatanaCenter,
            yabroudCenter,
            sahnayaCenter,
            jaramanaCenter
        };

        var headAssignments = new Dictionary<Guid, Guid>
        {
            { syria.Id, Guid.Parse("11111111-1111-1111-1111-111111111111") },
            { rifDimashq.Id, Guid.Parse("22222222-2222-2222-2222-222222222222") },
            { kiswahCenter.Id, Guid.Parse("33333333-3333-3333-3333-333333333303") },
            { harastaCenter.Id, Guid.Parse("33333333-3333-3333-3333-333333333304") },
            { nabekCenter.Id, Guid.Parse("33333333-3333-3333-3333-333333333305") },
            { qatanaCenter.Id, Guid.Parse("33333333-3333-3333-3333-333333333306") },
            { yabroudCenter.Id, Guid.Parse("33333333-3333-3333-3333-333333333307") },
            { sahnayaCenter.Id, Guid.Parse("33333333-3333-3333-3333-333333333308") },
            { jaramanaCenter.Id, Guid.Parse("33333333-3333-3333-3333-333333333309") }
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

        // إلحاق المستخدم "ماجد" بقسم المصالح العقارية
        var majedUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "ماجد");
        if (majedUser != null)
        {
            majedUser.DepartmentId = realEstate.Id;
        }

        await context.SaveChangesAsync();
    }
}
