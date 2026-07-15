import { useMutation, useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archivingService } from '../api/archivingService';
import { useUIStore } from '@/app/store/uiStore';
import { CreateFolderDto, CreateDynamicFormTemplateDto, UpdateDynamicFormTemplateDto, UpdateArchiveConfigDto, CreateFolderIconDto, AssignFolderIconDto } from './types';
import { extractErrorMessage } from '@/shared/lib/error-utils';

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
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة إنشاء المجلد.');

            showStatus({
                type: 'error',
                title: 'خطأ في إنشاء المجلد',
                message: message
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
            // console.error('Failed to update folder', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة تحديث المجلد.');

            showStatus({
                type: 'error',
                title: 'خطأ في تحديث المجلد',
                message: message
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
            // console.error('Failed to move folder', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة نقل المجلد.');

            showStatus({
                type: 'error',
                title: 'خطأ في نقل المجلد',
                message: message
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
            // console.error('Failed to delete folder', error);
            const message = extractErrorMessage(error, 'فشل حذف المجلد، يرجى التأكد من خلوه من الملفات أو المجلدات الفرعية.');

            showStatus({
                type: 'error',
                title: 'خطأ في حذف المجلد',
                message: message
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
                files: File[];
                content: { key: string; value: string | null }[];
            };
            onUploadProgress?: (progressEvent: any) => void;
        }) => archivingService.createArchiveRecord(data, onUploadProgress),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
            showStatus({
                type: 'success',
                title: 'تمت الأرشفة',
                message: 'تم أرشفة المستند بنجاح.'
            });
        },
        onError: (error: any) => {
            // console.error('Failed to create archive record', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة أرشفة المستند.');

            showStatus({
                type: 'error',
                title: 'خطأ في الأرشفة',
                message: message
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
                files?: File[];
                content: { key: string; value: string | null }[];
                fileIdsToRemove?: string[];
                replaceFiles?: boolean;
            };
            onUploadProgress?: (progressEvent: any) => void;
        }) => archivingService.updateArchiveRecord(id, data, onUploadProgress),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
            showStatus({
                type: 'success',
                title: 'تم تحديث السجل',
                message: 'تم تحديث مستند الأرشيف بنجاح.'
            });
        },
        onError: (error: any) => {
            // console.error('Failed to update archive record', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة تحديث مستند الأرشيف.');

            showStatus({
                type: 'error',
                title: 'خطأ في التحديث',
                message: message
            });
        }
    });
};

export const useMoveArchiveRecord = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ id, destinationFolderId }: { id: string; destinationFolderId: string }) =>
            archivingService.moveArchiveRecord(id, destinationFolderId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.records.all });
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folders.all });
            showStatus({
                type: 'success',
                title: 'تم نقل المستند',
                message: 'تم نقل المستند إلى المجلد الجديد بنجاح.'
            });
        },
        onError: (error: any) => {
            // console.error('Failed to move archive record', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة نقل المستند.');

            showStatus({
                type: 'error',
                title: 'خطأ في النقل',
                message: message
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
            // console.error('Failed to delete archive record', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة حذف مستند الأرشيف.');

            showStatus({
                type: 'error',
                title: 'خطأ في الحذف',
                message: message
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
            // console.error('Failed to create dynamic form', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة إنشاء النموذج الأرشيفي.');

            showStatus({
                type: 'error',
                title: 'خطأ في الإنشاء',
                message: message
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
            // console.error('Failed to update dynamic form', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة تحديث النموذج الأرشيفي.');

            showStatus({
                type: 'error',
                title: 'خطأ في حفظ التعديلات',
                message: message
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
            // console.error('Failed to delete dynamic form', error);
            const message = extractErrorMessage(error, 'فشل حذف النموذج الأرشيفي، قد يكون مرتبطاً ببيانات قائمة.');
            showStatus({
                type: 'error',
                title: 'خطأ في الحذف',
                message: message
            });
        }
    });
};

// ---------------------------------------------------------
// Folder Icons Mutations
// ---------------------------------------------------------
export const useCreateFolderIcon = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (dto: CreateFolderIconDto) => archivingService.createFolderIcon(dto),
        onSuccess: (data) => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folderIcons.all });
            showStatus({
                type: 'success',
                title: 'تم إنشاء الأيقونة',
                message: `تم إنشاء أيقونة "${data.name}" بنجاح.`
            });
        },
        onError: (error: any) => {
            // console.error('Failed to create folder icon', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة إنشاء الأيقونة.');

            showStatus({
                type: 'error',
                title: 'خطأ في إنشاء الأيقونة',
                message: message
            });
        }
    });
};

export const useUpdateFolderIcon = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: ({ id, dto }: { id: string; dto: Partial<CreateFolderIconDto> }) => archivingService.updateFolderIcon(id, dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folderIcons.all });
            showStatus({
                type: 'success',
                title: 'تم تحديث الأيقونة',
                message: 'تم تحديث الأيقونة بنجاح.'
            });
        },
        onError: (error: any) => {
            // console.error('Failed to update folder icon', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة تحديث الأيقونة.');

            showStatus({
                type: 'error',
                title: 'خطأ في تحديث الأيقونة',
                message: message
            });
        }
    });
};

export const useDeleteFolderIcon = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (id: string) => archivingService.deleteFolderIcon(id),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folderIcons.all });
            showStatus({
                type: 'success',
                title: 'تم حذف الأيقونة',
                message: 'تم حذف الأيقونة بنجاح.'
            });
        },
        onError: (error: any) => {
            // console.error('Failed to delete folder icon', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة حذف الأيقونة.');

            showStatus({
                type: 'error',
                title: 'خطأ في حذف الأيقونة',
                message: message
            });
        }
    });
};

export const useAssignIconToFolder = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (dto: AssignFolderIconDto) => archivingService.assignIconToFolder(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.folders.all });
            showStatus({
                type: 'success',
                title: 'تم تعيين الأيقونة',
                message: 'تم تعيين الأيقونة للمجلد بنجاح.'
            });
        },
        onError: (error: any) => {
            // console.error('Failed to assign icon to folder', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة تعيين الأيقونة للمجلد.');

            showStatus({
                type: 'error',
                title: 'خطأ في تعيين الأيقونة',
                message: message
            });
        }
    });
};

// ---------------------------------------------------------
// Archive Config Mutations
// ---------------------------------------------------------
export const useUpdateArchiveConfig = () => {
    const queryClient = useQueryClient();
    const { showStatus } = useUIStore();

    return useMutation({
        mutationFn: (dto: UpdateArchiveConfigDto) => archivingService.updateArchiveConfig(dto),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: queryKeys.archiving.config.all });
            showStatus({
                type: 'success',
                title: 'تم حفظ الإعدادات',
                message: 'تم تحديث إعدادات نظام الأرشفة بنجاح.'
            });
        },
        onError: (error: any) => {
            // console.error('Failed to update archive config', error);
            const message = extractErrorMessage(error, 'حدث خطأ أثناء محاولة تحديث إعدادات الأرشفة.');

            showStatus({
                type: 'error',
                title: 'خطأ في حفظ الإعدادات',
                message: message
            });
        }
    });
};
