import api from '@/shared/api/baseApi';
import { Folder, ArchiveRecord, DynamicFormTemplate, CreateFolderDto, CreateDynamicFormTemplateDto, UpdateDynamicFormTemplateDto, FolderPermissionDto, CreateFolderPermissionDto, SemanticSearchRequest, SemanticSearchResultItem, ArchiveAuditLog } from '../model/types';
import { Department } from '@/entities/department/model/types';

export const archivingService = {
    // ---------------------------------------------
    // Folders API
    // ---------------------------------------------
    getAllFolders: async (): Promise<Folder[]> => {
        const response = await api.get<any>('/ArchiveSystem/Folders');
        return response.data.data;
    },

    getFolderById: async (id: string): Promise<Folder> => {
        const response = await api.get<any>(`/ArchiveSystem/Folders/${id}`);
        return response.data.data;
    },

    createFolder: async (dto: CreateFolderDto): Promise<Folder> => {
        const response = await api.post<any>('/ArchiveSystem/Folders', dto);
        return response.data.data;
    },

    updateFolder: async (id: string, name: string): Promise<Folder> => {
        const response = await api.put<any>(`/ArchiveSystem/Folders/${id}`, { name });
        return response.data.data;
    },

    moveFolder: async (folderId: string, destinationFolderId: string): Promise<void> => {
        await api.put('/ArchiveSystem/Folders/MoveFolder', { folderId, destnationFolderId: destinationFolderId });
    },

    deleteFolder: async (id: string): Promise<void> => {
        await api.delete(`/ArchiveSystem/Folders/${id}`);
    },

    // ---------------------------------------------
    // Archive Records API
    // ---------------------------------------------
    getArchiveRecordsByFolder: async (folderId: string, page = 1, pageSize = 10): Promise<{ items: ArchiveRecord[], totalItems: number }> => {
        const response = await api.get<any>(`/archive-records/folder/${folderId}`, {
            params: { page, pageSize }
        });
        const data = response.data.data;
        return {
            items: data?.items || [],
            totalItems: data?.totalItems || 0
        };
    },

    getArchiveRecordById: async (id: string): Promise<ArchiveRecord> => {
        const response = await api.get<any>(`/archive-records/${id}`);
        return response.data.data;
    },

    createArchiveRecord: async (
        data: {
            id?: string;
            folderId: string;
            formId: string | null;
            archivalNumber: string;
            files: File[];
            content: { key: string; value: string | null }[];
        },
        onUploadProgress?: (progressEvent: any) => void
    ): Promise<ArchiveRecord> => {
        const formData = new FormData();
        
        if (data.id) {
            formData.append('Id', data.id);
        }
        formData.append('FolderId', data.folderId);
        if (data.formId) {
            formData.append('FormId', data.formId);
        }
        formData.append('ArchivalNumber', data.archivalNumber);

        // إضافة حقول النموذج الديناميكي ككائن Content
        data.content.forEach((item, index) => {
            formData.append(`Content[${index}].Key`, item.key);
            if (item.value !== null && item.value !== undefined) {
                formData.append(`Content[${index}].Value`, item.value);
            }
        });

        // إضافة الملفات
        data.files.forEach((file) => {
            formData.append('Files', file);
        });

        const response = await api.post<any>('/archive-records', formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
            onUploadProgress
        });
        return response.data.data;
    },

    updateArchiveRecord: async (
        id: string,
        data: {
            folderId: string;
            formId: string | null;
            archivalNumber: string;
            files?: File[];
            content: { key: string; value: string | null }[];
            fileIdsToRemove?: string[];
            replaceFiles?: boolean;
        },
        onUploadProgress?: (progressEvent: any) => void
    ): Promise<ArchiveRecord> => {
        const formData = new FormData();
        formData.append('FolderId', data.folderId);
        if (data.formId) {
            formData.append('FormId', data.formId);
        }
        formData.append('ArchivalNumber', data.archivalNumber);

        data.content.forEach((item, index) => {
            formData.append(`Content[${index}].Key`, item.key);
            if (item.value !== null && item.value !== undefined) {
                formData.append(`Content[${index}].Value`, item.value);
            }
        });

        if (data.files) {
            data.files.forEach((file) => {
                formData.append('Files', file);
            });
        }

        if (data.fileIdsToRemove) {
            data.fileIdsToRemove.forEach((fileId) => {
                formData.append('FileIdsToRemove', fileId);
            });
        }

        if (data.replaceFiles !== undefined) {
            formData.append('ReplaceFiles', String(data.replaceFiles));
        }

        const response = await api.put<any>(`/archive-records/${id}`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
            onUploadProgress
        });
        return response.data.data;
    },

    deleteArchiveRecord: async (id: string): Promise<void> => {
        await api.delete(`/archive-records/${id}`);
    },

    getPagedArchiveRecords: async (filter: {
        page?: number;
        pageSize?: number;
        searchText?: string;
        archivalNumber?: string;
        recordId?: string;
        inputValueFilters?: { key: string; value: string }[];
        logicalOperator?: number;
    }): Promise<{ items: ArchiveRecord[], totalItems: number }> => {
        const params: any = {
            page: filter.page ?? 1,
            pageSize: filter.pageSize ?? 10,
            searchText: filter.searchText || undefined,
            archivalNumber: filter.archivalNumber || undefined,
            recordId: filter.recordId || undefined,
            logicalOperator: filter.logicalOperator ?? 0,
        };

        if (filter.inputValueFilters) {
            filter.inputValueFilters.forEach((f, index) => {
                params[`InputValueFilters[${index}].Key`] = f.key;
                params[`InputValueFilters[${index}].Value`] = f.value;
            });
        }

        const response = await api.get<any>('/archive-records/paged', { params });
        const data = response.data.data;
        return {
            items: data?.items || [],
            totalItems: data?.totalItems || 0
        };
    },

    // ---------------------------------------------
    // File Operations API
    // ---------------------------------------------
    downloadFile: async (
        recordId: string,
        fileId: string,
        onDownloadProgress?: (progressEvent: any) => void
    ): Promise<Blob> => {
        const response = await api.get(`/archive-records/${recordId}/files/${fileId}`, {
            params: { download: true },
            responseType: 'blob',
            onDownloadProgress
        });
        return response.data;
    },

    viewFileBlob: async (recordId: string, fileId: string): Promise<Blob> => {
        const response = await api.get(`/archive-records/${recordId}/files/${fileId}`, {
            params: { download: false },
            responseType: 'blob'
        });
        return response.data;
    },

    downloadFileById: async (
        fileId: string,
        onDownloadProgress?: (progressEvent: any) => void
    ): Promise<Blob> => {
        const response = await api.get(`/archive-records/files/${fileId}`, {
            params: { download: true },
            responseType: 'blob',
            onDownloadProgress
        });
        return response.data;
    },

    viewFileInlineUrl: (recordId: string, fileId: string): string => {
        const baseURL = api.defaults.baseURL || 'http://localhost:5173/api';
        const token = sessionStorage.getItem('token');
        return `${baseURL}/archive-records/${recordId}/files/${fileId}?download=false&access_token=${token || ''}`;
    },

    viewFileInlineUrlById: (fileId: string): string => {
        const baseURL = api.defaults.baseURL || 'http://localhost:5173/api';
        const token = sessionStorage.getItem('token');
        return `${baseURL}/archive-records/files/${fileId}?download=false&access_token=${token || ''}`;
    },

    viewFileBlobById: async (fileId: string, includeDeleted: boolean = false): Promise<Blob> => {
        const response = await api.get(`/archive-records/files/${fileId}`, {
            params: { download: false, includeDeleted },
            responseType: 'blob'
        });
        return response.data;
    },

    downloadZip: async (
        recordId: string,
        options?: { flatten?: boolean; password?: string; includeMetadata?: boolean },
        onDownloadProgress?: (progressEvent: any) => void
    ): Promise<Blob> => {
        const response = await api.get(`/archive-records/${recordId}/files/zip`, {
            params: {
                flatten: options?.flatten ?? false,
                password: options?.password || undefined,
                includeMetadata: options?.includeMetadata ?? false
            },
            responseType: 'blob',
            onDownloadProgress
        });
        return response.data;
    },

    addFilesToArchiveRecord: async (
        id: string,
        files: File[],
        onUploadProgress?: (progressEvent: any) => void
    ): Promise<ArchiveRecord> => {
        const formData = new FormData();
        files.forEach((file) => {
            formData.append('files', file);
        });
        const response = await api.post<any>(`/archive-records/${id}/files`, formData, {
            headers: {
                'Content-Type': 'multipart/form-data',
            },
            onUploadProgress
        });
        return response.data.data;
    },

    removeFileFromArchiveRecord: async (id: string, fileId: string): Promise<void> => {
        await api.delete(`/archive-records/${id}/files/${fileId}`);
    },

    logPrint: async (recordId: string): Promise<void> => {
        await api.post(`/archive-records/${recordId}/print`);
    },

    // ---------------------------------------------
    // Dynamic Form Templates API
    // ---------------------------------------------
    getAllDynamicForms: async (): Promise<DynamicFormTemplate[]> => {
        const response = await api.get<any>('/ArchiveSystem/DynamicForms');
        return response.data.data;
    },

    getDynamicFormsPaged: async (page = 1, pageSize = 10): Promise<{ items: DynamicFormTemplate[], totalItems: number }> => {
        const response = await api.get<any>('/ArchiveSystem/DynamicForms/paged', {
            params: { page, pageSize }
        });
        const data = response.data.data;
        return {
            items: data?.items || [],
            totalItems: data?.totalItems || 0
        };
    },

    getDynamicFormById: async (id: string): Promise<DynamicFormTemplate> => {
        const response = await api.get<any>(`/ArchiveSystem/DynamicForms/${id}`);
        return response.data.data;
    },

    getDynamicFormByName: async (name: string): Promise<DynamicFormTemplate> => {
        const response = await api.get<any>(`/ArchiveSystem/DynamicForms/by-name/${name}`);
        return response.data.data;
    },

    createDynamicForm: async (dto: CreateDynamicFormTemplateDto): Promise<DynamicFormTemplate> => {
        const response = await api.post<any>('/ArchiveSystem/DynamicForms', dto);
        return response.data.data;
    },

    updateDynamicForm: async (id: string, dto: UpdateDynamicFormTemplateDto): Promise<DynamicFormTemplate> => {
        const response = await api.put<any>(`/ArchiveSystem/DynamicForms/${id}`, dto);
        return response.data.data;
    },

    deleteDynamicForm: async (id: string): Promise<void> => {
        await api.delete(`/ArchiveSystem/DynamicForms/${id}`);
    },

    // ---------------------------------------------
    // Folder Permissions API
    // ---------------------------------------------
    getFolderPermissions: async (folderId: string): Promise<FolderPermissionDto[]> => {
        const response = await api.get<any>(`/ArchiveSystem/Folders/${folderId}/permissions`);
        return response.data.data;
    },

    createFolderPermission: async (folderId: string, dto: Omit<CreateFolderPermissionDto, 'folderId'>): Promise<FolderPermissionDto> => {
        const response = await api.post<any>(`/ArchiveSystem/Folders/${folderId}/permissions`, dto);
        return response.data.data;
    },

    deleteFolderPermission: async (permissionId: string): Promise<void> => {
        await api.delete(`/ArchiveSystem/Folders/permissions/${permissionId}`);
    },

    // ---------------------------------------------
    // Semantic Search API
    // ---------------------------------------------
    semanticSearch: async (request: SemanticSearchRequest): Promise<SemanticSearchResultItem[]> => {
        const response = await api.post<any>('/semantic-search/search', request);
        return response.data.data;
    },

    // ---------------------------------------------
    // Audit Logs API
    // ---------------------------------------------
    getAuditLogs: async (params: {
        page?: number;
        pageSize?: number;
        action?: number | null;
        fromDate?: string | null;
        toDate?: string | null;
        departmentId?: string | null;
    }): Promise<{ items: ArchiveAuditLog[], totalItems: number }> => {
        const response = await api.get<any>('/archive-records/audit-logs', {
            params: {
                page: params.page ?? 1,
                pageSize: params.pageSize ?? 50,
                action: params.action ?? undefined,
                fromDate: params.fromDate ?? undefined,
                toDate: params.toDate ?? undefined,
                departmentId: params.departmentId ?? undefined,
            }
        });
        const data = response.data.data;
        return {
            items: data?.items || [],
            totalItems: data?.totalItems || 0,
        };
    },

    getLedDepartments: async (): Promise<Department[]> => {
        const response = await api.get<any>('/archive-records/departments/led');
        return response.data.data;
    }
};
