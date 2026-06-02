import React, { useState, useEffect, useRef } from 'react';
import { PhysicalFile, ArchiveRecord } from '../model/types';
import { archivingService } from '../api/archivingService';
import { Button } from '@/shared/ui/button';
import { useUIStore } from '@/app/store/uiStore';
import { QRPreviewTemplate } from './QRPreviewTemplate';
import * as htmlToImage from 'html-to-image';
import { 
    FileText, 
    Image, 
    Video, 
    FileIcon, 
    Download, 
    FileSpreadsheet, 
    ExternalLink, 
    AlertCircle,
    Loader2,
    Trash2,
    Upload,
    QrCode
} from 'lucide-react';

interface DocumentGalleryProps {
    recordId: string;
    files: PhysicalFile[];
    onFilesChanged?: () => void;
    record?: ArchiveRecord;
    formName?: string;
}

export const DocumentGallery: React.FC<DocumentGalleryProps> = ({
    recordId,
    files,
    onFilesChanged,
    record,
    formName
}) => {
    const { showConfirm, showStatus } = useUIStore();
    const [localFiles, setLocalFiles] = useState<PhysicalFile[]>(files);
    const [selectedFile, setSelectedFile] = useState<PhysicalFile | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [downloadingFileId, setDownloadingFileId] = useState<string | null>(null);
    const [downloadProgress, setDownloadProgress] = useState<number>(0);
    const [textContent, setTextContent] = useState<string | null>(null);
    const [previewBlobUrl, setPreviewBlobUrl] = useState<string | null>(null);
    const [isUploading, setIsUploading] = useState<boolean>(false);
    const qrCoverRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        setLocalFiles(files);
        if (files && files.length > 0) {
            // المحافظة على الملف المحدد حالياً إن وجد ضمن القائمة الجديدة، وإلا نختار الأول
            setSelectedFile(prev => {
                if (prev && files.some(f => f.id === prev.id)) {
                    return files.find(f => f.id === prev.id) || files[0];
                }
                return files[0];
            });
        } else {
            setSelectedFile(null);
        }
    }, [files]);

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
                } catch (error) {
                    console.error('Failed to load preview blob:', error);
                    setPreviewBlobUrl(null);
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
    }, [selectedFile, recordId]);

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
        try {
            showStatus({
                type: 'info',
                title: 'جاري رفع الملفات',
                message: `جاري رفع عدد ${fileList.length} ملفات جديدة وإضافتها للمستند...`
            });
            const updatedRecord = await archivingService.addFilesToArchiveRecord(recordId, fileList);
            const updatedFiles = updatedRecord.physicalFiles || [];
            setLocalFiles(updatedFiles);
            
            // تحديد أول ملف من الملفات الجديدة المضافة لمعاينته
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

    const handleGenerateAndAddQrCover = async () => {
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
                    const updatedRecord = await archivingService.addFilesToArchiveRecord(recordId, [qrFile]);
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

    const isImageFile = (fileName: string) => {
        const ext = fileName.split('.').pop()?.toLowerCase();
        return ['png', 'jpg', 'jpeg', 'gif', 'webp', 'svg'].includes(ext || '');
    };

    const isVideoFile = (fileName: string) => {
        const ext = fileName.split('.').pop()?.toLowerCase();
        return ['mp4', 'webm', 'ogg', 'mov'].includes(ext || '');
    };

    const isPdfFile = (fileName: string) => {
        return fileName.split('.').pop()?.toLowerCase() === 'pdf';
    };

    const isTextFile = (fileName: string) => {
        const ext = fileName.split('.').pop()?.toLowerCase();
        return ['txt', 'md', 'json', 'xml'].includes(ext || '');
    };

    const isOfficeFile = (fileName: string) => {
        const ext = fileName.split('.').pop()?.toLowerCase();
        return ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx'].includes(ext || '');
    };

    const fetchTextContent = async (file: PhysicalFile) => {
        setLoading(true);
        try {
            const blob = await archivingService.downloadFile(recordId, file.id);
            const text = await blob.text();
            setTextContent(text);
        } catch (error) {
            console.error('Failed to load text content:', error);
            setTextContent('فشل تحميل محتوى الملف النصي.');
        } finally {
            setLoading(false);
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

            // إنشاء رابط تنزيل وحفظ الملف محلياً
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

    const formatBytes = (bytes: number, decimals = 2) => {
        if (!+bytes) return '0 Bytes';
        const k = 1024;
        const dm = decimals < 0 ? 0 : decimals;
        const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
    };

    const getFileIcon = (fileName: string) => {
        if (isImageFile(fileName)) return <Image className="h-4 w-4" />;
        if (isVideoFile(fileName)) return <Video className="h-4 w-4" />;
        if (isPdfFile(fileName)) return <FileIcon className="h-4 w-4 text-red-500" />;
        if (isOfficeFile(fileName)) return <FileSpreadsheet className="h-4 w-4 text-emerald-600" />;
        return <FileText className="h-4 w-4 text-muted-foreground" />;
    };

    const renderPreview = () => {
        if (!selectedFile) return null;

        if (loading) {
            return (
                <div className="flex-1 flex items-center justify-center bg-muted/50 border border-border rounded-2xl p-12">
                    <div className="flex flex-col items-center gap-3">
                        <Loader2 className="h-8 w-8 animate-spin text-primary" />
                        <span className="text-sm font-medium text-muted-foreground">جاري تحميل المعاينة...</span>
                    </div>
                </div>
            );
        }

        // 1. معاينة الصور
        if (isImageFile(selectedFile.fileName) && previewBlobUrl) {
            return (
                <div className="flex-1 flex items-center justify-center bg-slate-900 border border-slate-800 rounded-2xl p-4 overflow-hidden relative group">
                    <img 
                        src={previewBlobUrl} 
                        alt={selectedFile.fileName}
                        className="max-h-[500px] max-w-full object-contain rounded-lg shadow-lg group-hover:scale-[1.01] transition-transform duration-300"
                    />
                </div>
            );
        }

        // 2. معاينة الفيديو
        if (isVideoFile(selectedFile.fileName) && previewBlobUrl) {
            return (
                <div className="flex-1 flex items-center justify-center bg-slate-900 border border-slate-800 rounded-2xl p-2">
                    <video 
                        src={previewBlobUrl} 
                        controls 
                        className="max-h-[500px] w-full rounded-lg shadow-lg"
                    />
                </div>
            );
        }

        // 3. معاينة الـ PDF
        if (isPdfFile(selectedFile.fileName) && previewBlobUrl) {
            return (
                <div className="flex-1 flex flex-col bg-card border border-border rounded-2xl overflow-hidden h-[500px]">
                    <iframe 
                        src={previewBlobUrl} 
                        className="w-full h-full border-none"
                        title={selectedFile.fileName}
                    />
                </div>
            );
        }

        // 4. معاينة ملفات النصوص
        if (isTextFile(selectedFile.fileName) && textContent !== null) {
            return (
                <div className="flex-1 flex flex-col bg-muted/50 border border-border rounded-2xl overflow-hidden p-6 h-[500px]">
                    <div className="flex items-center justify-between border-b pb-3 mb-4">
                        <span className="text-xs text-muted-foreground/60 font-bold">معاينة نصية للمستند</span>
                        <span className="text-xs text-muted-foreground/60">{formatBytes(selectedFile.fileSize)}</span>
                    </div>
                    <pre className="flex-1 overflow-auto text-xs text-foreground font-mono bg-background p-4 rounded-xl border border-border leading-relaxed text-right" style={{ direction: 'ltr' }}>
                        {textContent}
                    </pre>
                </div>
            );
        }

        // 5. ملفات الأوفيس Word/Excel (كرت معلومات أنيق بدون معاينة سحابية)
        if (isOfficeFile(selectedFile.fileName)) {
            return (
                <div className="flex-1 flex items-center justify-center bg-gradient-to-br from-muted/55 to-muted/20 border border-border rounded-3xl p-12">
                    <div className="max-w-md w-full bg-card border border-border rounded-3xl p-8 shadow-xl shadow-background/30 flex flex-col items-center text-center gap-6 relative overflow-hidden">
                        {/* الخلفية المائية */}
                        <div className="absolute -top-10 -right-10 w-32 h-32 bg-primary/5 rounded-full blur-2xl"></div>
                        
                        <div className="w-20 h-20 rounded-2xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center shadow-inner scale-110">
                            <FileSpreadsheet className="h-10 w-10" />
                        </div>
                        
                        <div className="flex flex-col gap-2">
                            <h3 className="text-base font-bold text-foreground break-all px-4">{selectedFile.fileName}</h3>
                            <span className="text-xs text-muted-foreground font-bold">مستند أوفيس (Office Document)</span>
                        </div>

                        <div className="grid grid-cols-2 gap-4 w-full bg-muted/50 p-4 rounded-2xl border border-border text-right text-xs">
                            <div>
                                <span className="text-muted-foreground block mb-0.5">الحجم:</span>
                                <span className="font-bold text-foreground">{formatBytes(selectedFile.fileSize)}</span>
                            </div>
                            <div>
                                <span className="text-muted-foreground block mb-0.5">النوع:</span>
                                <span className="font-bold text-foreground">{selectedFile.fileName.split('.').pop()?.toUpperCase()} file</span>
                            </div>
                        </div>

                        <div className="w-full flex flex-col gap-3">
                            <Button 
                                className="w-full rounded-2xl py-6 font-bold shadow-lg shadow-emerald-500/10 hover:shadow-emerald-500/20 bg-emerald-600 hover:bg-emerald-700 flex items-center justify-center gap-2"
                                onClick={() => handleDownload(selectedFile)}
                                disabled={downloadingFileId === selectedFile.id}
                            >
                                {downloadingFileId === selectedFile.id ? (
                                    <>
                                        <Loader2 className="h-5 w-5 animate-spin" />
                                        <span>جاري التحميل ({downloadProgress}%)</span>
                                    </>
                                ) : (
                                    <>
                                        <Download className="h-5 w-5" />
                                        <span>تحميل الملف الآن</span>
                                    </>
                                )}
                            </Button>
                            <span className="text-[10px] text-muted-foreground/60">
                                * لا يدعم النظام المعاينة التفاعلية المباشرة لملفات الأوفيس لضمان الأمان والسرعة.
                            </span>
                        </div>
                    </div>
                </div>
            );
        }

        // 6. ملفات أخرى غير مدعومة للمعاينة
        return (
            <div className="flex-1 flex items-center justify-center bg-muted/50 border border-border rounded-3xl p-12">
                <div className="max-w-md w-full bg-card border border-border rounded-3xl p-8 shadow-xl shadow-background/30 flex flex-col items-center text-center gap-6">
                    <div className="w-20 h-20 rounded-2xl bg-muted text-muted-foreground flex items-center justify-center shadow-inner">
                        <AlertCircle className="h-10 w-10" />
                    </div>
                    
                    <div className="flex flex-col gap-2">
                        <h3 className="text-base font-bold text-foreground break-all px-4">{selectedFile.fileName}</h3>
                        <span className="text-xs text-muted-foreground font-bold">الملف غير مدعوم للمعاينة المباشرة</span>
                    </div>

                    <Button 
                        className="w-full rounded-2xl py-6 font-bold flex items-center justify-center gap-2"
                        onClick={() => handleDownload(selectedFile)}
                        disabled={downloadingFileId === selectedFile.id}
                    >
                        {downloadingFileId === selectedFile.id ? (
                            <>
                                <Loader2 className="h-5 w-5 animate-spin" />
                                <span>جاري التحميل ({downloadProgress}%)</span>
                            </>
                        ) : (
                            <>
                                <Download className="h-5 w-5" />
                                <span>تحميل الملف</span>
                            </>
                        )}
                    </Button>
                </div>
            </div>
        );
    };

    return (
        <div className="flex flex-col md:flex-row gap-6 bg-card p-2 sm:p-4 rounded-3xl h-full border border-border" dir="rtl">
            {/* القائمة الجانبية للملفات */}
            <div className="w-full md:w-64 border-l border-border pl-0 md:pl-6 flex flex-col gap-4">
                <div className="flex items-center justify-between pb-3 border-b border-border">
                    <h3 className="text-sm font-bold text-foreground">الملفات المرفقة ({localFiles.length})</h3>
                    
                    {isUploading ? (
                        <Loader2 className="h-4 w-4 animate-spin text-primary" />
                    ) : (
                        <div className="flex items-center gap-1.5">
                            {record && (
                                <button
                                    type="button"
                                    onClick={handleGenerateAndAddQrCover}
                                    className="p-1.5 rounded-lg text-amber-500 hover:bg-amber-500/10 transition-colors cursor-pointer"
                                    title="توليد وإدراج صفحة الغلاف (QR)"
                                >
                                    <QrCode className="h-4 w-4" />
                                </button>
                            )}
                            <label className="p-1.5 rounded-lg text-primary hover:bg-primary/10 transition-colors cursor-pointer" title="إضافة ملفات جديدة">
                                <Upload className="h-4 w-4" />
                                <input
                                    type="file"
                                    multiple
                                    className="hidden"
                                    onChange={handleAddFiles}
                                    disabled={isUploading}
                                />
                            </label>
                        </div>
                    )}
                </div>
                
                <div className="flex flex-col gap-2 max-h-[400px] md:max-h-full overflow-y-auto">
                    {localFiles.map((file) => {
                        const isSelected = selectedFile?.id === file.id;
                        const isDownloading = downloadingFileId === file.id;

                        return (
                            <div
                                key={file.id}
                                onClick={() => !isDownloading && setSelectedFile(file)}
                                className={`flex items-center justify-between p-3 rounded-xl cursor-pointer border-2 transition-all ${
                                    isSelected
                                        ? 'bg-primary/5 border-primary shadow-sm'
                                        : 'bg-muted/30 border-transparent hover:border-border'
                                }`}
                            >
                                <div className="flex items-center gap-2.5 overflow-hidden flex-1">
                                    <div className={`p-2 rounded-lg ${isSelected ? 'bg-primary/10 text-primary' : 'bg-muted text-muted-foreground'}`}>
                                        {getFileIcon(file.fileName)}
                                    </div>
                                    <div className="flex flex-col overflow-hidden text-right">
                                        <span className="text-xs font-semibold text-foreground truncate block">
                                            {file.fileName}
                                        </span>
                                        <span className="text-[10px] text-muted-foreground">
                                            {formatBytes(file.fileSize)}
                                        </span>
                                    </div>
                                </div>
                                <div className="flex items-center gap-0.5">
                                    <button
                                        type="button"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleDeleteFile(file);
                                        }}
                                        className="p-1.5 rounded-lg text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
                                        title="حذف الملف"
                                    >
                                        <Trash2 className="h-3.5 w-3.5" />
                                    </button>
                                    <button
                                        type="button"
                                        onClick={(e) => {
                                            e.stopPropagation();
                                            handleDownload(file);
                                        }}
                                        disabled={isDownloading}
                                        className="p-1.5 rounded-lg text-muted-foreground hover:bg-muted hover:text-foreground transition-colors"
                                        title="تحميل الملف"
                                    >
                                        {isDownloading ? (
                                            <Loader2 className="h-3.5 w-3.5 animate-spin text-primary" />
                                        ) : (
                                            <Download className="h-3.5 w-3.5" />
                                        )}
                                    </button>
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>

            {/* مساحة المعاينة الرئيسية */}
            <div className="flex-1 flex flex-col gap-4">
                {selectedFile ? (
                    <>
                        <div className="flex items-center justify-between bg-muted/50 p-4 rounded-2xl border border-border">
                            <div className="flex flex-col gap-0.5 text-right">
                                <span className="text-sm font-bold text-foreground break-all">{selectedFile.fileName}</span>
                                <span className="text-xs text-muted-foreground font-medium">حجم الملف: {formatBytes(selectedFile.fileSize)}</span>
                            </div>
                            <div className="flex gap-2">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    className="rounded-xl flex items-center gap-1.5 text-foreground border-border"
                                    onClick={() => handleDownload(selectedFile)}
                                    disabled={downloadingFileId === selectedFile.id}
                                >
                                    <Download className="h-4 w-4" />
                                    <span>تحميل</span>
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    className="rounded-xl flex items-center gap-1.5 text-foreground border-border"
                                    onClick={() => {
                                        if (previewBlobUrl) {
                                            window.open(previewBlobUrl, '_blank');
                                        } else {
                                            window.open(archivingService.viewFileInlineUrl(recordId, selectedFile.id), '_blank');
                                        }
                                    }}
                                >
                                    <ExternalLink className="h-4 w-4" />
                                    <span>فتح في نافذة جديدة</span>
                                </Button>
                            </div>
                        </div>

                        {renderPreview()}
                    </>
                ) : (
                    <div className="flex-1 flex flex-col items-center justify-center p-12 border-2 border-dashed border-border rounded-3xl bg-muted/20 text-muted-foreground gap-2">
                        <FileIcon className="h-10 w-10 stroke-[1.5]" />
                        <span className="text-sm font-semibold">الرجاء اختيار ملف للمعاينة</span>
                    </div>
                )}
            </div>

            {/* Off-screen QR Preview Template for canvas generation */}
            {record && (
                <div style={{ position: 'absolute', left: '-9999px', top: '-9999px' }}>
                    <QRPreviewTemplate
                        ref={qrCoverRef}
                        guid={recordId}
                        archivalNumber={record.archivalNumber}
                        formName={formName}
                        content={record.archiveRecordTemplateValues?.archiveRecordFormInputValues || []}
                    />
                </div>
            )}
        </div>
    );
};
