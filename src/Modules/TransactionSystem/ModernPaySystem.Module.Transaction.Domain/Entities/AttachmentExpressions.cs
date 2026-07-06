using ModernPaySystem.SharedKernel.Domain.Entities;
using System.Linq.Expressions;

namespace ModernPaySystem.Module.Transaction.Domain.Entities;

public static class AttachmentExpressions
{
    public static Expression<Func<RequestAttachment, bool>> RequestAttachmentByRequestId(Guid requestId) =>
        ra => ra.RequestId == requestId;

    public static Expression<Func<RequestAttachment, bool>> RequestAttachmentByAttachmentId(Guid attachmentId) =>
        ra => ra.AttachmentId == attachmentId;

    public static Expression<Func<RequestAttachment, bool>> RequestAttachmentByRequestIdAndAttachmentId(Guid requestId, Guid attachmentId) =>
        ra => ra.RequestId == requestId && ra.AttachmentId == attachmentId;

    public static Expression<Func<ResponseAttachment, bool>> ResponseAttachmentByResponseId(Guid responseId) =>
        ra => ra.ResponseId == responseId;

    public static Expression<Func<ResponseAttachment, bool>> ResponseAttachmentByAttachmentId(Guid attachmentId) =>
        ra => ra.AttachmentId == attachmentId;

    public static Expression<Func<ResponseAttachment, bool>> ResponseAttachmentByResponseIdAndAttachmentId(Guid responseId, Guid attachmentId) =>
        ra => ra.ResponseId == responseId && ra.AttachmentId == attachmentId;

    public static Expression<Func<RequestTransactionAttachment, bool>> RequestTransactionAttachmentByTransactionId(Guid transactionId) =>
        ra => ra.RequestTransactionId == transactionId;

    public static Expression<Func<RequestTransactionAttachment, bool>> RequestTransactionAttachmentByAttachmentId(Guid attachmentId) =>
        ra => ra.AttachmentId == attachmentId;

    public static Expression<Func<RequestTransactionAttachment, bool>> RequestTransactionAttachmentByTransactionIdAndAttachmentId(Guid transactionId, Guid attachmentId) =>
        ra => ra.RequestTransactionId == transactionId && ra.AttachmentId == attachmentId;
}
