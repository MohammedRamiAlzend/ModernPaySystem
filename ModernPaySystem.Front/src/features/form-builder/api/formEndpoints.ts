
import api from '@/shared/api/baseApi';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import type {
    Template,
    CreateTemplateDto,
    CreateRequestDto,
    CreateResponseDto,
    TemplateRequest,
    TemplateResponse
} from '@/entities/form/model/types';

// --- API Service ---

export const formEndpoints = {
    // Templates 
    // هون استدعيت  متل العادة 
    createTemplate: async (data: CreateTemplateDto): Promise<{ data: Template }> => {
        const response = await api.post('/Templates', data);
        return response.data;
    },

    updateTemplate: async (id: string, data: CreateTemplateDto): Promise<{ data: Template }> => {
        const response = await api.put(`/Templates/${id}`, data);
        return response.data;
    },

    getTemplates: async (): Promise<Template[] | { data: Template[] }> => {
        // User said: "For display, use endpoint: Templates of type post"
        const response = await api.get('/Templates', {});
        return response.data;
    },

    // Requests
    createRequest: async (data: CreateRequestDto): Promise<{ data: TemplateRequest }> => {
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

    getRequestsByActionStatus: async (hasResponse: boolean): Promise<{ data: TemplateRequest[] }> => {
        const response = await api.get(`/Requests/GetAllRequestsNeedAction/${hasResponse}`);
        return response.data;
    },


    // Responses
    getResponsesByRequestId: async (requestId: string): Promise<{ data: TemplateResponse[] }> => {
        const response = await api.get(`/Responses/by-request/${requestId}`);
        return response.data;
    },

    createResponse: async (data: CreateResponseDto): Promise<{ data: TemplateResponse }> => {
        const formData = new FormData();
        if (data.comment) formData.append('comment', data.comment);
        formData.append('requestId', data.requestId);
        formData.append('respondedByUserId', data.respondedByUserId);

        if (data.files && data.files.length > 0) {
            data.files.forEach((file) => {
                formData.append('files', file);
            });
        }

        const response = await api.post('/Responses', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },

    // Attachments
    fetchRequestAttachmentsBlob: async (requestId: string): Promise<Blob> => {
        const response = await api.get(`/Attachments/request/${requestId}/download-all`, {
            responseType: 'blob',
        });
        return new Blob([response.data]);
    },

    downloadRequestAttachments: async (requestId: string): Promise<void> => {
        const blob = await formEndpoints.fetchRequestAttachmentsBlob(requestId);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `request_${requestId}_attachments.zip`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    },

    fetchResponseAttachmentsBlob: async (responseId: string): Promise<Blob> => {
        const response = await api.get(`/Attachments/response/${responseId}/download-all`, {
            responseType: 'blob',
        });
        return new Blob([response.data]);
    },

    downloadResponseAttachments: async (responseId: string): Promise<void> => {
        const blob = await formEndpoints.fetchResponseAttachmentsBlob(responseId);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `response_${responseId}_attachments.zip`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    }
};


// --- Hooks ---

export const useRequests = (hasResponse: boolean = false) => {
    return useQuery({
        queryKey: ['requests', hasResponse],
        queryFn: async () => {
            const res = await formEndpoints.getRequestsByActionStatus(hasResponse);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useTemplates = () => {
    return useQuery({
        queryKey: ['templates'],
        queryFn: async (): Promise<Template[]> => {
            const res = await formEndpoints.getTemplates();
            // Handle if data is wrapped in { data: [] }
            if (res && !Array.isArray(res) && 'data' in res && Array.isArray(res.data)) {
                return res.data;
            }

            if (Array.isArray(res)) return res;
            // Shouldn't normally reach here, but handle gracefully
            return [];
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

export const useUpdateTemplate = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: CreateTemplateDto }) =>
            formEndpoints.updateTemplate(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['templates'] });
            queryClient.invalidateQueries({ queryKey: ['forms'] }); // Some hooks use 'forms' key
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

export const useRequestResponses = (requestId: string | null) => {
    return useQuery({
        queryKey: ['responses', requestId],
        queryFn: async () => {
            if (!requestId) return [];
            const res = await formEndpoints.getResponsesByRequestId(requestId);
            // Handle API wrapping: { data: [...] } vs direct array
            if ('data' in res && Array.isArray(res.data)) {
                return res.data;
            }
            if (Array.isArray(res)) return res as TemplateResponse[];
            return [] as TemplateResponse[];
        },
        enabled: !!requestId,
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};
