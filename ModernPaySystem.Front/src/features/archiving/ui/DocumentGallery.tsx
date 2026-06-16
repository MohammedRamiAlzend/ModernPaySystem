import React, { useRef, useState } from 'react';
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
import { useUIStore } from '@/app/store/uiStore';
import {
    Download,
    ExternalLink,
    Printer,
    FileIcon,
    Loader2
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

const convertImageBlobToPdfBlob = async (imageBlob: Blob): Promise<Blob> => {
    const { jsPDF } = await import('jspdf');
    const pdf = new jsPDF();
    const pageWidth = pdf.internal.pageSize.getWidth();
    const pageHeight = pdf.internal.pageSize.getHeight();

    const dataUrl = await new Promise<string>((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = (e) => resolve(e.target?.result as string);
        reader.onerror = reject;
        reader.readAsDataURL(imageBlob);
    });

    if (dataUrl && dataUrl.startsWith('data:')) {
        const img = new Image();
        await new Promise((resolve, reject) => {
            img.onload = resolve;
            img.onerror = reject;
            img.src = dataUrl;
        });

        const imgWidth = img.width;
        const imgHeight = img.height;

        const ratio = Math.min(pageWidth / imgWidth, pageHeight / imgHeight);
        const finalWidth = imgWidth * ratio;
        const finalHeight = imgHeight * ratio;

        const x = (pageWidth - finalWidth) / 2;
        const y = (pageHeight - finalHeight) / 2;

        pdf.addImage(dataUrl, 'JPEG', x, y, finalWidth, finalHeight, undefined, 'FAST');
    }

    return pdf.output('blob');
};

export const DocumentGallery: React.FC<DocumentGalleryProps> = ({
    recordId,
    files,
    onFilesChanged,
    record,
    formName
}) => {
    const qrCoverRef = useRef<HTMLDivElement>(null);
    const { showStatus } = useUIStore();
    const [isConvertingPdf, setIsConvertingPdf] = useState(false);
    const [isPrinting, setIsPrinting] = useState(false);
    const [isDownloadingPdf, setIsDownloadingPdf] = useState(false);

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

    const handleDownloadImagesAsPdf = async () => {
        const imageFiles = localFiles.filter(f => isImageFile(f.fileName));
        if (imageFiles.length === 0) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'لا توجد مرفقات صور في هذا المستند لتجميعها.'
            });
            return;
        }

        setIsConvertingPdf(true);
        try {
            const { jsPDF } = await import('jspdf');
            const pdf = new jsPDF();
            const pageWidth = pdf.internal.pageSize.getWidth();
            const pageHeight = pdf.internal.pageSize.getHeight();

            let isFirstPage = true;

            for (const file of imageFiles) {
                try {
                    const imageBlob = await archivingService.viewFileBlob(recordId, file.id, file.fileName);
                    const dataUrl = await new Promise<string>((resolve, reject) => {
                        const reader = new FileReader();
                        reader.onload = (e) => resolve(e.target?.result as string);
                        reader.onerror = reject;
                        reader.readAsDataURL(imageBlob);
                    });

                    if (dataUrl && dataUrl.startsWith('data:')) {
                        const img = new window.Image();
                        await new Promise((resolve, reject) => {
                            img.onload = resolve;
                            img.onerror = reject;
                            img.src = dataUrl;
                        });

                        if (!isFirstPage) {
                            pdf.addPage();
                        } else {
                            isFirstPage = false;
                        }

                        const imgWidth = img.width;
                        const imgHeight = img.height;

                        const ratio = Math.min(pageWidth / imgWidth, pageHeight / imgHeight);
                        const finalWidth = imgWidth * ratio;
                        const finalHeight = imgHeight * ratio;

                        const x = (pageWidth - finalWidth) / 2;
                        const y = (pageHeight - finalHeight) / 2;

                        let format = 'JPEG';
                        const ext = file.fileName.split('.').pop()?.toLowerCase();
                        if (ext === 'png') format = 'PNG';
                        else if (ext === 'webp') format = 'WEBP';

                        pdf.addImage(dataUrl, format, x, y, finalWidth, finalHeight, undefined, 'FAST');
                    }
                } catch (fileErr) {
                    console.error(`Failed to process file ${file.fileName} for PDF compilation`, fileErr);
                }
            }

            const docName = record?.archivalNumber
                ? `document_${record.archivalNumber}_images`
                : `document_images_${recordId.substring(0, 8)}`;

            pdf.save(`${docName}.pdf`);

            showStatus({
                type: 'success',
                title: 'نجاح العملية',
                message: 'تم تجميع وتنزيل صور المرفقات كملف PDF بنجاح.'
            });
        } catch (err) {
            console.error('Failed to generate compiled PDF', err);
            showStatus({
                type: 'error',
                title: 'خطأ',
                message: 'حدث خطأ أثناء محاولة تجميع وتنزيل الصور كملف PDF.'
            });
        } finally {
            setIsConvertingPdf(false);
        }
    };

    return (
        <div className="flex flex-col md:flex-row gap-6 bg-card p-2 sm:p-4 rounded-3xl h-full border border-border" dir="rtl">
            {/* Sidebar List of Files */}
            <DocumentGallerySidebar
                localFiles={localFiles}
                selectedFile={selectedFile}
                setSelectedFile={setSelectedFile}
                record={record}
                formName={formName}
                isUploading={isUploading || isConvertingPdf}
                uploadProgress={uploadProgress}
                downloadingFileId={downloadingFileId}
                downloadProgress={downloadProgress}
                onGenerateAndAddQrCover={() => handleGenerateAndAddQrCover(qrCoverRef, printBlob)}
                onAddFiles={handleAddFiles}
                onDeleteFile={handleDeleteFile}
                onDownload={handleDownload}
                onDownloadImagesAsPdf={handleDownloadImagesAsPdf}
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
                            <div className="flex gap-2 font-semibold">
                                {selectedFile && (isImageFile(selectedFile.fileName) || isPdfFile(selectedFile.fileName)) && (
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="rounded-xl flex items-center gap-1.5 text-foreground border-border"
                                        disabled={isPrinting || isDownloadingPdf || downloadingFileId === selectedFile.id || isConvertingPdf}
                                        onClick={async () => {
                                            if (isPrinting) return;
                                            setIsPrinting(true);
                                            try {
                                                let blob = await archivingService.viewFileBlob(recordId, selectedFile.id, selectedFile.fileName);
                                                const isImage = isImageFile(selectedFile.fileName);
                                                const isPdf = isPdfFile(selectedFile.fileName);
                                                
                                                if (isImage) {
                                                    blob = await convertImageBlobToPdfBlob(blob);
                                                }
                                                
                                                printBlob(blob, isPdf || isImage);
                                                archivingService.logPrint(recordId).catch(() => { });
                                            } catch (err) {
                                                console.error('Failed to print file', err);
                                            } finally {
                                                setIsPrinting(false);
                                            }
                                        }}
                                    >
                                        {isPrinting ? (
                                            <Loader2 className="h-4 w-4 animate-spin text-amber-500" />
                                        ) : (
                                            <Printer className="h-4 w-4 text-amber-500" />
                                        )}
                                        <span>طباعة{isImageFile(selectedFile.fileName) && ' كـ PDF'}</span>
                                    </Button>
                                )}
                                {selectedFile && isImageFile(selectedFile.fileName) && (
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="rounded-xl flex items-center gap-1.5 text-foreground border-border"
                                        disabled={isPrinting || isDownloadingPdf || downloadingFileId === selectedFile.id || isConvertingPdf}
                                        onClick={async () => {
                                            if (isDownloadingPdf) return;
                                            setIsDownloadingPdf(true);
                                            try {
                                                const rawBlob = await archivingService.viewFileBlob(recordId, selectedFile.id, selectedFile.fileName);
                                                const pdfBlob = await convertImageBlobToPdfBlob(rawBlob);
                                                const url = URL.createObjectURL(pdfBlob);
                                                const a = document.createElement('a');
                                                a.href = url;
                                                a.download = `${selectedFile.fileName.substring(0, selectedFile.fileName.lastIndexOf('.')) || selectedFile.fileName}.pdf`;
                                                document.body.appendChild(a);
                                                a.click();
                                                document.body.removeChild(a);
                                                URL.revokeObjectURL(url);
                                            } catch (err) {
                                                console.error('Failed to download image as PDF', err);
                                            } finally {
                                                setIsDownloadingPdf(false);
                                            }
                                        }}
                                    >
                                        {isDownloadingPdf ? (
                                            <Loader2 className="h-4 w-4 animate-spin text-rose-500" />
                                        ) : (
                                            <FileIcon className="h-4 w-4 text-rose-500" />
                                        )}
                                        <span>تحميل كـ PDF</span>
                                    </Button>
                                )}
                                <Button
                                    variant="outline"
                                    size="sm"
                                    className="rounded-xl flex items-center gap-1.5 text-foreground border-border"
                                    onClick={() => handleDownload(selectedFile)}
                                    disabled={isPrinting || isDownloadingPdf || downloadingFileId === selectedFile.id || isConvertingPdf}
                                >
                                    <Download className="h-4 w-4 text-primary" />
                                    <span>تحميل</span>
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    className="rounded-xl flex items-center gap-1.5 text-foreground border-border"
                                    disabled={isPrinting || isDownloadingPdf || downloadingFileId === selectedFile.id || isConvertingPdf}
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
