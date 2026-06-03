import React, { useState, useEffect, useCallback } from 'react';
import { PhysicalFile, ArchiveRecord } from '../model/types';
import { archivingService } from '../api/archivingService';
import { useUIStore } from '@/app/store/uiStore';
import * as htmlToImage from 'html-to-image';

export const isImageFile = (fileName: string) => {
    const ext = fileName.split('.').pop()?.toLowerCase();
    return ['png', 'jpg', 'jpeg', 'gif', 'webp', 'svg'].includes(ext || '');
};

export const isVideoFile = (fileName: string) => {
    const ext = fileName.split('.').pop()?.toLowerCase();
    return ['mp4', 'webm', 'ogg', 'mov'].includes(ext || '');
};

export const isPdfFile = (fileName: string) => {
    return fileName.split('.').pop()?.toLowerCase() === 'pdf';
};

export const isTextFile = (fileName: string) => {
    const ext = fileName.split('.').pop()?.toLowerCase();
    return ['txt', 'md', 'json', 'xml'].includes(ext || '');
};

export const isOfficeFile = (fileName: string) => {
    const ext = fileName.split('.').pop()?.toLowerCase();
    return ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx'].includes(ext || '');
};

interface UseDocumentPreviewParams {
    recordId: string;
    files: PhysicalFile[];
    onFilesChanged?: () => void;
    record?: ArchiveRecord;
}

export function useDocumentPreview({
    recordId,
    files,
    onFilesChanged,
    record
}: UseDocumentPreviewParams) {
    const { showConfirm, showStatus } = useUIStore();
    const [localFiles, setLocalFiles] = useState<PhysicalFile[]>(files);
    const [prevFiles, setPrevFiles] = useState<PhysicalFile[]>(files);
    const [selectedFile, setSelectedFile] = useState<PhysicalFile | null>(null);

    // Sync prop files to state
    if (files !== prevFiles) {
        setLocalFiles(files);
        setPrevFiles(files);
        if (files && files.length > 0) {
            setSelectedFile(prev => {
                if (prev && files.some(f => f.id === prev.id)) {
                    return files.find(f => f.id === prev.id) || files[0];
                }
                return files[0];
            });
        } else {
            setSelectedFile(null);
        }
    }

    const [loading, setLoading] = useState<boolean>(false);
    const [downloadingFileId, setDownloadingFileId] = useState<string | null>(null);
    const [downloadProgress, setDownloadProgress] = useState<number>(0);
    const [uploadProgress, setUploadProgress] = useState<number>(0);
    const [textContent, setTextContent] = useState<string | null>(null);
    const [previewBlobUrl, setPreviewBlobUrl] = useState<string | null>(null);
    const [isUploading, setIsUploading] = useState<boolean>(false);

    const fetchTextContent = useCallback(async (file: PhysicalFile) => {
        setLoading(true);
        try {
            const blob = await archivingService.downloadFile(recordId, file.id);
            const text = await blob.text();
            setTextContent(text);
        } catch (error) {
            console.error('Failed to load text content:', error);
            setTextContent('فشل تحميل محتوى الملف نصي.');
        } finally {
            setLoading(false);
        }
    }, [recordId]);

    useEffect(() => {
        let activeUrl: string | null = null;

        const loadPreview = async () => {
            if (!selectedFile) {
                setPreviewBlobUrl(null);
                setTextContent(null);
                return;
            }

            const isText = isTextFile(selectedFile.fileName);
            const isImg = isImageFile(selectedFile.fileName);
            const isVid = isVideoFile(selectedFile.fileName);
            const isPdf = isPdfFile(selectedFile.fileName);

            if (isText) {
                setPreviewBlobUrl(null);
                fetchTextContent(selectedFile);
            } else if (isImg || isVid || isPdf) {
                setTextContent(null);
                setLoading(true);
                try {
                    const blob = await archivingService.viewFileBlob(recordId, selectedFile.id);
                    const url = URL.createObjectURL(blob);
                    activeUrl = url;
                    setPreviewBlobUrl(url);
                } catch (error: any) {
                    console.error('Failed to load preview blob:', error);
                    setPreviewBlobUrl(null);
                    if (error?.response?.status === 410) {
                        showStatus({
                            type: 'error',
                            title: 'الملف غير موجود',
                            message: 'هذا الملف تم حذفه أو غير متوفر حالياً على الخادم (خطأ 410).'
                        });
                    }
                } finally {
                    setLoading(false);
                }
            } else {
                setTextContent(null);
                setPreviewBlobUrl(null);
            }
        };

        loadPreview();

        return () => {
            if (activeUrl) {
                URL.revokeObjectURL(activeUrl);
            }
        };
    }, [selectedFile, recordId, fetchTextContent, showStatus]);

    const handleDeleteFile = (file: PhysicalFile) => {
        showConfirm({
            title: 'حذف ملف مرفق',
            message: `هل أنت متأكد من حذف الملف "${file.fileName}" نهائياً من هذا المستند؟`,
            variant: 'destructive',
            confirmLabel: 'حذف الملف',
            onConfirm: async () => {
                try {
                    await archivingService.removeFileFromArchiveRecord(recordId, file.id);
                    setLocalFiles(prev => {
                        const updated = prev.filter(f => f.id !== file.id);
                        if (selectedFile?.id === file.id) {
                            setSelectedFile(updated.length > 0 ? updated[0] : null);
                        }
                        return updated;
                    });
                    showStatus({
                        type: 'success',
                        title: 'تم حذف الملف',
                        message: `تمت إزالة الملف "${file.fileName}" بنجاح.`
                    });
                    onFilesChanged?.();
                } catch (err) {
                    console.error('Failed to delete file', err);
                    showStatus({
                        type: 'error',
                        title: 'خطأ في الحذف',
                        message: 'تعذر حذف الملف المرفق من الخادم.'
                    });
                }
            }
        });
    };

    const handleAddFiles = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const fileList = Array.from(e.target.files || []);
        if (fileList.length === 0) return;

        setIsUploading(true);
        setUploadProgress(0);
        try {
            showStatus({
                type: 'info',
                title: 'جاري رفع الملفات',
                message: `جاري رفع عدد ${fileList.length} ملفات جديدة وإضافتها للمستند...`
            });
            const updatedRecord = await archivingService.addFilesToArchiveRecord(recordId, fileList, (progressEvent) => {
                const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                setUploadProgress(percentCompleted);
            });
            const updatedFiles = updatedRecord.physicalFiles || [];
            setLocalFiles(updatedFiles);
            
            if (fileList.length > 0) {
                const newAdded = updatedFiles.filter(uf => !localFiles.some(lf => lf.id === uf.id));
                if (newAdded.length > 0) {
                    setSelectedFile(newAdded[0]);
                }
            }

            showStatus({
                type: 'success',
                title: 'تمت الإضافة بنجاح',
                message: 'تم رفع الملفات الجديدة وإضافتها للمستند بنجاح.'
            });
            onFilesChanged?.();
        } catch (err) {
            console.error('Failed to add files', err);
            showStatus({
                type: 'error',
                title: 'خطأ في الرفع',
                message: 'تعذر رفع وإضافة الملفات الجديدة إلى المستند.'
            });
        } finally {
            setIsUploading(false);
            if (e.target) {
                e.target.value = '';
            }
        }
    };

    const handleDownload = async (file: PhysicalFile) => {
        setDownloadingFileId(file.id);
        setDownloadProgress(0);
        try {
            const blob = await archivingService.downloadFile(recordId, file.id, (progressEvent: any) => {
                const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                setDownloadProgress(percentCompleted);
            });

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = file.fileName;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        } catch (error) {
            console.error('Download failed:', error);
        } finally {
            setDownloadingFileId(null);
            setDownloadProgress(0);
        }
    };

    const handleGenerateAndAddQrCover = async (
        qrCoverRef: React.RefObject<HTMLDivElement | null>,
        printBlob: (blob: Blob, isPdf: boolean) => void
    ) => {
        if (!record) return;
        setIsUploading(true);
        try {
            showStatus({
                type: 'info',
                title: 'جاري توليد صفحة الغلاف',
                message: 'يتم الآن تصميم وتوليد صفحة غلاف QR كملف صورة...'
            });
            await new Promise(resolve => setTimeout(resolve, 300));
            if (qrCoverRef.current) {
                const blob = await htmlToImage.toBlob(qrCoverRef.current, {
                    pixelRatio: 2,
                    backgroundColor: '#ffffff'
                });
                if (blob) {
                    const qrFile = new File([blob], `QR_Cover_${record.archivalNumber}.png`, { type: 'image/png' });
                    showStatus({
                        type: 'info',
                        title: 'جاري إدراج صفحة الغلاف',
                        message: 'يتم الآن رفع وإدراج صفحة غلاف QR إلى المستند...'
                    });
                    const updatedRecord = await archivingService.addFilesToArchiveRecord(recordId, [qrFile], (progressEvent) => {
                        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
                        setUploadProgress(percentCompleted);
                    });
                    const updatedFiles = updatedRecord.physicalFiles || [];
                    setLocalFiles(updatedFiles);
                    const newAdded = updatedFiles.find(uf => uf.fileName.startsWith('QR_Cover_'));
                    if (newAdded) setSelectedFile(newAdded);
                    showStatus({
                        type: 'success',
                        title: 'تمت إضافة صفحة الغلاف',
                        message: 'تم توليد صفحة غلاف QR وإدراجها بنجاح.'
                    });
                    onFilesChanged?.();

                    printBlob(blob, false);
                }
            }
        } catch (err) {
            console.error('Failed to generate QR cover', err);
            showStatus({
                type: 'error',
                title: 'خطأ في توليد الغلاف',
                message: 'تعذر توليد صفحة غلاف QR، يرجى المحاولة.'
            });
        } finally {
            setIsUploading(false);
        }
    };

    return {
        localFiles,
        selectedFile,
        setSelectedFile,
        loading,
        downloadingFileId,
        downloadProgress,
        uploadProgress,
        textContent,
        previewBlobUrl,
        isUploading,
        handleDeleteFile,
        handleAddFiles,
        handleDownload,
        handleGenerateAndAddQrCover
    };
}
