import api from '@/shared/api/baseApi';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

export interface LookUpField {
    id: string;
    filedName: string;
}

export interface LookUpFieldValue {
    id: string;
    lookUpFiledId: string;
    desc: string;
}

// LookUp Fields
export const fetchLookUpFields = async (): Promise<LookUpField[]> => {
    const response = await api.get<{ data: LookUpField[] }>('/LookUpFields');
    return response.data.data;
};

export const createLookUpField = async (filedName: string): Promise<LookUpField> => {
    const response = await api.post<LookUpField>('/LookUpFields', { filedName });
    return response.data;
};

export const updateLookUpField = async ({ id, filedName }: { id: string, filedName: string }): Promise<LookUpField> => {
    const response = await api.put<LookUpField>(`/LookUpFields/${id}`, { filedName });
    return response.data;
};

export const deleteLookUpField = async (id: string): Promise<void> => {
    await api.delete(`/LookUpFields/${id}`);
};

// LookUp Field Values
export const fetchLookUpFieldValues = async (lookUpFieldId: string): Promise<LookUpFieldValue[]> => {
    const response = await api.get<{ data: LookUpFieldValue[] }>(`/LookUpFiledValues/by-lookup-field/${lookUpFieldId}`);
    return response.data.data;
};

export const createLookUpFieldValue = async ({ lookUpFiledId, desc }: { lookUpFiledId: string, desc: string }): Promise<LookUpFieldValue> => {
    const response = await api.post<LookUpFieldValue>('/LookUpFiledValues', { lookUpFiledId, desc });
    return response.data;
};

export const updateLookUpFieldValue = async ({ id, lookUpFiledId, desc }: { id: string, lookUpFiledId: string, desc: string }): Promise<LookUpFieldValue> => {
    const response = await api.put<LookUpFieldValue>(`/LookUpFiledValues/${id}`, { lookUpFiledId, desc });
    return response.data;
};

export const deleteLookUpFieldValue = async (id: string): Promise<void> => {
    await api.delete(`/LookUpFiledValues/${id}`);
};

// Hooks
export const useLookUpFields = () => {
    return useQuery({
        queryKey: ['lookUpFields'],
        queryFn: fetchLookUpFields,
    });
};

export const useLookUpFieldValues = (lookUpFieldId: string | null) => {
    return useQuery({
        queryKey: ['lookUpFieldValues', lookUpFieldId],
        queryFn: () => fetchLookUpFieldValues(lookUpFieldId!),
        enabled: !!lookUpFieldId,
    });
};

export const useLookUpMutations = () => {
    const queryClient = useQueryClient();

    const createFieldMutation = useMutation({
        mutationFn: createLookUpField,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookUpFields'] }),
    });

    const updateFieldMutation = useMutation({
        mutationFn: updateLookUpField,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookUpFields'] }),
    });

    const deleteFieldMutation = useMutation({
        mutationFn: deleteLookUpField,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookUpFields'] }),
    });

    const createValueMutation = useMutation({
        mutationFn: createLookUpFieldValue,
        onSuccess: (_, variables) => queryClient.invalidateQueries({ queryKey: ['lookUpFieldValues', variables.lookUpFiledId] }),
    });

    const updateValueMutation = useMutation({
        mutationFn: updateLookUpFieldValue,
        onSuccess: (_, variables) => queryClient.invalidateQueries({ queryKey: ['lookUpFieldValues', variables.lookUpFiledId] }),
    });

    const deleteValueMutation = useMutation({
        mutationFn: deleteLookUpFieldValue,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['lookUpFieldValues'] }),
    });

    return {
        createField: createFieldMutation,
        updateField: updateFieldMutation,
        deleteField: deleteFieldMutation,
        createValue: createValueMutation,
        updateValue: updateValueMutation,
        deleteValue: deleteValueMutation,
    };
};
