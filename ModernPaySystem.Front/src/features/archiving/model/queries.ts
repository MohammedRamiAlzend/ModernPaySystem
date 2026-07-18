import { useQuery, useInfiniteQuery } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import { createDirectReportQuery } from '@/shared/lib/query-factory';
import { archivingService } from '../api/archivingService';

// ---------------------------------------------------------
// Folders Queries
// ---------------------------------------------------------
export const useFolders = () => {
    return useQuery({
        queryKey: queryKeys.archiving.folders.lists(),
        queryFn: () => archivingService.getAllFolders(),
    });
};

export const useFolder = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.folders.detail(id),
        queryFn: () => archivingService.getFolderById(id!),
        enabled: !!id,
    });
};

// ---------------------------------------------------------
// Archive Records Queries
// ---------------------------------------------------------
export const useArchiveRecords = (folderId: string | null, page = 1, pageSize = 10) => {
    return useQuery({
        queryKey: queryKeys.archiving.records.list(folderId || '', page, pageSize),
        queryFn: () => archivingService.getArchiveRecordsByFolder(folderId!, page, pageSize),
        enabled: !!folderId,
    });
};

export const useArchiveRecord = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.records.detail(id),
        queryFn: () => archivingService.getArchiveRecordById(id!),
        enabled: !!id,
    });
};

// ---------------------------------------------------------
// Dynamic Form Templates Queries
// ---------------------------------------------------------
export const useDynamicFormsPaged = (page = 1, pageSize = 10) => {
    return useQuery({
        queryKey: queryKeys.archiving.dynamicForms.list(page, pageSize),
        queryFn: () => archivingService.getDynamicFormsPaged(page, pageSize),
    });
};

export const useInfiniteDynamicForms = (pageSize = 10) => {
    return useInfiniteQuery({
        queryKey: [...queryKeys.archiving.dynamicForms.lists(), 'infinite', pageSize] as const,
        queryFn: ({ pageParam = 1 }) => archivingService.getDynamicFormsPaged(pageParam, pageSize),
        initialPageParam: 1,
        getNextPageParam: (lastPage, allPages) => {
            const loadedCount = allPages.reduce((sum, p) => sum + p.items.length, 0);
            return loadedCount < lastPage.totalItems ? allPages.length + 1 : undefined;
        }
    });
};

export const useAllDynamicForms = () => {
    return useQuery({
        queryKey: [...queryKeys.archiving.dynamicForms.all, 'all'] as const,
        queryFn: () => archivingService.getAllDynamicForms(),
    });
};

export const useDynamicForm = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.dynamicForms.detail(id),
        queryFn: () => archivingService.getDynamicFormById(id!),
        enabled: !!id,
    });
};

// ---------------------------------------------------------
// Audit Logs Queries
// ---------------------------------------------------------
export const useArchiveAuditLogs = (params: {
    page?: number;
    pageSize?: number;
    action?: number | null;
    fromDate?: string | null;
    toDate?: string | null;
    departmentId?: string | null;
}) => {
    return useQuery({
        queryKey: queryKeys.archiving.auditLogs.list(params),
        queryFn: () => archivingService.getAuditLogs(params),
        enabled: !!params.departmentId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useDailyWorkReport = createDirectReportQuery(
    (date?: string | null) => archivingService.getDailyWorkReport(date ?? undefined),
    (date?: string | null) => queryKeys.archiving.dailyWork.detail(date),
);

export const useArchiveConfig = () => {
    return useQuery({
        queryKey: queryKeys.archiving.config.all,
        queryFn: () => archivingService.getArchiveConfig(),
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

// ---------------------------------------------------------
// Folder Icons Queries
// ---------------------------------------------------------
export const useFolderIcons = () => {
    return useQuery({
        queryKey: queryKeys.archiving.folderIcons.lists(),
        queryFn: () => archivingService.getAllFolderIcons(),
    });
};

export const useFolderIcon = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.folderIcons.detail(id),
        queryFn: () => archivingService.getFolderIconById(id!),
        enabled: !!id,
    });
};

export const useLedDepartments = () => {
    return useQuery({
        queryKey: queryKeys.archiving.ledDepartments.all,
        queryFn: () => archivingService.getLedDepartments(),
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useSystemDrives = () => {
    return useQuery({
        queryKey: [...queryKeys.archiving.config.all, 'drives'] as const,
        queryFn: () => archivingService.getSystemDrives(),
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useSubdirectories = (path: string) => {
    return useQuery({
        queryKey: [...queryKeys.archiving.config.all, 'subdirs', path] as const,
        queryFn: () => archivingService.getSubdirectories(path),
        enabled: !!path,
        ...QUERY_STRATEGIES[UpdateStrategy.CRITICAL]
    });
};

// ---------------------------------------------------------
// Archive Report Queries
// ---------------------------------------------------------
export const useMyDepartments = () => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.myDepartments(),
        queryFn: () => archivingService.getMyDepartments(),
    });
};

export const useDepartmentDashboard = createDirectReportQuery(
    () => archivingService.getDepartmentDashboard(),
    () => queryKeys.archiving.reports.dashboard(),
);

export const useDailyReport = createDirectReportQuery(
    (date?: string | null) => archivingService.getDailyReport(date ?? undefined),
    (date?: string | null) => queryKeys.archiving.reports.daily(date),
);

export const useWeeklyReport = createDirectReportQuery(
    (weekStart?: string | null) => archivingService.getWeeklyReport(weekStart ?? undefined),
    (weekStart?: string | null) => queryKeys.archiving.reports.weekly(weekStart),
);

export const useMonthlyReport = createDirectReportQuery(
    (year?: number | null, month?: number | null) => archivingService.getMonthlyReport(year ?? undefined, month ?? undefined),
    (year?: number | null, month?: number | null) => queryKeys.archiving.reports.monthly(year, month),
);

export const useUserActivityReport = createDirectReportQuery(
    (fromDate?: string | null, toDate?: string | null) => archivingService.getUserActivityReport(fromDate ?? undefined, toDate ?? undefined),
    (fromDate?: string | null, toDate?: string | null) => queryKeys.archiving.reports.userActivity(fromDate, toDate),
);

export const useActiveUsersReport = createDirectReportQuery(
    (fromDate?: string | null, toDate?: string | null) => archivingService.getActiveUsers(fromDate ?? undefined, toDate ?? undefined),
    (fromDate?: string | null, toDate?: string | null) => queryKeys.archiving.reports.activeUsers(fromDate, toDate),
);

export const useStorageReport = createDirectReportQuery(
    () => archivingService.getStorageReport(),
    () => queryKeys.archiving.reports.storage(),
);

export const useChartsData = createDirectReportQuery(
    (fromDate?: string | null, toDate?: string | null) => archivingService.getChartsData(fromDate ?? undefined, toDate ?? undefined),
    (fromDate?: string | null, toDate?: string | null) => queryKeys.archiving.reports.charts(fromDate, toDate),
);
