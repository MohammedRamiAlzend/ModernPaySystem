using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class RequestTransactionExpressions
{
    public static Expression<Func<RequestTransaction, bool>> ByTransactionId(Guid transactionId) =>
        rt => rt.Id == transactionId;

    public static Expression<Func<RequestTransaction, bool>> ByRequestId(Guid requestId) =>
        rt => rt.RequestId == requestId;

    public static Expression<Func<RequestTransaction, bool>> ByParentTransactionId(Guid? parentTransactionId) =>
        rt => rt.ParentTransactionId == parentTransactionId;

    public static Expression<Func<RequestTransaction, bool>> ByCurrentUserHolderId(Guid userId) =>
        rt => rt.CurrentUserHolderId == userId;

    public static Expression<Func<RequestTransaction, bool>> HasParentTransaction() =>
        rt => rt.ParentTransactionId.HasValue;

    public static Expression<Func<RequestTransaction, bool>> IsRootLevel() =>
        rt => !rt.ParentTransactionId.HasValue;

    public static Expression<Func<RequestTransaction, bool>> CanReadByUserId(Guid userId) =>
        rt => rt.CurrentUserHolderId == userId
             || rt.Request.RequesterId == userId
             || rt.Request.ApproverId == userId;

    public static Expression<Func<RequestTransaction, bool>> CanMakeUpdateByUserId(Guid userId) =>
        rt => rt.CurrentUserHolderId == userId;

    public static List<Expression<Func<RequestTransaction, bool>>> ByTransactionIdWithIncludes(Guid transactionId) =>
    [
        ByTransactionId(transactionId)
    ];

    public static List<Expression<Func<RequestTransaction, bool>>> ByRequestIdWithIncludes(Guid requestId) =>
    [
        ByRequestId(requestId)
    ];

    public static List<Expression<Func<RequestTransaction, bool>>> ByParentTransactionIdWithIncludes(Guid parentTransactionId) =>
    [
        ByParentTransactionId(parentTransactionId)
    ];

    public static List<Expression<Func<RequestTransaction, bool>>> ByCurrentUserHolderIdWithIncludes(Guid userId) =>
    [
        ByCurrentUserHolderId(userId)
    ];

    public static List<Expression<Func<RequestTransaction, bool>>> CanReadByUserIdWithIncludes(Guid userId) =>
    [
        CanReadByUserId(userId)
    ];

    public static List<Expression<Func<RequestTransaction, bool>>> CanMakeUpdateByUserIdWithIncludes(Guid userId) =>
    [
        CanMakeUpdateByUserId(userId)
    ];
}
