import { useQuery } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archiveEditRequestsService } from '../api/archiveEditRequestsService';

export const usePendingEditRequests = (departmentId: string | null, page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: queryKeys.archiving.editRequests.listPending(departmentId || '', page, pageSize),
        queryFn: () => archiveEditRequestsService.getPendingForDepartment(departmentId!, page, pageSize),
        enabled: !!departmentId,
    });
};

export const useMyEditRequests = (page = 1, pageSize = 20) => {
    return useQuery({
        queryKey: queryKeys.archiving.editRequests.listMy(page, pageSize),
        queryFn: () => archiveEditRequestsService.getMyEditRequests(page, pageSize),
    });
};

export const useEditRequest = (id: string | null) => {
    return useQuery({
        queryKey: queryKeys.archiving.editRequests.detail(id),
        queryFn: () => archiveEditRequestsService.getEditRequestById(id!),
        enabled: !!id,
    });
};
