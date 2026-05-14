import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { Card } from '@/shared/ui/card';
import { Forward, Send, MessageSquare } from 'lucide-react';
import { Skeleton } from '@/shared/ui/common/skeleton';
import { Pagination } from '@/shared/ui/common/pagination';
import { ProcessRequestModal } from '@/features/form-builder/ui/ProcessRequestModal';
import { useReferralsLogic } from '@/features/form-builder/model/useReferralsLogic';
import { ReferralCard } from '@/features/form-builder/ui/ReferralCard';
import { RequestFilterPanel } from '@/features/request-filter/ui/RequestFilterPanel';

interface ReferralsPageProps {
    status: number; // 0 for PendingAction, 1 for Transferred
}

export const ReferralsPage = ({ status }: ReferralsPageProps) => {
    const isPending = status === 0;
    const {
        page,
        setPage,
        filter,
        pagedData,
        isLoadingData,
        isProcessModalOpen,
        selectedRequest,
        comment,
        files,
        submissionMode,
        targetUserId,
        selectedTemplate,
        isTemplateLoading,
        isSubmitting,
        setComment,
        setSubmissionMode,
        setTargetUserId,
        handleOpenProcessModal,
        handleCloseProcessModal,
        handleSubmit,
        handleFileChange,
        handleFilesAdd,
        removeFile
    } = useReferralsLogic(status);

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

            <RequestFilterPanel filter={filter} />

            {/* Content Section */}
            <div className="grid grid-cols-1 gap-6">
                {isLoadingData ? (
                    Array.from({ length: 3 }).map((_, i) => (
                        <Card key={i} className="p-1">
                            <Skeleton className="h-[200px] w-full rounded-2xl" />
                        </Card>
                    ))
                ) : pagedData?.items.length ? (
                    pagedData.items.map((referral) => (
                        <ReferralCard
                            key={referral.id}
                            referral={referral}
                            isPending={isPending}
                            onAction={handleOpenProcessModal}
                        />
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

            {/* Processing Modal */}
            {selectedRequest && (
                <ProcessRequestModal
                    isOpen={isProcessModalOpen}
                    onClose={handleCloseProcessModal}
                    request={selectedRequest}
                    template={selectedTemplate || null}
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

            {isTemplateLoading && (
                <div className="fixed inset-0 bg-black/20 backdrop-blur-[2px] z-[9999] flex items-center justify-center pointer-events-none">
                    <div className="bg-white p-4 rounded-2xl shadow-xl flex items-center gap-3 border animate-in fade-in zoom-in duration-200">
                        <div className="w-5 h-5 border-2 border-primary border-t-transparent rounded-full animate-spin" />
                        <span className="text-sm font-bold text-primary">جاري جلب بيانات النموذج...</span>
                    </div>
                </div>
            )}
        </AnimatedContainer>
    );
};

export default ReferralsPage;
