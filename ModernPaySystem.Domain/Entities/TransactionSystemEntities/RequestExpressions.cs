using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class RequestExpressions
{
    public static Expression<Func<Request, bool>> ByRequesterId(Guid requesterId) =>
        r => r.RequesterId == requesterId;

    public static Expression<Func<Request, bool>> ByApproverId(Guid approverId) =>
        r => r.ApproverId == approverId;

    public static Expression<Func<Request, bool>> ByTemplateId(Guid templateId) =>
        r => r.TemplateId == templateId;

    public static Expression<Func<Request, bool>> HasResponse(bool hasResponse) =>
        r => r.ResponseId.HasValue == hasResponse;

    public static Expression<Func<Request, bool>> ByUserId(Guid userId) =>
        r => r.RequesterId == userId || r.ApproverId == userId || r.ReadOnlyUsers.Any(u => u.Id == userId);

    public static Expression<Func<Request, bool>> IsActive() =>
        r => r.ResponseId == null;

    public static Expression<Func<Request, bool>> IsCompleted() =>
        r => r.ResponseId.HasValue;

    public static List<Expression<Func<Request, bool>>> ByApproverIdAndResponse(Guid approverId, bool hasResponse) =>
    [
        ByApproverId(approverId),
        HasResponse(hasResponse)
    ];

    public static List<Expression<Func<Request, bool>>> ByRequesterIdWithIncludes(Guid requesterId) =>
    [
        ByRequesterId(requesterId)
    ];

    public static List<Expression<Func<Request, bool>>> ByTemplateIdWithIncludes(Guid templateId) =>
    [
        ByTemplateId(templateId)
    ];

    public static Expression<Func<Request, bool>> CanMakeUpdate(Request request, Guid userId) =>
        r => r.Id == request.Id
             && userId == request.RequesterId
             && userId != request.ApproverId
             && (request.ReadOnlyUsers == null || !request.ReadOnlyUsers.Any(u => u.Id == userId));

    public static Expression<Func<Request, bool>> CanRead(Request request, Guid userId) =>
        r => r.Id == request.Id
             && (userId == request.RequesterId
                 || userId == request.ApproverId
                 || (request.ReadOnlyUsers != null && request.ReadOnlyUsers.Any(u => u.Id == userId)));

    public static Expression<Func<Request, bool>> CanReadByUserId(Guid userId) =>
        r => r.RequesterId == userId
             || r.ApproverId == userId
             || r.ReadOnlyUsers.Any(u => u.Id == userId);

    public static Expression<Func<Request, bool>> CanMakeUpdateByUserId(Guid userId) =>
        r => r.RequesterId == userId
             && r.ApproverId != userId
             && (r.ReadOnlyUsers == null || !r.ReadOnlyUsers.Any(u => u.Id == userId));
}
