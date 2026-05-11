import api from '@/shared/api/baseApi';
import { CreateDepartmentDto, UpdateDepartmentDto, Department } from '@/entities/department/model/types';

export const departmentActionsApi = {
    create: async (dto: CreateDepartmentDto) => {
        const response = await api.post<{ data: Department }>('/Departments', dto);
        return response.data.data;
    },

    update: async (id: string, dto: UpdateDepartmentDto) => {
        const response = await api.put<{ data: Department }>(`/Departments/${id}`, dto);
        return response.data.data;
    },

    delete: async (id: string) => {
        const response = await api.delete<{ data: any }>(`/Departments/${id}`);
        return response.data.data;
    },

    assignUser: async (departmentId: string, userId: string) => {
        const response = await api.post<{ data: any }>(`/Departments/${departmentId}/assign-user`, { userId });
        return response.data.data;
    },

    removeUser: async (departmentId: string, userId: string) => {
        const response = await api.delete<{ data: any }>(`/Departments/${departmentId}/remove-user/${userId}`);
        return response.data.data;
    },

    assignHead: async (departmentId: string, userId: string) => {
        const response = await api.post<{ data: any }>(`/Departments/${departmentId}/assign-head`, { userId });
        return response.data.data;
    }
};
