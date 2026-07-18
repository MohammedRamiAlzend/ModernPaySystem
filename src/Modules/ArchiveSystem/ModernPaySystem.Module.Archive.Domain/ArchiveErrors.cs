using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Domain;

public static class ArchiveErrors
{
    public static readonly Error InvalidInput = Error.Validation("700", "The provided input is invalid.", "المدخلات المقدمة غير صالحة.");
    public static readonly Error InternalServerError = Error.Failure("800", "An unexpected error occurred.", "حدث خطأ غير متوقع.");
    public static readonly Error DatabaseError = Error.Failure("801", "A database error occurred.", "حدث خطأ في قاعدة البيانات.");

    public static readonly Error FolderNotFound = Error.NotFound("10000", "The specified folder was not found.", "المجلد المحدد غير موجود.");
    public static readonly Error FolderHasChildren = Error.Conflict("10002", "The folder contains child folders or records and cannot be deleted.", "المجلد يحتوي على مجلدات فرعية أو مستندات ولا يمكن حذفه.");
    public static readonly Error FolderDepartmentNotConfigured = Error.Validation("10003", "The folder is not scoped to a department.", "المجلد غير تابع لأي قسم.");
    public static readonly Error FolderArchiveLeaderRequired = Error.Forbidden("10004", "Only a department archive leader can delete this folder directly.", "فقط مسؤول أرشيف القسم يمكنه حذف هذا المجلد مباشرة.");
    public static readonly Error FolderDeleteRequestExists = Error.Conflict("10005", "A pending delete request already exists for this folder.", "يوجد بالفعل طلب حذف معلق لهذا المجلد.");
    public static readonly Error FolderAccessDenied = Error.Forbidden("10040", "You do not have access to this folder.", "ليس لديك صلاحية الوصول إلى هذا المجلد.");
    public static readonly Error UserNotEnrolledInDepartment = Error.Forbidden("10043", "You are not enrolled in any department.", "أنت غير مسجل في أي قسم.");

    public static readonly Error DynamicFormNotFound = Error.NotFound("10010", "The specified form was not found.", "النموذج المحدد غير موجود.");
    public static readonly Error DynamicFormAlreadyExists = Error.Conflict("10011", "A form with this name already exists.", "يوجد بالفعل نموذج بهذا الاسم.");
    public static readonly Error InvalidJsonDefinition = Error.Validation("10012", "The provided JSON definition is invalid.", "تعريف الـ JSON المقدم غير صالح.");
    public static readonly Error DynamicFormInUse = Error.Conflict("10013", "The form is currently in use and cannot be deleted.", "النموذج قيد الاستخدام حالياً ولا يمكن حذفه.");

    public static readonly Error ArchiveRecordNotFound = Error.NotFound("10020", "The specified archive record was not found.", "المستند المؤرشف المحدد غير موجود.");
    public static readonly Error ArchiveRecordDepartmentNotConfigured = Error.Validation("10027", "The archive record is not scoped to a department.", "المستند المؤرشف غير تابع لأي قسم.");
    public static readonly Error ArchiveRecordArchiveLeaderRequired = Error.Forbidden("10028", "Only a department archive leader can delete this archive record directly.", "فقط مسؤول أرشيف القسم يمكنه حذف هذا المستند المؤرشف مباشرة.");
    public static readonly Error ArchiveRecordAccessDenied = Error.Forbidden("10041", "You do not have access to this archive record.", "ليس لديك صلاحية الوصول إلى هذا المستند المؤرشف.");
    public static readonly Error ArchiveRecordHasNoFiles = Error.NotFound("10024", "The specified archive record does not contain any files.", "المستند المؤرشف المحدد لا يحتوي على أي ملفات.");
    public static readonly Error QrPageAlreadyExists = Error.Conflict("10038", "A QR cover page already exists for this archive record.", "توجد بالفعل صفحة غلاف رمز الاستجابة السريعة (QR) لهذا المستند المؤرشف.");
    public static readonly Error ArchiveRecordFileDeletionNotBelongToRecord = Error.Validation("10039", "One or more files selected for deletion do not belong to this archive record.", "واحد أو أكثر من الملفات المحددة للحذف لا تنتمي لهذا المستند المؤرشف.");

    public static readonly Error DeleteRequestNotFound = Error.NotFound("10029", "The specified delete request was not found.", "طلب الحذف المحدد غير موجود.");
    public static readonly Error DeleteRequestAlreadyHandled = Error.Conflict("10030", "The delete request has already been processed.", "تمت معالجة طلب الحذف بالفعل.");
    public static readonly Error DeleteRequestApprovalRequiresDepartmentHead = Error.Forbidden("10031", "Only the department head can approve this delete request.", "فقط رئيس القسم يمكنه الموافقة على طلب الحذف هذا.");
    public static readonly Error DeleteRequestRejectionRequiresReason = Error.Validation("10032", "A rejection reason is required.", "يجب تحديد سبب الرفض.");
    public static readonly Error DeleteRequestTargetNotFound = Error.NotFound("10033", "The target item for the delete request was not found.", "العنصر المستهدف لطلب الحذف غير موجود.");

    public static readonly Error EditRequestNotFound = Error.NotFound("10036", "The specified edit request was not found.", "طلب التعديل المحدد غير موجود.");
    public static readonly Error EditRequestAlreadyHandled = Error.Conflict("10037", "The edit request has already been processed.", "تمت معالجة طلب التعديل بالفعل.");

    public static readonly Error DepartmentArchiveLeaderNotAssigned = Error.Validation("10034", "The department has no active archive leader assigned.", "لا يوجد مسؤول أرشيف نشط معين لهذا القسم.");
    public static readonly Error DepartmentHeadMissing = Error.Validation("10035", "The department does not have a department head assigned.", "لا يوجد رئيس معين لهذا القسم.");

    public static readonly Error AttachmentNotFound = Error.NotFound("600", "The specified attachment was not found.", "المرفق المحدد غير موجود.");
    public static readonly Error PhysicalFileAccessDenied = Error.Forbidden("10042", "You do not have access to this file.", "ليس لديك صلاحية الوصول إلى هذا الملف.");

    public static readonly Error FolderIconNotFound = Error.NotFound("10050", "The specified folder icon was not found.", "أيقونة المجلد المحددة غير موجودة.");
    public static readonly Error CannotDeleteDefaultFolderIcon = Error.Conflict("10051", "The default folder icon cannot be deleted.", "لا يمكن حذف أيقونة المجلد الافتراضية.");

    public static readonly Error FolderPermissionNotFound = Error.NotFound("10060", "The specified folder permission was not found.", "صلاحية المجلد المحددة غير موجودة.");
    public static readonly Error FolderPermissionAlreadyExists = Error.Conflict("10061", "A permission for this user on this folder already exists.", "توجد بالفعل صلاحية لهذا المستخدم على هذا المجلد.");
    public static readonly Error CannotRemoveOwnFolderPermission = Error.Conflict("10062", "You cannot remove your own folder permission.", "لا يمكنك إزالة صلاحية المجلد الخاصة بك.");
    public static readonly Error FolderAlreadyExists = Error.Conflict("10063", "A folder with this name already exists in the target location.", "يوجد بالفعل مجلد بنفس الاسم في الموقع المستهدف.");
    public static readonly Error FolderPermissionDepartmentOrUserRequired = Error.Validation("10064", "Either userId or departmentId must be provided.", "يجب تحديد مستخدم أو قسم.");
    public static readonly Error FolderPermissionDepartmentAlreadyExists = Error.Conflict("10065", "A permission for this department on this folder already exists.", "توجد بالفعل صلاحية لهذا القسم على هذا المجلد.");

    public static Error InvalidAttachmentType(List<string> rejectedFileNames) =>
        Error.Validation("601", $"The following file types are not allowed: {string.Join(", ", rejectedFileNames)}.", $"أنواع الملفات التالية غير مسموح بها: {string.Join(", ", rejectedFileNames)}.");
}
