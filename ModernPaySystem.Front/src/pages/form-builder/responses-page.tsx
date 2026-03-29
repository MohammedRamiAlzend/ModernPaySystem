import { useState, useEffect } from 'react';
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
        currentUser,
        requests,
        selectedRequest,
        isLoading,
        templates,
        totalPages,
        page,
        setPage,
        isPending,
        setComment,
        setRequestId,
        handleSubmit,
        handleFileChange,
        removeFile,
        handleSelectRequest,
        handleViewRequest,
        showActioned,
        setShowActioned
    } = useResponsePageLogic();

    const [isProcessModalOpen, setIsProcessModalOpen] = useState(false);

    // Automatically open process modal when a request is selected (row click)
    useEffect(() => {
        if (requestId && selectedRequest) {
            setIsProcessModalOpen(true);
        }
    }, [requestId, selectedRequest]);

    // Cleanup when closing process modal
    const handleCloseProcessModal = () => {
        setIsProcessModalOpen(false);
        setRequestId(''); // Clear selection
    };

    return (
        <AnimatedContainer className="container mx-auto p-4 space-y-6">
            <div className="flex items-center justify-between pb-4 border-b">
                <div>
                     <h1 className="text-3xl font-black text-primary">الردود والطلبات الواردة</h1>
                     <p className="text-muted-foreground text-sm mt-1">قم بمراجعة الطلبات واتخاذ القرارات اللازمة بشأنها</p>
                </div>
                 <div className="flex items-center gap-3">
                      <div className="flex p-1 bg-muted rounded-2xl overflow-hidden border">
                          <button 
                              onClick={() => setShowActioned(false)}
                              className={`px-4 py-1.5 text-xs font-bold rounded-xl transition-all ${!showActioned ? 'bg-primary text-white shadow-sm' : 'text-muted-foreground hover:bg-muted-foreground/10'}`}
                          >
                              قيد الانتظار
                          </button>
                          <button 
                              onClick={() => setShowActioned(true)}
                              className={`px-4 py-1.5 text-xs font-bold rounded-xl transition-all ${showActioned ? 'bg-primary text-white shadow-sm' : 'text-muted-foreground hover:bg-muted-foreground/10'}`}
                          >
                              تم الرد عليها
                          </button>
                      </div>
                      <span className="px-4 py-2 bg-primary/5 text-primary text-xs font-bold rounded-2xl border border-primary/10 flex items-center gap-2">
                          <div className={`w-2 h-2 rounded-full ${!showActioned ? 'bg-primary animate-pulse' : 'bg-emerald-500'}`} />
                          {requests.length} {showActioned ? 'طلبات تمت معالجتها' : 'طلبات متاحة للمعالجة'}
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
                    showActioned={showActioned}
                />
            </div>

            {/* Modal 1: Processing Workflow (Row Click) */}
            {selectedRequest && (
                <ProcessRequestModal
                    isOpen={isProcessModalOpen}
                    onClose={handleCloseProcessModal}
                    request={selectedRequest}
                    template={templates.find(t => t.id === selectedRequest.templateId) || null}
                    comment={comment}
                    files={files}
                    isPending={isPending}
                    currentUser={currentUser}
                    onCommentChange={setComment}
                    onFileChange={handleFileChange}
                    onRemoveFile={removeFile}
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
        </AnimatedContainer>
    );
};

export default ResponsesPage;
