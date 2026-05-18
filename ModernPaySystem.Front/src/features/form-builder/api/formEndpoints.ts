
import api from '@/shared/api/baseApi';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import type {
    Template,
    FormSchema,
    CreateTemplateDto,
    CreateRequestDto,
    CreateResponseDto,
    TemplateRequest,
    TemplateResponse,
    PagedResult,
    CreateRequestTransactionDto,
    RequestTransactionDto,
    TemplateOwnershipDto,
    UserTemplateOwnershipDto,
    RequestPagedFilterDto
} from '@/entities/form/model/types';

// --- API Service ---

export const formEndpoints = {
    // Templates 
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

    getTemplateById: async (id: string): Promise<Template> => {
        const response = await api.get(`/Templates/${id}`);
        const raw = response.data;
        // Handle both { data: Template } and direct Template response
        return raw?.data ?? raw;
    },

    // Ownerships
    getTemplateOwnerships: async (id: string): Promise<{ data: TemplateOwnershipDto[] } | TemplateOwnershipDto[]> => {
        const response = await api.get(`/Templates/${id}/ownerships`);
        return response.data;
    },
    addTemplateOwnership: async (id: string, departmentId: string): Promise<{ data: TemplateOwnershipDto } | TemplateOwnershipDto> => {
        const response = await api.post(`/Templates/${id}/ownerships`, { departmentId });
        return response.data;
    },
    removeTemplateOwnership: async (id: string, departmentId: string): Promise<void> => {
        await api.delete(`/Templates/${id}/ownerships/${departmentId}`);
    },
    getUserOwnerships: async (id: string): Promise<{ data: UserTemplateOwnershipDto[] } | UserTemplateOwnershipDto[]> => {
        const response = await api.get(`/Templates/${id}/ownerships/user`);
        return response.data;
    },
    addUserOwnership: async (id: string, userId: string): Promise<{ data: UserTemplateOwnershipDto } | UserTemplateOwnershipDto> => {
        const response = await api.post(`/Templates/${id}/ownerships/user`, { userId });
        return response.data;
    },
    removeUserOwnership: async (id: string, userId: string): Promise<void> => {
        await api.delete(`/Templates/${id}/ownerships/user/${userId}`);
    },
    getTemplatesByDepartment: async (departmentId: string): Promise<Template[] | { data: Template[] }> => {
        const response = await api.get(`/Templates/department/${departmentId}`);
        return response.data;
    },
    getTemplatesByUserDirect: async (userId: string): Promise<Template[] | { data: Template[] }> => {
        const response = await api.get(`/Templates/user/${userId}`);
        return response.data;
    },

    // Requests
    createRequest: async (data: CreateRequestDto): Promise<{ data: TemplateRequest }> => {
        const formData = new FormData();
        formData.append('TemplateId', data.TemplateId);
        formData.append('DepartmentId', data.DepartmentId);

        // Handle structured content for model binding
        data.Content.forEach((item, index) => {
            formData.append(`Content[${index}].Key`, item.key);

            // Handle complex types (arrays, objects) by stringifying them
            const valueToAppend = (typeof item.value === 'object' && item.value !== null)
                ? JSON.stringify(item.value)
                : String(item.value ?? '');

            formData.append(`Content[${index}].Value`, valueToAppend);
        });

        if (data.ReadOnlyUsers && data.ReadOnlyUsers.length > 0) {
            data.ReadOnlyUsers.forEach((userId) => {
                formData.append('ReadOnlyUsers', userId);
            });
        }

        if (data.files && data.files.length > 0) {
            data.files.forEach((file) => {
                formData.append('Files', file);
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

    getRequestsByActionStatus: async (hasResponse: boolean, filterDto: RequestPagedFilterDto): Promise<{ data: PagedResult<TemplateRequest> }> => {
        const response = await api.post(`/Requests/GetPagedRequestsNeedAction/${hasResponse}`, filterDto);
        return response.data;
    },

    getRequestsByRequesterId: async (requesterId: string, filterDto: RequestPagedFilterDto): Promise<{ data: PagedResult<TemplateRequest> }> => {
        const response = await api.post(`/Requests/by-requester/${requesterId}`, filterDto);
        return response.data;
    },


    // Responses
    getResponsesByRequestId: async (requestId: string, filterDto: RequestPagedFilterDto = { page: 1, pageSize: 100 }): Promise<{ data: PagedResult<TemplateResponse> | TemplateResponse[] }> => {
        const response = await api.post(`/Responses/by-request/${requestId}`, filterDto);
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
    },

    fetchTransactionAttachmentsBlob: async (transactionId: string): Promise<Blob> => {
        const response = await api.get(`/Attachments/transaction/${transactionId}/download-all`, {
            responseType: 'blob',
        });
        return new Blob([response.data]);
    },

    downloadTransactionAttachments: async (transactionId: string): Promise<void> => {
        const blob = await formEndpoints.fetchTransactionAttachmentsBlob(transactionId);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `transaction_${transactionId}_attachments.zip`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    },


    getResponsesByRequesterId: async (requesterId: string, filterDto: RequestPagedFilterDto): Promise<{ data: PagedResult<TemplateResponse> }> => {
        const response = await api.post(`/Responses/by-requester/${requesterId}`, filterDto);
        return response.data;
    },

    createReferral: async (data: CreateRequestTransactionDto): Promise<any> => {
        const formData = new FormData();
        formData.append('RequestId', data.requestId);
        if (data.notes) formData.append('Notes', data.notes);
        if (data.parentTransactionId) formData.append('ParentTransactionId', data.parentTransactionId);
        formData.append('CurrentUserHolderId', data.targetUserId);

        if (data.files && data.files.length > 0) {
            data.files.forEach((file) => {
                formData.append('Files', file);
            });
        }

        const url = data.parentTransactionId ? '/RequestTransactions/AddTransactionChildren' : '/RequestTransactions';
        const response = await api.post(url, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
        });
        return response.data;
    },

    getRequestTransactions: async (status: number, filterDto: RequestPagedFilterDto): Promise<{ data: PagedResult<RequestTransactionDto> }> => {
        const response = await api.post(`/RequestTransactions/paged?status=${status}`, filterDto);
        return response.data;
    },

    getRequestTransactionsByRequestId: async (requestId: string): Promise<{ data: RequestTransactionDto[] }> => {
        const response = await api.get(`/RequestTransactions/by-request/${requestId}`);
        return response.data;
    },

    getAllPendingRequestsPaged: async (page: number = 1, pageSize: number = 10): Promise<{ data: PagedResult<TemplateRequest> }> => {
        const response = await api.get(`/Requests/my-pending/paged?page=${page}&pageSize=${pageSize}`);
        return response.data;
    },

    getRequestsReport: async (pageNumber: number, pageSize: number, startDate?: string, endDate?: string, forCurrentDepartment: boolean = false): Promise<{ data: PagedResult<TemplateRequest> }> => {
        const params = new URLSearchParams();
        params.append('pageNumber', String(pageNumber));
        params.append('pageSize', String(pageSize));
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        params.append('forCurrentDepartment', String(forCurrentDepartment));
        
        const response = await api.get(`/Reports/requests?${params.toString()}`);
        return response.data;
    },

    getResponsesReport: async (pageNumber: number, pageSize: number, startDate?: string, endDate?: string, forCurrentDepartment: boolean = false): Promise<{ data: PagedResult<TemplateResponse> }> => {
        const params = new URLSearchParams();
        params.append('pageNumber', String(pageNumber));
        params.append('pageSize', String(pageSize));
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        params.append('forCurrentDepartment', String(forCurrentDepartment));
        
        const response = await api.get(`/Reports/responses?${params.toString()}`);
        return response.data;
    }

};


// --- Hooks ---

export const useRequests = (hasResponse: boolean = false, filterOrPage: RequestPagedFilterDto | number = 1, pageSize: number = 15) => {
    const filter = typeof filterOrPage === 'object'
        ? filterOrPage
        : { page: filterOrPage, pageSize };

    return useQuery({
        queryKey: queryKeys.form.list({ hasResponse, ...filter }),
        queryFn: async () => {
            // Only send filter values if hasResponse is true
            const filterToSend = hasResponse ? filter : { page: filter.page, pageSize: filter.pageSize };
            const res = await formEndpoints.getRequestsByActionStatus(hasResponse, filterToSend);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useAllPendingRequests = (page: number = 1, pageSize: number = 15) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'all-pending', page, pageSize }),
        queryFn: async () => {
            const res = await formEndpoints.getAllPendingRequestsPaged(page, pageSize);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useTemplates = (showExternal: boolean = false) => {
    return useQuery({
        queryKey: queryKeys.template.list({ showExternal }),
        queryFn: async (): Promise<Template[]> => {
            const res = await formEndpoints.getTemplates();
            const filterFn = (t: Template) => showExternal || (!t.isExternal && !t.templateName.toLocaleLowerCase().includes("delphi"));

            if (Array.isArray(res)) return res.filter(filterFn);

            // Handle if data is wrapped in { data: [] }
            if (res && !Array.isArray(res) && 'data' in res && Array.isArray(res.data)) {
                return (res.data as Template[]).filter(filterFn);
            }

            return [];
        },
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useTemplateById = (templateId: string | null) => {
    return useQuery({
        queryKey: queryKeys.template.detail(templateId),
        queryFn: async (): Promise<FormSchema | null> => {
            if (!templateId) return null;
            const t = await formEndpoints.getTemplateById(templateId);
            if (!t || !t.contentAsJson) return null;
            try {
                let parsed;
                try {
                    parsed = JSON.parse(t.contentAsJson);
                } catch {
                    parsed = JSON.parse(t.contentAsJson.replace(/'/g, '"'));
                }
                const baseSchema = Array.isArray(parsed) ? parsed[0] : parsed;
                if (!baseSchema || typeof baseSchema !== 'object') return null;
                const schema = baseSchema as FormSchema;
                schema.id = t.id;
                schema.title = t.templateName;
                schema.description = t.templateDescription || '';
                return schema;
            } catch {
                console.error('Failed to parse template content by ID', t);
                return null;
            }
        },
        enabled: !!templateId,
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useCreateTemplate = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: formEndpoints.createTemplate,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.form.all });
        }
    });
};

export const useUpdateTemplate = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: ({ id, data }: { id: string; data: CreateTemplateDto }) =>
            formEndpoints.updateTemplate(id, data),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.form.all }); // Some hooks use 'forms' key
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

export const useCreateReferral = () => {
    return useMutation({
        mutationFn: formEndpoints.createReferral
    });
};

export const useRequestResponses = (requestId: string | null) => {
    return useQuery({
        queryKey: queryKeys.form.responses(requestId),
        queryFn: async () => {
            if (!requestId) return [];
            const res = await formEndpoints.getResponsesByRequestId(requestId);

            // Handle PagedResult wrapping: { data: { items: [...] } }
            if (res && typeof res === 'object' && 'data' in res) {
                const inner = res.data as any;
                if (Array.isArray(inner)) return inner;
                if (inner && Array.isArray(inner.items)) return inner.items;
            }

            if (Array.isArray(res)) return res as TemplateResponse[];

            return [] as TemplateResponse[];
        },
        enabled: !!requestId,
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};

export const useResponsesByRequester = (requesterId: string | null, filter: RequestPagedFilterDto) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'responses', requester: requesterId, ...filter }),
        queryFn: async () => {
            if (!requesterId) return null;
            const res = await formEndpoints.getResponsesByRequesterId(requesterId, filter);
            return res.data;
        },
        enabled: !!requesterId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useRequestsByRequester = (requesterId: string | null, filter: RequestPagedFilterDto) => {
    return useQuery({
        queryKey: queryKeys.form.list({ requester: requesterId, ...filter }),
        queryFn: async () => {
            if (!requesterId) return null;
            const res = await formEndpoints.getRequestsByRequesterId(requesterId, filter);
            return res.data;
        },
        enabled: !!requesterId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useRequestTransactions = (status: number, filter: RequestPagedFilterDto) => {
    return useQuery({
        queryKey: queryKeys.process.list({ status, ...filter }),
        queryFn: async () => {
            const res = await formEndpoints.getRequestTransactions(status, filter);
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useRequestTransactionsHistory = (requestId: string | null) => {
    return useQuery({
        queryKey: queryKeys.form.transactions(requestId),
        queryFn: async () => {
            if (!requestId) return [];
            const res = await formEndpoints.getRequestTransactionsByRequestId(requestId);
            return res.data;
        },
        enabled: !!requestId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useRequestsReport = (pageNumber: number, pageSize: number, startDate?: string, endDate?: string, forCurrentDepartment: boolean = false, enabled: boolean = false) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'report-requests', page: pageNumber, pageSize, startDate, endDate, forCurrentDepartment }),
        queryFn: async () => {
            const res = await formEndpoints.getRequestsReport(pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
            const payload = (res as any).data || res;
            return payload?.value || payload;
        },
        enabled: enabled && pageNumber > 0,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useResponsesReport = (pageNumber: number, pageSize: number, startDate?: string, endDate?: string, forCurrentDepartment: boolean = false, enabled: boolean = false) => {
    return useQuery({
        queryKey: queryKeys.form.list({ type: 'report-responses', page: pageNumber, pageSize, startDate, endDate, forCurrentDepartment }),
        queryFn: async () => {
            const res = await formEndpoints.getResponsesReport(pageNumber, pageSize, startDate, endDate, forCurrentDepartment);
            const payload = (res as any).data || res;
            return payload?.value || payload;
        },
        enabled: enabled && pageNumber > 0,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};
