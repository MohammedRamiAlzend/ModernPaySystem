using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeds the default Templates (e.g. المصالح العقارية).
/// Order: 6 (depends on Departments and Users being created first).
/// </summary>
public class TemplateSeeder : IEntitySeeder
{
    public int Order => 6;

    // Fixed IDs
    private static readonly Guid TemplateId = Guid.Parse("019f2469-66f4-7a2f-8a86-d59503281c67");
    private static readonly Guid CreatorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid ReceiverDepartmentId = Guid.Parse("019f2467-2fd0-7eae-aa59-eb2714f9cee7");

    // Users in citizen service centers
    private static readonly Guid[] CenterUserIds =
    [
        Guid.Parse("33333333-3333-3333-3333-333333333303"), // الكسوة
        Guid.Parse("33333333-3333-3333-3333-333333333304"), // حرستا
        Guid.Parse("33333333-3333-3333-3333-333333333305"), // النبك
        Guid.Parse("33333333-3333-3333-3333-333333333306"), // قطنا
        Guid.Parse("33333333-3333-3333-3333-333333333307"), // يبرود
        Guid.Parse("33333333-3333-3333-3333-333333333308"), // صحنايا
        Guid.Parse("33333333-3333-3333-3333-333333333309"), // جرمانا
    ];

    private const string ContentAsJson = "{\"id\":\"019f2469-66f4-7a2f-8a86-d59503281c67\",\"title\":\"المصالح العقارية\",\"fields\":[{\"id\":\"c383d3e2-aa56-4716-be56-baa20930db5c\",\"name\":\"field_1783022211152\",\"type\":\"text\",\"label\":\"رقم العقار\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}]},{\"id\":\"143522a0-f6d3-437b-9d6d-91990777ea81\",\"name\":\"field_1783022219155\",\"type\":\"text\",\"label\":\"مقدم الطلب\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":3}},{\"id\":\"ab2ee11c-4a48-4e45-b060-b36faebb37b2\",\"name\":\"field_1783022230443\",\"type\":\"text\",\"label\":\"بن\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":3}},{\"id\":\"27dbcc06-983d-4150-a67f-6a161dd691ab\",\"name\":\"field_1783022235424\",\"type\":\"text\",\"label\":\"والدته\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":3}},{\"id\":\"15560b59-a5b6-49ab-ba68-0f0ae7cdf194\",\"name\":\"field_1783022246943\",\"type\":\"text\",\"label\":\"مكان وتاريخ الولادة\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":3}},{\"id\":\"9d2e4972-7dbf-460d-93ab-7234b5190e33\",\"name\":\"field_1783022259766\",\"type\":\"text\",\"label\":\"رقم القيد ومكانه\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":4}},{\"id\":\"f9529fcb-1582-4e99-b670-194a9c5f5c3c\",\"name\":\"field_1783022282289\",\"type\":\"text\",\"label\":\"الرقم الوطني\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":4}},{\"id\":\"1acd12d0-818e-4686-bcdb-67644aad97bb\",\"name\":\"field_1783022290481\",\"type\":\"text\",\"label\":\"الهاتف\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":4}},{\"id\":\"9f91a0cc-30a4-4c34-9250-b76b82e4dd4f\",\"name\":\"field_1783022618825\",\"type\":\"select\",\"label\":\"يرجى\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"dataSource\":{\"type\":\"lookup\",\"lookUpFieldId\":\"019f2469-dee3-7329-86e4-9f886bd59c1a\"}},{\"id\":\"d8e38c65-9323-4120-ad96-201cf6d64319\",\"name\":\"field_1783022383562\",\"type\":\"text\",\"label\":\"المحافظة\",\"validation\":[],\"layout\":{\"colSpan\":4}},{\"id\":\"7cdfc590-89d3-45b9-a376-a24035bb0f79\",\"name\":\"field_1783022382530\",\"type\":\"text\",\"label\":\"من المنطقة العقارية\",\"validation\":[{\"rule\":\"required\",\"value\":\"\",\"message\":\"هذا الحقل مطلوب\"}],\"layout\":{\"colSpan\":4}},{\"id\":\"3821133c-0ceb-4061-9d18-fa2351caa790\",\"name\":\"field_1783022383012\",\"type\":\"text\",\"label\":\"تابعة للمنطقة العقارية\",\"validation\":[],\"layout\":{\"colSpan\":4}},{\"id\":\"31f54b62-4a86-4456-aead-5e6ec8b9fef4\",\"name\":\"field_1783022389337\",\"type\":\"textarea\",\"label\":\"مالك الحصة المطلوبة أو أسماء الملاك\",\"validation\":[]},{\"id\":\"df4e0889-d4a5-4d1c-a765-280f5f42c87f\",\"name\":\"field_1783022390568\",\"type\":\"text\",\"label\":\"رقم ايصال الدفع\",\"validation\":[]},{\"id\":\"ce2252ab-b25a-4218-b86d-7ea80a8bb805\",\"name\":\"field_1783022391470\",\"type\":\"textarea\",\"label\":\"ملاحظات اضافية\",\"validation\":[]}],\"logic\":[],\"isRequireAttachments\":false,\"defaultReceiverDepartmentId\":\"019f2467-2fd0-7eae-aa59-eb2714f9cee7\",\"description\":\"\"}";

    public async Task<bool> HasDataAsync(AppDbContext context)
    {
        return await context.Templates.AnyAsync(t => t.Id == TemplateId);
    }

    public async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        // 1. Create the Template
        var template = new Template
        {
            Id = TemplateId,
            TemplateName = "المصالح العقارية",
            ContentAsJson = ContentAsJson,
            IsRequireAttachments = false,
            DefaultReceiverDepartmentId = ReceiverDepartmentId,
            CreatedByUserId = CreatorUserId.ToString(),
            CreatedAt = DateTime.UtcNow,
        };

        await context.Templates.AddAsync(template);
        await context.SaveChangesAsync();

        // 2. Ownership: user "1" owns the template (TemplateDepartmentOwnership)
        var departmentOwnership = new TemplateDepartmentOwnership
        {
            Id = Guid.NewGuid(),
            TemplateId = TemplateId,
            DepartmentId = ReceiverDepartmentId,
        };
        await context.TemplateOwnerships.AddAsync(departmentOwnership);

        // 3. UserTemplateOwnerships: allow all center users to access the template
        var userOwnerships = new List<UserTemplateOwnership>();

        // include the creator (user "1")
        userOwnerships.Add(new UserTemplateOwnership
        {
            Id = Guid.NewGuid(),
            TemplateId = TemplateId,
            UserId = CreatorUserId,
        });

        // include all citizen service center users
        foreach (var userId in CenterUserIds)
        {
            var userExists = await context.Users.AnyAsync(u => u.Id == userId);
            if (userExists)
            {
                userOwnerships.Add(new UserTemplateOwnership
                {
                    Id = Guid.NewGuid(),
                    TemplateId = TemplateId,
                    UserId = userId,
                });
            }
        }

        await context.UserTemplateOwnerships.AddRangeAsync(userOwnerships);
        await context.SaveChangesAsync();
    }

    public string GetEntityName() => "Templates";
}
