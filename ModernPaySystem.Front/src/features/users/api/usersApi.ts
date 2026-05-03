import api from '@/shared/api/baseApi';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

// User type definition
export interface User {
    id: string;
    userName: string;
    subSystemUserId: string | null;
    subSystem: number | null;
    departmentId: string | null;
    departmentName: string | null;
    isDepartmentHead: boolean;
    createdByUserId: string | null;
    createdAt: string | null;
    updatedByUserId: string | null;
    updatedAt: string | null;
}

export interface UserCreateDto {
    userName: string;
    password?: string;
    subSystem: number;
}

export interface SubSystem {
    name: string;
    value: string;
}

interface ApiResponse<T> {
    data: T;
}

// API functions
export const fetchUsers = async (): Promise<User[]> => {
    const response = await api.get<ApiResponse<User[]>>('/Users');
    return response.data.data;
};

export const fetchUserById = async (userId: string): Promise<User> => {
    const response = await api.get<ApiResponse<User>>(`/Users/${userId}`);
    return response.data.data;
};

export const createUser = async (user: UserCreateDto): Promise<void> => {
    await api.post('/Users', user);
};

export const updateUser = async ({ id, ...user }: UserCreateDto & { id: string }): Promise<void> => {
    await api.put(`/Users/${id}`, user);
};

export const deleteUser = async (id: string): Promise<void> => {
    await api.delete(`/Users/${id}`);
};

export const fetchSubSystems = async (): Promise<SubSystem[]> => {
    const response = await api.get<ApiResponse<SubSystem[]>>('/Users/subsystems');
    return response.data.data;
};

export const fetchUsersBySubSystem = async (subSystemId: string): Promise<User[]> => {
    const response = await api.get<ApiResponse<User[]>>(`/Users/by-subsystem/${subSystemId}`);
    return response.data.data;
};

// React Query hooks
export const useUser = (userId: string | null | undefined) => {
    return useQuery({
        queryKey: ['user', userId],
        queryFn: () => fetchUserById(userId!),
        enabled: !!userId,
        staleTime: 5 * 60 * 1000,
        retry: 1,
    });
};

export const useUsers = (subSystemId?: string) => {
    // If we have an array of IDs in some cases, we might need a different hook or overloaded one.
    // But for management we usually need the list.
    return useQuery({
        queryKey: ['users', subSystemId],
        queryFn: () => subSystemId && subSystemId !== 'all' ? fetchUsersBySubSystem(subSystemId) : fetchUsers(),
    });
};

export const useSubSystems = () => {
    return useQuery({
        queryKey: ['subsystems'],
        queryFn: fetchSubSystems,
    });
};

export const useUserMutations = () => {
    const queryClient = useQueryClient();

    const createMutation = useMutation({
        mutationFn: createUser,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['users'] });
        },
    });

    const updateMutation = useMutation({
        mutationFn: updateUser,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['users'] });
            queryClient.invalidateQueries({ queryKey: ['user'] });
        },
    });

    const deleteMutation = useMutation({
        mutationFn: deleteUser,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['users'] });
        },
    });

    return {
        createUser: createMutation,
        updateUser: updateMutation,
        deleteUser: deleteMutation,
    };
};
