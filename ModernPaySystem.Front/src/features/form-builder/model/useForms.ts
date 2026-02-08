import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { formApi } from '@/entities/form/api/formApi';
import { formService } from '../api/formService';
import type { FormSchema } from '@/entities/form/model/types';

/**
 * Hook for fetching forms - Read only (delegated to Entity layer)
 */
export const useForms = () => {
    return useQuery({
        queryKey: ['forms'],
        queryFn: () => formApi.getForms(),
    });
};

/**
 * Hook for deleting a form - Action (delegated to Feature layer)
 */
export const useDeleteForm = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => formService.deleteForm(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['forms'] });
        },
    });
};

/**
 * Hook for saving a form - Action (delegated to Feature layer)
 */
export const useSaveForm = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (form: FormSchema) => formService.saveForm(form),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['forms'] });
        },
    });
};
