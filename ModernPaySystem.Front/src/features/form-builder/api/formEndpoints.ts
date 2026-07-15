import api from '@/shared/api/baseApi';
import type {
    Template,
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
    RequestPagedFilterDto,
    RequestRelationDto
} from '@/entities/form/model/types';

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
        const response = await api.get('/Templates', {});
        return response.data;
    },

    getTemplateById: async (id: string): Promise<Template> => {
        const response = await api.get(`/Templates/${id}`);
        const raw = response.data;
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
    createRequest: async (data: CreateRequestDto, onUploadProgress?: (progressEvent: any) => void): Promise<{ data: TemplateRequest }> => {
        const formData = new FormData();
        formData.append('TemplateId', data.TemplateId);
        formData.append('DepartmentId', data.DepartmentId);

        data.Content.forEach((item, index) => {
            formData.append(`Content[${index}].Key`, item.key);

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

        if (data.RelatedRequests && data.RelatedRequests.length > 0) {
            data.RelatedRequests.forEach((related, index) => {
                formData.append(`RelatedRequests[${index}].TargetRequestId`, related.targetRequestId);
                formData.append(`RelatedRequests[${index}].RelationType`, String(related.relationType));
                if (related.notes) {
                    formData.append(`RelatedRequests[${index}].Notes`, related.notes);
                }
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
            onUploadProgress
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

    createResponse: async (data: CreateResponseDto, onUploadProgress?: (progressEvent: any) => void): Promise<{ data: TemplateResponse }> => {
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
            onUploadProgress
        });
        return response.data;
    },

    // Attachments
    fetchRequestAttachmentsBlob: async (requestId: string, onDownloadProgress?: (progressEvent: any) => void): Promise<Blob> => {
        const response = await api.get(`/Attachments/request/${requestId}/download-all`, {
            responseType: 'blob',
            onDownloadProgress
        });
        return new Blob([response.data]);
    },

    downloadRequestAttachments: async (requestId: string, onDownloadProgress?: (progressEvent: any) => void): Promise<void> => {
        const blob = await formEndpoints.fetchRequestAttachmentsBlob(requestId, onDownloadProgress);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `request_${requestId}_attachments.zip`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    },

    fetchResponseAttachmentsBlob: async (responseId: string, onDownloadProgress?: (progressEvent: any) => void): Promise<Blob> => {
        const response = await api.get(`/Attachments/response/${responseId}/download-all`, {
            responseType: 'blob',
            onDownloadProgress
        });
        return new Blob([response.data]);
    },

    downloadResponseAttachments: async (responseId: string, onDownloadProgress?: (progressEvent: any) => void): Promise<void> => {
        const blob = await formEndpoints.fetchResponseAttachmentsBlob(responseId, onDownloadProgress);
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', `response_${responseId}_attachments.zip`);
        document.body.appendChild(link);
        link.click();
        link.remove();
        window.URL.revokeObjectURL(url);
    },

    fetchTransactionAttachmentsBlob: async (transactionId: string, onDownloadProgress?: (progressEvent: any) => void): Promise<Blob> => {
        const response = await api.get(`/Attachments/transaction/${transactionId}/download-all`, {
            responseType: 'blob',
            onDownloadProgress
        });
        return new Blob([response.data]);
    },

    downloadTransactionAttachments: async (transactionId: string, onDownloadProgress?: (progressEvent: any) => void): Promise<void> => {
        const blob = await formEndpoints.fetchTransactionAttachmentsBlob(transactionId, onDownloadProgress);
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

    createReferral: async (data: CreateRequestTransactionDto, onUploadProgress?: (progressEvent: any) => void): Promise<any> => {
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
            onUploadProgress
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
    },

    getTransactionDashboard: async (): Promise<{ data: any }> => {
        const response = await api.get('/Reports/dashboard');
        return response.data;
    },

    getTransactionDailyReport: async (date?: string | null): Promise<{ data: any }> => {
        const params = date ? `?date=${date}` : '';
        const response = await api.get(`/Reports/daily${params}`);
        return response.data;
    },

    getTransactionWeeklyReport: async (weekStart?: string | null): Promise<{ data: any }> => {
        const params = weekStart ? `?weekStart=${weekStart}` : '';
        const response = await api.get(`/Reports/weekly${params}`);
        return response.data;
    },

    getTransactionMonthlyReport: async (year?: number | null, month?: number | null): Promise<{ data: any }> => {
        const params = new URLSearchParams();
        if (year) params.append('year', String(year));
        if (month) params.append('month', String(month));
        const query = params.toString();
        const response = await api.get(`/Reports/monthly${query ? `?${query}` : ''}`);
        return response.data;
    },

    getTransactionUserActivity: async (fromDate?: string | null, toDate?: string | null): Promise<{ data: any }> => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        const query = params.toString();
        const response = await api.get(`/Reports/user-activity${query ? `?${query}` : ''}`);
        return response.data;
    },

    getTransactionActiveUsers: async (fromDate?: string | null, toDate?: string | null): Promise<{ data: any }> => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        const query = params.toString();
        const response = await api.get(`/Reports/active-users${query ? `?${query}` : ''}`);
        return response.data;
    },

    getTransactionStorageReport: async (): Promise<{ data: any }> => {
        const response = await api.get('/Reports/storage');
        return response.data;
    },

    getTransactionChartsData: async (fromDate?: string | null, toDate?: string | null): Promise<{ data: any }> => {
        const params = new URLSearchParams();
        if (fromDate) params.append('fromDate', fromDate);
        if (toDate) params.append('toDate', toDate);
        const query = params.toString();
        const response = await api.get(`/Reports/charts${query ? `?${query}` : ''}`);
        return response.data;
    },

    getTransactionDailyWork: async (date?: string | null): Promise<{ data: any }> => {
        const params = date ? `?date=${date}` : '';
        const response = await api.get(`/Reports/daily-work${params}`);
        return response.data;
    },

    getRequestsPaged: async (filterDto: RequestPagedFilterDto): Promise<{ data: PagedResult<TemplateRequest> }> => {
        const response = await api.post('/Requests/paged', filterDto);
        return response.data;
    },

    getRequestById: async (id: string): Promise<TemplateRequest> => {
        const response = await api.get(`/Requests/${id}`);
        return response.data?.data ?? response.data;
    },

    getRequestRelations: async (requestId: string): Promise<{ data: RequestRelationDto[] }> => {
        const response = await api.get(`/Requests/${requestId}/relations`);
        return response.data;
    },

    createRelation: async (dto: { sourceRequestId: string; targetRequestId: string; relationType: number; notes?: string }): Promise<{ data: RequestRelationDto }> => {
        const response = await api.post('/Requests/relations', dto);
        return response.data;
    },

    deleteRelation: async (id: string): Promise<{ data: boolean }> => {
        const response = await api.delete(`/Requests/relations/${id}`);
        return response.data;
    }
};

// Re-export all hooks from model/queries.ts
// This maintains backward compatibility for existing imports
export {
    useRequests,
    useAllPendingRequests,
    useTemplates,
    useTemplateById,
    useCreateTemplate,
    useUpdateTemplate,
    useCreateRequest,
    useRequestsPaged,
    useRequestById,
    useRequestRelations,
    useCreateRelation,
    useDeleteRelation,
    useCreateResponse,
    useCreateReferral,
    useRequestResponses,
    useResponsesByRequester,
    useRequestsByRequester,
    useRequestTransactions,
    useRequestTransactionsHistory,
    useRequestsReport,
    useResponsesReport,
    useTransactionDashboard,
    useTransactionDailyReport,
    useTransactionWeeklyReport,
    useTransactionMonthlyReport,
    useTransactionUserActivityReport,
    useTransactionActiveUsersReport,
    useTransactionStorageReport,
    useTransactionChartsData,
    useTransactionDailyWorkReport,
} from '../model/queries';
