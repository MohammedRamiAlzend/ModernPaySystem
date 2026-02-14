import api from '@/shared/api/baseApi';
import { useQuery } from '@tanstack/react-query';

// User type definition
export interface User {
    id: string;
    userName: string;
    subSystemUserId: string | null;
    createdByUserId: string | null;
    createdAt: string | null;
    updatedByUserId: string | null;
    updatedAt: string | null;
}

interface UserResponse {
    data: User;
}

// API function to fetch user by ID
export const fetchUserById = async (userId: string): Promise<User> => {
    const response = await api.get<UserResponse>(`/Users/${userId}`);
    return response.data.data;
};

// React Query hook to fetch user by ID
export const useUser = (userId: string | null | undefined) => {
    return useQuery({
        queryKey: ['user', userId],
        queryFn: () => fetchUserById(userId!),
        enabled: !!userId, // Only run if userId exists
        staleTime: 5 * 60 * 1000, // Cache for 5 minutes
        retry: 1,
    });
};

// Hook to fetch multiple users at once
export const useUsers = (userIds: (string | null | undefined)[]) => {
    const validIds = userIds.filter((id): id is string => !!id);

    return useQuery({
        queryKey: ['users', validIds],
        queryFn: async () => {
            const users = await Promise.all(
                validIds.map(id => fetchUserById(id))
            );
            // Create a map of userId -> user for easy lookup
            return users.reduce((acc, user) => {
                acc[user.id] = user;
                return acc;
            }, {} as Record<string, User>);
        },
        enabled: validIds.length > 0,
        staleTime: 5 * 60 * 1000,
        retry: 1,
    });
};
