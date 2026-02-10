
import api from '@/shared/api/baseApi';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import type {
    Template,
    CreateTemplateDto,
    CreateRequestDto,
    CreateResponseDto,
    TemplateRequest
} from '@/entities/form/model/types';

// --- API Service ---

export const formEndpoints = {
    // Templates
    createTemplate: async (data: CreateTemplateDto): Promise<{ data: Template }> => {
        const response = await api.post('/Templates', data);
        return response.data;
    },

    getTemplates: async (): Promise<Template[] | { data: Template[] }> => {
        // User said: "For display, use endpoint: Templates of type post"
        const response = await api.get('/Templates', {});
        return response.data;
    },

    // Requests
    createRequest: async (data: CreateRequestDto): Promise<any> => {
        const formData = new FormData();
        formData.append('TemplateId', data.TemplateId);
        formData.append('RequesterId', data.RequesterId);
        if (data.ApproverId) formData.append('ApproverId', data.ApproverId);
        formData.append('Content', data.Content);

        if (data.files && data.files.length > 0) {
            data.files.forEach((file) => {
                formData.append('files', file);
            });
        }

        const response = await api.post('/Requests', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },

    getRequests: async (): Promise<{ data: TemplateRequest[] }> => {
        const response = await api.get('/Requests');
        return response.data;
    },

    // Responses
    createResponse: async (data: CreateResponseDto): Promise<any> => {
        const response = await api.post('/Responses', data);
        return response.data;
    }
};


// --- Hooks ---

export const useRequests = () => {
    return useQuery({
        queryKey: ['requests'],
        queryFn: async () => {
            const res = await formEndpoints.getRequests();
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useTemplates = () => {
    return useQuery({
        queryKey: ['templates'],
        queryFn: async () => {
            const res = await formEndpoints.getTemplates();
            // Handle if data is array or single object or { data: [] }
            let data: any = res;
            if (res && 'data' in res && Array.isArray((res as any).data)) {
                data = (res as any).data;
            }

            if (Array.isArray(data)) return data as Template[];
            return [data] as Template[];
        },
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useCreateTemplate = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: formEndpoints.createTemplate,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['templates'] });
        }
    });
};

export const useCreateRequest = () => {
    return useMutation({
        mutationFn: formEndpoints.createRequest
    });
};

export const useCreateResponse = () => {
    return useMutation({
        mutationFn: formEndpoints.createResponse
    });
};
