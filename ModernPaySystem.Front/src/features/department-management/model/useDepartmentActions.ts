import { useMutation, useQueryClient } from '@tanstack/react-query';
import { departmentActionsApi } from '../api/departmentActionsApi';
import { queryKeys } from '@/shared/constants/query-keys';
import { useUIStore } from '@/app/store/uiStore';
import { CreateDepartmentDto, UpdateDepartmentDto } from '@/entities/department/model/types';

export const useDepartmentActions = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    const createMutation = useMutation({
        mutationFn: (dto: CreateDepartmentDto) => departmentActionsApi.create(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.department.all });
            showStatus({
                type: 'success',
                title: 'تمت العملية بنجاح',
                message: 'تم إنشاء القسم الجديد بنجاح'
            });
        },
        onError: (error: any) => {
            const backendErrors = error.response?.data?.errors;
            let errorMessage = error.response?.data?.message || 'حدث خطأ أثناء إنشاء القسم';

            if (Array.isArray(backendErrors) && backendErrors.length > 0) {
                const firstError = backendErrors[0];
                if (firstError.code === 'CIRCULAR_REFERENCE') {
                    errorMessage = 'لا يمكن إنشاء علاقة دائرية (لا يمكن للقسم أن يكون أب لنفسه أو لأحد أبنائه)';
                } else {
                    errorMessage = firstError.description || errorMessage;
                }
            }

            showStatus({
                type: 'error',
                title: 'خطأ في العملية',
                message: errorMessage
            });
        }
    });

    const updateMutation = useMutation({
        mutationFn: ({ id, dto }: { id: string, dto: UpdateDepartmentDto }) => 
            departmentActionsApi.update(id, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.department.all });
            showStatus({
                type: 'success',
                title: 'تم التحديث',
                message: 'تم تحديث بيانات القسم بنجاح'
            });
        },
        onError: (error: any) => {
            const backendErrors = error.response?.data?.errors;
            let errorMessage = error.response?.data?.message || 'حدث خطأ أثناء تحديث القسم';

            if (Array.isArray(backendErrors) && backendErrors.length > 0) {
                const firstError = backendErrors[0];
                if (firstError.code === 'CIRCULAR_REFERENCE') {
                    errorMessage = 'لا يمكن إنشاء علاقة دائرية (لا يمكن للقسم أن يكون أب لنفسه أو لأحد أبنائه)';
                } else {
                    errorMessage = firstError.description || errorMessage;
                }
            }

            showStatus({
                type: 'error',
                title: 'خطأ في التحديث',
                message: errorMessage
            });
        }
    });

    const deleteMutation = useMutation({
        mutationFn: (id: string) => departmentActionsApi.delete(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.department.all });
            showStatus({
                type: 'success',
                title: 'تم الحذف',
                message: 'تم حذف القسم بنجاح'
            });
        },
        onError: (error: any) => {
            showStatus({
                type: 'error',
                title: 'خطأ في الحذف',
                message: error.response?.data?.message || 'لا يمكن حذف هذا القسم لأنه قد يحتوي على أقسام فرعية أو بيانات مرتبطة'
            });
        }
    });

    const assignUserMutation = useMutation({
        mutationFn: ({ departmentId, userId }: { departmentId: string, userId: string }) => 
            departmentActionsApi.assignUser(departmentId, userId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.department.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.user.all });
            showStatus({
                type: 'success',
                title: 'تم التعيين',
                message: 'تم تعيين المستخدم للقسم بنجاح'
            });
        },
        onError: (error: any) => {
            showStatus({
                type: 'error',
                title: 'خطأ في التعيين',
                message: error.response?.data?.message || 'حدث خطأ أثناء تعيين المستخدم للقسم'
            });
        }
    });

    const assignHeadMutation = useMutation({
        mutationFn: ({ departmentId, userId }: { departmentId: string, userId: string }) => 
            departmentActionsApi.assignHead(departmentId, userId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.department.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.user.all });
            showStatus({
                type: 'success',
                title: 'تم تعيين المدير',
                message: 'تم تعيين مدير القسم بنجاح'
            });
        },
        onError: (error: any) => {
            showStatus({
                type: 'error',
                title: 'خطأ في تعيين المدير',
                message: error.response?.data?.message || 'حدث خطأ أثناء تعيين مدير القسم'
            });
        }
    });

    return {
        createDepartment: createMutation.mutateAsync,
        updateDepartment: updateMutation.mutateAsync,
        deleteDepartment: deleteMutation.mutateAsync,
        assignUserToDepartment: assignUserMutation.mutateAsync,
        assignDepartmentHead: assignHeadMutation.mutateAsync,
        isLoading: createMutation.isPending || 
                  updateMutation.isPending || 
                  deleteMutation.isPending || 
                  assignUserMutation.isPending ||
                  assignHeadMutation.isPending
    };
};
