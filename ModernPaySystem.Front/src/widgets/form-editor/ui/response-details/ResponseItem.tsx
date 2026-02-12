import { useState, useEffect } from 'react';
import { Button } from '@/shared/ui/button';
import { Download, Loader2, FileArchive, ChevronDown, ChevronUp, Maximize2, FileDown } from 'lucide-react';
import { formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { extractImagesFromZip, revokeZipImages, imagesToPdf, type ZipImage, type ZipContent } from '@/shared/utils/zip-handler';
import type { TemplateResponse } from '@/entities/form/model/types';

interface ResponseItemProps {
    response: TemplateResponse;
    onViewImage: (img: ZipImage) => void;
}

export const ResponseItem = ({ response, onViewImage }: ResponseItemProps) => {
    const [showAttachments, setShowAttachments] = useState(false);
    const [images, setImages] = useState<ZipImage[]>([]);
    const [isLoadingImages, setIsLoadingImages] = useState(false);
    const [isAllImages, setIsAllImages] = useState(false);
    const [isGeneratingPdf, setIsGeneratingPdf] = useState(false);

    useEffect(() => {
        return () => {
            if (images.length > 0) {
                revokeZipImages(images);
            }
        };
    }, []);

    const handleToggleAttachments = async () => {
        const willShow = !showAttachments;
        setShowAttachments(willShow);

        if (willShow && images.length === 0) {
            setIsLoadingImages(true);
            try {
                const blob = await formEndpoints.fetchResponseAttachmentsBlob(response.id);
                const content: ZipContent = await extractImagesFromZip(blob);
                setImages(content.images);
                setIsAllImages(content.isAllImages);
            } catch (error) {
                console.error('Failed to load response images', error);
                alert('فشل تحميل محتوى المرفقات');
            } finally {
                setIsLoadingImages(false);
            }
        }
    };

    const handleDownloadPdf = async () => {
        if (images.length === 0) return;
        setIsGeneratingPdf(true);
        try {
            await imagesToPdf(images, `Response_Attachments_${response.id.split('-')[0]}`);
        } catch (error) {
            console.error('Error generating PDF:', error);
            alert('فشل إنشاء ملف PDF');
        } finally {
            setIsGeneratingPdf(false);
        }
    };

    const handleDownloadOriginal = async () => {
        try {
            await formEndpoints.downloadResponseAttachments(response.id);
        } catch (e) {
            console.error(e);
            alert('فشل تحميل المرفقات');
        }
    };

    return (
        <div className="border border-emerald-100 rounded-2xl p-5 shadow-sm bg-white">
            <div className="flex justify-between items-start mb-3">
                <div className="flex items-center gap-2">
                    <div className="font-bold text-sm text-emerald-800">
                        تم الرد بواسطة: <span className="font-mono text-emerald-600">{response.respondedByUserId.split('-')[0]}...</span>
                    </div>
                </div>
                <div className="text-xs text-muted-foreground">
                    {response.createdAt ? new Date(response.createdAt).toLocaleString('ar-EG') : '-'}
                </div>
            </div>

            <p className="text-sm leading-relaxed mb-4 font-medium whitespace-pre-wrap text-gray-700">
                {response.comment}
            </p>

            {response.responseAttachments && response.responseAttachments.length > 0 && (
                <div className="pt-3 border-t border-emerald-50">
                    <div className="flex items-center justify-between">
                        <span className="text-xs font-bold text-emerald-600 flex items-center gap-1">
                            <FileArchive className="w-3 h-3" />
                            {response.responseAttachments.length} مرفقات
                        </span>

                        <div className="flex gap-2">
                            <Button
                                size="sm"
                                variant="ghost"
                                onClick={handleToggleAttachments}
                                className="h-7 text-xs gap-1 text-emerald-700 hover:bg-emerald-50"
                            >
                                {showAttachments ? (
                                    <>
                                        <ChevronUp className="w-3 h-3" /> إخفاء المعاينة
                                    </>
                                ) : (
                                    <>
                                        <ChevronDown className="w-3 h-3" /> معاينة المرفقات
                                    </>
                                )}
                            </Button>

                            <Button
                                size="sm"
                                variant="outline"
                                className="h-7 gap-2 text-xs border-emerald-200 hover:bg-emerald-100 hover:text-emerald-700"
                                onClick={handleDownloadOriginal}
                            >
                                <Download className="w-3 h-3" />
                                تحميل zip
                            </Button>
                        </div>
                    </div>

                    {showAttachments && (
                        <div className="mt-4 animate-in fade-in slide-in-from-top-2 duration-300">
                            {isLoadingImages ? (
                                <div className="flex items-center justify-center py-8">
                                    <Loader2 className="w-6 h-6 animate-spin text-emerald-500" />
                                </div>
                            ) : (
                                <div className="space-y-4">
                                    {images.length > 0 ? (
                                        <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
                                            {images.map((img, idx) => (
                                                <div
                                                    key={idx}
                                                    className="relative aspect-square rounded-lg overflow-hidden border border-emerald-100 cursor-pointer group"
                                                    onClick={() => onViewImage(img)}
                                                >
                                                    <img
                                                        src={img.url}
                                                        alt={img.name}
                                                        className="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
                                                    />
                                                    <div className="absolute inset-0 bg-black/20 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                                        <Maximize2 className="text-white w-4 h-4" />
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        <div className="text-center py-4 text-xs text-muted-foreground rounded-lg">
                                            لا يمكن عرض المرفقات (قد تكون ملفات غير مدعومة للمعاينة)
                                        </div>
                                    )}

                                    {isAllImages && images.length > 0 && (
                                        <Button
                                            onClick={handleDownloadPdf}
                                            disabled={isGeneratingPdf}
                                            variant="secondary"
                                            className="w-full h-9 text-xs font-bold gap-2 bg-emerald-100 text-emerald-800 hover:bg-emerald-200"
                                        >
                                            {isGeneratingPdf ? (
                                                <>
                                                    <Loader2 className="w-3 h-3 animate-spin" /> جاري الدمج...
                                                </>
                                            ) : (
                                                <>
                                                    <FileDown className="w-3 h-3" /> تحميل الصور كملف PDF
                                                </>
                                            )}
                                        </Button>
                                    )}
                                </div>
                            )}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};
