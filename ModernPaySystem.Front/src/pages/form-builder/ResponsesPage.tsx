import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { formEndpoints, useRequests } from '@/features/form-builder/api/formEndpoints';
import { Button } from '@/shared/ui/button';
import { Card } from '@/shared/ui/card';
import { Textarea } from '@/shared/ui/textarea';
import { Label } from '@/shared/ui/label';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { Input } from '@/shared/ui/input';
import { useAppSelector } from '@/app/store';
import { selectCurrentUser } from '@/app/store/authSlice';
import { MessageSquare, Clock, FileText, User, ChevronRight, Eye } from 'lucide-react';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { useForms } from '@/features/form-builder/model/useForms';
import type { FormResponse } from '@/shared/lib/form-engine/responses';
import type { TemplateRequest } from '@/entities/form/model/types';

export const ResponsesPage = () => {
    const [requestId, setRequestId] = useState('');
    const [comment, setComment] = useState('');
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [viewingResponse, setViewingResponse] = useState<FormResponse | null>(null);

    const currentUser = useAppSelector(selectCurrentUser);
    const { data: requests = [], isLoading } = useRequests();
    const { data: templates = [] } = useForms();

    const responseMutation = useMutation({
        mutationFn: formEndpoints.createResponse,
        onSuccess: () => {
            alert('تم إرسال الرد بنجاح');
            setComment('');
            setRequestId('');
        },
        onError: () => {
            alert('فشل إرسال الرد');
        }
    });

    const handleSubmit = () => {
        if (!requestId || !currentUser) return;
        responseMutation.mutate({
            requestId,
            comment,
            respondedByUserId: currentUser.id
        });
    };

    const handleSelectRequest = (id: string) => {
        setRequestId(id);
    };

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
            <h1 className="text-3xl font-bold">الردود والموافقات</h1>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Requests List */}
                <Card className="lg:col-span-2 p-6 overflow-hidden flex flex-col h-[700px]">
                    <h2 className="text-xl font-semibold mb-4 flex items-center gap-2">
                        <Clock className="w-5 h-5 text-primary" />
                        الطلبات الواردة
                    </h2>

                    <div className="flex-1 overflow-y-auto space-y-4 pr-2 custom-scrollbar">
                        {isLoading ? (
                            Array.from({ length: 4 }).map((_, i) => (
                                <Skeleton key={i} className="h-32 w-full rounded-xl" />
                            ))
                        ) : requests.length > 0 ? (
                            requests.map((request) => (
                                <div
                                    key={request.id}
                                    onClick={() => handleSelectRequest(request.id)}
                                    className={`p-4 rounded-xl border-2 transition-all cursor-pointer group ${requestId === request.id
                                        ? 'border-primary bg-primary/5'
                                        : 'border-border hover:border-primary/50 hover:bg-muted/30'
                                        }`}
                                >
                                    <div className="flex justify-between items-start mb-3">
                                        <div className="flex items-center gap-2">
                                            <div className="p-2 bg-primary/10 rounded-lg">
                                                <FileText className="w-4 h-4 text-primary" />
                                            </div>
                                            <div>
                                                <div className="font-bold text-sm truncate max-w-[200px]">
                                                    {request.id.split('-')[0].toUpperCase()} ... ID
                                                </div>
                                                <div className="flex items-center gap-1 text-[10px] text-muted-foreground mt-0.5">
                                                    <User className="w-3 h-3" />
                                                    {request.requesterId}
                                                </div>
                                            </div>
                                        </div>
                                        <div className="flex items-center gap-2">
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-8 w-8 rounded-full hover:bg-primary/20 hover:text-primary transition-colors"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleViewRequest(request);
                                                }}
                                            >
                                                <Eye className="w-4 h-4" />
                                            </Button>
                                            <ChevronRight className={`w-4 h-4 transition-transform ${requestId === request.id ? 'translate-x-[-4px] text-primary' : 'text-muted-foreground group-hover:translate-x-[-2px]'}`} />
                                        </div>
                                    </div>

                                    <div className="text-xs text-muted-foreground bg-muted/20 p-2 rounded-md font-mono truncate">
                                        {request.content}
                                    </div>

                                    <div className="mt-3 flex items-center gap-4 text-[10px] text-muted-foreground">
                                        {request.requestAttachmentDtos && request.requestAttachmentDtos.length > 0 && (
                                            <span className="flex items-center gap-1">
                                                <FileText className="w-3 h-3" />
                                                {request.requestAttachmentDtos.length} ملفات
                                            </span>
                                        )}
                                    </div>
                                </div>
                            ))
                        ) : (
                            <div className="h-64 flex flex-col items-center justify-center text-muted-foreground border-2 border-dashed rounded-xl">
                                <MessageSquare className="w-12 h-12 mb-2 opacity-20" />
                                <p>لا توجد طلبات واردة حالياً</p>
                            </div>
                        )}
                    </div>
                </Card>

                {/* Response Form */}
                <div className="space-y-6">
                    <Card className="p-6 space-y-4 shadow-xl border-primary/10">
                        <h2 className="text-xl font-semibold flex items-center gap-2">
                            <MessageSquare className="w-5 h-5 text-primary" />
                            إرسال رد
                        </h2>
                        <div className="space-y-4">
                            <div className="space-y-2">
                                <Label>معرف الطلب المستهدف</Label>
                                <Input
                                    placeholder="اختر طلباً من القائمة أو أدخل المعرف"
                                    value={requestId}
                                    onChange={(e: any) => setRequestId(e.target.value)}
                                    className="bg-muted/30"
                                />
                            </div>

                            <div className="space-y-2">
                                <Label>نص الرد / القرار</Label>
                                <Textarea
                                    placeholder="اكتب تعليقك هنا..."
                                    value={comment}
                                    onChange={(e: any) => setComment(e.target.value)}
                                    rows={5}
                                    className="resize-none"
                                />
                            </div>

                            <Button
                                onClick={handleSubmit}
                                disabled={!requestId || responseMutation.isPending}
                                className="w-full h-12 text-md font-bold shadow-lg shadow-primary/20"
                            >
                                {responseMutation.isPending ? 'جاري الإرسال...' : 'إرسال الرد النهائي'}
                            </Button>
                        </div>
                    </Card>

                    <Card className="p-4 bg-muted/30 border-dashed text-[10px] text-muted-foreground">
                        <p>• سيتم ربط هذا الرد بطلبك الملحق أعلاه.</p>
                        <p>• تأكد من صحة البيانات قبل الضغط على إرسال.</p>
                        <p>• المنفذ الحالي: {currentUser?.username}</p>
                    </Card>
                </div>
            </div>

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



