using Microsoft.EntityFrameworkCore;
using ModernPaySystem.Domain.Entities.SharedEntities;

namespace ModernPaySystem.Infrastructure.Persistence.Seeding.Seeders;

/// <summary>
/// Seeds LookUpFields and their values for the المصالح العقارية template.
/// Order: 7 (depends on Templates being created first).
/// </summary>
public class LookUpFieldsSeeder : IEntitySeeder
{
    public int Order => 7;

    // The ID referenced by the "خدمات المصالح العقارية" select field in the template JSON
    private static readonly Guid LookupFieldId = Guid.Parse("019f2469-dee3-7329-86e4-9f886bd59c1a");

    public async Task<bool> HasDataAsync(AppDbContext context)
    {
        return await context.LookUpFields.AnyAsync(f => f.Id == LookupFieldId);
    }

    public async Task SeedAsync(AppDbContext context, SeedingConfiguration configuration)
    {
        var lookupField = new LookUpField
        {
            Id = LookupFieldId,
            FiledName = "خدمات المصالح العقارية",
        };

        var values = new List<LookUpFiledValues>
        {
            new() { Id = Guid.NewGuid(), LookUpFiledId = LookupFieldId, Desc = "اخراج قيد عقاري" },
        };

        lookupField.LookUpFiledValues = values;

        await context.LookUpFields.AddAsync(lookupField);
        await context.SaveChangesAsync();
    }

    public string GetEntityName() => "LookUpFields";
}
