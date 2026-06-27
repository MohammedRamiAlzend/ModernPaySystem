import { useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archiveDeletionRequestsService } from '../api/archiveDeletionRequestsService';
import { CreateDeleteArchiveRequestDto } from './types';

export const useSubmitDeletionRequest = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: (dto: CreateDeleteArchiveRequestDto) => archiveDeletionRequestsService.submitDeletionRequest(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.deletionRequests.all });
        }
    });
};

export const useApproveDeletionRequest = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, notes }: { id: string; notes?: string }) =>
            archiveDeletionRequestsService.approveDeletionRequest(id, notes),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.deletionRequests.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.deletionRequests.detail(variables.id) });
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folders.all });
        }
    });
};

export const useRejectDeletionRequest = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, reason }: { id: string; reason: string }) =>
            archiveDeletionRequestsService.rejectDeletionRequest(id, reason),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.deletionRequests.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.deletionRequests.detail(variables.id) });
        }
    });
};
