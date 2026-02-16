import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { IncomingRequestsList } from '@/features/form-builder/ui/IncomingRequestsList';
import { ResponseForm } from '@/features/form-builder/ui/ResponseForm';
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
        isLoading,
        templates,
        totalPages,
        page,
        setPage,
        isPending,
        setComment,
        handleSubmit,
        handleFileChange,
        removeFile,
        handleSelectRequest,
        handleViewRequest
    } = useResponsePageLogic();

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6">
            <h1 className="text-3xl font-bold">الرد على الطلبات</h1>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
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

                <ResponseForm
                    requestId={requestId}
                    comment={comment}
                    files={files}
                    isPending={isPending}
                    currentUser={currentUser}
                    onCommentChange={setComment}
                    onFileChange={handleFileChange}
                    onRemoveFile={removeFile}
                    onSubmit={handleSubmit}
                />
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

export default ResponsesPage;
