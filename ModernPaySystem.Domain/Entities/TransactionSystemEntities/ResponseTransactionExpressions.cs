using System.Linq.Expressions;
using ExpressionBuilderLib.src.Core;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class ResponseTransactionExpressions
{
    public static Expression<Func<ResponseTransaction, bool>> ByTransactionId(Guid transactionId) =>
        rt => rt.Id == transactionId;

    public static Expression<Func<ResponseTransaction, bool>> ByResponseId(Guid responseId) =>
        rt => rt.ResponseId == responseId;

    public static Expression<Func<ResponseTransaction, bool>> ByParentTransactionId(Guid? parentTransactionId) =>
        rt => rt.ParentTransactionId == parentTransactionId;

    public static Expression<Func<ResponseTransaction, bool>> ByCurrentUserHolderId(Guid userId) =>
        rt => rt.CurrentUserHolderId == userId;

    public static Expression<Func<ResponseTransaction, bool>> HasParentTransaction() =>
        rt => rt.ParentTransactionId.HasValue;

    public static Expression<Func<ResponseTransaction, bool>> IsRootLevel() =>
        rt => !rt.ParentTransactionId.HasValue;

    public static Expression<Func<ResponseTransaction, bool>> CanReadByUserId(Guid userId) =>
        rt => rt.CurrentUserHolderId == userId
             || rt.Response.Request.RequesterId == userId
             || rt.Response.Request.ApproverId == userId;

    public static Expression<Func<ResponseTransaction, bool>> CanMakeUpdateByUserId(Guid userId) =>
        rt => rt.CurrentUserHolderId == userId;

    public static List<Expression<Func<ResponseTransaction, bool>>> ByTransactionIdWithIncludes(Guid transactionId) =>
    [
        ByTransactionId(transactionId)
    ];

    public static List<Expression<Func<ResponseTransaction, bool>>> ByResponseIdWithIncludes(Guid responseId) =>
    [
        ByResponseId(responseId)
    ];

    public static List<Expression<Func<ResponseTransaction, bool>>> ByParentTransactionIdWithIncludes(Guid parentTransactionId) =>
    [
        ByParentTransactionId(parentTransactionId)
    ];

    public static List<Expression<Func<ResponseTransaction, bool>>> ByCurrentUserHolderIdWithIncludes(Guid userId) =>
    [
        ByCurrentUserHolderId(userId)
    ];

    public static List<Expression<Func<ResponseTransaction, bool>>> CanReadByUserIdWithIncludes(Guid userId) =>
    [
        CanReadByUserId(userId)
    ];

    public static List<Expression<Func<ResponseTransaction, bool>>> CanMakeUpdateByUserIdWithIncludes(Guid userId) =>
    [
        CanMakeUpdateByUserId(userId)
    ];
}
