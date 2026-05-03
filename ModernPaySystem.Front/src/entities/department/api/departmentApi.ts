import api from '@/shared/api/baseApi';
import { Department, DepartmentTree } from '../model/types';

export const departmentApi = {
    getTree: async () => {
        const response = await api.get<{ data: DepartmentTree[] }>('/Departments/tree');
        return response.data.data;
    },

    getSubTree: async (id: string) => {
        const response = await api.get<{ data: DepartmentTree[] }>(`/Departments/${id}/subtree`);
        return response.data.data;
    },

    getById: async (id: string) => {
        const response = await api.get<{ data: Department }>(`/Departments/${id}`);
        return response.data.data;
    },

    getChildren: async (id: string) => {
        const response = await api.get<{ data: Department[] }>(`/Departments/${id}/children`);
        return response.data.data;
    },

    getPathToRoot: async (id: string) => {
        const response = await api.get<{ data: Department[] }>(`/Departments/${id}/path`);
        return response.data.data;
    },

    search: async (searchTerm?: string, level: number = 0) => {
        const response = await api.get<{ data: Department[] }>('/Departments/search', {
            params: { searchTerm, level }
        });
        return response.data.data;
    },

    getByLevel: async (level: number) => {
        const response = await api.get<{ data: Department[] }>(`/Departments/level/${level}`);
        return response.data.data;
    }
};
