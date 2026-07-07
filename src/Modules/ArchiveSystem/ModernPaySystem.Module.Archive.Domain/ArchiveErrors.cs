using ModernPaySystem.SharedKernel.Domain.Commons;

namespace ModernPaySystem.Module.Archive.Domain;

public static class ArchiveErrors
{
    public static readonly Error InvalidInput = Error.Validation("700", "The provided input is invalid.");
    public static readonly Error InternalServerError = Error.Failure("800", "An unexpected error occurred.");
    public static readonly Error DatabaseError = Error.Failure("801", "A database error occurred.");

    public static readonly Error FolderNotFound = Error.NotFound("10000", "The specified folder was not found.");
    public static readonly Error FolderHasChildren = Error.Conflict("10002", "The folder contains child folders or records and cannot be deleted.");
    public static readonly Error FolderDepartmentNotConfigured = Error.Validation("10003", "The folder is not scoped to a department.");
    public static readonly Error FolderArchiveLeaderRequired = Error.Forbidden("10004", "Only a department archive leader can delete this folder directly.");
    public static readonly Error FolderDeleteRequestExists = Error.Conflict("10005", "A pending delete request already exists for this folder.");
    public static readonly Error FolderAccessDenied = Error.Forbidden("10040", "You do not have access to this folder.");
    public static readonly Error UserNotEnrolledInDepartment = Error.Forbidden("10043", "You are not enrolled in any department.");

    public static readonly Error DynamicFormNotFound = Error.NotFound("10010", "The specified form was not found.");
    public static readonly Error DynamicFormAlreadyExists = Error.Conflict("10011", "A form with this name already exists.");
    public static readonly Error InvalidJsonDefinition = Error.Validation("10012", "The provided JSON definition is invalid.");
    public static readonly Error DynamicFormInUse = Error.Conflict("10013", "The form is currently in use and cannot be deleted.");

    public static readonly Error ArchiveRecordNotFound = Error.NotFound("10020", "The specified archive record was not found.");
    public static readonly Error ArchiveRecordDepartmentNotConfigured = Error.Validation("10027", "The archive record is not scoped to a department.");
    public static readonly Error ArchiveRecordArchiveLeaderRequired = Error.Forbidden("10028", "Only a department archive leader can delete this archive record directly.");
    public static readonly Error ArchiveRecordAccessDenied = Error.Forbidden("10041", "You do not have access to this archive record.");
    public static readonly Error ArchiveRecordHasNoFiles = Error.NotFound("10024", "The specified archive record does not contain any files.");
    public static readonly Error QrPageAlreadyExists = Error.Conflict("10038", "A QR cover page already exists for this archive record.");
    public static readonly Error ArchiveRecordFileDeletionNotBelongToRecord = Error.Validation("10039", "One or more files selected for deletion do not belong to this archive record.");

    public static readonly Error DeleteRequestNotFound = Error.NotFound("10029", "The specified delete request was not found.");
    public static readonly Error DeleteRequestAlreadyHandled = Error.Conflict("10030", "The delete request has already been processed.");
    public static readonly Error DeleteRequestApprovalRequiresDepartmentHead = Error.Forbidden("10031", "Only the department head can approve this delete request.");
    public static readonly Error DeleteRequestRejectionRequiresReason = Error.Validation("10032", "A rejection reason is required.");
    public static readonly Error DeleteRequestTargetNotFound = Error.NotFound("10033", "The target item for the delete request was not found.");

    public static readonly Error EditRequestNotFound = Error.NotFound("10036", "The specified edit request was not found.");
    public static readonly Error EditRequestAlreadyHandled = Error.Conflict("10037", "The edit request has already been processed.");

    public static readonly Error DepartmentArchiveLeaderNotAssigned = Error.Validation("10034", "The department has no active archive leader assigned.");
    public static readonly Error DepartmentHeadMissing = Error.Validation("10035", "The department does not have a department head assigned.");

    public static readonly Error AttachmentNotFound = Error.NotFound("600", "The specified attachment was not found.");
    public static readonly Error PhysicalFileAccessDenied = Error.Forbidden("10042", "You do not have access to this file.");

    public static readonly Error FolderIconNotFound = Error.NotFound("10050", "The specified folder icon was not found.");
    public static readonly Error CannotDeleteDefaultFolderIcon = Error.Conflict("10051", "The default folder icon cannot be deleted.");

    public static readonly Error FolderPermissionNotFound = Error.NotFound("10060", "The specified folder permission was not found.");
    public static readonly Error FolderPermissionAlreadyExists = Error.Conflict("10061", "A permission for this user on this folder already exists.");
    public static readonly Error CannotRemoveOwnFolderPermission = Error.Conflict("10062", "You cannot remove your own folder permission.");
    public static readonly Error FolderAlreadyExists = Error.Conflict("10063", "A folder with this name already exists in the target location.");

    public static Error InvalidAttachmentType(List<string> rejectedFileNames) =>
        Error.Validation("601", $"The following file types are not allowed: {string.Join(", ", rejectedFileNames)}.");
}
