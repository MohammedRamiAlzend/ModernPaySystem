import { useQuery } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archiveDeletionRequestsService } from '../api/archiveDeletionRequestsService';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';

export const usePendingDeletionRequests = (departmentId: string | null, page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: queryKeys.archiving.deletionRequests.listPending(departmentId || '', page, pageSize),
        queryFn: () => archiveDeletionRequestsService.getPendingForDepartment(departmentId!, page, pageSize),
        enabled: !!departmentId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE],
    });
};

export const useDeletionRequest = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.deletionRequests.detail(id),
        queryFn: () => archiveDeletionRequestsService.getDeletionRequestById(id!),
        enabled: !!id,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE],
    });
};
