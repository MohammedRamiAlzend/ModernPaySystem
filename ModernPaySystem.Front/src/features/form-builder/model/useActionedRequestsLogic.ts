import { useState } from 'react';
import { useRequests } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useRequestDetails } from './useRequestDetails';

export const useActionedRequestsLogic = () => {
    const [page, setPage] = useState(1);
    const { data: pagedRequests, isLoading } = useRequests(true, page, 15); // Fetch responded requests
    const requests = pagedRequests?.items || [];
    const { data: templates = [] } = useForms();

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest } = useRequestDetails(templates);

    return {
        requests,
        isLoading,
        templates,
        totalItems: pagedRequests?.totalItems || 0,
        totalPages: pagedRequests?.totalPages || 0,
        page,
        setPage,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest
    };
};
