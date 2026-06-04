import React from 'react';
import { PhysicalFile } from '../model/types';
import { Button } from '@/shared/ui/button';
import { 
    isImageFile, 
    isVideoFile, 
    isPdfFile, 
    isTextFile, 
    isOfficeFile 
} from '../hooks/useDocumentPreview';
import { 
    Loader2, 
    FileSpreadsheet, 
    Download, 
    AlertCircle, 
    FileIcon 
} from 'lucide-react';

interface DocumentPreviewRendererProps {
    selectedFile: PhysicalFile | null;
    loading: boolean;
    previewBlobUrl: string | null;
    textContent: string | null;
    downloadingFileId: string | null;
    downloadProgress: number;
    onDownload: (file: PhysicalFile) => void;
}

const formatBytes = (bytes: number, decimals = 2) => {
    if (!+bytes) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
};

export const DocumentPreviewRenderer: React.FC<DocumentPreviewRendererProps> = ({
    selectedFile,
    loading,
    previewBlobUrl,
    textContent,
    downloadingFileId,
    downloadProgress,
    onDownload
}) => {
    if (!selectedFile) {
        return (
            <div className="flex-1 flex flex-col items-center justify-center p-12 border-2 border-dashed border-border rounded-3xl bg-muted/20 text-muted-foreground gap-2">
                <FileIcon className="h-10 w-10 stroke-[1.5]" />
                <span className="text-sm font-semibold">الرجاء اختيار ملف للمعاينة</span>
            </div>
        );
    }

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

    // 1. Image Preview
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

    // 2. Video Preview
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

    // 3. PDF Preview
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

    // 4. Text Preview
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

    // 5. Office Preview (Card representation without active frame)
    if (isOfficeFile(selectedFile.fileName)) {
        return (
            <div className="flex-1 flex items-center justify-center bg-gradient-to-br from-muted/55 to-muted/20 border border-border rounded-3xl p-12">
                <div className="max-w-md w-full bg-card border border-border rounded-3xl p-8 shadow-xl shadow-background/30 flex flex-col items-center text-center gap-6 relative overflow-hidden">
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
                            onClick={() => onDownload(selectedFile)}
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

    // 6. Unsupported File Type
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
                    onClick={() => onDownload(selectedFile)}
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
