using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Transaction.Domain;

public static class TransactionErrors
{
    public static readonly Error InvalidInput = Error.Validation("700", "The provided input is invalid.", "المدخلات المقدمة غير صالحة.");
    public static readonly Error InternalServerError = Error.Failure("800", "An unexpected error occurred.", "حدث خطأ غير متوقع.");
    public static readonly Error DatabaseError = Error.Failure("801", "A database error occurred.", "حدث خطأ في قاعدة البيانات.");

    // Request
    public static readonly Error RequestNotFound = Error.NotFound("400", "The specified request was not found.", "الطلب المحدد غير موجود.");
    public static readonly Error RequestNumberConflict = Error.Conflict("401", "Could not generate a unique request number. Please try again.", "تعذر إنشاء رقم طلب فريد. يرجى المحاولة مرة أخرى.");
    public static readonly Error UnauthorizedRequestAccess = Error.Validation("402", "You do not have access to this request.", "ليس لديك صلاحية الوصول إلى هذا الطلب.");
    public static readonly Error UnauthorizedDeleteRequest = Error.Validation("403", "You do not have access to delete this request.", "ليس لديك صلاحية حذف هذا الطلب.");
    public static readonly Error UnauthorizedModifyRequest = Error.Validation("404", "You do not have access to modify this request.", "ليس لديك صلاحية تعديل هذا الطلب.");
    public static readonly Error MissingRequiredField = Error.Validation("405", "Attachments are required for this template.", "المرفقات مطلوبة لهذا القالب.");
    public static readonly Error DepartmentHeadIsNotSet = Error.Validation("406", "The department head is not set for the selected department.", "رئيس القسم غير محدد للقسم المحدد.");

    // Request Relations
    public static readonly Error RequestRelationAlreadyExists = Error.Validation("410", "This relation already exists.", "هذه العلاقة موجودة بالفعل.");
    public static readonly Error DuplicateRelation = Error.Validation("411", "Duplicate relation.", "علاقة مكررة.");
    public static readonly Error RequestRelationNotFound = Error.Validation("412", "The specified request relation was not found.", "علاقة الطلب المحددة غير موجودة.");
    public static readonly Error UnauthorizedRequestRelationAccess = Error.Validation("413", "You do not have access to view related requests.", "ليس لديك صلاحية عرض الطلبات المرتبطة.");
    public static readonly Error UnauthorizedViewRelation = Error.Validation("414", "You do not have access to view this relation.", "ليس لديك صلاحية عرض هذه العلاقة.");
    public static readonly Error UnauthorizedModifySourceRequest = Error.Validation("415", "You do not have access to modify the source request.", "ليس لديك صلاحية تعديل الطلب المصدر.");
    public static readonly Error UnauthorizedViewTargetRequest = Error.Validation("416", "You do not have access to view the target request.", "ليس لديك صلاحية عرض الطلب الهدف.");
    public static readonly Error UnauthorizedModifyNewSourceRequest = Error.Validation("417", "You do not have access to modify the new source request.", "ليس لديك صلاحية تعديل الطلب المصدر الجديد.");
    public static readonly Error UnauthorizedDeleteRelation = Error.Validation("418", "You do not have access to delete this relation.", "ليس لديك صلاحية حذف هذه العلاقة.");

    // Response
    public static readonly Error ResponseNotFound = Error.NotFound("500", "The specified response was not found.", "الرد المحدد غير موجود.");

    // Template
    public static readonly Error TemplateNotFound = Error.NotFound("300", "The specified template was not found.", "القالب المحدد غير موجود.");
    public static readonly Error TemplateNameRequired = Error.Validation("301", "Template name is required.", "اسم القالب مطلوب.");
    public static readonly Error DepartmentAlreadyOwner = Error.Validation("302", "Department is already an owner of this template.", "القسم مالك لهذا القالب بالفعل.");
    public static readonly Error UserAlreadyOwner = Error.Validation("303", "User is already an owner of this template.", "المستخدم مالك لهذا القالب بالفعل.");

    // Department
    public static readonly Error DepartmentNotFound = Error.NotFound("310", "The specified department was not found.", "القسم المحدد غير موجود.");

    // Attachment
    public static readonly Error AttachmentNotFound = Error.NotFound("600", "The specified attachment was not found.", "المرفق المحدد غير موجود.");

    // Request Transaction
    public static readonly Error RequestTransactionNotFound = Error.NotFound("907", "The specified request transaction was not found.", "معاملة الطلب المحددة غير موجودة.");
    public static readonly Error RequestAlreadyHasTransaction = Error.Validation("908", "Request already has a transaction.", "الطلب لديه معاملة بالفعل.");

    // LookUp Field
    public static readonly Error LookUpFieldNotFound = Error.NotFound("200", "The specified lookup field was not found.", "حقل البحث المحدد غير موجود.");
    public static readonly Error LookUpFiledValueNotFound = Error.NotFound("210", "The specified lookup field value was not found.", "قيمة حقل البحث المحددة غير موجودة.");

    // Report
    public static readonly Error StartDateMustBeEarlier = Error.Validation("R001", "startDate must be earlier than endDate.", "يجب أن يكون تاريخ البداية أقدم من تاريخ النهاية.");

    // RequestTransaction
    public static readonly Error FetchRequestTransactionsFailed = Error.Failure("900", "An unexpected error occurred while fetching request transactions.", "حدث خطأ غير متوقع أثناء جلب معاملات الطلب.");
    public static readonly Error FetchRequestTransactionFailed = Error.Failure("901", "An unexpected error occurred while fetching request transaction.", "حدث خطأ غير متوقع أثناء جلب معاملة الطلب.");
    public static readonly Error FetchChildTransactionsFailed = Error.Failure("902", "An unexpected error occurred while fetching child transactions.", "حدث خطأ غير متوقع أثناء جلب المعاملات الفرعية.");
    public static readonly Error FetchRootTransactionFailed = Error.Failure("903", "An unexpected error occurred while fetching root transaction.", "حدث خطأ غير متوقع أثناء جلب المعاملة الجذرية.");
    public static readonly Error FetchTransactionTreeFailed = Error.Failure("904", "An unexpected error occurred while fetching transaction tree.", "حدث خطأ غير متوقع أثناء جلب شجرة المعاملات.");
    public static readonly Error CreateRequestTransactionFailed = Error.Failure("905", "An unexpected error occurred while creating request transaction.", "حدث خطأ غير متوقع أثناء إنشاء معاملة الطلب.");
    public static readonly Error AddChildTransactionFailed = Error.Failure("906", "An unexpected error occurred while adding child transaction.", "حدث خطأ غير متوقع أثناء إضافة معاملة فرعية.");
    public static readonly Error MarkTransactionManagedFailed = Error.Failure("909", "An unexpected error occurred while marking request transaction as managed.", "حدث خطأ غير متوقع أثناء تحديث حالة معاملة الطلب.");

    // Response
    public static readonly Error FetchResponsesFailed = Error.Failure("500", "An unexpected error occurred while fetching responses.", "حدث خطأ غير متوقع أثناء جلب الردود.");
    public static readonly Error FetchResponseFailed = Error.Failure("501", "An unexpected error occurred while fetching response.", "حدث خطأ غير متوقع أثناء جلب الرد.");
    public static readonly Error FetchResponsesByRequestFailed = Error.Failure("502", "An unexpected error occurred while fetching responses by request.", "حدث خطأ غير متوقع أثناء جلب الردود حسب الطلب.");
    public static readonly Error FetchResponsesByResponderFailed = Error.Failure("503", "An unexpected error occurred while fetching responses by responder.", "حدث خطأ غير متوقع أثناء جلب الردود حسب المستجيب.");
    public static readonly Error FetchResponsesByRequesterFailed = Error.Failure("504", "An unexpected error occurred while fetching responses by requester.", "حدث خطأ غير متوقع أثناء جلب الردود حسب مقدم الطلب.");
    public static readonly Error CreateResponseFailed = Error.Failure("505", "An unexpected error occurred while creating response.", "حدث خطأ غير متوقع أثناء إنشاء الرد.");
    public static readonly Error UpdateResponseFailed = Error.Failure("506", "An unexpected error occurred while updating response.", "حدث خطأ غير متوقع أثناء تحديث الرد.");
    public static readonly Error DeleteResponseFailed = Error.Failure("507", "An unexpected error occurred while deleting response.", "حدث خطأ غير متوقع أثناء حذف الرد.");
    public static readonly Error AddFilesToResponseFailed = Error.Failure("508", "An unexpected error occurred while adding files to response.", "حدث خطأ غير متوقع أثناء إضافة الملفات إلى الرد.");

    public static Error FailedToReadFile(string message) =>
        Error.Failure("1000", $"Failed to read file: {message}", $"فشل في قراءة الملف: {message}");
}
