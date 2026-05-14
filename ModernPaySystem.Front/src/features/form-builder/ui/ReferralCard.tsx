import { Card } from '@/shared/ui/card';
import {
    FileText,
    User,
    Calendar,
    Forward,
    MessageSquare,
    ArrowRight,
    Eye
} from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { RequestFieldsPreview } from './RequestFieldsPreview';
import { TemplateTitle } from './TemplateTitle';
import { AttachmentsGallery } from './AttachmentsGallery';
import { useAttachments } from '../model/useAttachments';
import { formEndpoints } from '../api/formEndpoints';
import type { RequestTransactionDto, FormResponse } from '@/entities/form/model/types';
import { useForms } from '../model/useForms';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import React from 'react';

interface ReferralCardProps {
    referral: RequestTransactionDto;
    isPending: boolean;
    onAction?: (referral: RequestTransactionDto) => void;
}

const ReferralAttachments = ({ referral }: { referral: RequestTransactionDto }) => {
    const {
        zipImages,
        isLoading,
        isAllImages,
        totalFiles
    } = useAttachments(
        referral.requestTransactionAttachments && referral.requestTransactionAttachments.length > 0
            ? () => formEndpoints.fetchTransactionAttachmentsBlob(referral.id)
            : null,
        [referral.id, referral.requestTransactionAttachments]
    );

    if (totalFiles === 0 && !isLoading) return null;

    return (
        <AttachmentsGallery
            images={zipImages}
            isLoading={isLoading}
            isAllImages={isAllImages}
            totalFiles={totalFiles}
            requestId={referral.id}
            title="مرفقات الإحالة"
            onDownloadAll={() => formEndpoints.downloadTransactionAttachments(referral.id)}
            className="mt-2 pt-4"
        />
    );
};

export const ReferralCard = ({ referral, isPending, onAction }: ReferralCardProps) => {
    const { data: templates = [] } = useForms(true);

    const [isDetailsModalOpen, setIsDetailsModalOpen] = React.useState(false);

    const getTemplateFields = (templateId: string) => {
        return templates.find(t => t.id === templateId)?.fields || [];
    };

    const getTemplateTitleFallback = (templateId: string) => {
        return templates.find(t => t.id === templateId)?.title || 'خدمة غير معروف';
    };

    const pseudoResponse: FormResponse | null = React.useMemo(() => {
        const req = referral.request;
        if (!req) return null;

        const template = templates.find(t => t.id === req.templateId) || null;

        // Convert InputValueDto[] to Record
        let parsedData = {};
        try {
            parsedData = (req.content || []).reduce((acc, curr) => {
                acc[curr.key] = curr.value;
                return acc;
            }, {} as Record<string, any>);
        } catch (e) {
            console.error("Failed to map request content", e);
        }

        return {
            ...req,
            id: req.id,
            formId: req.templateId,
            submittedAt: req.createdAt || '',
            data: parsedData as Record<string, any>,
            schema: template || { id: req.templateId, title: '...', fields: [], settings: {} as any } as any
        };
    }, [referral.request, templates]);

    return (
        <Card className="group overflow-hidden border-2 border-transparent hover:border-primary/20 transition-all duration-300 hover:shadow-xl hover:shadow-primary/5 bg-card/50 backdrop-blur-sm">
            <div className="flex flex-col lg:flex-row">
                {/* Side info panel */}
                <div className={`lg:w-72 p-6 flex flex-col gap-4 border-l border-primary/5 ${isPending ? 'bg-amber-500/5' : 'bg-sky-500/5'}`}>
                    <div className="flex items-center gap-3">
                        <div className="p-2 bg-white dark:bg-zinc-900 rounded-xl shadow-sm">
                            <FileText className="w-5 h-5 text-primary" />
                        </div>
                        <div className="flex flex-col overflow-hidden">
                            <span className="text-xs text-muted-foreground font-bold uppercase tracking-wider">نوع الخدمة</span>
                            <span className="text-sm font-black truncate">
                                <TemplateTitle
                                    templateId={referral.request?.templateId || ''}
                                    fallbackTitle={getTemplateTitleFallback(referral.request?.templateId || '')}
                                />
                            </span>
                        </div>
                    </div>

                    <div className="space-y-3 mt-2">
                        <div className="flex items-center gap-3 text-xs">
                            <User className="w-4 h-4 text-muted-foreground" />
                            <div className="flex flex-col">
                                <span className="text-muted-foreground">صاحب الطلب</span>
                                <UserDisplay userId={referral.request?.requesterId || ''} className="font-bold" />
                            </div>
                        </div>
                        <div className="flex items-center gap-3 text-xs">
                            <Calendar className="w-4 h-4 text-muted-foreground" />
                            <div className="flex flex-col">
                                <span className="text-muted-foreground">تاريخ الإحالة</span>
                                <span className="font-bold">{new Date(referral.createdAt).toLocaleString('ar-EG')}</span>
                            </div>
                        </div>
                        {referral.createdByUserId && (
                            <div className="flex items-center gap-3 text-xs">
                                <Forward className="w-4 h-4 text-muted-foreground" />
                                <div className="flex flex-col">
                                    <span className="text-muted-foreground">مرسل الإحالة</span>
                                    <UserDisplay userId={referral.createdByUserId} className="font-bold" />
                                </div>
                            </div>
                        )}
                    </div>

                    <div className="mt-auto space-y-2">
                        {isPending && onAction && (
                            <Button
                                className="w-full h-12 rounded-2xl font-black bg-primary hover:bg-primary/90 text-primary-foreground shadow-lg shadow-primary/20 group-hover:scale-[1.02] transition-transform"
                                onClick={() => onAction(referral)}
                            >
                                اتخاذ إجراء
                                <ArrowRight className="w-4 h-4 mr-2" />
                            </Button>
                        )}
                        <Button
                            variant="outline"
                            className="w-full h-10 rounded-xl font-bold border-primary/20 hover:bg-primary/5 text-primary"
                            onClick={() => setIsDetailsModalOpen(true)}
                        >
                            <Eye className="w-4 h-4 ml-2" />
                            عرض التفاصيل
                        </Button>
                    </div>
                </div>

                {/* Main content panel */}
                <div className="flex-1 p-6 flex flex-col gap-5">
                    {/* Referral Notes */}
                    {referral.notes && (
                        <div className="bg-muted/30 p-4 rounded-2xl border border-dashed border-primary/10 relative">
                            <div className="absolute -top-3 right-4 px-2 bg-background text-[10px] font-bold text-primary flex items-center gap-1">
                                <MessageSquare className="w-3 h-3" />
                                ملاحظات الإحالة
                            </div>
                            <p className="text-sm italic leading-relaxed text-foreground/80">
                                &ldquo;{referral.notes}&rdquo;
                            </p>
                        </div>
                    )}

                    <ReferralAttachments referral={referral} />

                    {/* Request Preview */}
                    <div className="bg-background/40 p-5 rounded-2xl border border-primary/5">
                        <div className="text-[10px] font-black text-muted-foreground uppercase tracking-widest mb-3 opacity-50">معاينة بيانات الطلب</div>
                        <RequestFieldsPreview
                            content={referral.request?.content || '{}'}
                            fields={getTemplateFields(referral.request?.templateId || '')}
                            templateId={referral.request?.templateId}
                            variant="card"
                        />
                    </div>

                    {/* Footer / Status */}
                    <div className="flex items-center justify-between mt-auto pt-4 border-t border-primary/5 text-[10px]">
                        <div className="flex items-center gap-5">
                            <span className="flex items-center gap-1.5 font-bold text-muted-foreground">
                                <User className="w-3.5 h-3.5" />
                                المستلم الحالي: <UserDisplay userId={referral.currentUserHolderId} />
                            </span>
                            {referral.parentTransactionId && (
                                <span className="flex items-center gap-1.5 px-2 py-0.5 bg-muted rounded-full">
                                    <ArrowRight className="w-3 h-3" />
                                    إحالة فرعية مستوى {referral.level}
                                </span>
                            )}
                        </div>

                        <div className="flex items-center gap-2">
                            <div className={`w-2 h-2 rounded-full ${referral.status === 0 ? 'bg-amber-500' : 'bg-emerald-500'}`} />
                            <span className="font-black">
                                {referral.status === 0 ? 'قيد الانتظار' : 'تمت الإحالة بنجاح'}
                            </span>
                        </div>
                    </div>
                </div>
            </div>

            {pseudoResponse && (
                <ResponseDetailsModal
                    isOpen={isDetailsModalOpen}
                    onClose={() => setIsDetailsModalOpen(false)}
                    response={pseudoResponse}
                    schema={pseudoResponse.schema}
                />
            )}
        </Card>
    );
};
