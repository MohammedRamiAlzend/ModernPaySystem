namespace ModernPaySystem.Domain.Commons;

using System.Net;

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

    // Department Errors (310-319)
    public static readonly Error DepartmentNotFound = new("310", "The specified department was not found.", ErrorKind.NotFound, "لم يتم العثور على القسم المحدد.");
    public static Error UserAlreadyDepartmentHeader(Guid id, string departmentName) => new("311", $"user already department header for this id :({id},{departmentName})", ErrorKind.NotFound, "");

    // Request Errors (400-499)
    public static readonly Error RequestNotFound = new("400", "The specified request was not found.", ErrorKind.NotFound, "لم يتم العثور على الطلب المحدد.");
    public static readonly Error RequestAlreadyApproved = new("401", "The request has already been approved.", ErrorKind.Conflict, "تم بالفعل الموافقة على الطلب.");
    public static readonly Error RequestAlreadyRejected = new("402", "The request has already been rejected.", ErrorKind.Conflict, "تم بالفعل رفض الطلب.");
    public static readonly Error InvalidRequestStatus = new("403", "The request has an invalid status.", ErrorKind.Validation, "الطلب له حالة غير صحيحة.");
    public static readonly Error CannotApproveOwnRequest = new("404", "You cannot approve your own request.", ErrorKind.Forbidden, "لا يمكنك الموافقة على طلبك الخاص.");
    public static readonly Error RequesterNotFound = new("405", "The requester was not found.", ErrorKind.NotFound, "لم يتم العثور على طالب الطلب.");
    public static readonly Error ApproverNotFound = new("406", "The approver was not found.", ErrorKind.NotFound, "لم يتم العثور على الموافق.");
    public static readonly Error UnauthorizedRequestAccess = new("407", "You do not have access to this request.", ErrorKind.Forbidden, "ليس لديك حق الوصول إلى هذا الطلب.");
    public static readonly Error RequestRelationNotFound = new("408", "The specified request relation was not found.", ErrorKind.NotFound, "لم يتم العثور على علاقة الطلب المحددة.");
    public static readonly Error UnauthorizedRequestRelationAccess = new("409", "You do not have access to this request relation.", ErrorKind.Forbidden, "ليس لديك حق الوصول إلى علاقة الطلب المحددة.");
    public static readonly Error RequestRelationAlreadyExists = new("410", "A request relation with the same source, target, and type already exists.", ErrorKind.Conflict, "يوجد بالفعل ارتباط بنفس المصدر والهدف والنوع.");

    // Response Errors (500-599)
    public static readonly Error ResponseNotFound = new("500", "The specified response was not found.", ErrorKind.NotFound, "لم يتم العثور على الرد المحدد.");
    public static readonly Error RequestAlreadyHasResponse = new("501", "A response already exists for this request.", ErrorKind.Conflict, "يوجد بالفعل رد على هذا الطلب.");
    public static readonly Error CannotRespondToOwnRequest = new("502", "You cannot respond to your own request.", ErrorKind.Forbidden, "لا يمكنك الرد على طلبك الخاص.");
    public static readonly Error InvalidResponseContent = new("503", "The response content is invalid.", ErrorKind.Validation, "محتوى الرد غير صحيح.");

    // Attachment Errors (600-699)
    public static readonly Error AttachmentNotFound = new("600", "The specified attachment was not found.", ErrorKind.NotFound, "لم يتم العثور على المرفق المحدد.");
    public static Error InvalidAttachmentType(List<string> rejectedFileNames) =>
        new("601", $"The following file types are not allowed: {string.Join(", ", rejectedFileNames)}.", ErrorKind.Validation, $"أنواع الملفات التالية غير مسموح بها: {string.Join("، ", rejectedFileNames)}.");
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

    // General duplicate/conflict
    public static readonly Error DuplicateEntry = new("805", "A duplicate entry was detected.", ErrorKind.Conflict, "تم العثور على إدخال مكرر.");

    // Transaction System Errors (900-999)
    public static readonly Error TransactionNotFound = new("900", "The specified transaction was not found.", ErrorKind.NotFound, "لم يتم العثور على المعاملة المحددة.");
    public static readonly Error InvalidTransactionAmount = new("901", "The transaction amount is invalid.", ErrorKind.Validation, "مبلغ المعاملة غير صحيح.");
    public static readonly Error InsufficientFunds = new("902", "Insufficient funds for this transaction.", ErrorKind.Validation, "رصيد غير كافي لهذه المعاملة.");
    public static readonly Error TransactionAlreadyProcessed = new("903", "The transaction has already been processed.", ErrorKind.Conflict, "تم معالجة المعاملة بالفعل.");
    public static readonly Error DuplicateTransaction = new("904", "A duplicate transaction was detected.", ErrorKind.Conflict, "تم كشف معاملة مكررة.");
    public static readonly Error ResponseTransactionNotFound = new("905", "The specified response transaction was not found.", ErrorKind.NotFound, "لم يتم العثور على معاملة الرد المحددة.");
    public static readonly Error ResponseAlreadyHasTransaction = new("906", "Response Already Has Transaction", ErrorKind.NotFound, "يوجد احالة مسبقا");
    public static readonly Error RequestTransactionNotFound = new("907", "The specified request transaction was not found.", ErrorKind.NotFound, "لم يتم العثور على معاملة الطلب المحددة.");
    public static readonly Error RequestAlreadyHasTransaction = new("908", "Request Already Has Transaction", ErrorKind.NotFound, "يوجد طلب احالة مسبقا");
    public static readonly Error DepartmentHeadIsNotSet = new("909", "DepartmentHeadIsNotSet", ErrorKind.Validation, "DepartmentHeadIsNotSet.");

    // Archiving Errors (10000-10099)
    public static readonly Error FolderNotFound = new("10000", "The specified folder was not found.", ErrorKind.NotFound, "لم يتم العثور على المجلد المحدد.");
    public static readonly Error FolderAlreadyExists = new("10001", "A folder with this name already exists.", ErrorKind.Conflict, "يوجد بالفعل مجلد بهذا الاسم.");
    public static readonly Error FolderHasChildren = new("10002", "The folder contains child folders or records and cannot be deleted.", ErrorKind.Conflict, "المجلد يحتوي على مجلدات أو سجلات فرعية ولا يمكن حذفه.");
    public static readonly Error FolderDepartmentNotConfigured = new("10003", "The folder is not scoped to a department.", ErrorKind.Validation, "المجلد غير مرتبط بقسم.");
    public static readonly Error FolderArchiveLeaderRequired = new("10004", "Only a department archive leader can delete this folder directly.", ErrorKind.Forbidden, "فقط قائد الأرشيف في القسم يمكنه حذف هذا المجلد مباشرة.");
    public static readonly Error FolderDeleteRequestExists = new("10005", "A pending delete request already exists for this folder.", ErrorKind.Conflict, "يوجد طلب حذف قيد الانتظار لهذا المجلد.");
    public static readonly Error DynamicFormNotFound = new("10010", "The specified form was not found.", ErrorKind.NotFound, "لم يتم العثور على النموذج المحدد.");
    public static readonly Error DynamicFormAlreadyExists = new("10011", "A form with this name already exists.", ErrorKind.Conflict, "يوجد بالفعل نموذج بهذا الاسم.");
    public static readonly Error InvalidJsonDefinition = new("10012", "The provided JSON definition is invalid.", ErrorKind.Validation, "تعريف JSON المقدم غير صحيح.");
    public static readonly Error DynamicFormInUse = new("10013", "The form is currently in use and cannot be deleted.", ErrorKind.Conflict, "النموذج قيد الاستخدام ولا يمكن حذفه.");
    public static readonly Error ArchiveRecordNotFound = new("10020", "The specified archive record was not found.", ErrorKind.NotFound, "لم يتم العثور على سجل الأرشفة المحدد.");
    public static readonly Error FormIdMustHasValue = new("10022", "The specified archive record's form ID must have a value.", ErrorKind.Validation, "يجب أن يحتوي معرف النموذج المحدد لسجل الأرشفة على قيمة.");
    public static Error ArchivePhysicalFileMissingFromStorage(string storagePath) =>
        new("10023", $"The archive file metadata exists but the physical file is missing from storage: {storagePath}.", ErrorKind.NotFound, "الملف المادي غير موجود في التخزين رغم وجود بياناته الوصفية.", HttpStatusCode.Gone);
    public static readonly Error ArchiveRecordHasNoFiles = new("10024", "The specified archive record does not contain any files.", ErrorKind.NotFound, "سجل الأرشفة المحدد لا يحتوي على أي ملفات.");
    public static readonly Error ArchiveRecordZipTooLarge = new("10025", "The archive record files exceed the configured ZIP size limit.", ErrorKind.Validation, "تتجاوز ملفات سجل الأرشفة الحد المسموح به لحجم ملف ZIP.");
    public static readonly Error ArchiveRecordZipGenerationTimedOut = new("10026", "The ZIP archive generation timed out.", ErrorKind.Failure, "استغرق إنشاء ملف ZIP وقتاً أطول من المسموح.", HttpStatusCode.RequestTimeout);
    public static readonly Error ArchiveRecordDepartmentNotConfigured = new("10027", "The archive record is not scoped to a department.", ErrorKind.Validation, "سجل الأرشفة غير مرتبط بقسم.");
    public static readonly Error ArchiveRecordArchiveLeaderRequired = new("10028", "Only a department archive leader can delete this archive record directly.", ErrorKind.Forbidden, "فقط قائد الأرشيف في القسم يمكنه حذف سجل الأرشفة مباشرة.");
    public static readonly Error DeleteRequestNotFound = new("10029", "The specified delete request was not found.", ErrorKind.NotFound, "لم يتم العثور على طلب الحذف المحدد.");
    public static readonly Error DeleteRequestAlreadyHandled = new("10030", "The delete request has already been processed.", ErrorKind.Conflict, "تمت معالجة طلب الحذف بالفعل.");
    public static readonly Error DeleteRequestApprovalRequiresDepartmentHead = new("10031", "Only the department head can approve this delete request.", ErrorKind.Forbidden, "فقط رئيس القسم يمكنه الموافقة على طلب الحذف.");
    public static readonly Error DeleteRequestRejectionRequiresReason = new("10032", "A rejection reason is required.", ErrorKind.Validation, "سبب الرفض مطلوب.");
    public static readonly Error DeleteRequestTargetNotFound = new("10033", "The target item for the delete request was not found.", ErrorKind.NotFound, "لم يتم العثور على العنصر المستهدف لطلب الحذف.");
    public static readonly Error DepartmentArchiveLeaderNotAssigned = new("10034", "The department has no active archive leader assigned.", ErrorKind.Validation, "لا يوجد قائد أرشيف نشط معين لهذا القسم.");
    public static readonly Error DepartmentHeadMissing = new("10035", "The department does not have a department head assigned.", ErrorKind.Validation, "لا يوجد رئيس قسم معين.");
    public static readonly Error EditRequestNotFound = new("10036", "The specified edit request was not found.", ErrorKind.NotFound, "لم يتم العثور على طلب التعديل المحدد.");
    public static readonly Error EditRequestAlreadyHandled = new("10037", "The edit request has already been processed.", ErrorKind.Conflict, "تمت معالجة طلب التعديل بالفعل.");
    public static readonly Error QrPageAlreadyExists = new("10038", "A QR cover page already exists for this archive record.", ErrorKind.Conflict, "توجد بالفعل صفحة غلاف QR لهذا المستند.");
    public static readonly Error ArchiveRecordFileDeletionNotBelongToRecord = new("10039", "One or more files selected for deletion do not belong to this archive record.", ErrorKind.Validation, "ملف أو أكثر من الملفات المحددة للحذف لا ينتمي إلى سجل الأرشفة هذا.");
    public static readonly Error FolderAccessDenied = new("10040", "You do not have access to this folder.", ErrorKind.Forbidden, "ليس لديك صلاحية الوصول إلى هذا المجلد.");
    public static readonly Error ArchiveRecordAccessDenied = new("10041", "You do not have access to this archive record.", ErrorKind.Forbidden, "ليس لديك صلاحية الوصول إلى سجل الأرشفة هذا.");
    public static readonly Error PhysicalFileAccessDenied = new("10042", "You do not have access to this file.", ErrorKind.Forbidden, "ليس لديك صلاحية الوصول إلى هذا الملف.");
    public static readonly Error FolderPermissionAlreadyExists = new("10043", "This user already has a permission for this folder.", ErrorKind.Conflict, "هذا المستخدم لديه صلاحية لهذا المجلد بالفعل.");
    public static readonly Error FolderPermissionNotFound = new("10044", "The specified folder permission was not found.", ErrorKind.NotFound, "لم يتم العثور على الصلاحية المحددة للمجلد.");
    public static readonly Error CannotRemoveOwnFolderPermission = new("10045", "You cannot remove your own permission. Transfer ownership instead.", ErrorKind.Forbidden, "لا يمكنك إزالة صلاحيتك الخاصة. قم بنقل الملكية بدلاً من ذلك.");
    public static readonly Error ArchiveConfigNotFound = new("10046", "Archive configuration not found.", ErrorKind.NotFound, "لم يتم العثور على إعدادات الأرشفة.");
    public static readonly Error ArchiveConfigUpdateNotAuthorized = new("10047", "Only archive leaders can update the archive configuration.", ErrorKind.Forbidden, "فقط قادة الأرشيف يمكنهم تحديث إعدادات الأرشفة.");

    // File Operation Errors (1000-1099)
    public static Error FileNotFound(string path, string? message = null) => new("1000", $"The specified file was not found at path : {path}.", ErrorKind.NotFound, "لم يتم العثور على الملف المحدد.");
    public static readonly Error DocumentNotFound = new("1100", "The specified document was not found.", ErrorKind.NotFound, "لم يتم العثور على المستند المحدد.");

    public static Error FileOperationFailed(string message)
        => Error.Failure("1001", $"File operation failed: {message}", "فشلت عملية الملف:");
}
