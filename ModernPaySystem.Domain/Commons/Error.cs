using System.Net;
using System.Threading;

namespace ModernPaySystem.Domain.Commons;

public readonly record struct Error
{
    public Error(string code, string description, ErrorKind type = ErrorKind.Failure, string? arabicDescription = null, HttpStatusCode httpStatus = HttpStatusCode.BadRequest)
    {
        Code = code;
        Description = description;
        Type = type;
        ArabicDescription = arabicDescription;
        HttpStatus = httpStatus;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorKind Type { get; }
    public string? ArabicDescription { get; }
    public HttpStatusCode HttpStatus { get; }

    public static Error Failure(string code = nameof(Failure), string description = "General failure.", string? arabicDescription = null)
        => new(code, description, ErrorKind.Failure, arabicDescription, HttpStatusCode.InternalServerError);
    public static Error Unexpected(string code = nameof(Unexpected), string description = "Unexpected Error.", string? arabicDescription = null)
        => new(code, description, ErrorKind.Unexpected, arabicDescription, HttpStatusCode.InternalServerError);
    public static Error Validation(string code = nameof(Validation), string description = " Validation Error.", string? arabicDescription = null)
        => new(code, description, ErrorKind.Validation, arabicDescription, HttpStatusCode.BadRequest);
    public static Error Conflict(string code = nameof(Conflict), string description = " Conflict Error.", string? arabicDescription = null)
        => new(code, description, ErrorKind.Conflict, arabicDescription, HttpStatusCode.Conflict);
    public static Error NotFound(string code = nameof(NotFound), string description = " NotFound Error.", string? arabicDescription = null)
        => new(code, description, ErrorKind.NotFound, arabicDescription, HttpStatusCode.NotFound);
    public static Error Unauthorized(string code = nameof(Unauthorized), string description = "Unauthorized Error.", string? arabicDescription = null)
        => new(code, description, ErrorKind.Unauthorized, arabicDescription, HttpStatusCode.Unauthorized);
    public static Error Forbidden(string code = nameof(Forbidden), string description = "Forbidden Error.", string? arabicDescription = null)
        => new(code, description, ErrorKind.Forbidden, arabicDescription, HttpStatusCode.Forbidden);

    public string GetDescription(string? culture = null)
    {
        string? currentCulture = culture ?? Thread.CurrentThread.CurrentUICulture?.Name;
        return (currentCulture?.StartsWith("ar", StringComparison.OrdinalIgnoreCase) == true ? ArabicDescription : null) ?? Description;
    }
}

public enum ErrorKind
{
    Failure,
    Unexpected,
    Validation,
    Conflict,
    NotFound,
    Unauthorized,
    Forbidden,
}
