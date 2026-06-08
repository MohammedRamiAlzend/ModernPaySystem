import React, { useRef } from 'react';
import { PhysicalFile, ArchiveRecord } from '../model/types';
import { archivingService } from '../api/archivingService';
import { Button } from '@/shared/ui/button';
import { QRPreviewTemplate } from './QRPreviewTemplate';
import { 
    useDocumentPreview, 
    isImageFile, 
    isPdfFile 
} from '../hooks/useDocumentPreview';
import { DocumentPreviewRenderer } from './DocumentPreviewRenderer';
import { DocumentGallerySidebar } from './DocumentGallerySidebar';
import { 
    Download, 
    ExternalLink, 
    Printer,
    FileIcon
} from 'lucide-react';

interface DocumentGalleryProps {
    recordId: string;
    files: PhysicalFile[];
    onFilesChanged?: () => void;
    record?: ArchiveRecord;
    formName?: string;
}

const formatBytes = (bytes: number, decimals = 2) => {
    if (!+bytes) return '0 Bytes';
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
};

const printBlob = (blob: Blob, isPdf: boolean) => {
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
        if (isPdf) {
            iframe.src = url;
            iframe.onload = () => {
                iframe.contentWindow?.focus();
                iframe.contentWindow?.print();
            };
        } else {
            doc.write(`<!DOCTYPE html>
                <html>
                    <head>
                        <title>طباعة مستند</title>
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
    }
    
    setTimeout(() => {
        document.body.removeChild(iframe);
        URL.revokeObjectURL(url);
    }, 5000);
};

export const DocumentGallery: React.FC<DocumentGalleryProps> = ({
    recordId,
    files,
    onFilesChanged,
    record,
    formName
}) => {
    const qrCoverRef = useRef<HTMLDivElement>(null);

    const {
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
    } = useDocumentPreview({
        recordId,
        files,
        onFilesChanged,
        record
    });

    return (
        <div className="flex flex-col md:flex-row gap-6 bg-card p-2 sm:p-4 rounded-3xl h-full border border-border" dir="rtl">
            {/* Sidebar List of Files */}
            <DocumentGallerySidebar
                localFiles={localFiles}
                selectedFile={selectedFile}
                setSelectedFile={setSelectedFile}
                record={record}
                formName={formName}
                isUploading={isUploading}
                uploadProgress={uploadProgress}
                downloadingFileId={downloadingFileId}
                downloadProgress={downloadProgress}
                onGenerateAndAddQrCover={() => handleGenerateAndAddQrCover(qrCoverRef, printBlob)}
                onAddFiles={handleAddFiles}
                onDeleteFile={handleDeleteFile}
                onDownload={handleDownload}
            />

            {/* Main Preview Area */}
            <div className="flex-1 flex flex-col gap-4 min-w-0">
                {selectedFile ? (
                    <>
                        <div className="flex items-center justify-between bg-muted/50 p-4 rounded-2xl border border-border">
                            <div className="flex flex-col gap-0.5 text-right">
                                <span className="text-sm font-bold text-foreground break-all">{selectedFile.fileName}</span>
                                <span className="text-xs text-muted-foreground font-medium">حجم الملف: {formatBytes(selectedFile.fileSize)}</span>
                            </div>
                            <div className="flex gap-2">
                                {selectedFile && (isImageFile(selectedFile.fileName) || isPdfFile(selectedFile.fileName)) && (
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="rounded-xl flex items-center gap-1.5 text-foreground border-border"
                                        onClick={async () => {
                                            try {
                                                const blob = await archivingService.viewFileBlob(recordId, selectedFile.id);
                                                printBlob(blob, isPdfFile(selectedFile.fileName));
                                            } catch (err) {
                                                console.error('Failed to print file', err);
                                            }
                                        }}
                                    >
                                        <Printer className="h-4 w-4 text-amber-500" />
                                        <span>طباعة</span>
                                    </Button>
                                )}
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

                        <DocumentPreviewRenderer
                            selectedFile={selectedFile}
                            loading={loading}
                            previewBlobUrl={previewBlobUrl}
                            textContent={textContent}
                            downloadingFileId={downloadingFileId}
                            downloadProgress={downloadProgress}
                            onDownload={handleDownload}
                        />
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
