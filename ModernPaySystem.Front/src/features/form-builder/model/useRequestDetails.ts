import { useState, useMemo } from 'react';
import type { FormResponse, TemplateRequest } from '@/entities/form/model/types';
import { useTemplateById } from '@/features/form-builder/api/formEndpoints';

export const useRequestDetails = () => {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [activeRequest, setActiveRequest] = useState<TemplateRequest | null>(null);

    // Fetch the template specifically by ID to ensure permissions and fresh data
    const { data: fetchedTemplate, isLoading: isTemplateLoading } = useTemplateById(activeRequest?.templateId || null);

    // Derived state for the viewing response - calculated during render
    const viewingResponse = useMemo(() => {
        if (!activeRequest || !fetchedTemplate) return null;
        try {
            return {
                id: activeRequest.id,
                formId: activeRequest.templateId,
                submittedAt: activeRequest.createdAt || new Date().toISOString(),
                data: JSON.parse(activeRequest.content),
                schema: fetchedTemplate,
                attachments: activeRequest.requestAttachmentDtos
            } as FormResponse;
        } catch (e) {
            console.error('Failed to parse request content', e);
            return null;
        }
    }, [activeRequest, fetchedTemplate]);

    const handleViewRequest = (request: TemplateRequest) => {
        setActiveRequest(request);
        setIsModalOpen(true);
    };

    return {
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest,
        isTemplateLoading
    };
};
