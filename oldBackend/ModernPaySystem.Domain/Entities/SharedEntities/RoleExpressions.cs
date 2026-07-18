using System.Linq.Expressions;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public static class RoleExpressions
{
    public static Expression<Func<Role, bool>> ById(Guid id) =>
        r => r.Id == id;

    public static Expression<Func<Role, bool>> ByName(string name) =>
        r => r.Name == name;

    public static Expression<Func<Role, bool>> ByNameContains(string name) =>
        r => r.Name.Contains(name);

    public static List<Expression<Func<Role, bool>>> ByIdWithIncludes(Guid id) =>
    [
        ById(id)
    ];

    public static List<Expression<Func<Role, bool>>> ByNameWithIncludes(string name) =>
    [
        ByName(name)
    ];
}
