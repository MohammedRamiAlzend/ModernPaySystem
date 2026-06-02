import { useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archivingService } from '../api/archivingService';
import { useUIStore } from '@/app/store/uiStore';
import { CreateFolderDto, CreateDynamicFormTemplateDto, UpdateDynamicFormTemplateDto } from './types';

// ---------------------------------------------------------
// Folders Mutations
// ---------------------------------------------------------
export const useCreateFolder = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (dto: CreateFolderDto) => archivingService.createFolder(dto),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folders.all });
            showStatus({
                type: 'success',
                title: 'تم إنشاء المجلد',
                message: `تم إنشاء المجلد "${data.name}" بنجاح.`
            });
        },
        onError: (error: any) => {
            console.error('Failed to create folder', error);
            showStatus({
                type: 'error',
                title: 'خطأ في إنشاء المجلد',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة إنشاء المجلد.'
            });
        }
    });
};

export const useUpdateFolder = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ id, name }: { id: string; name: string }) => archivingService.updateFolder(id, name),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folders.all });
            showStatus({
                type: 'success',
                title: 'تم تحديث المجلد',
                message: `تم تغيير اسم المجلد إلى "${data.name}" بنجاح.`
            });
        },
        onError: (error: any) => {
            console.error('Failed to update folder', error);
            showStatus({
                type: 'error',
                title: 'خطأ في تحديث المجلد',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة تحديث المجلد.'
            });
        }
    });
};

export const useMoveFolder = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ folderId, destinationFolderId }: { folderId: string; destinationFolderId: string }) => 
            archivingService.moveFolder(folderId, destinationFolderId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folders.all });
            showStatus({
                type: 'success',
                title: 'تم نقل المجلد',
                message: 'تم نقل المجلد إلى موقعه الجديد بنجاح.'
            });
        },
        onError: (error: any) => {
            console.error('Failed to move folder', error);
            showStatus({
                type: 'error',
                title: 'خطأ في نقل المجلد',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة نقل المجلد.'
            });
        }
    });
};

export const useDeleteFolder = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (id: string) => archivingService.deleteFolder(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folders.all });
            showStatus({
                type: 'success',
                title: 'تم حذف المجلد',
                message: 'تم حذف المجلد بنجاح.'
            });
        },
        onError: (error: any) => {
            console.error('Failed to delete folder', error);
            showStatus({
                type: 'error',
                title: 'خطأ في حذف المجلد',
                message: error?.response?.data?.message || 'فشل حذف المجلد، يرجى التأكد من خلوه من الملفات أو المجلدات الفرعية.'
            });
        }
    });
};

// ---------------------------------------------------------
// Archive Records Mutations
// ---------------------------------------------------------
export const useCreateArchiveRecord = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ data, onUploadProgress }: { 
            data: {
                id?: string;
                folderId: string;
                formId: string | null;
                archivalNumber: string;
                files: File[];
                content: { key: string; value: string | null }[];
            };
            onUploadProgress?: (progressEvent: any) => void;
        }) => archivingService.createArchiveRecord(data, onUploadProgress),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
            showStatus({
                type: 'success',
                title: 'تمت الأرشفة',
                message: `تم أرشفة المستند برقم "${data.archivalNumber}" بنجاح.`
            });
        },
        onError: (error: any) => {
            console.error('Failed to create archive record', error);
            showStatus({
                type: 'error',
                title: 'خطأ في الأرشفة',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة أرشفة المستند.'
            });
        }
    });
};

export const useUpdateArchiveRecord = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ id, data, onUploadProgress }: {
            id: string;
            data: {
                folderId: string;
                formId: string;
                archivalNumber: string;
                files?: File[];
                content: { key: string; value: string | null }[];
                fileIdsToRemove?: string[];
                replaceFiles?: boolean;
            };
            onUploadProgress?: (progressEvent: any) => void;
        }) => archivingService.updateArchiveRecord(id, data, onUploadProgress),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
            showStatus({
                type: 'success',
                title: 'تم تحديث السجل',
                message: `تم تحديث مستند الأرشيف رقم "${data.archivalNumber}" بنجاح.`
            });
        },
        onError: (error: any) => {
            console.error('Failed to update archive record', error);
            showStatus({
                type: 'error',
                title: 'خطأ في التحديث',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة تحديث مستند الأرشيف.'
            });
        }
    });
};

export const useDeleteArchiveRecord = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (id: string) => archivingService.deleteArchiveRecord(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
            showStatus({
                type: 'success',
                title: 'تم حذف السجل',
                message: 'تم حذف مستند الأرشيف بنجاح.'
            });
        },
        onError: (error: any) => {
            console.error('Failed to delete archive record', error);
            showStatus({
                type: 'error',
                title: 'خطأ في الحذف',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة حذف مستند الأرشيف.'
            });
        }
    });
};

// ---------------------------------------------------------
// Dynamic Form Templates Mutations
// ---------------------------------------------------------
export const useCreateDynamicForm = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (dto: CreateDynamicFormTemplateDto) => archivingService.createDynamicForm(dto),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.dynamicForms.all });
            showStatus({
                type: 'success',
                title: 'تم إنشاء النموذج',
                message: `تم إنشاء النموذج الأرشيفي "${data.templateFormName}" بنجاح.`
            });
        },
        onError: (error: any) => {
            console.error('Failed to create dynamic form', error);
            showStatus({
                type: 'error',
                title: 'خطأ في الإنشاء',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة إنشاء النموذج الأرشيفي.'
            });
        }
    });
};

export const useUpdateDynamicForm = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ id, dto }: { id: string; dto: UpdateDynamicFormTemplateDto }) => 
            archivingService.updateDynamicForm(id, dto),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.dynamicForms.all });
            showStatus({
                type: 'success',
                title: 'تم حفظ التعديلات',
                message: `تم تحديث النموذج الأرشيفي "${data.templateFormName}" بنجاح.`
            });
        },
        onError: (error: any) => {
            console.error('Failed to update dynamic form', error);
            showStatus({
                type: 'error',
                title: 'خطأ في حفظ التعديلات',
                message: error?.response?.data?.message || 'حدث خطأ أثناء محاولة تحديث النموذج الأرشيفي.'
            });
        }
    });
};

export const useDeleteDynamicForm = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (id: string) => archivingService.deleteDynamicForm(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.dynamicForms.all });
            showStatus({
                type: 'success',
                title: 'تم حذف النموذج',
                message: 'تم حذف النموذج الأرشيفي بنجاح.'
            });
        },
        onError: (error: any) => {
            console.error('Failed to delete dynamic form', error);
            showStatus({
                type: 'error',
                title: 'خطأ في الحذف',
                message: error?.response?.data?.message || 'فشل حذف النموذج الأرشيفي، قد يكون مرتبطاً ببيانات قائمة.'
            });
        }
    });
};
