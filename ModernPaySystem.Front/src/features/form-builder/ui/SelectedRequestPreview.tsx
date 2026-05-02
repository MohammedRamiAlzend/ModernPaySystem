import { useState } from 'react';
import { Card } from '@/shared/ui/card';
import { FileText, Loader2, Printer, Download, FileDown } from 'lucide-react';
import type { FormSchema, TemplateRequest, FormResponse } from '@/entities/form/model/types';
import { getVisibleFields } from '@/shared/lib/form-engine/response-evaluator';
import { ResponseDetailsData } from '@/widgets/form-editor/ui/response-details/ResponseDetailsData';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { imagesToPdf } from '@/shared/utils/zip-handler';
import { printFormResponse, generateFormPDF } from '@/shared/lib/pdf-generator';
import { prepareFieldsForPrint } from '@/shared/lib/form-engine/response-evaluator';
import { Button } from '@/shared/ui/button';
import { useUIStore } from '@/app/store/uiStore';
import { useAttachments } from '@/features/form-builder/model/useAttachments';
import { AttachmentsGallery } from '@/features/form-builder/ui/AttachmentsGallery';

import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/shared/ui/tabs';
import { RequestTransactionsHistory } from './RequestTransactionsHistory';

interface SelectedRequestPreviewProps {
    request: TemplateRequest | null;
    template: FormSchema | null;
}

export const SelectedRequestPreview = ({ request, template }: SelectedRequestPreviewProps) => {
    const { showStatus } = useUIStore();
    const [isGeneratingPDF, setIsGeneratingPDF] = useState(false);
    const [isGeneratingImagesPDF, setIsGeneratingImagesPDF] = useState(false);

    const { 
        zipImages, 
        isLoading: isLoadingImages, 
        isAllImages, 
        totalFiles 
    } = useAttachments(
        request?.id && request.requestAttachmentDtos && request.requestAttachmentDtos.length > 0
            ? () => formEndpoints.fetchRequestAttachmentsBlob(request.id)
            : null,
        [request?.id, request?.requestAttachmentDtos]
    );

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
            <div className="p-4 border-b bg-muted/20 flex items-center justify-between gap-4 shrink-0">
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

            <div className="flex-1 overflow-hidden flex flex-col p-4">
                <Tabs defaultValue="details" className="flex-1 flex flex-col overflow-hidden" dir="rtl">
                    <TabsList className="w-full justify-start border-b border-primary/10 rounded-none p-0 h-auto bg-transparent mb-4 shrink-0">
                        <TabsTrigger 
                            value="details" 
                            className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 py-2 font-bold"
                        >
                            بيانات الطلب
                        </TabsTrigger>
                        <TabsTrigger 
                            value="referrals" 
                            className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 py-2 font-bold"
                        >
                            متابعة الإحالات
                        </TabsTrigger>
                    </TabsList>
                    
                    <TabsContent value="details" className="flex-1 overflow-y-auto custom-scrollbar pr-2 m-0 outline-none">
                        <div className="space-y-6">
                            <div className="bg-background/80 p-4 rounded-xl shadow-sm border">
                                <ResponseDetailsData visibleFields={visibleFields} />
                            </div>
                            
                            <AttachmentsGallery 
                                images={zipImages}
                                isLoading={isLoadingImages}
                                isAllImages={isAllImages}
                                totalFiles={totalFiles}
                                requestId={request.id}
                                onDownloadAll={() => formEndpoints.downloadRequestAttachments(request.id)}
                                initialExpanded={true}
                            />
                        </div>
                    </TabsContent>
                    
                    <TabsContent value="referrals" className="flex-1 overflow-y-auto custom-scrollbar pr-2 m-0 outline-none">
                        <RequestTransactionsHistory requestId={request.id} />
                    </TabsContent>
                </Tabs>
            </div>
        </Card>
    );
};
