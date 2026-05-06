import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '@/shared/api/baseApi';
import type { Contract } from '@/entities/contracts/types/contract-types';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import { queryKeys } from '@/shared/constants/query-keys';

export type CreateContractInput = Omit<Contract, 'id' | 'status'>;
export type UpdateContractInput = Partial<CreateContractInput>;

/**
 * هوك لجلب قائمة كافة العقود من الخادم.
 * يستخدم استراتيجية BACKGROUND (تحديث كل 10 دقائق) لتوفير الطلبات.
 */
export const useContractsQuery = () => {
    return useQuery<Contract[]>({
        queryKey: queryKeys.contract.all,
        queryFn: async () => {
            const response = await api.get('/contracts');
            return response.data;
        },
        ...QUERY_STRATEGIES[UpdateStrategy.BACKGROUND],
    });
};

/**
 * هوك لجلب بيانات عقد واحد محدد باستخدام معرفه (ID).
 * يستخدم استراتيجية CRITICAL لضمان الحصول على بيانات دقيقة عند فتح العقد.
 * @param id معرّف العقد
 */
export const useContractQuery = (id: number | string | undefined) => {
    return useQuery<Contract>({
        queryKey: queryKeys.contract.detail(id || ''),
        queryFn: async () => {
            const response = await api.get(`/contracts/${id}`);
            return response.data;
        },
        enabled: !!id, // لا يتم الطلب إلا إذا كان الـ ID موجوداً
        ...QUERY_STRATEGIES[UpdateStrategy.CRITICAL],
    });
};

/**
 * هوك لإضافة عقد جديد.
 * يقوم بتفريغ كاش قائمة العقود عند النجاح لضمان تحديث الواجهات.
 */
export const useCreateContractMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async (newContract: CreateContractInput) => {
            const response = await api.post('/contracts', newContract);
            return response.data;
        },
        onSuccess: () => {
            // تحديث قائمة العقود في الكاش فوراً
            queryClient.invalidateQueries({ queryKey: queryKeys.contract.all });
        },
    });
};

/**
 * هوك لتعديل بيانات عقد موجود.
 * يقوم بتحديث كاش القائمة وكاش العقد المحدد عند النجاح.
 * @param id معرّف العقد المراد تعديله
 */
export const useUpdateContractMutation = (id: number | string) => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async (updates: UpdateContractInput) => {
            const response = await api.put(`/contracts/${id}`, updates);
            return response.data;
        },
        onSuccess: () => {
            // تفريغ كاش القائمة وكاش العقد المحدد لضمان المزامنة
            queryClient.invalidateQueries({ queryKey: queryKeys.contract.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.contract.detail(id) });
        },
    });
};

/**
 * هوك لحذف عقد محدد.
 * يقوم بتحديث الكاش لحذف العقد من القائمة.
 */
export const useDeleteContractMutation = () => {
    const queryClient = useQueryClient();
    return useMutation({
        mutationFn: async (id: number | string) => {
            await api.delete(`/contracts/${id}`);
        },
        onSuccess: () => {
            // تحديث القائمة بعد الحذف
            queryClient.invalidateQueries({ queryKey: queryKeys.contract.all });
        },
    });
};

