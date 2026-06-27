import api from '@/shared/api/baseApi';
import {
    DeleteArchiveRequest,
    CreateDeleteArchiveRequestDto
} from '../model/types';

export const archiveDeletionRequestsService = {
    submitDeletionRequest: async (dto: CreateDeleteArchiveRequestDto): Promise<DeleteArchiveRequest> => {
        const response = await api.post<any>('/archive-deletion-requests', dto);
        return response.data.data;
    },

    getDeletionRequestById: async (id: string): Promise<DeleteArchiveRequest> => {
        const response = await api.get<any>(`/archive-deletion-requests/${id}`);
        return response.data.data;
    },

    getPendingForDepartment: async (
        departmentId: string,
        page = 1,
        pageSize = 20
    ): Promise<{ items: DeleteArchiveRequest[]; totalItems: number }> => {
        const response = await api.get<any>(`/archive-deletion-requests/department/${departmentId}`, {
            params: { page, pageSize }
        });
        const data = response.data.data;
        return {
            items: data?.items || [],
            totalItems: data?.totalItems || 0
        };
    },

    approveDeletionRequest: async (id: string, notes?: string): Promise<DeleteArchiveRequest> => {
        const response = await api.post<any>(`/archive-deletion-requests/${id}/approve`, { notes });
        return response.data.data;
    },

    rejectDeletionRequest: async (id: string, reason: string): Promise<DeleteArchiveRequest> => {
        const response = await api.post<any>(`/archive-deletion-requests/${id}/reject`, { reason });
        return response.data.data;
    }
};
