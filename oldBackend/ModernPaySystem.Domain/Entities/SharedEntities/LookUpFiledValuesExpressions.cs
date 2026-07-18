using System.Linq.Expressions;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public static class LookUpFiledValuesExpressions
{
    public static Expression<Func<LookUpFiledValues, bool>> ById(Guid id) =>
        l => l.Id == id;

    public static Expression<Func<LookUpFiledValues, bool>> ByLookUpFieldId(Guid lookUpFieldId) =>
        l => l.LookUpFiledId == lookUpFieldId;

    public static List<Expression<Func<LookUpFiledValues, bool>>> ByLookUpFieldIdWithIncludes(Guid lookUpFieldId) =>
    [
        ByLookUpFieldId(lookUpFieldId)
    ];
}
