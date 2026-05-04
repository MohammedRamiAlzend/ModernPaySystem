using System.Linq;
using System.Linq.Expressions;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class TemplateExpressions
{
    public static Expression<Func<Template, bool>> ById(Guid id)
    {
        return t => t.Id == id;
    }

    public static Expression<Func<Template, bool>> ByName(string name)
    {
        return t => t.TemplateName == name;
    }

    public static Expression<Func<Template, bool>> ByNameContains(string name)
    {
        return t => t.TemplateName.Contains(name);
    }

    public static Expression<Func<Template, bool>> ByCreatedByUserId(string userId)
    {
        return t => t.CreatedByUserId == userId;
    }

    public static Expression<Func<Template, bool>> CanReadByUserId(Guid userId)
    {
        return t => t.DepartmentOwnerships == null
             || t.DepartmentOwnerships.Any(o => o.Department != null && o.Department.Users.Any(x => x.Id == userId))
             || (t.UserOwnerships != null && t.UserOwnerships.Any(u => u.UserId == userId));
    }

    public static Expression<Func<Template, bool>> CanMakeUpdateByUserId(Guid userId)
    {
        return t => t.DepartmentOwnerships == null
             || t.DepartmentOwnerships.Any(o => o.Department != null && o.Department.Users.Any(x => x.Id == userId))
             || (t.UserOwnerships != null && t.UserOwnerships.Any(u => u.UserId == userId));
    }

    public static List<Expression<Func<Template, bool>>> ByIdWithIncludes(Guid id)
    {
        return [
        ById(id)
    ];
    }

    public static List<Expression<Func<Template, bool>>> ByNameWithIncludes(string name)
    {
        return [
        ByName(name)
    ];
    }
}
