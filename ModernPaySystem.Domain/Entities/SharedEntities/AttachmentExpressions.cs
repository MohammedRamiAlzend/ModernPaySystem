using System.Linq.Expressions;
using ModernPaySystem.Domain.Entities.TransactionSystemEntities;

namespace ModernPaySystem.Domain.Entities.SharedEntities;

public static class AttachmentExpressions
{
    public static Expression<Func<Attachment, bool>> ById(Guid id) =>
        a => a.Id == id;

    public static Expression<Func<Attachment, bool>> ByFileName(string fileName) =>
        a => a.FileName == fileName;

    public static Expression<Func<Attachment, bool>> ByExtension(string extension) =>
        a => a.Extension == extension;

    public static Expression<Func<Attachment, bool>> BySafeName(string safeName) =>
        a => a.SafeName == safeName;

    public static Expression<Func<Attachment, bool>> ByCreatedByUserId(string userId) =>
        a => a.CreatedByUserId == userId;

    // RequestAttachment expressions
    public static Expression<Func<RequestAttachment, bool>> RequestAttachmentByRequestId(Guid requestId) =>
        ra => ra.RequestId == requestId;

    public static Expression<Func<RequestAttachment, bool>> RequestAttachmentByAttachmentId(Guid attachmentId) =>
        ra => ra.AttachmentId == attachmentId;

    public static Expression<Func<RequestAttachment, bool>> RequestAttachmentByRequestIdAndAttachmentId(Guid requestId, Guid attachmentId) =>
        ra => ra.RequestId == requestId && ra.AttachmentId == attachmentId;

    // ResponseAttachment expressions
    public static Expression<Func<ResponseAttachment, bool>> ResponseAttachmentByResponseId(Guid responseId) =>
        ra => ra.ResponseId == responseId;

    public static Expression<Func<ResponseAttachment, bool>> ResponseAttachmentByAttachmentId(Guid attachmentId) =>
        ra => ra.AttachmentId == attachmentId;

    public static Expression<Func<ResponseAttachment, bool>> ResponseAttachmentByResponseIdAndAttachmentId(Guid responseId, Guid attachmentId) =>
        ra => ra.ResponseId == responseId && ra.AttachmentId == attachmentId;

    public static List<Expression<Func<Attachment, bool>>> ByIdWithIncludes(Guid id) =>
    [
        ById(id)
    ];

    public static List<Expression<Func<RequestAttachment, bool>>> RequestAttachmentByRequestIdWithIncludes(Guid requestId) =>
    [
        RequestAttachmentByRequestId(requestId)
    ];

    public static List<Expression<Func<ResponseAttachment, bool>>> ResponseAttachmentByResponseIdWithIncludes(Guid responseId) =>
    [
        ResponseAttachmentByResponseId(responseId)
    ];
}
