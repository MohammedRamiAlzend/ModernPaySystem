namespace ModernPaySystem.Domain.Commons;

/// <summary>
/// Contains predefined application errors with unique numeric codes.
/// </summary>
public static class ApplicationErrors
{
    // Authentication Errors (1-99)
    public static readonly Error InvalidCredentials = new("1", "Username or password is incorrect.", ErrorKind.Unauthorized, "اسم المستخدم أو كلمة المرور غير صحيحة.");
    public static readonly Error TokenExpired = new("3", "The authentication token has expired.", ErrorKind.Unauthorized, "انتهت صلاحية رمز المصادقة.");
    public static readonly Error InvalidToken = new("4", "The authentication token is invalid.", ErrorKind.Unauthorized, "رمز المصادقة غير صحيح.");
    public static readonly Error InsufficientPermissions = new("5", "User lacks required permissions.", ErrorKind.Forbidden, "المستخدم لا يملك الأذونات المطلوبة.");
    public static readonly Error UserNotFound = new("6", "User with the specified ID was not found.", ErrorKind.NotFound, "لم يتم العثور على المستخدم.");
    public static readonly Error UserAlreadyExists = new("7", "A user with this username already exists.", ErrorKind.Conflict, "يوجد بالفعل مستخدم بهذا الاسم.");
    public static readonly Error UserNotActive = new("8", "The user account is not active.", ErrorKind.Forbidden, "حساب المستخدم غير نشط.");

    // Role Errors (100-199)
    public static readonly Error RoleNotFound = new("100", "The specified role was not found.", ErrorKind.NotFound, "لم يتم العثور على الدور المحدد.");
    public static readonly Error RoleAlreadyExists = new("101", "A role with this name already exists.", ErrorKind.Conflict, "يوجد بالفعل دور بهذا الاسم.");
    public static readonly Error CannotDeleteDefaultRole = new("102", "Cannot delete a default system role.", ErrorKind.Forbidden, "لا يمكن حذف دور النظام الافتراضي.");
    public static readonly Error RoleNotAssignedToUser = new("103", "The role is not assigned to this user.", ErrorKind.NotFound, "لم يتم تعيين الدور لهذا المستخدم.");

    // Permission Errors (200-299)
    public static readonly Error PermissionNotFound = new("200", "The specified permission was not found.", ErrorKind.NotFound, "لم يتم العثور على الإذن المحدد.");
    public static readonly Error PermissionAlreadyExists = new("201", "A permission with this name already exists.", ErrorKind.Conflict, "يوجد بالفعل إذن بهذا الاسم.");
    public static readonly Error PermissionNotAssignedToRole = new("202", "The permission is not assigned to this role.", ErrorKind.NotFound, "لم يتم تعيين الإذن لهذا الدور.");

    // Template Errors (300-399)
    public static readonly Error TemplateNotFound = new("300", "The specified template was not found.", ErrorKind.NotFound, "لم يتم العثور على القالب المحدد.");
    public static readonly Error TemplateAlreadyExists = new("301", "A template with this name already exists.", ErrorKind.Conflict, "يوجد بالفعل قالب بهذا الاسم.");
    public static readonly Error InvalidTemplateContent = new("302", "The template content is invalid.", ErrorKind.Validation, "محتوى القالب غير صحيح.");
    public static readonly Error TemplateInUse = new("303", "The template is currently in use and cannot be deleted.", ErrorKind.Conflict, "القالب قيد الاستخدام ولا يمكن حذفه.");
    public static readonly Error UnauthorizedTemplateAccess = new("304", "You do not have access to this template.", ErrorKind.Forbidden, "ليس لديك حق الوصول إلى هذا القالب.");

    // Request Errors (400-499)
    public static readonly Error RequestNotFound = new("400", "The specified request was not found.", ErrorKind.NotFound, "لم يتم العثور على الطلب المحدد.");
    public static readonly Error RequestAlreadyApproved = new("401", "The request has already been approved.", ErrorKind.Conflict, "تم بالفعل الموافقة على الطلب.");
    public static readonly Error RequestAlreadyRejected = new("402", "The request has already been rejected.", ErrorKind.Conflict, "تم بالفعل رفض الطلب.");
    public static readonly Error InvalidRequestStatus = new("403", "The request has an invalid status.", ErrorKind.Validation, "الطلب له حالة غير صحيحة.");
    public static readonly Error CannotApproveOwnRequest = new("404", "You cannot approve your own request.", ErrorKind.Forbidden, "لا يمكنك الموافقة على طلبك الخاص.");
    public static readonly Error RequesterNotFound = new("405", "The requester was not found.", ErrorKind.NotFound, "لم يتم العثور على طالب الطلب.");
    public static readonly Error ApproverNotFound = new("406", "The approver was not found.", ErrorKind.NotFound, "لم يتم العثور على الموافق.");
    public static readonly Error UnauthorizedRequestAccess = new("407", "You do not have access to this request.", ErrorKind.Forbidden, "ليس لديك حق الوصول إلى هذا الطلب.");

    // Response Errors (500-599)
    public static readonly Error ResponseNotFound = new("500", "The specified response was not found.", ErrorKind.NotFound, "لم يتم العثور على الرد المحدد.");
    public static readonly Error RequestAlreadyHasResponse = new("501", "A response already exists for this request.", ErrorKind.Conflict, "يوجد بالفعل رد على هذا الطلب.");
    public static readonly Error CannotRespondToOwnRequest = new("502", "You cannot respond to your own request.", ErrorKind.Forbidden, "لا يمكنك الرد على طلبك الخاص.");
    public static readonly Error InvalidResponseContent = new("503", "The response content is invalid.", ErrorKind.Validation, "محتوى الرد غير صحيح.");

    // Attachment Errors (600-699)
    public static readonly Error AttachmentNotFound = new("600", "The specified attachment was not found.", ErrorKind.NotFound, "لم يتم العثور على المرفق المحدد.");
    public static readonly Error InvalidAttachmentType = new("601", "The attachment type is not allowed.", ErrorKind.Validation, "نوع المرفق غير مسموح.");
    public static readonly Error AttachmentTooLarge = new("602", "The attachment file is too large.", ErrorKind.Validation, "ملف المرفق كبير جداً.");
    public static readonly Error FailedToUploadAttachment = new("603", "Failed to upload the attachment.", ErrorKind.Failure, "فشل في تحميل المرفق.");
    public static readonly Error FailedToDeleteAttachment = new("604", "Failed to delete the attachment.", ErrorKind.Failure, "فشل في حذف المرفق.");

    // Validation Errors (700-799)
    public static readonly Error InvalidInput = new("700", "The provided input is invalid.", ErrorKind.Validation, "الإدخال المقدم غير صحيح.");
    public static readonly Error MissingRequiredField = new("701", "A required field is missing.", ErrorKind.Validation, "حقل مطلوب مفقود.");
    public static readonly Error InvalidEmailFormat = new("702", "The email format is invalid.", ErrorKind.Validation, "صيغة البريد الإلكتروني غير صحيحة.");
    public static readonly Error InvalidPasswordLength = new("703", "The password must be at least 8 characters long.", ErrorKind.Validation, "يجب أن تكون كلمة المرور 8 أحرف على الأقل.");
    public static readonly Error PasswordsDoNotMatch = new("704", "The passwords do not match.", ErrorKind.Validation, "كلمات المرور غير متطابقة.");

    // General Errors (800-899)
    public static readonly Error InternalServerError = new("800", "An unexpected error occurred. Please try again later.", ErrorKind.Failure, "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً.");
    public static readonly Error DatabaseError = new("801", "A database error occurred.", ErrorKind.Failure, "حدث خطأ في قاعدة البيانات.");
    public static readonly Error OperationFailed = new("802", "The operation could not be completed.", ErrorKind.Failure, "لم يتمكن من إكمال العملية.");
    public static readonly Error NotImplemented = new("803", "This feature has not been implemented yet.", ErrorKind.Failure, "هذه الميزة لم يتم تنفيذها بعد.");
    public static readonly Error ConcurrencyError = new("804", "The resource was modified by another user. Please refresh and try again.", ErrorKind.Conflict, "تم تعديل المورد من قبل مستخدم آخر. يرجى التحديث والمحاولة مرة أخرى.");

    // Transaction System Errors (900-999)
    public static readonly Error TransactionNotFound = new("900", "The specified transaction was not found.", ErrorKind.NotFound, "لم يتم العثور على المعاملة المحددة.");
    public static readonly Error InvalidTransactionAmount = new("901", "The transaction amount is invalid.", ErrorKind.Validation, "مبلغ المعاملة غير صحيح.");
    public static readonly Error InsufficientFunds = new("902", "Insufficient funds for this transaction.", ErrorKind.Validation, "رصيد غير كافي لهذه المعاملة.");
    public static readonly Error TransactionAlreadyProcessed = new("903", "The transaction has already been processed.", ErrorKind.Conflict, "تم معالجة المعاملة بالفعل.");
    public static readonly Error DuplicateTransaction = new("904", "A duplicate transaction was detected.", ErrorKind.Conflict, "تم كشف معاملة مكررة.");

    // File Operation Errors (1000-1099)
    public static Error FileNotFound(string path, string? message = null) => new("1000", $"The specified file was not found at path : {path}.", ErrorKind.NotFound, "لم يتم العثور على الملف المحدد.");

    public static Error FileOperationFailed(string message)
        => Error.Failure("1001", $"File operation failed: {message}", "فشلت عملية الملف:");
}
