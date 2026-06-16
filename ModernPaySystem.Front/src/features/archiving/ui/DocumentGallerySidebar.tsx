import React, { useState } from 'react';
import { PhysicalFile, ArchiveRecord } from '../model/types';
import { Progress } from '@/shared/ui/progress';
import {
    isImageFile,
    isVideoFile,
    isPdfFile,
    isOfficeFile
} from '../hooks/useDocumentPreview';
import {
    Image,
    Video,
    FileIcon,
    FileSpreadsheet,
    FileText,
    Loader2,
    QrCode,
    // Upload,
    Download,
    FileImage
} from 'lucide-react';

interface DocumentGallerySidebarProps {
    localFiles: PhysicalFile[];
    selectedFile: PhysicalFile | null;
    setSelectedFile: (file: PhysicalFile | null) => void;
    record: ArchiveRecord | undefined;
    formName?: string;
    isUploading: boolean;
    uploadProgress: number;
    downloadingFileId: string | null;
    downloadProgress: number;
    onGenerateAndAddQrCover: () => void;
    onAddFiles: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onDeleteFile: (file: PhysicalFile) => void;
    onDownload: (file: PhysicalFile) => void;
    onDownloadImagesAsPdf?: () => void;
}

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

export const DocumentGallerySidebar: React.FC<DocumentGallerySidebarProps> = ({
    localFiles,
    selectedFile,
    setSelectedFile,
    record,
    formName,
    isUploading,
    uploadProgress,
    downloadingFileId,
    downloadProgress,
    onGenerateAndAddQrCover,
    onDownload,
    onDownloadImagesAsPdf
}) => {
    const [activeTab, setActiveTab] = useState<'files' | 'fields'>('files');
    const hasQrPage = localFiles.some((file) => file.isQrPage);
    const hasFields = !!(record?.archiveRecordTemplateValues?.archiveRecordFormInputValues && record.archiveRecordTemplateValues.archiveRecordFormInputValues.length > 0);
    const imageFilesCount = localFiles.filter(file => isImageFile(file.fileName)).length;

    return (
        <div className="w-full md:w-80 border-l border-border pl-0 md:pl-6 flex flex-col gap-4">
            <div className="flex flex-col gap-3 pb-3 border-b border-border">
                <div className="flex items-center justify-between gap-4">
                    {/* Tab Switcher */}
                    <div className="flex bg-muted/50 p-0.5 rounded-lg border border-border/40 flex-1">
                        <button
                            type="button"
                            onClick={() => setActiveTab('files')}
                            className={`flex-1 text-center py-1.5 text-[11px] font-bold rounded-md transition-all ${activeTab === 'files'
                                ? 'bg-card text-foreground shadow-sm'
                                : 'text-muted-foreground hover:text-foreground'
                                }`}
                        >
                            الملفات ({localFiles.length})
                        </button>
                        {hasFields && (
                            <button
                                type="button"
                                onClick={() => setActiveTab('fields')}
                                className={`flex-1 text-center py-1.5 text-[11px] font-bold rounded-md transition-all ${activeTab === 'fields'
                                    ? 'bg-card text-foreground shadow-sm'
                                    : 'text-muted-foreground hover:text-foreground'
                                    }`}
                            >
                                البيانات
                            </button>
                        )}
                    </div>

                    {activeTab === 'files' && (
                        isUploading ? (
                            <Loader2 className="h-4 w-4 animate-spin text-primary flex-shrink-0" />
                        ) : (
                            <div className="flex items-center gap-1.5 flex-shrink-0">
                                {imageFilesCount > 0 && onDownloadImagesAsPdf && (
                                    <button
                                        type="button"
                                        onClick={onDownloadImagesAsPdf}
                                        className="p-1.5 rounded-lg text-rose-500 hover:bg-rose-500/10 cursor-pointer transition-colors"
                                        title="تنزيل كافة الصور كملف PDF مجمع"
                                    >
                                        <FileImage className="h-4 w-4" />
                                    </button>
                                )}
                                {record && (
                                    <button
                                        type="button"
                                        onClick={onGenerateAndAddQrCover}
                                        disabled={hasQrPage}
                                        className={`p-1.5 rounded-lg transition-colors ${hasQrPage
                                            ? 'text-muted-foreground opacity-55 cursor-not-allowed'
                                            : 'text-amber-500 hover:bg-amber-500/10 cursor-pointer'
                                            }`}
                                        title={hasQrPage ? "تم توليد صفحة غلاف لهذا المستند بالفعل" : "توليد وإدراج صفحة الغلاف (QR)"}
                                    >
                                        <QrCode className="h-4 w-4" />
                                    </button>
                                )}
                            </div>
                        )
                    )}
                </div>

                {activeTab === 'files' && isUploading && (
                    <div className="flex flex-col gap-1.5 animate-in fade-in slide-in-from-top-1 duration-200">
                        <div className="flex justify-between text-[10px] font-bold text-primary">
                            <span>جاري الرفع...</span>
                            <span>{uploadProgress}%</span>
                        </div>
                        <Progress value={uploadProgress} className="h-1" />
                    </div>
                )}
            </div>

            {activeTab === 'files' ? (
                <div className="flex flex-col gap-2 flex-1 min-h-0 overflow-y-auto pr-1">
                    {localFiles.map((file) => {
                        const isSelected = selectedFile?.id === file.id;
                        const isDownloading = downloadingFileId === file.id;

                        return (
                            <React.Fragment key={file.id}>
                                <div
                                    onClick={() => !isDownloading && setSelectedFile(file)}
                                    className={`flex items-center justify-between p-3 rounded-xl cursor-pointer border-2 transition-all ${isSelected
                                        ? 'bg-primary/5 border-primary shadow-sm'
                                        : 'bg-muted/30 border-transparent hover:border-border'
                                        }`}
                                >
                                    <div className="flex items-center gap-2.5 overflow-hidden flex-1">
                                        <div className={`p-2 rounded-lg ${isSelected ? 'bg-primary/10 text-primary' : 'bg-muted text-muted-foreground'}`}>
                                            {getFileIcon(file.fileName)}
                                        </div>
                                        <div className="flex flex-col overflow-hidden text-right flex-1">
                                            <span className="text-xs font-semibold text-foreground truncate block">
                                                {file.fileName}
                                            </span>
                                            <div className="flex items-center gap-1.5 flex-wrap">
                                                <span className={`text-[10px] ${isSelected ? 'text-primary/70' : 'text-muted-foreground'}`}>
                                                    {formatBytes(file.fileSize)}
                                                </span>
                                                {file.isQrPage && (
                                                    <span className="inline-flex items-center px-1.5 py-0.5 rounded text-[8px] font-black bg-amber-500/15 text-amber-600 border border-amber-500/20">
                                                        صفحة الغلاف (QR)
                                                    </span>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                    <div className="flex flex-col items-end gap-1">
                                        <div className="flex items-center gap-0.5">
                                            <button
                                                type="button"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    onDownload(file);
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
                                </div>
                                {isDownloading && (
                                    <div className="px-1 mb-2 -mt-1 animate-in fade-in slide-in-from-top-1 duration-200">
                                        <Progress value={downloadProgress} className="h-0.5 bg-primary/10" />
                                    </div>
                                )}
                            </React.Fragment>
                        );
                    })}
                </div>
            ) : (
                <div className="flex flex-col gap-3 flex-1 min-h-0 overflow-y-auto pr-1">
                    {formName && (
                        <div className="bg-muted/30 flex items-center justify-center gap-1 border border-border/40 rounded-xl p-3 text-right">
                            <span className="text-[10px] text-muted-foreground font-bold block">نموذج الأرشفة :  </span>
                            <span className="text-xs font-semibold text-foreground">{formName}</span>
                        </div>
                    )}
                    <div className="flex flex-col gap-2">
                        {record?.archiveRecordTemplateValues?.archiveRecordFormInputValues.map((field, idx) => (
                            <div key={idx} className="bg-muted/10 border border-border/40 rounded-xl p-3 flex flex-col gap-1 text-right">
                                <span className="text-[10px] font-bold text-muted-foreground">{field.key}</span>
                                <span className="text-xs font-semibold text-foreground break-words">{field.value || '-'}</span>
                            </div>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
};
