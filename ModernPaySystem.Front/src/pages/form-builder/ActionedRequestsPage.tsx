import { useState } from 'react';
import { useRequests } from '@/features/form-builder/api/formEndpoints';
import { Card } from '@/shared/ui/card';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { FileText, Eye, History } from 'lucide-react';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { useForms } from '@/features/form-builder/model/useForms';
import { Button } from '@/shared/ui/button';
import type { TemplateRequest, FormResponse } from '@/entities/form/model/types';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { useAppDispatch } from '@/app/store';
import { showStatus } from '@/app/store/uiSlice';

export default function ActionedRequestsPage() {
    const dispatch = useAppDispatch();
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [viewingResponse, setViewingResponse] = useState<FormResponse | null>(null);

    const { data: requests = [], isLoading } = useRequests(true); // Fetch responded requests
    const { data: templates = [] } = useForms();

    const handleViewRequest = (request: TemplateRequest) => {
        const schema = templates.find(t => t.id === request.templateId);
        if (!schema) {
            dispatch(showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'النموذج المرتبط بهذا الطلب غير موجود'
            }));
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
            dispatch(showStatus({
                type: 'error',
                title: 'خطأ في التحليل',
                message: 'خطأ في تحليل بيانات الطلب'
            }));
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

                <div className="overflow-x-auto custom-scrollbar">
                    <table className="w-full text-right border-collapse" dir="rtl">
                        <thead className="bg-muted/30">
                            <tr>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">رقم الطلب</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">اسم النموذج (Form)</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">البيانات المقدمة</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">المستخدم (Requester)</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">تاريخ التقديم</th>
                                <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-center">الإجراءات</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-border/50">
                            {isLoading ? (
                                Array.from({ length: 5 }).map((_, i) => (
                                    <tr key={i} className="animate-pulse">
                                        <td colSpan={6} className="px-6 py-4">
                                            <Skeleton className="h-10 w-full rounded-lg" />
                                        </td>
                                    </tr>
                                ))
                            ) : requests.length > 0 ? (
                                requests.map((request) => (
                                    <tr key={request.id} className="hover:bg-muted/20 transition-colors group">
                                        <td className="px-6 py-4 text-xs font-mono text-muted-foreground">
                                            {request.id.split('-')[0].toUpperCase()}
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-3">
                                                <div className="p-2 bg-primary/10 rounded-lg group-hover:bg-primary/20 transition-colors">
                                                    <FileText className="w-4 h-4 text-primary" />
                                                </div>
                                                <span className="font-bold text-sm">
                                                    {templates.find(t => t.id === request.templateId)?.title || "نموذج غير معروف"}
                                                </span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="space-y-1.5 max-w-xs">
                                                {(() => {
                                                    try {
                                                        const data = JSON.parse(request.content);
                                                        const schema = templates.find(t => t.id === request.templateId);
                                                        const fields = schema?.fields || [];

                                                        // Get first 3 fields that have values
                                                        const displayFields = fields
                                                            .filter(field => data[field.name] !== undefined && data[field.name] !== null && data[field.name] !== '')
                                                            .slice(0, 3);

                                                        if (displayFields.length === 0) {
                                                            return (
                                                                <div className="text-xs text-muted-foreground italic">
                                                                    لا توجد بيانات
                                                                </div>
                                                            );
                                                        }

                                                        return displayFields.map((field, idx) => (
                                                            <div key={idx} className="flex items-start gap-2 text-xs">
                                                                <span className="font-semibold text-muted-foreground whitespace-nowrap">
                                                                    {field.label}:
                                                                </span>
                                                                <span className="text-foreground font-medium truncate">
                                                                    {String(data[field.name])}
                                                                </span>
                                                            </div>
                                                        ));
                                                    } catch (e) {
                                                        return (
                                                            <div className="text-xs text-muted-foreground italic">
                                                                خطأ في البيانات
                                                            </div>
                                                        );
                                                    }
                                                })()}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <UserDisplay
                                                userId={request.requesterId}
                                                showIcon={true}
                                                iconClassName="w-3 h-3"
                                                className="text-sm"
                                            />
                                        </td>
                                        <td className="px-6 py-4 text-xs text-muted-foreground">
                                            {new Date(request.createdAt || '').toLocaleDateString('ar-EG') || request.createdAt}
                                        </td>
                                        <td className="px-6 py-4 text-center">
                                            <Button
                                                variant="outline"
                                                size="sm"
                                                className="h-9 px-4 rounded-xl gap-2 text-xs font-bold hover:bg-primary hover:text-white transition-all shadow-sm"
                                                onClick={() => handleViewRequest(request)}
                                            >
                                                <Eye className="w-4 h-4" />
                                                عرض التفاصيل
                                            </Button>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan={6} className="py-20 text-center">
                                        <div className="flex flex-col items-center justify-center text-muted-foreground">
                                            <History className="w-12 h-12 mb-3 opacity-20" />
                                            <p className="font-medium">لا توجد طلبات مؤرشفة حالياً</p>
                                        </div>
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
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
