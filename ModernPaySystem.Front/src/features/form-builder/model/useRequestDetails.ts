import { useState } from 'react';
import { useUIStore } from '@/app/store/uiStore';
import type { FormResponse, TemplateRequest, FormSchema } from '@/entities/form/model/types';

export const useRequestDetails = (templates: FormSchema[]) => {
    const { showStatus } = useUIStore();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [viewingResponse, setViewingResponse] = useState<FormResponse | null>(null);

    const handleViewRequest = (request: TemplateRequest) => {
        const schema = templates.find(t => t.id === request.templateId);
        if (!schema) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'النموذج المرتبط بهذا الطلب غير موجود'
            });
            return;
        }

        try {
            const mappedResponse: FormResponse = {
                id: request.id,
                formId: request.templateId,
                submittedAt: request.createdAt || new Date().toISOString(),
                data: JSON.parse(request.content),
                schema: schema,
                attachments: request.requestAttachmentDtos
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
    };

    return {
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest
    };
};
