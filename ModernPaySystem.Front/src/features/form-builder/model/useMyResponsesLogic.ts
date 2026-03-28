import { useQueryState, parseAsInteger } from 'nuqs';
import { useResponsesByRequester } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useRequestDetails } from './useRequestDetails';
import { useAuthStore } from '@/app/store/authStore';

export const useMyResponsesLogic = () => {
    const { user } = useAuthStore();
    const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(1));
    const { data: pagedResponses, isLoading } = useResponsesByRequester(user?.id || null, page, 15);
    
    // We handle both cases: Paginated Object with .items OR direct array from API
    const responseItems = Array.isArray(pagedResponses) 
        ? pagedResponses 
        : (pagedResponses as any)?.items || [];

    const totalItems = Array.isArray(pagedResponses)
        ? pagedResponses.length
        : (pagedResponses as any)?.totalItems || 0;

    const totalPages = Array.isArray(pagedResponses)
        ? 1
        : (pagedResponses as any)?.totalPages || 0;

    // We map responses back to requests for the UI, as the page expects a list of requests
    // but with the response details attached/accessible.
    const requests = responseItems
        .filter((resp: any) => !!resp.request) // Only show responses that have an associated request
        .map((resp: any) => ({
            ...resp.request!,
            responseId: resp.id,
            comment: resp.comment || '',
            respondedAt: resp.createdAt || ''
        })) || [];

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
