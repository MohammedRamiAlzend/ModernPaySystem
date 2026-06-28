export interface TransactionDashboard {
    totalRequests: number;
    pending: number;
    inProcess: number;
    managed: number;
    delivered: number;
    totalResponses: number;
    totalAttachments: number;
    requestsToday: number;
    requestsThisWeek: number;
    requestsThisMonth: number;
    responsesToday: number;
    responsesThisWeek: number;
    responsesThisMonth: number;
    activeUsersToday: number;
    activeUsersThisWeek: number;
    activeUsersThisMonth: number;
    statusBreakdown: Record<string, number>;
}

export interface TransactionDailyReport {
    date: string;
    requestsCreated: number;
    responsesMade: number;
    attachmentsAdded: number;
    views: number;
    activeUsers: number;
    hourlyBreakdown: HourlyBreakdown[];
}

export interface HourlyBreakdown {
    hour: number;
    recordsCreated: number;
    actions: number;
}

export interface TransactionPeriodReport {
    periodStart: string;
    periodEnd: string;
    periodLabel: string;
    totalRequestsCreated: number;
    totalResponsesMade: number;
    totalAttachmentsAdded: number;
    totalViews: number;
    uniqueActiveUsers: number;
    dailyBreakdown: DailyBreakdownItem[];
    topUsers: TransactionUserSummary[];
}

export interface DailyBreakdownItem {
    date: string;
    recordsCreated: number;
    actions: number;
    activeUsers: number;
}

export interface TransactionUserSummary {
    userId: string;
    userName: string;
    requestsCreated: number;
    responsesMade: number;
    totalActions: number;
}

export interface TransactionUserActivityItem {
    userId: string;
    userName: string;
    departmentName: string | null;
    requestsCreated: number;
    responsesMade: number;
    attachmentsAdded: number;
    totalActions: number;
    lastActivityDate: string | null;
}

export interface TransactionActiveUserItem {
    userId: string;
    userName: string;
    departmentName: string | null;
    totalActions: number;
    firstActionDate: string | null;
    lastActionDate: string | null;
    actionsPerformed: string[];
}

export interface TransactionStorageReport {
    totalStorageBytes: number;
    totalFiles: number;
    perUser: TransactionStoragePerUser[];
    fileTypeBreakdown: StoragePerType[];
}

export interface TransactionStoragePerUser {
    userId: string;
    userName: string;
    totalFiles: number;
    totalBytes: number;
    percentageOfTotal: number;
    lastFileAddedAt: string | null;
}

export interface StoragePerType {
    extension: string;
    count: number;
    totalBytes: number;
    percentageOfTotal: number;
}

export interface ChartDataPoint {
    label: string;
    value: number;
    color: string | null;
}

export interface TransactionChartsData {
    dailyActivity: ChartDataPoint[];
    actionTypeBreakdown: ChartDataPoint[];
    hourlyDistribution: ChartDataPoint[];
    topActiveUsers: ChartDataPoint[];
    topStorageUsers: ChartDataPoint[];
    trend7Days: ChartDataPoint[];
}

export interface TransactionDailyWork {
    date: string;
    departmentName: string | null;
    auditLogs: TransactionDailyWorkAuditLogItem[];
    requests: TransactionDailyWorkRequestItem[];
}

export interface TransactionDailyWorkAuditLogItem {
    id: string;
    requestId: string;
    requestNumber: number | null;
    userName: string;
    action: string;
    details: string | null;
    timestamp: string;
}

export interface TransactionDailyWorkRequestItem {
    id: string;
    requestNumber: number;
    templateName: string | null;
    requesterName: string | null;
    status: number;
    createdAt: string;
    updatedAt: string | null;
    formValues: DailyWorkFormValueItem[];
}

export interface DailyWorkFormValueItem {
    key: string;
    value: string | null;
}
