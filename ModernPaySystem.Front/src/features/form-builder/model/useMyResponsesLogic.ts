import { useQueryState, parseAsInteger } from 'nuqs';
import { useResponsesByRequester } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useRequestDetails } from './useRequestDetails';
import { useAuthStore } from '@/app/store/authStore';
import { useState, useEffect } from 'react';

const SEEN_RESPONSES_KEY = 'seen_responses_ids';

export const useMyResponsesLogic = () => {
    const { user } = useAuthStore();
    const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(1));
    const { data: pagedResponses, isLoading } = useResponsesByRequester(user?.id || null, page, 15);
    const [seenIds, setSeenIds] = useState<string[]>([]);

    // Load seen IDs from localStorage on mount
    useEffect(() => {
        const saved = localStorage.getItem(SEEN_RESPONSES_KEY);
        if (saved) {
            try {
                setSeenIds(JSON.parse(saved));
            } catch (e) {
                console.error("Failed to parse seen responses", e);
            }
        }
    }, []);

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
            respondedAt: resp.createdAt || '',
            isNew: !seenIds.includes(resp.id) // Check if this response ID has been seen
        })) || [];

    const { data: templates = [] } = useForms(true);

    const { isModalOpen: isDetailsOpen, setIsModalOpen, viewingResponse, handleViewRequest: originalHandleViewRequest } = useRequestDetails(templates);

    // Override handleViewRequest to mark as seen
    const handleViewRequest = (request: any) => {
        if (request.responseId && !seenIds.includes(request.responseId)) {
            const newSeenIds = [...seenIds, request.responseId];
            setSeenIds(newSeenIds);
            localStorage.setItem(SEEN_RESPONSES_KEY, JSON.stringify(newSeenIds));
        }
        originalHandleViewRequest(request);
    };

    return {
        requests,
        isLoading,
        templates,
        totalItems,
        totalPages,
        page,
        setPage,
        isModalOpen: isDetailsOpen,
        setIsModalOpen,
        viewingResponse,
        handleViewRequest
    };
};
