import { useState, useEffect } from 'react';
import { useUIStore } from '@/app/store/uiStore';
import type { FormResponse, TemplateRequest } from '@/entities/form/model/types';
import { useTemplateById } from '@/features/form-builder/api/formEndpoints';

export const useRequestDetails = () => {
    const { showStatus } = useUIStore();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [viewingResponse, setViewingResponse] = useState<FormResponse | null>(null);
    const [activeRequest, setActiveRequest] = useState<TemplateRequest | null>(null);

    // Fetch the template specifically by ID to ensure permissions and fresh data
    const { data: fetchedTemplate, isLoading: isTemplateLoading } = useTemplateById(activeRequest?.templateId || null);

    useEffect(() => {
        if (activeRequest && fetchedTemplate) {
            try {
                const mappedResponse: FormResponse = {
                    id: activeRequest.id,
                    formId: activeRequest.templateId,
                    submittedAt: activeRequest.createdAt || new Date().toISOString(),
                    data: JSON.parse(activeRequest.content),
                    schema: fetchedTemplate,
                    attachments: activeRequest.requestAttachmentDtos
                };
                setViewingResponse(mappedResponse);
                setIsModalOpen(true);
            } catch {
                showStatus({
                    type: 'error',
                    title: 'خطأ في التحليل',
                    message: 'خطأ في تحليل بيانات الطلب'
                });
            }
        }
    }, [activeRequest, fetchedTemplate, showStatus]);

    const handleViewRequest = (request: TemplateRequest) => {
        // Just set the active request, the useEffect will trigger when template data is ready
        setActiveRequest(request);
    };

    return {
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest,
        isTemplateLoading
    };
};
