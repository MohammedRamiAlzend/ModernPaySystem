import api from '@/shared/api/baseApi';
import { useQuery } from '@tanstack/react-query';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import { queryKeys } from '@/shared/constants/query-keys';
import type { UserReference } from '@/entities/form/model/types';

export const userEndpoints = {
    getAllUsers: async (): Promise<{ data: UserReference[] }> => {
        const response = await api.get('/Users');
        return response.data;
    }
};

export const useUsers = () => {
    return useQuery({
        queryKey: queryKeys.user.all,
        queryFn: async () => {
            const res = await userEndpoints.getAllUsers();
            return res.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND]
    });
};
