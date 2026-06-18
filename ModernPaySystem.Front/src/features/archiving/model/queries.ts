import { useQuery, useInfiniteQuery } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
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
    });
};

export const useArchiveConfig = () => {
    return useQuery({
        queryKey: queryKeys.archiving.config.all,
        queryFn: () => archivingService.getArchiveConfig(),
    });
};

export const useLedDepartments = () => {
    return useQuery({
        queryKey: queryKeys.archiving.ledDepartments.all,
        queryFn: () => archivingService.getLedDepartments(),
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

export const useDepartmentDashboard = () => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.dashboard(),
        queryFn: () => archivingService.getDepartmentDashboard(),
    });
};

export const useDailyReport = (date?: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.daily(date),
        queryFn: () => archivingService.getDailyReport(date),
    });
};

export const useWeeklyReport = (weekStart?: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.weekly(weekStart),
        queryFn: () => archivingService.getWeeklyReport(weekStart),
    });
};

export const useMonthlyReport = (year?: number | null, month?: number | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.monthly(year, month),
        queryFn: () => archivingService.getMonthlyReport(year, month),
    });
};

export const useUserActivityReport = (fromDate?: string | null, toDate?: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.userActivity(fromDate, toDate),
        queryFn: () => archivingService.getUserActivityReport(fromDate, toDate),
    });
};

export const useActiveUsersReport = (fromDate?: string | null, toDate?: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.activeUsers(fromDate, toDate),
        queryFn: () => archivingService.getActiveUsers(fromDate, toDate),
    });
};

export const useStorageReport = () => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.storage(),
        queryFn: () => archivingService.getStorageReport(),
    });
};

export const useChartsData = (fromDate?: string | null, toDate?: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.reports.charts(fromDate, toDate),
        queryFn: () => archivingService.getChartsData(fromDate, toDate),
    });
};
