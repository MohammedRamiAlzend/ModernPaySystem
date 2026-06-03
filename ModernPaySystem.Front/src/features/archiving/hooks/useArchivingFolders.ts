import React, { useState, useEffect, useCallback, useMemo } from 'react';
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
    const [currentFolder, setCurrentFolder] = useState<Folder | null>(null);
    const [loadingFolders, setLoadingFolders] = useState(false);
    
    // Modal states
    const [showFolderModal, setShowFolderModal] = useState(false);
    const [folderModalMode, setFolderModalMode] = useState<'create' | 'edit'>('create');
    const [folderName, setFolderName] = useState('');
    const [selectedFolder, setSelectedFolder] = useState<Folder | null>(null);
    const [isSavingFolder, setIsSavingFolder] = useState(false);

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

    // Keep current folder in sync when folder tree updates
    useEffect(() => {
        if (currentFolder) {
            const fresh = folders.find(f => f.id === currentFolder.id);
            if (fresh && fresh !== currentFolder) {
                setCurrentFolder(fresh);
            }
        }
    }, [folders, currentFolder]);

    const handleOpenCreateFolder = () => {
        setFolderName('');
        setFolderModalMode('create');
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
                    parentId: currentFolder ? currentFolder.id : null
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
                    if (currentFolder && (currentFolder.id === folder.id || breadcrumbs.some(c => c.id === folder.id))) {
                        setCurrentFolder(null);
                    }
                    await loadFolders();
                } catch (error) {
                    console.error('Failed to delete folder', error);
                    showStatus({
                        type: 'error',
                        title: 'فشل حذف المجلد',
                        message: 'تعذر إتمام عملية الحذف. يرجى التحقق من محتويات المجلد.'
                    });
                }
            }
        });
    };

    const navigateToFolder = (folder: Folder | null) => {
        setCurrentFolder(folder);
    };

    return {
        folders,
        setFolders,
        currentFolder,
        setCurrentFolder,
        loadingFolders,
        loadFolders,
        breadcrumbs,
        showFolderModal,
        setShowFolderModal,
        folderModalMode,
        setFolderModalMode,
        folderName,
        setFolderName,
        selectedFolder,
        setSelectedFolder,
        isSavingFolder,
        handleOpenCreateFolder,
        handleOpenEditFolder,
        handleSaveFolder,
        handleDeleteFolder,
        navigateToFolder
    };
}
