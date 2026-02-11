import { useState } from 'react';
import { useRequests } from '@/features/form-builder/api/formEndpoints';
import { Card } from '@/shared/ui/card';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { FileText, User, Eye, CheckCircle2, History } from 'lucide-react';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { useForms } from '@/features/form-builder/model/useForms';
import { Button } from '@/shared/ui/button';
import type { FormResponse } from '@/shared/lib/form-engine/responses';
import type { TemplateRequest } from '@/entities/form/model/types';

export default function ActionedRequestsPage() {
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [viewingResponse, setViewingResponse] = useState<FormResponse | null>(null);

    const { data: requests = [], isLoading } = useRequests(true); // Fetch responded requests
    const { data: templates = [] } = useForms();

    const handleViewRequest = (request: TemplateRequest) => {
        const schema = templates.find(t => t.id === request.templateId);
        if (!schema) {
            alert('النموذج المرتبط بهذا الطلب غير موجود');
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
        } catch (e) {
            alert('خطأ في تحليل بيانات الطلب');
        }
    };

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6">
            <h1 className="text-3xl font-bold">أرشيف الطلبات المستجاب لها</h1>

            <Card className="p-6 overflow-hidden flex flex-col min-h-[600px]">
                <div className="flex items-center justify-between mb-6">
                    <h2 className="text-xl font-semibold flex items-center gap-2 text-primary">
                        <History className="w-5 h-5" />
                        الطلبات المؤرشفة
                    </h2>
                    <span className="px-3 py-1 bg-primary/10 text-primary text-xs font-bold rounded-full">
                        {requests.length} طلب مكتمل
                    </span>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 overflow-y-auto pr-2 custom-scrollbar">
                    {isLoading ? (
                        Array.from({ length: 6 }).map((_, i) => (
                            <Skeleton key={i} className="h-40 w-full rounded-2xl" />
                        ))
                    ) : requests.length > 0 ? (
                        requests.map((request) => (
                            <div
                                key={request.id}
                                className="p-5 rounded-2xl border-2 border-border hover:border-primary/50 hover:bg-muted/30 transition-all group flex flex-col"
                            >
                                <div className="flex justify-between items-start mb-4">
                                    <div className="flex items-center gap-3">
                                        <div className="p-2.5 bg-primary/10 rounded-xl">
                                            <FileText className="w-5 h-5 text-primary" />
                                        </div>
                                        <div>
                                            <div className="font-bold text-sm truncate max-w-[150px]">
                                                {request.id.split('-')[0].toUpperCase()}
                                            </div>
                                            <div className="flex items-center gap-1 text-[10px] text-muted-foreground mt-1">
                                                <User className="w-3 h-3" />
                                                {request.requesterId.split('-')[0]}...
                                            </div>
                                        </div>
                                    </div>
                                    <div className="flex items-center gap-1.5 px-2 py-1 bg-emerald-50 text-emerald-600 rounded-lg border border-emerald-100">
                                        <CheckCircle2 className="w-3 h-3" />
                                        <span className="text-[10px] font-bold">تم الرد</span>
                                    </div>
                                </div>

                                <div className="text-xs text-muted-foreground bg-muted/20 p-3 rounded-xl font-mono truncate mb-4">
                                    {request.content}
                                </div>

                                <div className="mt-auto pt-4 border-t flex items-center justify-between">
                                    <div className="text-[10px] text-muted-foreground">
                                        {new Date(request.createdAt || '').toLocaleDateString('ar-EG')}
                                    </div>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        className="h-9 px-4 rounded-xl gap-2 text-xs font-bold hover:bg-primary hover:text-white transition-all shadow-sm"
                                        onClick={() => handleViewRequest(request)}
                                    >
                                        <Eye className="w-4 h-4" />
                                        عرض التفاصيل
                                    </Button>
                                </div>
                            </div>
                        ))
                    ) : (
                        <div className="col-span-full h-64 flex flex-col items-center justify-center text-muted-foreground border-2 border-dashed rounded-3xl">
                            <History className="w-12 h-12 mb-3 opacity-20" />
                            <p className="font-medium">لا توجد طلبات مؤرشفة حالياً</p>
                        </div>
                    )}
                </div>
            </Card>

            {viewingResponse && (
                <ResponseDetailsModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    response={viewingResponse}
                    schema={viewingResponse.schema}
                />
            )}
        </AnimatedContainer>
    );
};
