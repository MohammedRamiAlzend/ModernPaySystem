// import { useState, useEffect, useRef } from 'react';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { IncomingRequestsList } from '@/features/form-builder/ui/IncomingRequestsList';
import { useAllPendingRequestsLogic } from '@/features/form-builder/model/useAllPendingRequestsLogic';

export const AllPendingRequestsPage = () => {
    const {
        requestId,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        requests,
        // selectedRequest,
        isLoading,
        templates,
        totalPages,
        page,
        setPage,
        // setRequestId,
        handleSelectRequest,
        handleViewRequest
    } = useAllPendingRequestsLogic();

    return (
        <AnimatedContainer className="container mx-auto p-4 space-y-6">
            <div className="flex items-center justify-between pb-4 border-b">
                <div>
                    <h1 className="text-3xl font-black text-primary">الطلبات المعلقة</h1>
                    <p className="text-muted-foreground text-sm mt-1">عرض جميع الطلبات التي لم يتم الرد عليها بعد</p>
                </div>
                <div className="flex items-center gap-3">
                    <span className="px-4 py-2 bg-primary/5 text-primary text-xs font-bold rounded-2xl border border-primary/10 flex items-center gap-2">
                        <div className="w-2 h-2 bg-primary rounded-full animate-pulse" />
                        {requests.length} طلبات معلقة
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

            {/* Modal: Viewing Details Only (Eye Icon Click) */}
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

export default AllPendingRequestsPage;
