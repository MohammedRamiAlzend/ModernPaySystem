using System.Linq;
using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;
using ExpressionBuilderLib.src.Core.Enums;
namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class RequestExpressions
{
    public static Expression<Func<Request, bool>> ByRequesterId(Guid requesterId) =>
        r => r.RequesterId == requesterId;

    public static Expression<Func<Request, bool>> ByApproverId(Guid approverId) =>
        r => r.ApproverId == approverId;

    public static Expression<Func<Request, bool>> ByTemplateId(Guid templateId) =>
        r => r.TemplateId == templateId;

    public static Expression<Func<Request, bool>> HasResponse() =>
        r => r.ResponseId.HasValue;
    public static Expression<Func<Request, bool>> HasTransaction() =>
       r => r.CurrentTransactionId.HasValue || r.FirstTransactionId.HasValue;

    public static Expression<Func<Request, bool>> ByUserId(Guid userId) =>
        r => r.RequesterId == userId || r.ApproverId == userId || r.ReadOnlyUsers.Any(u => u.Id == userId);

    public static Expression<Func<Request, bool>> IsActive() =>
        r => r.ResponseId == null;

    public static Expression<Func<Request, bool>> IsCompleted() =>
        r => r.ResponseId.HasValue;

    public static List<Expression<Func<Request, bool>>> ByApproverIdAndResponse(Guid approverId)
    {
        var combinedExpression1 = ExpressionCombiner.Combine(ByApproverId(approverId), HasResponse(), LogicalOperator.And);
        var combinedExpression2 = ExpressionCombiner.Combine(combinedExpression1, HasTransaction(), LogicalOperator.Or);

        return [
            combinedExpression2
        ];
    }

    // Plan (pseudocode):
    // 1. Create an expression `noResponse` that is true when a request has no response: r => !r.ResponseId.HasValue
    // 2. Create an expression `noTransaction` that is true when a request has no transaction:
    //    r => !(r.CurrentTransactionId.HasValue || r.FirstTransactionId.HasValue)
    // 3. Combine `noResponse` and `noTransaction` with LogicalOperator.Or to represent "needs action if missing response OR missing transaction".
    // 4. Combine the result with `ByApproverId(approverId)` using LogicalOperator.And so approverId must match.
    // 5. Return a list containing the single combined expression.
    public static List<Expression<Func<Request, bool>>> RequestsNeedAction(Guid approverId)
    {
        var noResponse = (Expression<Func<Request, bool>>)(r => !r.ResponseId.HasValue);
        var noTransaction = (Expression<Func<Request, bool>>)(r => !(r.CurrentTransactionId.HasValue || r.FirstTransactionId.HasValue));

        var needsActionInner = ExpressionCombiner.Combine(noResponse, noTransaction, LogicalOperator.Or);
        var finalExpression = ExpressionCombiner.Combine(ByApproverId(approverId), needsActionInner, LogicalOperator.And);

        return new List<Expression<Func<Request, bool>>> { finalExpression };
    }

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
