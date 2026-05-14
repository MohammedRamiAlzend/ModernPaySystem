import { useRequestsByRequester } from '../api/formEndpoints';
import { useRequestFilter } from '@/features/request-filter/model/useRequestFilter';
import { useForms } from './useForms';
import { useRequestDetails } from './useRequestDetails';
import { useAuthStore } from '@/app/store/authStore';

export const useMyRequestsLogic = () => {
    const { user } = useAuthStore();
    const filter = useRequestFilter('my-requests');
    const { data: pagedRequests, isLoading } = useRequestsByRequester(user?.id || null, filter.filterParams);
    
    const requests = pagedRequests?.items || [];
    const totalItems = pagedRequests?.totalItems || 0;
    const totalPages = pagedRequests?.totalPages || 0;

    const { data: templates = [] } = useForms(true);

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest } = useRequestDetails();

    return {
        requests,
        isLoading,
        templates,
        totalItems,
        totalPages,
        page: filter.page,
        setPage: filter.setPage,
        filter,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest
    };
};
