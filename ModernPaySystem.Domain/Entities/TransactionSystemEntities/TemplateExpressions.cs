using System.Linq;
using System.Linq.Expressions;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class TemplateExpressions
{
    public static Expression<Func<Template, bool>> ById(Guid id) =>
        t => t.Id == id;

    public static Expression<Func<Template, bool>> ByName(string name) =>
        t => t.TemplateName == name;

    public static Expression<Func<Template, bool>> ByNameContains(string name) =>
        t => t.TemplateName.Contains(name);

    public static Expression<Func<Template, bool>> ByCreatedByUserId(string userId) =>
        t => t.CreatedByUserId == userId;

    public static Expression<Func<Template, bool>> CanReadByUserId(string userId) =>
        t => t.CreatedByUserId == userId
             || t.Ownerships.Any(o => o.Department != null && o.Department.Users.Any(x => x.Id.ToString() == userId));

    public static Expression<Func<Template, bool>> CanMakeUpdateByUserId(string userId) =>
        t => t.CreatedByUserId == userId
             || t.Ownerships.Any(o => o.Department != null && o.Department.Users.Any(x => x.Id.ToString() == userId));

    public static List<Expression<Func<Template, bool>>> ByIdWithIncludes(Guid id) =>
    [
        ById(id)
    ];

    public static List<Expression<Func<Template, bool>>> ByNameWithIncludes(string name) =>
    [
        ByName(name)
    ];
}
