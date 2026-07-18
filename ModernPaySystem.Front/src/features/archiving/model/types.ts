export interface FolderIcon {
    id: string;
    name: string;
    svgContent: string;
    isDefault: boolean;
    createdByUserId?: string | null;
    createdAt?: string | null;
}

export interface CreateFolderIconDto {
    name: string;
    svgContent: string;
    isDefault: boolean;
}

export interface AssignFolderIconDto {
    folderId: string;
    iconId: string | null;
}

export interface Folder {
    id: string;
    name: string;
    level: number;
    defaultStoragePath?: string | null;
    parentId: string | null;
    iconId?: string | null;
    folderDtos: Folder[];
    departmentId?: string | null;
    departmentName?: string | null;
    createdByUserId?: string | null;
    createdAt?: string | null;
    updatedByUserId?: string | null;
    updatedAt?: string | null;
    canManagePermissions?: boolean;
    canEdit?: boolean;
}

export interface PhysicalFile {
    id: string;
    archiveRecordId: string;
    fileName: string;
    fileExtension: string;
    storagePath: string;
    fileSize: number;
    contentType: string;
    isDeleted: boolean;
    isQrPage?: boolean;
    deletedAt?: string | null;
    createdByUserId?: string | null;
    createdAt?: string | null;
}

export interface ArchiveRecordFormInputValue {
    key: string;
    value: string | null;
}

export interface ArchiveRecordTemplateValues {
    id: string;
    archiveRecordId: string;
    archiveFormTemplateId: string;
    archiveRecordFormInputValues: ArchiveRecordFormInputValue[];
}

export interface ArchiveRecord {
    id: string;
    name?: string | null;
    folderId: string;
    formId: string | null;
    departmentId?: string | null;
    departmentName?: string | null;
    archiveRecordTemplateValues?: ArchiveRecordTemplateValues | null;
    physicalFiles: PhysicalFile[];
    createdByUserId?: string | null;
    createdAt?: string | null;
    updatedByUserId?: string | null;
    updatedAt?: string | null;
}

export interface DynamicFormTemplate {
    id: string;
    templateFormName: string;
    contentAsJson: string;
    createdByUserId?: string | null;
    createdAt?: string | null;
}

export interface InitialFolderPermissionDto {
    userId: string;
    accessLevel: number;
}

export interface CreateFolderDto {
    name: string;
    defaultStoragePath?: string | null;
    parentId: string | null;
    initialPermissions?: InitialFolderPermissionDto[];
}

export interface CreateArchiveRecordDto {
    id?: string;
    name?: string;
    folderId: string;
    formId: string | null;
    files: File[];
    content: ArchiveRecordFormInputValue[];
}

export interface MoveArchiveRecordDto {
    destinationFolderId: string;
}

export interface CreateDynamicFormTemplateDto {
    templateFormName: string;
    contentAsJson: string;
}

export interface UpdateDynamicFormTemplateDto {
    templateFormName: string;
    contentAsJson: string;
}

// Semantic Search
export interface SemanticSearchRequest {
    query: string;
    topK: number;
    minScore: number;
    sourceType: number | null;
    archiveRecordId: string | null;
    folderId: string | null;
}

export interface FolderPermissionDto {
    id: string;
    folderId: string;
    userId: string | null;
    userName?: string | null;
    departmentId: string | null;
    departmentName?: string | null;
    accessLevel: number;
    isInherited: boolean;
    createdByUserId?: string | null;
    createdAt?: string | null;
    updatedByUserId?: string | null;
    updatedAt?: string | null;
}

export interface CreateFolderPermissionDto {
    folderId: string;
    userId?: string | null;
    departmentId?: string | null;
    accessLevel: number;
    isInherited: boolean;
}

export interface BulkCreateFolderPermissionDto {
    folderIds: string[];
    userId?: string | null;
    departmentId?: string | null;
    accessLevel: number;
    isInherited: boolean;
}

export interface SubFolderTreeNodeDto {
    id: string;
    name: string;
    level: number;
    children: SubFolderTreeNodeDto[];
}

export interface SemanticSearchResultItem {
    documentId: string;
    chunkId: string;
    sourceType: number;
    physicalFileId: string;
    archiveRecordId: string;
    archiveRecordNumber: string;
    fileName: string;
    chunkIndex: number;
    content: string;
    score: number;
}

// Audit Logs
export enum AuditAction {
    View = 1,
    Update = 2,
    Download = 3,
    Print = 4,
    Create = 5,
    Delete = 6,
    AddFiles = 7,
    RemoveFiles = 8,
    ApproveEdit = 9,
    RejectEdit = 10,
    ApproveDelete = 11,
    RejectDelete = 12,
    SubmitEditRequest = 13,
    SubmitDeleteRequest = 14,
    Move = 15,
}

export const AUDIT_ACTION_LABELS: Record<AuditAction, string> = {
    [AuditAction.View]: 'عرض',
    [AuditAction.Update]: 'تعديل',
    [AuditAction.Download]: 'تنزيل',
    [AuditAction.Print]: 'طباعة',
    [AuditAction.Create]: 'إنشاء',
    [AuditAction.Delete]: 'حذف',
    [AuditAction.AddFiles]: 'إضافة ملفات',
    [AuditAction.RemoveFiles]: 'حذف ملفات',
    [AuditAction.ApproveEdit]: 'الموافقة على تعديل',
    [AuditAction.RejectEdit]: 'رفض تعديل',
    [AuditAction.ApproveDelete]: 'الموافقة على حذف',
    [AuditAction.RejectDelete]: 'رفض حذف',
    [AuditAction.SubmitEditRequest]: 'طلب تعديل',
    [AuditAction.SubmitDeleteRequest]: 'طلب حذف',
    [AuditAction.Move]: 'نقل',
};

// ---------------------------------------------------------
// Archive Report Types
// ---------------------------------------------------------
export interface DepartmentArchiveDashboard {
    departmentId: string;
    departmentName: string;
    totalArchiveRecords: number;
    totalUsers: number;
    totalFolders: number;
    totalPhysicalFiles: number;
    totalStorageBytes: number;
    recordsCreatedToday: number;
    recordsCreatedThisWeek: number;
    recordsCreatedThisMonth: number;
    activeUsersToday: number;
    activeUsersThisWeek: number;
    activeUsersThisMonth: number;
    actionTypeBreakdown: Record<string, number>;
}

export interface HourlyBreakdown {
    hour: number;
    recordsCreated: number;
    actions: number;
}

export interface ArchiveDailyReport {
    date: string;
    recordsCreated: number;
    recordsDeleted: number;
    filesAdded: number;
    filesDownloaded: number;
    printActions: number;
    views: number;
    activeUsers: number;
    hourlyBreakdown: HourlyBreakdown[];
}

export interface DailyBreakdownItem {
    date: string;
    recordsCreated: number;
    actions: number;
    activeUsers: number;
}

export interface UserActivitySummary {
    userId: string;
    userName: string;
    recordsCreated: number;
    recordsViewed: number;
    filesDownloaded: number;
    printActions: number;
    totalActions: number;
    lastActivityDate: string | null;
}

export interface ArchivePeriodReport {
    periodStart: string;
    periodEnd: string;
    periodLabel: string;
    totalRecordsCreated: number;
    totalRecordsDeleted: number;
    totalFilesAdded: number;
    totalDownloads: number;
    totalPrints: number;
    totalViews: number;
    uniqueActiveUsers: number;
    dailyBreakdown: DailyBreakdownItem[];
    topUsers: UserActivitySummary[];
}

export interface UserActivityReportItem {
    userId: string;
    userName: string;
    recordsCreated: number;
    recordsViewed: number;
    filesDownloaded: number;
    printActions: number;
    totalActions: number;
    lastActivityDate: string | null;
}

export interface ActiveUserReportItem {
    userId: string;
    userName: string;
    departmentName: string | null;
    totalActions: number;
    lastActionDate: string | null;
    firstActionDate: string | null;
    actionsPerformed: string[];
}

export interface StoragePerUser {
    userId: string;
    userName: string;
    totalFiles: number;
    totalBytes: number;
    percentageOfTotal: number;
    fileTypeCounts: Record<string, number>;
    lastFileAddedAt: string | null;
}

export interface StoragePerType {
    extension: string;
    count: number;
    totalBytes: number;
    percentageOfTotal: number;
}

export interface StorageConsumptionReport {
    totalStorageBytes: number;
    totalFiles: number;
    perUser: StoragePerUser[];
    fileTypeBreakdown: StoragePerType[];
}

export interface ChartDataPoint {
    label: string;
    value: number;
    color: string | null;
}

export interface DepartmentChartsData {
    dailyActivity: ChartDataPoint[];
    actionTypeBreakdown: ChartDataPoint[];
    hourlyDistribution: ChartDataPoint[];
    topActiveUsers: ChartDataPoint[];
    topStorageUsers: ChartDataPoint[];
    trend7Days: ChartDataPoint[];
}

export interface ReportFiltersState {
    departmentId: string | null;
    fromDate: string | null;
    toDate: string | null;
    date: string | null;
    weekStart: string | null;
    year: number | null;
    month: number | null;
}

export type ReportTab = 'dashboard' | 'daily' | 'weekly' | 'monthly' | 'user-activity' | 'active-users' | 'storage' | 'charts';

// ---------------------------------------------------------
// Daily Work Report Types
// ---------------------------------------------------------
export interface DailyWorkReportDto {
    date: string;
    departmentName: string;
    auditLogs: DailyWorkAuditLogItemDto[];
    archiveRecords: DailyWorkArchiveRecordItemDto[];
}

export interface DailyWorkAuditLogItemDto {
    id: string;
    archiveRecordId: string;
    userName: string;
    action: string;
    details: string | null;
    timestamp: string;
}

export interface DailyWorkArchiveRecordItemDto {
    id: string;
    folderPath: string;
    formName: string | null;
    departmentName: string | null;
    createdByUserName: string | null;
    createdAt: string;
    updatedAt: string | null;
    formValues: DailyWorkFormValueItemDto[];
}

export interface DailyWorkFormValueItemDto {
    key: string;
    value: string | null;
}

// ---------------------------------------------------------
// Archive Config Types
// ---------------------------------------------------------
export interface ArchiveConfigDto {
    id: string;
    defaultPath: string;
    description: string | null;
    isActive: boolean;
    allowedFileExtensions?: string | null;
}

export interface UpdateArchiveConfigDto {
    defaultPath?: string;
    description?: string | null;
    isActive?: boolean;
    allowedFileExtensions?: string | null;
}

export interface ArchiveAuditLog {
    id: string;
    archiveRecordId: string;
    userId: string;
    action: AuditAction;
    details: string | null;
    ipAddress: string | null;
    userAgent: string | null;
    timestamp: string;
    createdByUserId?: string | null;
    createdAt?: string | null;
}
