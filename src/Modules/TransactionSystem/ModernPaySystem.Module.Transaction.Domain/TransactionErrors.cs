using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Transaction.Domain;

public static class TransactionErrors
{
    public static readonly Error AttachmentNotFound = Error.NotFound("600", "The specified attachment was not found.");
    public static readonly Error RequestNotFound = Error.NotFound("400", "The specified request was not found.");
    public static readonly Error ResponseNotFound = Error.NotFound("500", "The specified response was not found.");
    public static readonly Error RequestTransactionNotFound = Error.NotFound("907", "The specified request transaction was not found.");
    public static readonly Error TemplateNotFound = Error.NotFound("300", "The specified template was not found.");
    public static readonly Error DepartmentNotFound = Error.NotFound("310", "The specified department was not found.");
    public static readonly Error DatabaseError = Error.Failure("801", "A database error occurred.");
    public static readonly Error InvalidInput = Error.Validation("700", "The provided input is invalid.");
}
