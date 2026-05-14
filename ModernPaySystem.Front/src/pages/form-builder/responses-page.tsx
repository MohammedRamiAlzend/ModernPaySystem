/* eslint-disable react-hooks/set-state-in-effect */
import { useState, useEffect, useRef } from 'react';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { IncomingRequestsList } from '@/features/form-builder/ui/IncomingRequestsList';
import { ProcessRequestModal } from '@/features/form-builder/ui/ProcessRequestModal';
import { useResponsePageLogic } from '@/features/form-builder/model/useResponsePageLogic';

export const ResponsesPage = () => {
    const {
        requestId,
        comment,
        files,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        requests,
        selectedRequest,
        isLoading,
        templates,
        selectedTemplate,
        totalPages,
        page,
        setPage,
        isPending,
        setComment,
        setRequestId,
        handleSubmit,
        handleFileChange,
        handleFilesAdd,
        removeFile,
        handleSelectRequest,
        handleViewRequest,
        submissionMode,
        setSubmissionMode,
        targetUserId,
        setTargetUserId,
        isTemplateLoading
    } = useResponsePageLogic();

    const [isProcessModalOpen, setIsProcessModalOpen] = useState(false);
    const prevRequestIdRef = useRef<string | null>(null);

    // Automatically sync modal state with request selection
    useEffect(() => {
        if (requestId && selectedRequest) {
            if (prevRequestIdRef.current !== requestId) {
                setIsProcessModalOpen(true);
                prevRequestIdRef.current = requestId;
            }
        } else if (!requestId) {
            // Clear ref and close modal when requestId is cleared
            prevRequestIdRef.current = null;
            setIsProcessModalOpen(false);
        }
    }, [requestId, selectedRequest]);

    // Cleanup when closing process modal
    const handleCloseProcessModal = () => {
        setRequestId(''); // This will trigger the useEffect logic above
    };

    return (
        <AnimatedContainer className="container mx-auto p-4 space-y-6">
            <div className="flex items-center justify-between pb-4 border-b">
                <div>
                    <h1 className="text-3xl font-black text-primary">الطلبات الواردة</h1>
                    <p className="text-muted-foreground text-sm mt-1">قم بمراجعة الطلبات واتخاذ القرارات اللازمة بشأنها</p>
                </div>
                <div className="flex items-center gap-3">
                    <span className="px-4 py-2 bg-primary/5 text-primary text-xs font-bold rounded-2xl border border-primary/10 flex items-center gap-2">
                        <div className="w-2 h-2 bg-primary rounded-full animate-pulse" />
                        {requests.length} طلبات متاحة للمعالجة
                    </span>
                </div>
            </div>

            <div className="h-[calc(100vh-200px)]">
                <IncomingRequestsList
                    requests={requests}
                    isLoading={isLoading}
                    templates={templates}
                    selectedRequestId={requestId}
                    onSelectRequest={handleSelectRequest}
                    onViewRequest={handleViewRequest}
                    page={page}
                    totalPages={totalPages}
                    onPageChange={setPage}
                />
            </div>

            {/* Modal 1: Processing Workflow (Row Click) */}
            {selectedRequest && (
                <ProcessRequestModal
                    isOpen={isProcessModalOpen}
                    onClose={handleCloseProcessModal}
                    request={selectedRequest}
                    template={selectedTemplate ?? null}
                    comment={comment}
                    files={files}
                    isPending={isPending}
                    onCommentChange={setComment}
                    onFileChange={handleFileChange}
                    onFilesAdd={handleFilesAdd}
                    onRemoveFile={removeFile}
                    submissionMode={submissionMode}
                    onSubmissionModeChange={setSubmissionMode}
                    targetUserId={targetUserId}
                    onTargetUserChange={setTargetUserId}
                    onSubmit={async () => {
                        await handleSubmit();
                    }}
                />
            )}

            {/* Modal 2: Viewing Details Only (Eye Icon Click) */}
            {viewingResponse && (
                <ResponseDetailsModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    response={viewingResponse}
                    schema={viewingResponse.schema}
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

export default ResponsesPage;
