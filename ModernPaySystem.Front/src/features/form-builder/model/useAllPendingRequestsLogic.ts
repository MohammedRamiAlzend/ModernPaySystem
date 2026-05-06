import { useState } from 'react';
import { useQueryState, parseAsInteger } from 'nuqs';
import { useAllPendingRequests } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useAuthStore } from '@/app/store/authStore';
import { useRequestDetails } from './useRequestDetails';

const SEEN_ALL_PENDING_KEY = 'seen_all_pending_requests_ids';

export const useAllPendingRequestsLogic = () => {
    const [requestId, setRequestId] = useState('');

    // Load seen IDs from localStorage
    const [seenIds, setSeenIds] = useState<string[]>(() => {
        if (typeof window === 'undefined') return [];
        const saved = localStorage.getItem(SEEN_ALL_PENDING_KEY);
        if (saved) {
            try {
                const parsed = JSON.parse(saved);
                if (Array.isArray(parsed)) {
                    return parsed;
                }
            } catch {
                console.error("Failed to parse seen requests");
            }
        }
        return [];
    });

    const currentUser = useAuthStore((state) => state.user);
    const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(1));
    const { data: pagedRequests, isLoading } = useAllPendingRequests(page, 15);

    // Map requests to include isNew status
    const requests = (pagedRequests?.items || []).map(r => ({
        ...r,
        isNew: !seenIds.includes(r.id)
    }));

    const { data: templates = [] } = useForms(true);

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest: originalHandleViewRequest } = useRequestDetails();

    const markAsSeen = (id: string) => {
        if (id && !seenIds.includes(id)) {
            const newSeenIds = [...seenIds, id];
            setSeenIds(newSeenIds);
            localStorage.setItem(SEEN_ALL_PENDING_KEY, JSON.stringify(newSeenIds));
        }
    };

    const handleViewRequest = (request: any) => {
        markAsSeen(request.id);
        originalHandleViewRequest(request);
    };

    const handleSelectRequest = (id: string) => {
        markAsSeen(id);
        setRequestId(id);
    };

    const selectedRequest = requests.find(r => r.id === requestId);

    return {
        requestId,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        currentUser,
        requests,
        selectedRequest,
        isLoading,
        templates,
        totalItems: pagedRequests?.totalItems || 0,
        totalPages: pagedRequests?.totalPages || 0,
        page,
        setPage,
        setRequestId,
        handleSelectRequest,
        handleViewRequest
    };
};
