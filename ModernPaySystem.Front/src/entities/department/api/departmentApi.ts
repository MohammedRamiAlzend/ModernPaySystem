import api from '@/shared/api/baseApi';
import { Department, DepartmentTree, CreateDepartmentDto, UpdateDepartmentDto, AssignUserDto, ArchiveLeaderAssignment } from '../model/types';

const BASE = '/transaction/Departments';

export const departmentApi = {
    getTree: async () => {
        const response = await api.get<{ data: DepartmentTree[] }>(`${BASE}/tree`);
        return response.data.data;
    },

    getSubTree: async (id: string) => {
        const response = await api.get<{ data: DepartmentTree[] }>(`${BASE}/${id}/subtree`);
        return response.data.data;
    },

    getById: async (id: string) => {
        const response = await api.get<{ data: Department }>(`${BASE}/${id}`);
        return response.data.data;
    },

    getChildren: async (id: string) => {
        const response = await api.get<{ data: Department[] }>(`${BASE}/${id}/children`);
        return response.data.data;
    },

    getPathToRoot: async (id: string) => {
        const response = await api.get<{ data: Department[] }>(`${BASE}/${id}/path`);
        return response.data.data;
    },

    getParent: async (id: string) => {
        const response = await api.get<{ data: Department }>(`${BASE}/${id}/parent`);
        return response.data.data;
    },

    search: async (searchTerm?: string, level: number = 0) => {
        const response = await api.get<{ data: Department[] }>(`${BASE}/search`, {
            params: { searchTerm, level }
        });
        return response.data.data;
    },

    getByLevel: async (level: number) => {
        const response = await api.get<{ data: Department[] }>(`${BASE}/level/${level}`);
        return response.data.data;
    },

    create: async (dto: CreateDepartmentDto) => {
        const response = await api.post<Department>(`${BASE}`, dto);
        return response.data;
    },

    update: async (id: string, dto: UpdateDepartmentDto) => {
        const response = await api.put<{ data: Department }>(`${BASE}/${id}`, dto);
        return response.data.data;
    },

    delete: async (id: string) => {
        await api.delete(`${BASE}/${id}`);
    },

    getUsers: async (id: string, includeSubDepartments: boolean = false) => {
        const response = await api.get<{ data: any[] }>(`${BASE}/${id}/users`, {
            params: { includeSubDepartments }
        });
        return response.data.data;
    },

    assignUser: async (id: string, dto: AssignUserDto) => {
        const response = await api.post<{ data: any }>(`${BASE}/${id}/assign-user`, dto);
        return response.data.data;
    },

    assignDepartmentHead: async (id: string, dto: AssignUserDto) => {
        const response = await api.post<{ data: any }>(`${BASE}/${id}/assign-head`, dto);
        return response.data.data;
    },

    removeUser: async (id: string, userId: string) => {
        await api.delete(`${BASE}/${id}/remove-user/${userId}`);
    },

    getArchiveLeaders: async (id: string) => {
        const response = await api.get<{ data: ArchiveLeaderAssignment[] }>(`${BASE}/${id}/archive-leaders`);
        return response.data.data;
    },

    assignArchiveLeader: async (id: string, userId: string) => {
        const response = await api.post<{ data: ArchiveLeaderAssignment }>(`${BASE}/${id}/archive-leaders/${userId}`);
        return response.data.data;
    },

    unassignArchiveLeader: async (id: string, userId: string) => {
        await api.delete(`${BASE}/${id}/archive-leaders/${userId}`);
    }
};
