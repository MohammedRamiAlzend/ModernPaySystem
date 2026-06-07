import React from 'react';
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
    Trash2,
    Download
} from 'lucide-react';

interface DocumentGallerySidebarProps {
    localFiles: PhysicalFile[];
    selectedFile: PhysicalFile | null;
    setSelectedFile: (file: PhysicalFile | null) => void;
    record: ArchiveRecord | undefined;
    isUploading: boolean;
    uploadProgress: number;
    downloadingFileId: string | null;
    downloadProgress: number;
    onGenerateAndAddQrCover: () => void;
    onAddFiles: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onDeleteFile: (file: PhysicalFile) => void;
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
    isUploading,
    uploadProgress,
    downloadingFileId,
    downloadProgress,
    onGenerateAndAddQrCover,
    // onAddFiles,
    onDeleteFile,
    onDownload
}) => {
    const hasQrPage = localFiles.some((file) => file.isQrPage);

    return (
        <div className="w-full md:w-64 border-l border-border pl-0 md:pl-6 flex flex-col gap-4">
            <div className="flex flex-col gap-3 pb-3 border-b border-border">
                <div className="flex items-center justify-between">
                    <h3 className="text-sm font-bold text-foreground">الملفات المرفقة  ({localFiles.length})</h3>

                    {isUploading ? (
                        <Loader2 className="h-4 w-4 animate-spin text-primary" />
                    ) : (
                        <div className="flex items-center gap-1.5">
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
                            {/* <label className="p-1.5 rounded-lg text-primary hover:bg-primary/10 transition-colors cursor-pointer" title="إضافة ملفات جديدة">
                                <Upload className="h-4 w-4" />
                                <input
                                    type="file"
                                    multiple
                                    className="hidden"
                                    onChange={onAddFiles}
                                    disabled={isUploading}
                                />
                            </label> */}
                        </div>
                    )}
                </div>

                {isUploading && (
                    <div className="flex flex-col gap-1.5 animate-in fade-in slide-in-from-top-1 duration-200">
                        <div className="flex justify-between text-[10px] font-bold text-primary">
                            <span>جاري الرفع...</span>
                            <span>{uploadProgress}%</span>
                        </div>
                        <Progress value={uploadProgress} className="h-1" />
                    </div>
                )}
            </div>

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
                                        {/* <button
                                            type="button"
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                onDeleteFile(file);
                                            }}
                                            className="p-1.5 rounded-lg text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
                                            title="حذف الملف"
                                        >
                                            <Trash2 className="h-3.5 w-3.5" />
                                        </button> */}
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
        </div>
    );
};
