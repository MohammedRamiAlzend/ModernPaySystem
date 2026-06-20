/* eslint-disable react-hooks/set-state-in-effect */
import React, { useState, useEffect, useCallback } from 'react';
import { ArchiveRecord, DynamicFormTemplate, PhysicalFile } from '@/features/archiving/model/types';
import { archivingService } from '@/features/archiving/api/archivingService';
import { useUIStore } from '@/app/store/uiStore';
import { v4 } from '@/shared/utils/uuid';
import * as htmlToImage from 'html-to-image';
import { useUploadStore, storeFiles } from '@/features/upload-manager';

const printQrCover = (blob: Blob) => {
    const url = URL.createObjectURL(blob);
    const iframe = document.createElement('iframe');
    iframe.style.position = 'fixed';
    iframe.style.right = '0';
    iframe.style.bottom = '0';
    iframe.style.width = '0';
    iframe.style.height = '0';
    iframe.style.border = '0';
    document.body.appendChild(iframe);

    const doc = iframe.contentWindow?.document || iframe.contentDocument;
    if (doc) {
        doc.write(`<!DOCTYPE html>
            <html>
                <head>
                    <title>طباعة غلاف QR</title>
                    <style>
                        @page { size: auto; margin: 0mm; }
                        body { margin: 0; display: flex; align-items: center; justify-content: center; height: 100vh; }
                        img { max-width: 100%; max-height: 100%; object-fit: contain; }
                    </style>
                </head>
                <body>
                    <img src="${url}" onload="window.focus(); window.print();" />
                </body>
            </html>
        `);
        doc.close();
    }

    setTimeout(() => {
        document.body.removeChild(iframe);
        URL.revokeObjectURL(url);
    }, 5000);
};

export function useArchivingRecords(currentFolderId: string | null | undefined) {
    const { showStatus, showConfirm } = useUIStore();

    const [records, setRecords] = useState<ArchiveRecord[]>([]);
    const [dynamicTemplates, setDynamicTemplates] = useState<DynamicFormTemplate[]>([]);
    const [loadingRecords, setLoadingRecords] = useState(false);
    const [recordsPage, setRecordsPage] = useState(1);
    const [hasMoreRecords, setHasMoreRecords] = useState(false);

    // Record Modal States
    const [showRecordModal, setShowRecordModal] = useState(false);
    const [recordModalMode, setRecordModalMode] = useState<'create' | 'edit'>('create');
    const [selectedRecord, setSelectedRecord] = useState<ArchiveRecord | null>(null);
    const [selectedTemplateId, setSelectedTemplateId] = useState('');
    const [templateInputs, setTemplateInputs] = useState<Record<string, string>>({});
    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const [existingFiles, setExistingFiles] = useState<PhysicalFile[]>([]);
    const [fileIdsToRemove, setFileIdsToRemove] = useState<string[]>([]);
    const [isSavingRecord, setIsSavingRecord] = useState(false);
    const [uploadProgress, setUploadProgress] = useState(0);
    const [downloadingZipId, setDownloadingZipId] = useState<string | null>(null);
    const [downloadProgress, setDownloadProgress] = useState<number>(0);
    const [generateQrCover, setGenerateQrCover] = useState(true);
    const [qrCoverGuid, setQrCoverGuid] = useState<string>('');

    const loadDynamicTemplates = useCallback(async () => {
        try {
            const templates = await archivingService.getAllDynamicForms();
            setDynamicTemplates(templates);
        } catch (error) {
            console.error('Failed to load templates', error);
        }
    }, []);

    const loadRecords = useCallback(async (folderId: string, page = 1) => {
        if (page === 1) {
            setLoadingRecords(true);
        }
        try {
            const res = await archivingService.getArchiveRecordsByFolder(folderId, page, 10);
            if (page === 1) {
                setRecords(res.items);
            } else {
                setRecords(prev => [...prev, ...res.items]);
            }
            setHasMoreRecords(res.items.length === 10);
            setRecordsPage(page);
        } catch (error) {
            console.error('Failed to load records', error);
            showStatus({
                type: 'error',
                title: 'خطأ في تحميل المستندات',
                message: 'تعذر تحميل المستندات المؤرشفة لهذا المجلد.'
            });
        } finally {
            setLoadingRecords(false);
        }
    }, [showStatus]);

    const loadMoreRecords = () => {
        if (currentFolderId) {
            loadRecords(currentFolderId, recordsPage + 1);
        }
    };

    // Load templates on mount
    useEffect(() => {
        loadDynamicTemplates();
    }, [loadDynamicTemplates]);

    // Load records when current folder changes
    useEffect(() => {
        if (currentFolderId) {
            loadRecords(currentFolderId, 1);
        } else {
            setRecords([]);
            setHasMoreRecords(false);
        }
    }, [currentFolderId, loadRecords]);

    // Triggered on select change from UI
    const handleTemplateIdChange = (templateId: string) => {
        setSelectedTemplateId(templateId);
        const template = dynamicTemplates.find(t => t.id === templateId);
        if (template) {
            try {
                const fields = JSON.parse(template.contentAsJson);
                if (Array.isArray(fields)) {
                    const defaultInputs: Record<string, string> = {};
                    fields.forEach(f => {
                        defaultInputs[f.label] = '';
                    });
                    setTemplateInputs(defaultInputs);
                    return;
                }
            } catch (e) {
                console.error(e);
            }
        }
        setTemplateInputs({});
    };

    const handleTemplateInputChange = (label: string, value: string) => {
        setTemplateInputs(prev => ({
            ...prev,
            [label]: value
        }));
    };

    const handleOpenCreateRecord = () => {
        setSelectedRecord(null);
        setSelectedTemplateId('');
        setTemplateInputs({});
        setSelectedFiles([]);
        setExistingFiles([]);
        setFileIdsToRemove([]);
        setQrCoverGuid(v4());
        setGenerateQrCover(true);
        setRecordModalMode('create');
        setShowRecordModal(true);
    };

    const handleOpenEditRecord = (record: ArchiveRecord) => {
        setSelectedRecord(record);
        setSelectedTemplateId(record.formId || '');
        setExistingFiles(record.physicalFiles || []);
        setSelectedFiles([]);
        setFileIdsToRemove([]);
        setQrCoverGuid(record.id);

        // Load existing template values
        const inputs: Record<string, string> = {};
        if (record.archiveRecordTemplateValues?.archiveRecordFormInputValues) {
            record.archiveRecordTemplateValues.archiveRecordFormInputValues.forEach(val => {
                inputs[val.key] = val.value || '';
            });
        }
        setTemplateInputs(inputs);
        setRecordModalMode('edit');
        setShowRecordModal(true);
    };

    const handleSaveRecord = async (e: React.FormEvent, qrCoverRef?: React.RefObject<HTMLDivElement | null>) => {
        e.preventDefault();
        if (!currentFolderId) return;
        setIsSavingRecord(true);

        try {
            const fieldsArray = Object.keys(templateInputs).map(key => ({
                key,
                value: templateInputs[key]
            }));

            let qrCoverBlob: Blob | null = null;
            let qrFile: File | null = null;

            // Generate QR Cover if required
            if (recordModalMode === 'create' && generateQrCover) {
                const recordId = qrCoverGuid || v4();
                setQrCoverGuid(recordId);

                // Give template a moment to render with the correct guid
                await new Promise(resolve => setTimeout(resolve, 300));

                if (qrCoverRef && qrCoverRef.current) {
                    try {
                        const blob = await htmlToImage.toBlob(qrCoverRef.current, {
                            pixelRatio: 2,
                            backgroundColor: '#ffffff'
                        });
                        if (blob) {
                            qrCoverBlob = blob;
                            qrFile = new File([blob], `QR_Cover_${recordId}.png`, { type: 'image/png' });
                        }
                    } catch (err) {
                        console.error('Failed to generate QR cover using html-to-image', err);
                    }
                }
            }

            if (recordModalMode === 'create') {
                const recordId = qrCoverGuid || v4();
                
                // Only send the cover file (qrFile) initially
                const initialFiles: File[] = [];
                if (qrFile) {
                    initialFiles.push(qrFile);
                }

                await archivingService.createArchiveRecord({
                    id: recordId,
                    folderId: currentFolderId,
                    formId: selectedTemplateId || null,
                    files: initialFiles,
                    content: fieldsArray
                }, (progressEvent) => {
                    const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                    setUploadProgress(percentCompleted);
                });

                showStatus({
                    type: 'success',
                    title: 'تم إنشاء المستند',
                    message: 'تم حفظ بيانات المستند بنجاح. جاري رفع المرفقات في الخلفية.'
                });

                // Auto-print the cover page immediately
                if (qrCoverBlob) {
                    printQrCover(qrCoverBlob);
                }

                // If there are other selected files, create an upload session
                if (selectedFiles.length > 0) {
                    const sessionId = v4();
                    const uploadItems = selectedFiles.map(file => {
                        const itemId = v4();
                        return {
                            id: itemId,
                            file,
                            fileName: file.name,
                            fileSize: file.size,
                            status: 'pending' as const,
                            progress: 0,
                            retryCount: 0
                        };
                    });

                    // Store all actual File objects in IndexedDB
                    await storeFiles(uploadItems.map(item => ({ id: item.id, file: item.file })));

                    const createSession = useUploadStore.getState().createSession;
                    const firstInputVal = Object.values(templateInputs)[0];
                    const templateName = selectedTemplateId 
                        ? (dynamicTemplates.find(t => t.id === selectedTemplateId)?.templateFormName || 'مستند') 
                        : 'مستند';
                    const recordTitle = firstInputVal 
                        ? `${templateName} (${firstInputVal})`
                        : `مستند أرشيفي (${recordId.substring(0, 8)})`;

                    // Remove file objects from metadata items saved in Zustand/localStorage
                    const metaItems = uploadItems.map(({ file, ...meta }) => meta);

                    createSession({
                        id: sessionId,
                        recordId: recordId,
                        recordTitle,
                        folderId: currentFolderId,
                        files: metaItems,
                        createdAt: new Date().toISOString(),
                        status: 'uploading'
                    });
                }
            } else if (recordModalMode === 'edit' && selectedRecord) {
                // Update metadata and remove files, but don't upload new files in the same call
                await archivingService.updateArchiveRecord(selectedRecord.id, {
                    folderId: currentFolderId,
                    formId: selectedTemplateId,
                    files: [], // Do not upload new files in the PUT request
                    content: fieldsArray,
                    fileIdsToRemove,
                    replaceFiles: false
                }, (progressEvent) => {
                    const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                    setUploadProgress(percentCompleted);
                });

                showStatus({
                    type: 'success',
                    title: 'تم تحديث البيانات',
                    message: 'تم تعديل بيانات المستند. جاري رفع المرفقات الجديدة في الخلفية.'
                });

                // If there are new files selected, create an upload session
                if (selectedFiles.length > 0) {
                    const sessionId = v4();
                    const uploadItems = selectedFiles.map(file => {
                        const itemId = v4();
                        return {
                            id: itemId,
                            file,
                            fileName: file.name,
                            fileSize: file.size,
                            status: 'pending' as const,
                            progress: 0,
                            retryCount: 0
                        };
                    });

                    // Store all actual File objects in IndexedDB
                    await storeFiles(uploadItems.map(item => ({ id: item.id, file: item.file })));

                    const createSession = useUploadStore.getState().createSession;
                    const firstInputVal = Object.values(templateInputs)[0];
                    const templateName = selectedTemplateId 
                        ? (dynamicTemplates.find(t => t.id === selectedTemplateId)?.templateFormName || 'مستند') 
                        : 'مستند';
                    const recordTitle = firstInputVal 
                        ? `${templateName} (${firstInputVal}) [تعديل]`
                        : `تحديث مستند (${selectedRecord.id.substring(0, 8)})`;

                    // Remove file objects from metadata items saved in Zustand/localStorage
                    const metaItems = uploadItems.map(({ file, ...meta }) => meta);

                    createSession({
                        id: sessionId,
                        recordId: selectedRecord.id,
                        recordTitle,
                        folderId: currentFolderId,
                        files: metaItems,
                        createdAt: new Date().toISOString(),
                        status: 'uploading'
                    });
                }
            }

            setShowRecordModal(false);
            await loadRecords(currentFolderId, 1);
        } catch (error: any) {
            console.error('Failed to save record', error);
            if (error?.response?.data?.errors && error.response.data.errors[0]?.arabicDescription) {
                showStatus({
                    type: 'error',
                    title: 'خطأ في الأرشفة',
                    message: error.response.data.errors[0].arabicDescription
                });
            } else {
                showStatus({
                    type: 'error',
                    title: 'خطأ في الأرشفة',
                    message: 'حدث خطأ أثناء حفظ السجل، يرجى مراجعة البيانات المرفقة.'
                });
            }
        } finally {
            setIsSavingRecord(false);
        }
    };

    const handleDeleteRecord = (record: ArchiveRecord) => {
        showConfirm({
            title: 'حذف مستند',
            message: 'هل أنت متأكد من حذف هذا المستند نهائياً؟ لا يمكن التراجع عن هذا الإجراء.',
            variant: 'destructive',
            confirmLabel: 'حذف المستند',
            onConfirm: async () => {
                try {
                    await archivingService.deleteArchiveRecord(record.id);
                    showStatus({
                        type: 'success',
                        title: 'تم حذف المستند',
                        message: 'تم حذف المستند المؤرشف بنجاح.'
                    });
                    if (currentFolderId) {
                        await loadRecords(currentFolderId, 1);
                    }
                } catch (error) {
                    console.error('Failed to delete record', error);
                    showStatus({
                        type: 'error',
                        title: 'خطأ في الحذف',
                        message: 'تعذر حذف المستند المؤرشف من الخادم.'
                    });
                }
            }
        });
    };

    const handleDownloadRecordZip = async (record: ArchiveRecord) => {
        setDownloadingZipId(record.id);
        setDownloadProgress(0);
        try {
            showStatus({
                type: 'info',
                title: 'تحضير الملفات',
                message: 'جاري تجميع الملفات في ملف ZIP مضغوط وتنزيله...'
            });
            const blob = await archivingService.downloadZip(record.id, {}, (progressEvent) => {
                const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                setDownloadProgress(percentCompleted);
            });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `Record_${record.id}.zip`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Failed to download zip', error);
            showStatus({
                type: 'error',
                title: 'فشل التنزيل',
                message: 'تعذر تجميع وتنزيل الملفات المضغوطة.'
            });
        } finally {
            setDownloadingZipId(null);
            setDownloadProgress(0);
        }
    };

    return {
        records,
        setRecords,
        dynamicTemplates,
        loadingRecords,
        recordsPage,
        hasMoreRecords,
        loadRecords,
        loadMoreRecords,
        showRecordModal,
        setShowRecordModal,
        recordModalMode,
        setRecordModalMode,
        selectedRecord,
        setSelectedRecord,
        selectedTemplateId,
        setSelectedTemplateId,
        handleTemplateIdChange,
        templateInputs,
        setTemplateInputs,
        selectedFiles,
        setSelectedFiles,
        existingFiles,
        setExistingFiles,
        fileIdsToRemove,
        setFileIdsToRemove,
        isSavingRecord,
        uploadProgress,
        downloadingZipId,
        downloadProgress,
        generateQrCover,
        setGenerateQrCover,
        qrCoverGuid,
        setQrCoverGuid,
        handleTemplateInputChange,
        handleOpenCreateRecord,
        handleOpenEditRecord,
        handleSaveRecord,
        handleDeleteRecord,
        handleDownloadRecordZip
    };
}
