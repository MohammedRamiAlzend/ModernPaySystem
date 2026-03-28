import { useQueryState, parseAsInteger } from 'nuqs';
import { useRequestsByRequester } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useRequestDetails } from './useRequestDetails';
import { useAuthStore } from '@/app/store/authStore';

export const useMyRequestsLogic = () => {
    const { user } = useAuthStore();
    const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(1));
    const { data: pagedRequests, isLoading } = useRequestsByRequester(user?.id || null, page, 15);
    
    const requests = pagedRequests?.items || [];
    const totalItems = pagedRequests?.totalItems || 0;
    const totalPages = pagedRequests?.totalPages || 0;

    const { data: templates = [] } = useForms();

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest } = useRequestDetails(templates);

    return {
        requests,
        isLoading,
        templates,
        totalItems,
        totalPages,
        page,
        setPage,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest
    };
};
