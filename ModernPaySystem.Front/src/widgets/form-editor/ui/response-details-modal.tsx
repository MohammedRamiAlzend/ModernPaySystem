import React, { useEffect, useState } from 'react';
import { BaseModal } from '@/shared/ui/modals/base-modal';
import type { FormSchema, FormResponse } from '@/entities/form/model/types';
import { MessageSquare } from 'lucide-react';
import { printFormResponse, generateFormPDF } from '@/shared/lib/pdf-generator';
import { getVisibleFields, prepareFieldsForPrint } from '@/shared/lib/form-engine/response-evaluator';
import { formEndpoints, useRequestResponses } from '@/features/form-builder/api/formEndpoints';
import { extractImagesFromZip, revokeZipImages, imagesToPdf, type ZipImage, type ZipContent } from '@/shared/utils/zip-handler';
import { useUIStore } from '@/app/store/uiStore';

// Sub-components
import { ResponseItem } from './response-details/ResponseItem';
import { ResponseDetailsHeader } from './response-details/ResponseDetailsHeader';
import { ResponseDetailsData } from './response-details/ResponseDetailsData';
import { ResponseDetailsAttachments } from './response-details/ResponseDetailsAttachments';
import { ResponseDetailsFooter } from './response-details/ResponseDetailsFooter';
import { ImagePreview } from './response-details/ImagePreview';

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
    const { showStatus } = useUIStore();
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

    const handlePrint = () => {
        const printFields = prepareFieldsForPrint({ ...response, schema });
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
            const printFields = prepareFieldsForPrint({ ...response, schema });
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
            showStatus({
                type: 'error',
                title: 'خطأ في التحميل',
                message: 'فشل تحميل المرفقات'
            });
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
            showStatus({
                type: 'error',
                title: 'خطأ في التحميل',
                message: 'فشل إنشاء ملف PDF للصور'
            });
        } finally {
            setIsGeneratingImagesPDF(false);
        }
    };



    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            title={schema.title}
            maxWidth="2xl"
            maxHeight='4xl'
            footer={
                <ResponseDetailsFooter
                    onClose={onClose}
                    onPrint={handlePrint}
                    onDownloadPDF={handleDownloadPDF}
                    onDownloadAttachments={handleDownloadAttachments}
                    onDownloadImagesPDF={handleDownloadImagesAsPdf}
                    isGeneratingPDF={isGeneratingPDF}
                    isDownloadingAttachments={isDownloadingAttachments}
                    isGeneratingImagesPDF={isGeneratingImagesPDF}
                    attachmentsCount={response.attachments?.length || 0}
                    isAllImages={isAllImages}
                    hasZipImages={zipImages.length > 0}
                />
            }
        >
            <div className="space-y-8 text-right" dir="rtl">
                <ResponseDetailsHeader
                    title={schema.title}
                    submittedAt={response.submittedAt}
                    visibleCount={visibleFields.length}
                    totalCount={schema.fields.length}
                />

                <ResponseDetailsData visibleFields={visibleFields} />

                <ResponseDetailsAttachments
                    attachmentsCount={response.attachments?.length || 0}
                    isLoadingImages={isLoadingImages}
                    zipImages={zipImages}
                    setSelectedImage={setSelectedImage}
                />

                {responses.length > 0 && (
                    <div className="mt-8 pt-8 border-t-2 border-gray-100">
                        <div className="flex items-center justify-between mb-6">
                            <h3 className="text-xl font-bold flex items-center gap-2 text-emerald-600">
                                <MessageSquare className="w-6 h-6" />
                                الردود والقرارات
                            </h3>
                            <span className="px-3 py-1  text-emerald-700 text-xs font-bold rounded-full">
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

                <ImagePreview image={selectedImage} onClose={() => setSelectedImage(null)} />
            </div>
        </BaseModal>
    );
};
