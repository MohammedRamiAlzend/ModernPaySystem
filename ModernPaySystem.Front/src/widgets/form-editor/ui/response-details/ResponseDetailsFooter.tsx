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
    includeResponses: boolean;
    setIncludeResponses: (val: boolean) => void;
    includeReferrals: boolean;
    setIncludeReferrals: (val: boolean) => void;
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
    hasZipImages,
    includeResponses,
    setIncludeResponses,
    includeReferrals,
    setIncludeReferrals
}) => {
    // Save to localStorage when changed
    const handleIncludeResponsesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = e.target.checked;
        setIncludeResponses(val);
        localStorage.setItem('print_include_responses', String(val));
    };

    const handleIncludeReferralsChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const val = e.target.checked;
        setIncludeReferrals(val);
        localStorage.setItem('print_include_referrals', String(val));
    };

    return (
        <div className="flex flex-col gap-4 w-full">
            {/* Print Options */}
            <div className="flex items-center justify-center gap-6 py-2 border-y border-muted-foreground/10">
                <label className="flex items-center gap-2 cursor-pointer group">
                    <input
                        type="checkbox"
                        checked={includeResponses}
                        onChange={handleIncludeResponsesChange}
                        className="w-4 h-4 rounded border-muted-foreground/30 text-primary focus:ring-primary/20"
                    />
                    <span className="text-xs font-bold text-muted-foreground group-hover:text-primary transition-colors">تضمين الرد النهائي </span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer group">
                    <input
                        type="checkbox"
                        checked={includeReferrals}
                        onChange={handleIncludeReferralsChange}
                        className="w-4 h-4 rounded border-muted-foreground/30 text-primary focus:ring-primary/20"
                    />
                    <span className="text-xs font-bold text-muted-foreground group-hover:text-primary transition-colors">تضمين الإحالات</span>
                </label>
            </div>

            <div className="flex flex-col gap-2 w-full">
                <div className="flex gap-2 w-full">
                    <Button
                        onClick={onDownloadPDF}
                        disabled={isGeneratingPDF}
                        className="flex-1 h-10 rounded-xl shadow-md gap-2 font-bold text-sm"
                    >
                        {isGeneratingPDF ? (
                            <>
                                <Loader2 className="w-4 h-4 animate-spin" /> جاري التحميل...
                            </>
                        ) : (
                            <>
                                <Download className="w-4 h-4" /> تحميل PDF
                            </>
                        )}
                    </Button>
                    <Button
                        onClick={onPrint}
                        variant="outline"
                        className="flex-1 h-10 rounded-xl font-bold gap-2 text-sm"
                    >
                        <Printer className="w-4 h-4" /> طباعة
                    </Button>
                </div>

                {attachmentsCount > 0 && (
                    <div className="flex flex-col gap-2">
                        <Button
                            onClick={onDownloadAttachments}
                            disabled={isDownloadingAttachments}
                            variant="secondary"
                            className="w-full h-10 rounded-xl font-bold gap-2 text-sm"
                        >
                            {isDownloadingAttachments ? (
                                <>
                                    <Loader2 className="w-4 h-4 animate-spin" /> جاري التحميل...
                                </>
                            ) : (
                                <>
                                    <FileArchive className="w-4 h-4" /> تحميل كافة المرفقات ({attachmentsCount})
                                </>
                            )}
                        </Button>

                        {isAllImages && hasZipImages && (
                            <Button
                                onClick={onDownloadImagesPDF}
                                disabled={isGeneratingImagesPDF}
                                variant="outline"
                                className="w-full h-10 rounded-xl font-bold gap-2 border-primary/20 text-primary hover:bg-primary/5 text-sm"
                            >
                                {isGeneratingImagesPDF ? (
                                    <>
                                        <Loader2 className="w-4 h-4 animate-spin" /> جاري إنشاء PDF...
                                    </>
                                ) : (
                                    <>
                                        <FileDown className="w-4 h-4" /> دمج الصور في ملف PDF واحد
                                    </>
                                )}
                            </Button>
                        )}
                    </div>
                )}

                <Button variant="ghost" onClick={onClose} className="h-9 rounded-xl font-bold gap-2 px-6 text-xs text-muted-foreground">
                    إغلاق
                </Button>
            </div>
        </div>
    );
};
