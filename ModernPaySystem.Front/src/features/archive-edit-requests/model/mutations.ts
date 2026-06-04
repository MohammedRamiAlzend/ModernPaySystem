import { useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archiveEditRequestsService } from '../api/archiveEditRequestsService';
import { CreateEditArchiveRequestDto } from './types';

export const useSubmitEditRequest = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (dto: CreateEditArchiveRequestDto) => archiveEditRequestsService.submitEditRequest(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.editRequests.all });
        }
    });
};

export const useApproveEditRequest = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
            archiveEditRequestsService.approveEditRequest(id, notes),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.editRequests.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.editRequests.detail(variables.id) });
            // Also invalidate records since approval may modify records/files
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
        }
    });
};

export const useRejectEditRequest = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, reason }: { id: string; reason: string }) =>
            archiveEditRequestsService.rejectEditRequest(id, reason),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.editRequests.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.editRequests.detail(variables.id) });
        }
    });
};
