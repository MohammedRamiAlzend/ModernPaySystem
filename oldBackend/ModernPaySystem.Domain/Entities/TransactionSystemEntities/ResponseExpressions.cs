using ExpressionBuilderLib.src.Core;
using System.Linq.Expressions;

namespace ModernPaySystem.Domain.Entities.TransactionSystemEntities;

public static class ResponseExpressions
{
    public static Expression<Func<Response, bool>> ByResponseId(Guid responseId) =>
        r => r.Id == responseId;

    public static Expression<Func<Response, bool>> ByRequestId(Guid requestId) =>
        r => r.RequestId == requestId;

    public static Expression<Func<Response, bool>> ByRespondedByUserId(Guid userId) =>
        r => r.RespondedByUserId == userId;

    public static Expression<Func<Response, bool>> ByRequesterId(Guid requesterId) =>
        r => r.Request.RequesterId == requesterId;

    public static Expression<Func<Response, bool>> CanReadByUserId(Guid userId) =>
        r => r.RespondedByUserId == userId
             || r.Request.RequesterId == userId
             || r.Request.ApproverId == userId;

    public static Expression<Func<Response, bool>> CanMakeUpdateByUserId(Guid userId) =>
        r => r.RespondedByUserId == userId;

    public static List<Expression<Func<Response, bool>>> ByRequestIdWithIncludes(Guid requestId) =>
    [
        ByRequestId(requestId)
    ];

    public static List<Expression<Func<Response, bool>>> ByRespondedByUserIdWithIncludes(Guid userId) =>
    [
        ByRespondedByUserId(userId)
    ];

    public static List<Expression<Func<Response, bool>>> ByRequesterIdWithIncludes(Guid requesterId) =>
    [
        ByRequesterId(requesterId)
    ];
}
