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
