import api from '@/shared/api/baseApi';
import {
    EditArchiveRequest,
    CreateEditArchiveRequestDto
} from '../model/types';

export const archiveEditRequestsService = {
    submitEditRequest: async (
        dto: CreateEditArchiveRequestDto,
        onUploadProgress?: (progressEvent: any) => void
    ): Promise<EditArchiveRequest> => {
        const formData = new FormData();
        formData.append('ArchiveRecordId', dto.archiveRecordId);
        formData.append('Justification', dto.justification);

        dto.requestedChanges.forEach((item, index) => {
            formData.append(`RequestedChanges[${index}].Key`, item.key);
            if (item.value !== null && item.value !== undefined) {
                formData.append(`RequestedChanges[${index}].Value`, item.value);
            }
        });

        if (dto.files) {
            dto.files.forEach((file) => {
                formData.append('Files', file);
            });
        }

        const response = await api.post<any>('/archive-edit-requests', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
            onUploadProgress
        });
        return response.data.data;
    },

    getEditRequestById: async (id: string): Promise<EditArchiveRequest> => {
        const response = await api.get<any>(`/archive-edit-requests/${id}`);
        return response.data.data;
    },

    getPendingForDepartment: async (
        departmentId: string,
        page = 1,
        pageSize = 20
    ): Promise<{ items: EditArchiveRequest[]; totalItems: number }> => {
        const response = await api.get<any>(`/archive-edit-requests/department/${departmentId}`, {
            params: { page, pageSize }
        });
        const data = response.data.data;
        return {
            items: data?.items || [],
            totalItems: data?.totalItems || 0
        };
    },

    getMyEditRequests: async (
        page = 1,
        pageSize = 20
    ): Promise<{ items: EditArchiveRequest[]; totalItems: number }> => {
        const response = await api.get<any>('/archive-edit-requests/my-requests', {
            params: { page, pageSize }
        });
        const data = response.data.data;
        return {
            items: data?.items || [],
            totalItems: data?.totalItems || 0
        };
    },

    approveEditRequest: async (id: string, notes?: string): Promise<EditArchiveRequest> => {
        const response = await api.post<any>(`/archive-edit-requests/${id}/approve`, { notes });
        return response.data.data;
    },

    rejectEditRequest: async (id: string, reason: string): Promise<EditArchiveRequest> => {
        const response = await api.post<any>(`/archive-edit-requests/${id}/reject`, { reason });
        return response.data.data;
    }
};
