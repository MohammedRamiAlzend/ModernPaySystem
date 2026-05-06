import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { formEndpoints } from '../api/formEndpoints';
import { queryKeys } from '@/shared/constants/query-keys';
import { QUERY_STRATEGIES, UpdateStrategy } from '@/shared/constants/query-strategies';
import { useUIStore } from '@/app/store/uiStore';
import type { FormSchema } from '@/entities/form/model/types';

// Helper to map backend Template to FormSchema
const mapTemplateToFormSchema = (t: any): FormSchema | null => {
    try {
        let parsed;
        try {
            parsed = JSON.parse(t.contentAsJson);
        } catch {
            parsed = JSON.parse(t.contentAsJson.replace(/'/g, '"'));
        }

        const baseSchema = Array.isArray(parsed) ? parsed[0] : parsed;
        if (!baseSchema || typeof baseSchema !== 'object') return null;

        const schema = baseSchema as FormSchema;
        schema.id = t.id;
        schema.title = t.templateName;
        schema.description = t.templateDescription || '';
        return schema;
    } catch {
        return null;
    }
};

export const useTemplateOwnerships = (templateId: string) => {
    return useQuery({
        queryKey: queryKeys.template.ownerships(templateId),
        queryFn: async () => {
            const res = await formEndpoints.getTemplateOwnerships(templateId);
            return Array.isArray(res) ? res : (res.data || []);
        },
        enabled: !!templateId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useUserTemplateOwnerships = (templateId: string) => {
    return useQuery({
        queryKey: queryKeys.template.userOwnerships(templateId),
        queryFn: async () => {
            const res = await formEndpoints.getUserOwnerships(templateId);
            return Array.isArray(res) ? res : (res.data || []);
        },
        enabled: !!templateId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useTemplatesByDepartment = (departmentId: string | undefined) => {
    return useQuery({
        queryKey: queryKeys.template.byDepartment(departmentId || ''),
        queryFn: async () => {
            if (!departmentId) return [];
            const res = await formEndpoints.getTemplatesByDepartment(departmentId);
            const templates = Array.isArray(res) ? res : (res.data || []);
            return templates.map(mapTemplateToFormSchema).filter((f): f is FormSchema => f !== null);
        },
        enabled: !!departmentId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

export const useTemplatesByUserDirect = (userId: string | undefined) => {
    return useQuery({
        queryKey: queryKeys.template.byUser(userId || ''),
        queryFn: async () => {
            if (!userId) return [];
            const res = await formEndpoints.getTemplatesByUserDirect(userId);
            const templates = Array.isArray(res) ? res : (res.data || []);
            return templates.map(mapTemplateToFormSchema).filter((f): f is FormSchema => f !== null);
        },
        enabled: !!userId,
        ...QUERY_STRATEGIES[UpdateStrategy.LIVE]
    });
};

// Mutations
export const useAddTemplateOwnership = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ templateId, departmentId }: { templateId: string, departmentId: string }) =>
            formEndpoints.addTemplateOwnership(templateId, departmentId),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.ownerships(variables.templateId) });
            queryClient.invalidateQueries({ queryKey: queryKeys.template.byDepartment(variables.departmentId) });
            showStatus({ type: 'success', title: 'نجاح', message: 'تم إسناد النموذج للقسم بنجاح.' });
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'حدث خطأ أثناء إسناد النموذج.' });
        }
    });
};

export const useRemoveTemplateOwnership = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ templateId, departmentId }: { templateId: string, departmentId: string }) =>
            formEndpoints.removeTemplateOwnership(templateId, departmentId),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.ownerships(variables.templateId) });
            queryClient.invalidateQueries({ queryKey: queryKeys.template.byDepartment(variables.departmentId) });
            showStatus({ type: 'success', title: 'نجاح', message: 'تمت إزالة صلاحية القسم للنموذج بنجاح.' });
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'حدث خطأ أثناء إزالة صلاحية النموذج.' });
        }
    });
};

export const useAddUserTemplateOwnership = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ templateId, userId }: { templateId: string, userId: string }) =>
            formEndpoints.addUserOwnership(templateId, userId),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.userOwnerships(variables.templateId) });
            queryClient.invalidateQueries({ queryKey: queryKeys.template.byUser(variables.userId) });
            showStatus({ type: 'success', title: 'نجاح', message: 'تم منح صلاحية للنموذج للمستخدم بنجاح.' });
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'حدث خطأ أثناء منح الصلاحية للمستخدم.' });
        }
    });
};

export const useRemoveUserTemplateOwnership = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ templateId, userId }: { templateId: string, userId: string }) =>
            formEndpoints.removeUserOwnership(templateId, userId),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.template.userOwnerships(variables.templateId) });
            queryClient.invalidateQueries({ queryKey: queryKeys.template.byUser(variables.userId) });
            showStatus({ type: 'success', title: 'نجاح', message: 'تمت إزالة صلاحية المستخدم للنموذج بنجاح.' });
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'حدث خطأ أثناء إزالة صلاحية المستخدم.' });
        }
    });
};
