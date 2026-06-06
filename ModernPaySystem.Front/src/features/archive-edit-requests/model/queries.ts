import { useQuery } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archiveEditRequestsService } from '../api/archiveEditRequestsService';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';

export const usePendingEditRequests = (departmentId: string | null, page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: queryKeys.archiving.editRequests.listPending(departmentId || '', page, pageSize),
        queryFn: () => archiveEditRequestsService.getPendingForDepartment(departmentId!, page, pageSize),
        enabled: !!departmentId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE],
    });
};

export const useMyEditRequests = (page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: queryKeys.archiving.editRequests.listMy(page, pageSize),
        queryFn: () => archiveEditRequestsService.getMyEditRequests(page, pageSize),
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE],
    });
};

export const useEditRequest = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.editRequests.detail(id),
        queryFn: () => archiveEditRequestsService.getEditRequestById(id!),
        enabled: !!id,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE],
    });
};
