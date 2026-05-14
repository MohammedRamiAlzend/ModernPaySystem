import { useQueryState, parseAsInteger } from 'nuqs';
import { useRequests } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useRequestDetails } from './useRequestDetails';
import { useRequestFilter } from '@/features/request-filter/model/useRequestFilter';

export const useActionedRequestsLogic = () => {
    const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(1));
    const filter = useRequestFilter('actioned-requests');
    const { data: pagedRequests, isLoading } = useRequests(true, { ...filter.filterParams, page, pageSize: 15 }); // Fetch responded requests
    const requests = pagedRequests?.items || [];
    const { data: templates = [] } = useForms(true);

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest } = useRequestDetails();

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
        handleViewRequest,
        filter
    };
};
