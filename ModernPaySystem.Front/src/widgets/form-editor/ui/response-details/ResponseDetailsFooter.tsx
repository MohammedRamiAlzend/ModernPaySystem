import React from 'react';
import { Button } from '@/shared/ui/button';
import { Printer, Download, Loader2, FileArchive, FileDown } from 'lucide-react';

interface ResponseDetailsFooterProps {
    onClose: () => void;
    onPrint: () => void;
    onDownloadPDF: () => void;
    onDownloadAttachments: () => void;
    onDownloadImagesPDF: () => void;
    isGeneratingPDF: boolean;
    isDownloadingAttachments: boolean;
    isGeneratingImagesPDF: boolean;
    attachmentsCount: number;
    isAllImages: boolean;
    hasZipImages: boolean;
}

export const ResponseDetailsFooter: React.FC<ResponseDetailsFooterProps> = ({
    onClose,
    onPrint,
    onDownloadPDF,
    onDownloadAttachments,
    onDownloadImagesPDF,
    isGeneratingPDF,
    isDownloadingAttachments,
    isGeneratingImagesPDF,
    attachmentsCount,
    isAllImages,
    hasZipImages
}) => {
    return (
        <div className="flex flex-col gap-3 w-full">
            <div className="flex gap-3 w-full">
                <Button
                    onClick={onDownloadPDF}
                    disabled={isGeneratingPDF}
                    className="flex-1 h-12 rounded-xl shadow-lg shadow-primary/20 gap-2 font-bold"
                >
                    {isGeneratingPDF ? (
                        <>
                            <Loader2 className="w-5 h-5 animate-spin" /> جاري التحميل...
                        </>
                    ) : (
                        <>
                            <Download className="w-5 h-5" /> تحميل PDF
                        </>
                    )}
                </Button>
                <Button
                    onClick={onPrint}
                    variant="outline"
                    className="flex-1 h-12 rounded-xl font-bold gap-2"
                >
                    <Printer className="w-5 h-5" /> طباعة
                </Button>
            </div>

            {attachmentsCount > 0 && (
                <div className="flex flex-col gap-2">
                    <Button
                        onClick={onDownloadAttachments}
                        disabled={isDownloadingAttachments}
                        variant="secondary"
                        className="w-full h-12 rounded-xl font-bold gap-2"
                    >
                        {isDownloadingAttachments ? (
                            <>
                                <Loader2 className="w-5 h-5 animate-spin" /> جاري التحميل...
                            </>
                        ) : (
                            <>
                                <FileArchive className="w-5 h-5" /> تحميل كافة المرفقات ({attachmentsCount})
                            </>
                        )}
                    </Button>

                    {isAllImages && hasZipImages && (
                        <Button
                            onClick={onDownloadImagesPDF}
                            disabled={isGeneratingImagesPDF}
                            variant="outline"
                            className="w-full h-12 rounded-xl font-bold gap-2 border-primary/20 text-primary hover:bg-primary/5"
                        >
                            {isGeneratingImagesPDF ? (
                                <>
                                    <Loader2 className="w-5 h-5 animate-spin" /> جاري إنشاء PDF...
                                </>
                            ) : (
                                <>
                                    <FileDown className="w-5 h-5" /> دمج الصور في ملف PDF واحد
                                </>
                            )}
                        </Button>
                    )}
                </div>
            )}

            <Button variant="ghost" onClick={onClose} className="h-12 rounded-xl font-bold gap-2 px-6">
                إغلاق
            </Button>
        </div>
    );
};
