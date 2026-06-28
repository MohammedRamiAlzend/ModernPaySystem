import React, { useState, useCallback, useMemo } from 'react';
import { Folder } from '@/features/archiving/model/types';
import { archivingService } from '@/features/archiving/api/archivingService';
import { useUIStore } from '@/app/store/uiStore';

const flattenFolders = (nodes: Folder[]): Folder[] => {
    let result: Folder[] = [];
    for (const node of nodes) {
        result.push(node);
        if (node.folderDtos && node.folderDtos.length > 0) {
            result = result.concat(flattenFolders(node.folderDtos));
        }
    }
    return result;
};

export function useArchivingFolders() {
    const { showStatus, showConfirm } = useUIStore();

    const [folders, setFolders] = useState<Folder[]>([]);
    const [currentFolderId, setCurrentFolderId] = useState<string | null>(null);
    const [loadingFolders, setLoadingFolders] = useState(false);

    // Modal states
    const [showFolderModal, setShowFolderModal] = useState(false);
    const [folderModalMode, setFolderModalMode] = useState<'create' | 'edit'>('create');
    const [folderName, setFolderName] = useState('');
    const [folderStoragePath, setFolderStoragePath] = useState('');
    const [selectedFolder, setSelectedFolder] = useState<Folder | null>(null);
    const [isSavingFolder, setIsSavingFolder] = useState(false);

    // Initial permissions state (user IDs, always View level)
    const [initialPermissionIds, setInitialPermissionIds] = useState<string[]>([]);

    // Derive currentFolder from currentFolderId and folders list during render
    const currentFolder = useMemo(() => {
        if (!currentFolderId) return null;
        return folders.find(f => f.id === currentFolderId) || null;
    }, [currentFolderId, folders]);

    const breadcrumbs = useMemo(() => {
        if (!currentFolder) return [];
        const crumbs: Folder[] = [];
        let curr: Folder | undefined = currentFolder;
        while (curr) {
            crumbs.unshift(curr);
            const parentId: string | null = curr.parentId;
            curr = parentId ? folders.find(f => f.id === parentId) : undefined;
        }
        return crumbs;
    }, [currentFolder, folders]);

    const loadFolders = useCallback(async () => {
        setLoadingFolders(true);
        try {
            const data = await archivingService.getAllFolders();
            setFolders(flattenFolders(data));
        } catch (error) {
            console.error('Failed to load folders', error);
            showStatus({
                type: 'error',
                title: 'خطأ في تحميل المجلدات',
                message: 'تعذر تحميل المجلدات من الخادم.'
            });
        } finally {
            setLoadingFolders(false);
        }
    }, [showStatus]);

    const handleOpenCreateFolder = () => {
        setFolderName('');
        setFolderStoragePath('');
        setFolderModalMode('create');
        setInitialPermissionIds([]);
        setShowFolderModal(true);
    };

    const handleOpenEditFolder = (folder: Folder) => {
        setSelectedFolder(folder);
        setFolderName(folder.name);
        setFolderModalMode('edit');
        setShowFolderModal(true);
    };

    const handleSaveFolder = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!folderName.trim()) return;
        setIsSavingFolder(true);
        try {
            if (folderModalMode === 'create') {
                await archivingService.createFolder({
                    name: folderName,
                    defaultStoragePath: folderStoragePath.trim() || null,
                    parentId: currentFolderId,
                    initialPermissions: initialPermissionIds.length > 0
                        ? initialPermissionIds.map(id => ({ userId: id, accessLevel: 1 }))
                        : undefined
                });
                showStatus({
                    type: 'success',
                    title: 'تم إنشاء المجلد',
                    message: `تم إنشاء المجلد "${folderName}" بنجاح.`
                });
            } else if (folderModalMode === 'edit' && selectedFolder) {
                await archivingService.updateFolder(selectedFolder.id, folderName);
                showStatus({
                    type: 'success',
                    title: 'تم تعديل اسم المجلد',
                    message: `تم تعديل الاسم إلى "${folderName}" بنجاح.`
                });
            }
            setShowFolderModal(false);
            setFolderName('');
            await loadFolders();
        } catch (error) {
            console.error('Failed to save folder', error);
            showStatus({
                type: 'error',
                title: 'خطأ في الحفظ',
                message: 'حدث خطأ أثناء حفظ المجلد. يرجى المحاولة لاحقاً.'
            });
        } finally {
            setIsSavingFolder(false);
        }
    };

    const handleDeleteFolder = (folder: Folder) => {
        showConfirm({
            title: 'حذف المجلد',
            message: `هل أنت متأكد من حذف المجلد "${folder.name}" وجميع محتوياته؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: 'destructive',
            confirmLabel: 'حذف المجلد',
            onConfirm: async () => {
                try {
                    await archivingService.deleteFolder(folder.id);
                    showStatus({
                        type: 'success',
                        title: 'تم حذف المجلد',
                        message: 'تم حذف المجلد وكل محتوياته بنجاح.'
                    });
                    if (currentFolderId && (currentFolderId === folder.id || breadcrumbs.some(c => c.id === folder.id))) {
                        setCurrentFolderId(null);
                    }
                    await loadFolders();
                } catch (error: any) {
                    console.error('Failed to delete folder', error);
                    const errMsg = error?.response?.data?.errors && error.response.data.errors[0]?.arabicDescription || error.response?.data?.message || error.message || 'تعذر إتمام عملية الحذف. يرجى التحقق من محتويات المجلد.';
                    showStatus({
                        type: 'error',
                        title: 'فشل حذف المجلد',
                        message: errMsg
                    });
                }
            }
        });
    };

    const navigateToFolder = (folder: Folder | null) => {
        setCurrentFolderId(folder ? folder.id : null);
    };

    return {
        folders,
        setFolders,
        currentFolder,
        currentFolderId,
        setCurrentFolderId,
        loadingFolders,
        loadFolders,
        breadcrumbs,
        showFolderModal,
        setShowFolderModal,
        folderModalMode,
        setFolderModalMode,
        folderName,
        setFolderName,
        folderStoragePath,
        setFolderStoragePath,
        selectedFolder,
        setSelectedFolder,
        isSavingFolder,
        initialPermissionIds,
        setInitialPermissionIds,
        handleOpenCreateFolder,
        handleOpenEditFolder,
        handleSaveFolder,
        handleDeleteFolder,
        navigateToFolder
    };
}
