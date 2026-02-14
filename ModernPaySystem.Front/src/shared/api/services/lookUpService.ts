import api from '../baseApi';

export interface LookUpField {
    id: string;
    filedName: string;
}

export interface LookUpFieldValue {
    id: string;
    lookUpFiledId: string;
    desc: string;
}

export const lookUpService = {
    // LookUp Fields
    getLookUpFields: async () => {
        const response = await api.get<{ data: LookUpField[] }>('/LookUpFields');
        return response.data;
    },

    createLookUpField: async (filedName: string) => {
        const response = await api.post<LookUpField>('/LookUpFields', { filedName });
        return response.data;
    },

    updateLookUpField: async (id: string, filedName: string) => {
        const response = await api.put<LookUpField>(`/LookUpFields/${id}`, { filedName });
        return response.data;
    },

    deleteLookUpField: async (id: string) => {
        const response = await api.delete(`/LookUpFields/${id}`);
        return response.data;
    },

    // LookUp Field Values
    getLookUpFieldValues: async (lookUpFieldId: string) => {
        const response = await api.get<{ data: LookUpFieldValue[] }>(`/LookUpFiledValues/by-lookup-field/${lookUpFieldId}`);
        return response.data;
    },

    createLookUpFieldValue: async (lookUpFiledId: string, desc: string) => {
        const response = await api.post<LookUpFieldValue>('/LookUpFiledValues', { lookUpFiledId, desc });
        return response.data;
    },

    updateLookUpFieldValue: async (id: string, lookUpFiledId: string, desc: string) => {
        const response = await api.put<LookUpFieldValue>(`/LookUpFiledValues/${id}`, { lookUpFiledId, desc });
        return response.data;
    },

    deleteLookUpFieldValue: async (id: string) => {
        const response = await api.delete(`/LookUpFiledValues/${id}`);
        return response.data;
    },
};
