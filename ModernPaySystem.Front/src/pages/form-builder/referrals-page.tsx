
import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { Card } from '@/shared/ui/card';
import { 
    Forward, 
    Send, 
    FileText, 
    MessageSquare,
    User,
    Calendar,
    ArrowRight
} from 'lucide-react';

import { Button } from '@/shared/ui/button';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { Pagination } from '@/shared/ui/common/pagination';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { RequestFieldsPreview } from '@/features/form-builder/ui/RequestFieldsPreview';
import { ProcessRequestModal } from '@/features/form-builder/ui/ProcessRequestModal';
import { useRequestTransactions, formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { useForms } from '@/features/form-builder/model/useForms';
import { useAuthStore } from '@/app/store/authStore';
import { useUIStore } from '@/app/store/uiStore';
import { useAttachments } from '@/features/form-builder/model/useAttachments';
import { AttachmentsGallery } from '@/features/form-builder/ui/AttachmentsGallery';
import type { TemplateRequest, RequestTransactionDto } from '@/entities/form/model/types';

interface ReferralAttachmentsProps {
    referral: RequestTransactionDto;
}

const ReferralAttachments = ({ referral }: ReferralAttachmentsProps) => {
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

interface ReferralsPageProps {

    status: number; // 0 for PendingAction, 1 for Transferred
}

export const ReferralsPage = ({ status }: ReferralsPageProps) => {
    const isPending = status === 0;
    const [page, setPage] = useState(1);
    const pageSize = 10;

    const { data: pagedData, isLoading: isLoadingData } = useRequestTransactions(status, page, pageSize);
    const { data: templates = [], isLoading: isLoadingTemplates } = useForms(true);
    
    const currentUser = useAuthStore((state) => state.user);
    const { showStatus } = useUIStore();
    const queryClient = useQueryClient();

    // Local state for the process modal
    const [isProcessModalOpen, setIsProcessModalOpen] = useState(false);
    const [selectedRequest, setSelectedRequest] = useState<TemplateRequest | null>(null);
    const [selectedReferral, setSelectedReferral] = useState<RequestTransactionDto | null>(null);
    const [comment, setComment] = useState('');
    const [files, setFiles] = useState<File[]>([]);
    const [submissionMode, setSubmissionMode] = useState<'submit' | 'referral'>('submit');
    const [targetUserId, setTargetUserId] = useState('');

    // Mutations
    const responseMutation = useMutation({
        mutationFn: formEndpoints.createResponse,
        onSuccess: () => {
            showStatus({ type: 'success', title: 'تمت العملية', message: 'تم إرسال الرد بنجاح' });
            queryClient.invalidateQueries({ queryKey: ['request-transactions'] });
            queryClient.invalidateQueries({ queryKey: ['requests'] });
            handleCloseProcessModal();
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'فشل إرسال الرد' });
        }
    });

    const referralMutation = useMutation({
        mutationFn: formEndpoints.createReferral,
        onSuccess: () => {
            showStatus({ type: 'success', title: 'تمت الإحالة', message: 'تمت إحالة الطلب بنجاح' });
            queryClient.invalidateQueries({ queryKey: ['request-transactions'] });
            queryClient.invalidateQueries({ queryKey: ['requests'] });
            handleCloseProcessModal();
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'فشل إحالة الطلب' });
        }
    });

    const handleOpenProcessModal = (referral: RequestTransactionDto) => {
        if (referral.request) {
            setSelectedRequest(referral.request);
            setSelectedReferral(referral);
            setIsProcessModalOpen(true);
        }
    };

    const handleCloseProcessModal = () => {
        setIsProcessModalOpen(false);
        setSelectedRequest(null);
        setSelectedReferral(null);
        setComment('');
        setFiles([]);
        setTargetUserId('');
        setSubmissionMode('submit');
    };

    const handleSubmit = async () => {
        if (!selectedRequest || !currentUser) return;

        if (submissionMode === 'submit') {
            responseMutation.mutate({
                requestId: selectedRequest.id,
                comment,
                respondedByUserId: currentUser.id,
                files: files.length > 0 ? files : undefined
            });
        } else {
            if (!targetUserId) {
                showStatus({ type: 'warning', title: 'تنبيه', message: 'يرجى اختيار المستخدم المحال إليه' });
                return;
            }
            referralMutation.mutate({
                requestId: selectedRequest.id,
                notes: comment,
                parentTransactionId: selectedReferral?.id || selectedRequest.currentTransactionId,
                targetUserId,
                files: files.length > 0 ? files : undefined
            });
        }
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setFiles(prev => [...prev, ...Array.from(e.target.files!)]);
        }
    };

    const handleFilesAdd = (newFiles: File[]) => {
        setFiles(prev => [...prev, ...newFiles]);
    };

    const removeFile = (index: number) => {
        setFiles(prev => prev.filter((_, i) => i !== index));
    };

    const getTemplateTitle = (templateId: string) => {
        return templates.find(t => t.id === templateId)?.title || 'نموذج غير معروف';
    };

    const getTemplateFields = (templateId: string) => {
        return templates.find(t => t.id === templateId)?.fields || [];
    };

    const isSubmitting = responseMutation.isPending || referralMutation.isPending;



    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-8">
            {/* Header section */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-6 pb-6 border-b border-primary/10">
                <div className="flex items-center gap-4">
                    <div className={`p-4 rounded-3xl ${isPending ? 'bg-amber-500/10 text-amber-600' : 'bg-sky-500/10 text-sky-600'} shadow-sm`}>
                        {isPending ? <Forward className="w-8 h-8" /> : <Send className="w-8 h-8" />}
                    </div>
                    <div>
                        <h1 className="text-3xl font-black tracking-tight">
                            {isPending ? 'الرد على الإحالات' : 'الإحالات الصادرة'}
                        </h1>
                        <p className="text-muted-foreground text-sm mt-1 font-medium">
                            {isPending 
                                ? 'قائمة الطلبات التي تمت إحالتها إليك وتنتظر إجراءً منك' 
                                : 'سجل الطلبات التي قمت بإحالتها وتوجيهها لجهات أخرى'}
                        </p>
                    </div>
                </div>
                
                <div className="flex items-center gap-3">
                    <div className="px-5 py-2.5 bg-background border shadow-sm rounded-2xl flex items-center gap-3">
                        <div className={`w-3 h-3 rounded-full animate-pulse ${isPending ? 'bg-amber-500' : 'bg-sky-500'}`} />
                        <span className="text-sm font-bold">
                            {pagedData?.totalItems || 0} {isPending ? 'إحالات بانتظار الإجراء' : 'إحالات تم تصديرها'}
                        </span>
                    </div>
                </div>
            </div>

            {/* Content Section */}
            <div className="grid grid-cols-1 gap-6">
                {(isLoadingData || isLoadingTemplates) ? (
                    Array.from({ length: 3 }).map((_, i) => (
                        <Card key={i} className="p-1">
                            <Skeleton className="h-[200px] w-full rounded-2xl" />
                        </Card>
                    ))
                ) : pagedData?.items.length ? (
                    pagedData.items.map((referral) => (
                        <Card 
                            key={referral.id} 
                            className="group overflow-hidden border-2 border-transparent hover:border-primary/20 transition-all duration-300 hover:shadow-xl hover:shadow-primary/5 bg-card/50 backdrop-blur-sm"
                        >
                            <div className="flex flex-col lg:flex-row">
                                {/* Side info panel */}
                                <div className={`lg:w-72 p-6 flex flex-col gap-4 border-l border-primary/5 ${isPending ? 'bg-amber-500/5' : 'bg-sky-500/5'}`}>
                                    <div className="flex items-center gap-3">
                                        <div className="p-2 bg-white dark:bg-zinc-900 rounded-xl shadow-sm">
                                            <FileText className="w-5 h-5 text-primary" />
                                        </div>
                                        <div className="flex flex-col overflow-hidden">
                                            <span className="text-xs text-muted-foreground font-bold uppercase tracking-wider">نوع الطلب</span>
                                            <span className="text-sm font-black truncate">{getTemplateTitle(referral.request?.templateId || '')}</span>
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


                                    {isPending && (
                                        <Button 
                                            className="mt-auto w-full h-12 rounded-2xl font-black bg-primary hover:bg-primary/90 text-primary-foreground shadow-lg shadow-primary/20 group-hover:scale-[1.02] transition-transform"
                                            onClick={() => handleOpenProcessModal(referral)}
                                        >
                                            اتخاذ إجراء
                                            <ArrowRight className="w-4 h-4 mr-2" />
                                        </Button>
                                    )}
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


                                    {/* Request Preview */}
                                    <div className="bg-background/40 p-5 rounded-2xl border border-primary/5">
                                        <div className="text-[10px] font-black text-muted-foreground uppercase tracking-widest mb-3 opacity-50">معاينة بيانات الطلب</div>
                                        <RequestFieldsPreview 
                                            content={referral.request?.content || '{}'}
                                            fields={getTemplateFields(referral.request?.templateId || '')}
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
                        </Card>
                    ))
                ) : (
                    <div className="h-96 flex flex-col items-center justify-center text-muted-foreground border-4 border-dashed rounded-[40px] bg-muted/5 opacity-60">
                        <MessageSquare className="w-20 h-20 mb-6 opacity-10" />
                        <h3 className="text-xl font-bold">لا توجد إحالات متاحة</h3>
                        <p className="text-sm mt-2 font-medium">سيتم عرض الإحالات هنا بمجرد وصولها أو تصديرها</p>
                    </div>
                )}
            </div>

            {/* Pagination */}
            {pagedData && pagedData.totalPages > 1 && (
                <div className="flex justify-center pt-8">
                    <Pagination 
                        currentPage={page}
                        totalPages={pagedData.totalPages}
                        onPageChange={setPage}
                    />
                </div>
            )}

            {/* Processing Modal — uses the request from the referral directly */}
            {selectedRequest && (
                <ProcessRequestModal 
                    isOpen={isProcessModalOpen}
                    onClose={handleCloseProcessModal}
                    request={selectedRequest}
                    template={templates.find(t => t.id === selectedRequest.templateId) || null}
                    comment={comment}
                    files={files}
                    isPending={isSubmitting}
                    onCommentChange={setComment}
                    onFileChange={handleFileChange}
                    onFilesAdd={handleFilesAdd}
                    onRemoveFile={removeFile}
                    submissionMode={submissionMode}
                    onSubmissionModeChange={setSubmissionMode}
                    targetUserId={targetUserId}
                    onTargetUserChange={setTargetUserId}
                    onSubmit={handleSubmit}
                />
            )}
        </AnimatedContainer>
    );
};

export default ReferralsPage;
