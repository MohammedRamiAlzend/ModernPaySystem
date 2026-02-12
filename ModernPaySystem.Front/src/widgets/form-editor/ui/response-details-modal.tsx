import React from 'react';
import { BaseModal } from '@/shared/ui/modals/base-modal';
import type { FormSchema, TemplateResponse } from '@/entities/form/model/types';
import type { FormResponse } from '@/shared/lib/form-engine/responses';
import { Button } from '@/shared/ui/button';
import { Printer, Download, Loader2, FileArchive, Image as ImageIcon, Maximize2, XCircle } from 'lucide-react';
import { printFormResponse, generateFormPDF } from '@/shared/lib/pdf-generator';
import { getVisibleFields, prepareFieldsForPrint } from '@/shared/lib/form-engine/response-evaluator';
import { formEndpoints, useRequestResponses } from '@/features/form-builder/api/formEndpoints';
import { extractImagesFromZip, revokeZipImages, imagesToPdf, type ZipImage, type ZipContent } from '@/shared/utils/zip-handler';
import { useEffect, useState } from 'react';
import { FileDown, MessageSquare, ChevronDown, ChevronUp } from 'lucide-react';

// --- Sub-component for individual response items ---
const ResponseItem = ({ response, onViewImage }: { response: TemplateResponse, onViewImage: (img: ZipImage) => void }) => {
    const [showAttachments, setShowAttachments] = useState(false);
    const [images, setImages] = useState<ZipImage[]>([]);
    const [isLoadingImages, setIsLoadingImages] = useState(false);
    const [isAllImages, setIsAllImages] = useState(false);
    const [isGeneratingPdf, setIsGeneratingPdf] = useState(false);

    // Cleanup images on unmount or when closed
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
        <div className="border border-emerald-100 rounded-2xl p-5  shadow-sm">
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
                                className="h-7 text-xs gap-1  text-emerald-700"
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
                                                    <div className="absolute inset-0  opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                                        <Maximize2 className=" w-4 h-4" />
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        <div className="text-center py-4 text-xs  rounded-lg">
                                            لا يمكن عرض المرفقات (قد تكون ملفات غير مدعومة للمعاينة)
                                        </div>
                                    )}

                                    {/* Action Buttons for Expanded View */}
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

interface ResponseDetailsModalProps {
    isOpen: boolean;
    onClose: () => void;
    schema: FormSchema;
    response: FormResponse | null;
}

export const ResponseDetailsModal: React.FC<ResponseDetailsModalProps> = ({
    isOpen,
    onClose,
    schema: fallbackSchema,
    response
}) => {
    const [isGeneratingPDF, setIsGeneratingPDF] = useState(false);
    const [isDownloadingAttachments, setIsDownloadingAttachments] = useState(false);
    const [zipImages, setZipImages] = useState<ZipImage[]>([]);
    const [isAllImages, setIsAllImages] = useState(false);
    const [isLoadingImages, setIsLoadingImages] = useState(false);
    const [isGeneratingImagesPDF, setIsGeneratingImagesPDF] = useState(false);
    const [selectedImage, setSelectedImage] = useState<ZipImage | null>(null);

    const { data: responses = [] } = useRequestResponses(response?.id || null);

    // Load images from ZIP when modal opens
    useEffect(() => {
        if (isOpen && response?.attachments && response.attachments.length > 0) {
            const loadImages = async () => {
                setIsLoadingImages(true);
                try {
                    const blob = await formEndpoints.fetchRequestAttachmentsBlob(response.id);
                    const content: ZipContent = await extractImagesFromZip(blob);
                    setZipImages(content.images);
                    setIsAllImages(content.isAllImages);
                } catch (error) {
                    console.error('Failed to load images from ZIP', error);
                } finally {
                    setIsLoadingImages(false);
                }
            };
            loadImages();
        }

        return () => {
            if (zipImages.length > 0) {
                revokeZipImages(zipImages);
                setZipImages([]);
            }
        };
    }, [isOpen, response?.id]);

    if (!response) return null;

    // Use the saved schema from the response (captures form structure at submission time)
    // Fall back to the passed schema for backwards compatibility with older responses
    const schema = response.schema || fallbackSchema;

    // Get only visible fields based on logic evaluation
    const visibleFields = getVisibleFields({
        ...response,
        schema // ensure we're using the correct schema
    });

    const getPrintFields = () => {
        return prepareFieldsForPrint({
            ...response,
            schema
        });
    };

    const handlePrint = () => {
        const printFields = getPrintFields();
        printFormResponse(
            schema.title,
            new Date(response.submittedAt).toLocaleString('ar-EG'),
            printFields,
            'rtl'
        );
    };

    const handleDownloadPDF = async () => {
        setIsGeneratingPDF(true);
        try {
            const printFields = getPrintFields();
            await generateFormPDF(
                schema.title,
                new Date(response.submittedAt).toLocaleString('ar-EG'),
                printFields,
                'rtl'
            );
        } catch (error) {
            console.error('Error generating PDF:', error);
        } finally {
            setIsGeneratingPDF(false);
        }
    };

    const handleDownloadAttachments = async () => {
        if (!response) return;
        setIsDownloadingAttachments(true);
        try {
            await formEndpoints.downloadRequestAttachments(response.id);
        } catch (error) {
            console.error('Error downloading attachments:', error);
            alert('فشل تحميل المرفقات');
        } finally {
            setIsDownloadingAttachments(false);
        }
    };

    const handleDownloadImagesAsPdf = async () => {
        if (zipImages.length === 0) return;
        setIsGeneratingImagesPDF(true);
        try {
            await imagesToPdf(zipImages, `Attachments_${response?.id.split('-')[0]}`);
        } catch (error) {
            console.error('Error generating Images PDF:', error);
            alert('فشل إنشاء ملف PDF للصور');
        } finally {
            setIsGeneratingImagesPDF(false);
        }
    };

    // Keep the old handler for reference in parent (though effectively unused now replaced by child)
    const handleDownloadResponseAttachments = async (respId: string) => {
        try {
            await formEndpoints.downloadResponseAttachments(respId);
        } catch (e) {
            console.error(e);
            alert('فشل تحميل مرفقات الرد');
        }
    };

    const footer = (
        <div className="flex flex-col gap-3 w-full">
            <div className="flex gap-3 w-full">
                <Button
                    onClick={handleDownloadPDF}
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
                    onClick={handlePrint}
                    variant="outline"
                    className="flex-1 h-12 rounded-xl font-bold gap-2"
                >
                    <Printer className="w-5 h-5" /> طباعة
                </Button>
            </div>

            {response.attachments && response.attachments.length > 0 && (
                <div className="flex flex-col gap-2">
                    <Button
                        onClick={handleDownloadAttachments}
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
                                <FileArchive className="w-5 h-5" /> تحميل كافة المرفقات ({response.attachments.length})
                            </>
                        )}
                    </Button>

                    {isAllImages && zipImages.length > 0 && (
                        <Button
                            onClick={handleDownloadImagesAsPdf}
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

    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            title={schema.title}
            maxWidth="2xl"
            footer={footer}
        >
            <div className="space-y-8 text-right" dir="rtl">
                {/* Header */}
                <div className="text-center border-b-2 border-gray-100 pb-6 mb-8">
                    <h1 className="text-2xl font-bold mb-2 ">{schema.title}</h1>
                    <p className="text-gray-500 font-medium">تاريخ التقديم: {new Date(response.submittedAt).toLocaleString('ar-EG')}</p>
                    <p className="text-xs  mt-2">
                        يتم عرض {visibleFields.length} حقل من أصل {schema.fields.length} (بناءً على الشروط المنطقية)
                    </p>
                </div>

                {/* Data Display - Only showing visible fields based on logic */}
                <div className="bg-muted/10 rounded-3xl p-6 border border-muted-foreground/10">
                    <h3 className="text-xl font-bold mb-6 flex items-center gap-2 text-primary">
                        <FileArchive className="w-5 h-5" />
                        بيانات النموذج
                    </h3>
                    <div className="grid grid-cols-12 gap-x-8 gap-y-6">
                        {visibleFields.map(({ field, displayValue }) => {
                            const colSpan = field.layout?.colSpan || 12;

                            const spanMap = {
                                12: 'col-span-12',
                                6: 'col-span-12 md:col-span-6',
                                4: 'col-span-12 md:col-span-4',
                                3: 'col-span-12 md:col-span-3',
                            };

                            const spanClass = spanMap[colSpan as keyof typeof spanMap] || 'col-span-12';

                            return (
                                <div key={field.id} className={`${spanClass} border-b border-gray-100/50 pb-2`}>
                                    <div className="flex flex-col gap-1">
                                        <span className="text-xs font-bold text-muted-foreground">
                                            {field.label}
                                        </span>
                                        <span className="text-base font-semibold text-foreground break-words">
                                            {displayValue || '-'}
                                        </span>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </div>

                {/* Attachments Section */}
                {(response.attachments && response.attachments.length > 0) && (
                    <div className="mt-8 pt-8 border-t-2 border-gray-100">
                        <div className="flex items-center justify-between mb-6">
                            <h3 className="text-xl font-bold flex items-center gap-2">
                                <ImageIcon className="w-6 h-6 text-primary" />
                                المرفقات والصور
                            </h3>
                            <span className="px-3 py-1 bg-primary/10 text-primary text-xs font-bold rounded-full">
                                {response.attachments.length} ملف
                            </span>
                        </div>

                        {isLoadingImages ? (
                            <div className="flex flex-col items-center justify-center py-12 bg-muted/20 rounded-3xl border-2 border-dashed border-primary/20">
                                <Loader2 className="w-10 h-10 animate-spin text-primary mb-4" />
                                <p className="text-muted-foreground font-medium">جاري استخراج الصور من الملف المضغوط...</p>
                            </div>
                        ) : zipImages.length > 0 ? (
                            <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                                {zipImages.map((img, idx) => (
                                    <div
                                        key={idx}
                                        className="group relative aspect-square rounded-2xl overflow-hidden border-2 border-transparent hover:border-primary transition-all cursor-pointer shadow-md hover:shadow-xl"
                                        onClick={() => setSelectedImage(img)}
                                    >
                                        <img
                                            src={img.url}
                                            alt={img.name}
                                            className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                                        />
                                        <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                            <Maximize2 className="text-white w-8 h-8" />
                                        </div>
                                        <div className="absolute bottom-0 inset-x-0 p-2 bg-black/60 backdrop-blur-md opacity-0 group-hover:opacity-100 transition-opacity">
                                            <p className="text-[10px] text-white truncate text-center font-mono">{img.name}</p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        ) : (
                            <div className="p-8 text-center bg-muted/30 rounded-2xl text-muted-foreground">
                                <p>يحتوي الملف على مرفقات غير مرئية (مثل الملفات النصية)، يمكنك تحميلها بالكامل باستخدام الزر في الأسفل.</p>
                            </div>
                        )}
                    </div>
                )}

                {/* Responses Section */}
                {responses && responses.length > 0 && (
                    <div className="mt-8 pt-8 border-t-2 border-gray-100">
                        <div className="flex items-center justify-between mb-6">
                            <h3 className="text-xl font-bold flex items-center gap-2 text-emerald-600">
                                <MessageSquare className="w-6 h-6" />
                                الردود والقرارات
                            </h3>
                            <span className="px-3 py-1 bg-emerald-100 text-emerald-700 text-xs font-bold rounded-full">
                                {responses.length} رد
                            </span>
                        </div>

                        <div className="space-y-4">
                            {responses.map((resp) => (
                                <ResponseItem
                                    key={resp.id}
                                    response={resp}
                                    onViewImage={setSelectedImage}
                                />
                            ))}
                        </div>
                    </div>
                )}

                {/* Full Screen Image Preview */}
                {selectedImage && (
                    <div
                        className="fixed inset-0 z-[200] bg-black/95 flex flex-col items-center justify-center p-4 md:p-12 transition-all animate-in fade-in duration-300"
                        onClick={() => setSelectedImage(null)}
                    >
                        <button
                            className="absolute top-8 right-8 text-white/50 hover:text-white transition-colors"
                            onClick={() => setSelectedImage(null)}
                        >
                            <XCircle className="w-10 h-10" />
                        </button>
                        <img
                            src={selectedImage.url}
                            alt={selectedImage.name}
                            className="max-w-full max-h-full object-contain rounded-lg shadow-2xl animate-in zoom-in-95 duration-300"
                        />
                        <p className="mt-4 text-white font-bold text-lg">{selectedImage.name}</p>
                    </div>
                )}

                {visibleFields.length === 0 && (
                    <div className="text-center py-8 text-gray-400">
                        لا توجد بيانات لعرضها
                    </div>
                )}
            </div>
        </BaseModal>
    );
};
