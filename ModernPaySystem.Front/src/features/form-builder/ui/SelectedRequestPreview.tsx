import { useState, useEffect, useRef } from 'react';
import { Card } from '@/shared/ui/card';
import { FileText, Image as ImageIcon, Loader2 } from 'lucide-react';
import type { FormSchema, TemplateRequest, FormResponse } from '@/entities/form/model/types';
import { getVisibleFields } from '@/shared/lib/form-engine/response-evaluator';
import { ResponseDetailsData } from '@/widgets/form-editor/ui/response-details/ResponseDetailsData';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { extractImagesFromZip, revokeZipImages, imagesToPdf, type ZipImage } from '@/shared/utils/zip-handler';
import { Printer, Download, FileDown, ExternalLink } from 'lucide-react';
import { printFormResponse, generateFormPDF } from '@/shared/lib/pdf-generator';
import { prepareFieldsForPrint } from '@/shared/lib/form-engine/response-evaluator';
import { Button } from '@/shared/ui/button';
import { useUIStore } from '@/app/store/uiStore';
import { printImage, downloadImage } from '@/shared/utils/image-actions';

interface SelectedRequestPreviewProps {
    request: TemplateRequest | null;
    template: FormSchema | null;
}

export const SelectedRequestPreview = ({ request, template }: SelectedRequestPreviewProps) => {
    const { showStatus } = useUIStore();
    const [zipImages, setZipImages] = useState<ZipImage[]>([]);
    const [isLoadingImages, setIsLoadingImages] = useState(false);
    const [isGeneratingPDF, setIsGeneratingPDF] = useState(false);
    const [isGeneratingImagesPDF, setIsGeneratingImagesPDF] = useState(false);
    const [isAllImages, setIsAllImages] = useState(false);

    const zipImagesRef = useRef<ZipImage[]>([]);

    // Load images from ZIP
    useEffect(() => {
        if (request?.id && request.requestAttachmentDtos && request.requestAttachmentDtos.length > 0) {
            const loadImages = async () => {
                setIsLoadingImages(true);
                try {
                    const blob = await formEndpoints.fetchRequestAttachmentsBlob(request.id);
                    const data = await extractImagesFromZip(blob);
                    setZipImages(data.images);
                    zipImagesRef.current = data.images;
                    setIsAllImages(data.isAllImages);
                } catch (error) {
                    console.error('Failed to load images from ZIP', error);
                } finally {
                    setIsLoadingImages(false);
                }
            };
            loadImages();
        }

        return () => {
            if (zipImagesRef.current.length > 0) {
                revokeZipImages(zipImagesRef.current);
                zipImagesRef.current = [];
                setZipImages([]);
            }
        };
    }, [request?.id, request?.requestAttachmentDtos]);

    if (!request || !template) {
        return (
            <div className="h-full flex flex-col items-center justify-center text-muted-foreground p-8 border-2 border-dashed rounded-xl bg-muted/5">
                <FileText className="w-12 h-12 mb-4 opacity-20" />
                <p className="text-center font-bold">يرجى اختيار طلب من القائمة لمشاهدة التفاصيل</p>
                <p className="text-xs text-center mt-2 opacity-60 px-8">سيتم عرض كافة المعلومات والبيانات الخاصة بالطلب المختار بجانب نموذج الرد لتسهيل العملية</p>
            </div>
        );
    }

    // Parse content JSON string to Record
    let parsedData = {};
    try {
        parsedData = JSON.parse(request.content);
    } catch (e) {
        console.error("Failed to parse request content", e);
    }

    // Adapt TemplateRequest to FormResponse for evaluator
    const pseudoResponse: FormResponse = {
        ...request,
        id: request.id,
        formId: request.templateId, 
        submittedAt: request.createdAt || '',
        data: parsedData as Record<string, any>,
        schema: template
    };

    const visibleFields = getVisibleFields(pseudoResponse);

    const handlePrint = () => {
        if (!template) return;
        const printFields = prepareFieldsForPrint(pseudoResponse);
        printFormResponse(
            template.title,
            new Date(request.createdAt || '').toLocaleString('ar-EG'),
            printFields,
            'rtl'
        );
    };

    const handleDownloadPDF = async () => {
        if (!template) return;
        setIsGeneratingPDF(true);
        try {
            const printFields = prepareFieldsForPrint(pseudoResponse);
            await generateFormPDF(
                template.title,
                new Date(request.createdAt || '').toLocaleString('ar-EG'),
                printFields,
                'rtl'
            );
        } catch (error) {
            console.error('Error generating PDF:', error);
            showStatus({ type: 'error', title: 'خطأ', message: 'فشل إنشاء ملف PDF' });
        } finally {
            setIsGeneratingPDF(false);
        }
    };

    const handleDownloadImagesPDF = async () => {
        if (zipImages.length === 0) return;
        setIsGeneratingImagesPDF(true);
        try {
            await imagesToPdf(zipImages, `Attachments_${request.id.split('-')[0]}`);
        } catch (error) {
            console.error('Error generating images PDF:', error);
            showStatus({ type: 'error', title: 'خطأ', message: 'فشل إنشاء ملف PDF للصور' });
        } finally {
            setIsGeneratingImagesPDF(false);
        }
    };

    return (
        <Card className="flex flex-col h-full overflow-hidden border-primary/20 bg-muted/5">
            <div className="p-4 border-b bg-muted/20 flex items-center justify-between gap-4">
                <div className="flex items-center gap-3 overflow-hidden">
                    <div className="p-2 bg-primary/10 rounded-lg">
                        <FileText className="w-4 h-4 text-primary" />
                    </div>
                    <div className="overflow-hidden">
                        <h3 className="font-bold text-sm truncate">{template.title}</h3>
                        <div className="flex items-center gap-2 mt-0.5">
                            <UserDisplay userId={request.requesterId} className="text-[10px] text-muted-foreground" showIcon iconClassName="w-2.5 h-2.5" />
                            <span className="text-[10px] text-border">|</span>
                            <span className="text-[10px] text-muted-foreground">{new Date(request.createdAt || '').toLocaleString('ar-EG')}</span>
                        </div>
                    </div>
                </div>

                <div className="flex items-center gap-2 shrink-0">
                    <Button
                        variant="ghost"
                        size="icon"
                        onClick={handlePrint}
                        title="طباعة"
                        className="h-8 w-8 rounded-lg hover:bg-primary/10 hover:text-primary transition-colors"
                    >
                        <Printer className="w-4 h-4" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="icon"
                        disabled={isGeneratingPDF}
                        onClick={handleDownloadPDF}
                        title="تحميل PDF"
                        className="h-8 w-8 rounded-lg hover:bg-primary/10 hover:text-primary transition-colors"
                    >
                        {isGeneratingPDF ? <Loader2 className="w-4 h-4 animate-spin text-primary" /> : <Download className="w-4 h-4" />}
                    </Button>
                    {isAllImages && zipImages.length > 0 && (
                        <Button
                            variant="ghost"
                            size="icon"
                            disabled={isGeneratingImagesPDF}
                            onClick={handleDownloadImagesPDF}
                            title="تحميل الصور كـ PDF"
                            className="h-8 w-8 rounded-lg hover:bg-emerald-50 text-emerald-600 hover:text-emerald-700 transition-colors"
                        >
                            {isGeneratingImagesPDF ? <Loader2 className="w-4 h-4 animate-spin" /> : <FileDown className="w-4 h-4" />}
                        </Button>
                    )}
                </div>
            </div>

            <div className="flex-1 overflow-y-auto p-4 custom-scrollbar">
                <div className="space-y-6">
                   <div className="bg-background/80 p-4 rounded-xl shadow-sm border">
                       <ResponseDetailsData visibleFields={visibleFields} />
                   </div>
                   
                   {/* Images Gallery */}
                   {zipImages.length > 0 && (
                       <div className="mt-6 pt-6 border-t animate-in fade-in slide-in-from-top-4 duration-500">
                           <h4 className="text-sm font-bold text-primary mb-4 flex items-center gap-2">
                               <ImageIcon className="w-4 h-4" />
                               الصور المرفقة
                           </h4>
                           <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                               {zipImages.map((img, idx) => (
                                   <div key={idx} className="group relative aspect-video bg-muted rounded-xl overflow-hidden border shadow-sm">
                                       <img 
                                           src={img.url} 
                                           alt={img.name} 
                                           className="w-full h-full object-cover transition-transform group-hover:scale-105"
                                       />
                                       <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-3">
                                            <button 
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    printImage(img.url);
                                                }}
                                                className="p-2.5 bg-white/20 backdrop-blur-md rounded-xl text-white hover:bg-white/40 transition-all hover:scale-110"
                                                title="طباعة"
                                            >
                                                <Printer className="w-5 h-5" />
                                            </button>
                                            <button 
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    downloadImage(img.url, img.name);
                                                }}
                                                className="p-2.5 bg-white/20 backdrop-blur-md rounded-xl text-white hover:bg-white/40 transition-all hover:scale-110"
                                                title="تنزيل"
                                            >
                                                <Download className="w-5 h-5" />
                                            </button>
                                            <a 
                                                href={img.url} 
                                                target="_blank" 
                                                rel="noreferrer" 
                                                className="p-2.5 bg-white/20 backdrop-blur-md rounded-xl text-white hover:bg-white/40 transition-all hover:scale-110"
                                                title="عرض الحجم الكامل"
                                            >
                                                <ExternalLink className="w-5 h-5" />
                                            </a>
                                       </div>
                                       <div className="absolute bottom-0 left-0 right-0 p-2 bg-gradient-to-t from-black/60 to-transparent">
                                            <p className="text-[10px] text-white truncate">{img.name}</p>
                                       </div>
                                   </div>
                               ))}
                           </div>
                       </div>
                   )}

                   {isLoadingImages && (
                       <div className="flex flex-col items-center justify-center p-8 text-muted-foreground gap-2 border border-dashed rounded-xl">
                           <Loader2 className="w-6 h-6 animate-spin text-primary" />
                           <span className="text-xs font-bold">جاري استخراج الصور من المرفقات...</span>
                       </div>
                   )}
                   
                   {request.requestAttachmentDtos && request.requestAttachmentDtos.length > 0 && !zipImages.length && !isLoadingImages && (
                       <div className="mt-4 pt-4 border-t">
                           <h4 className="text-xs font-bold text-muted-foreground mb-3 flex items-center gap-2">
                               المرفقات المقدمة ({request.requestAttachmentDtos.length})
                           </h4>
                           <div className="grid grid-cols-1 gap-2">
                               {request.requestAttachmentDtos.map((att: any) => (
                                   <div key={att.id} className="p-2 bg-card border rounded-lg flex items-center gap-2 text-[10px]">
                                       <FileText className="w-3 h-3 text-primary" />
                                       <span className="truncate flex-1">{att.attachmentDto?.fileName || 'ملف مرفق'}</span>
                                   </div>
                               ))}
                           </div>
                       </div>
                   )}
                </div>
            </div>
        </Card>
    );
};
