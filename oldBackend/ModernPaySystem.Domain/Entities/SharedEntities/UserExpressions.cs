using ModernPaySystem.Domain.Entities.SharedEntities;
using System.Linq;
using System.Linq.Expressions;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public static class UserExpressions
{
    public static Expression<Func<User, bool>> ById(Guid id) =>
        u => u.Id == id;

    public static Expression<Func<User, bool>> ByUsername(string username) =>
        u => u.UserName == username;

    public static Expression<Func<User, bool>> BySubSystemUserId(Guid subSystemUserId) =>
        u => u.SubSystemUserId == subSystemUserId;

    public static Expression<Func<User, bool>> ByUserIds(IEnumerable<Guid> userIds) =>
        u => userIds.Contains(u.Id);

    public static Expression<Func<User, bool>> CanReadByUserId(Guid userId) =>
        u => u.Id == userId
             || u.Roles.Any(r => r.Users.Any(u2 => u2.Id == userId));

    public static List<Expression<Func<User, bool>>> ByIdWithIncludes(Guid id) =>
    [
        ById(id)
    ];

    public static List<Expression<Func<User, bool>>> ByUsernameWithIncludes(string username) =>
    [
        ByUsername(username)
    ];

    public static List<Expression<Func<User, bool>>> BySubSystemWithIncludes(SubSystem subSystem) =>
    [
        u => u.SubSystemUser != null && u.SubSystemUser.SubSystem == subSystem
    ];
}
