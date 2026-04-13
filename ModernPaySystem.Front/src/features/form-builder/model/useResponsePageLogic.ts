import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useQueryState, parseAsInteger } from 'nuqs';
import { formEndpoints, useRequests, useCreateReferral } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useAuthStore } from '@/app/store/authStore';
import { useUIStore } from '@/app/store/uiStore';
import { useRequestDetails } from './useRequestDetails';

const SEEN_REQUESTS_KEY = 'seen_incoming_requests_ids';

export const useResponsePageLogic = () => {
    const { showStatus } = useUIStore();
    const queryClient = useQueryClient();
    const [requestId, setRequestId] = useState('');
    const [comment, setComment] = useState('');
    const [files, setFiles] = useState<File[]>([]);
    const [submissionMode, setSubmissionMode] = useState<'submit' | 'referral'>('submit');
    const [targetUserId, setTargetUserId] = useState('');

    // Load seen IDs from localStorage
    const [seenIds, setSeenIds] = useState<string[]>(() => {
        if (typeof window === 'undefined') return [];
        const saved = localStorage.getItem(SEEN_REQUESTS_KEY);
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
    const { data: pagedRequests, isLoading } = useRequests(false, page, 15);

    // Map requests to include isNew status
    const requests = (pagedRequests?.items || []).map(r => ({
        ...r,
        isNew: !seenIds.includes(r.id)
    }));

    const { data: templates = [] } = useForms(true);

    const responseMutation = useMutation({
        mutationFn: formEndpoints.createResponse,
        onSuccess: () => {
            showStatus({
                type: 'success',
                title: 'تمت العملية',
                message: 'تم إرسال الرد بنجاح'
            });
            queryClient.invalidateQueries({ queryKey: ['requests'] });
            setComment('');
            setRequestId('');
            setFiles([]);
        },
        onError: () => {
            showStatus({
                type: 'error',
                title: 'خطأ',
                message: 'فشل إرسال الرد'
            });
        }
    });

    const referralMutation = useCreateReferral();

    const handleReferralSuccess = () => {
        showStatus({
            type: 'success',
            title: 'تمت الإحالة',
            message: 'تمت إحالة الطلب بنجاح'
        });
        queryClient.invalidateQueries({ queryKey: ['requests'] });
        setComment('');
        setRequestId('');
        setFiles([]);
        setTargetUserId('');
        setSubmissionMode('submit');
    };

    const handleMutationError = (title: string, message: string) => {
        showStatus({
            type: 'error',
            title,
            message
        });
    };

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest: originalHandleViewRequest } = useRequestDetails(templates);

    const markAsSeen = (id: string) => {
        if (id && !seenIds.includes(id)) {
            const newSeenIds = [...seenIds, id];
            setSeenIds(newSeenIds);
            localStorage.setItem(SEEN_REQUESTS_KEY, JSON.stringify(newSeenIds));
        }
    };

    const handleViewRequest = (request: any) => {
        markAsSeen(request.id);
        originalHandleViewRequest(request);
    };

    const handleSubmit = async () => {
        if (!requestId || !currentUser) return;

        if (submissionMode === 'submit') {
            responseMutation.mutate({
                requestId,
                comment,
                respondedByUserId: currentUser.id,
                files: files.length > 0 ? files : undefined
            });
        } else {
            if (!targetUserId) {
                showStatus({
                    type: 'warning',
                    title: 'تنبيه',
                    message: 'يرجى اختيار المستخدم المحال إليه'
                });
                return;
            }

            referralMutation.mutate({
                requestId: requestId,
                notes: comment,
                parentTransactionId: selectedRequest?.currentTransactionId,
                targetUserId: targetUserId,
                files: files.length > 0 ? files : undefined
            }, {
                onSuccess: handleReferralSuccess,
                onError: () => handleMutationError('خطأ', 'فشل إحالة الطلب')
            });
        }
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setFiles(prev => [...prev, ...Array.from(e.target.files!)]);
        }
    };

    const handleFilesAdd = (newFiles: File[]) => {
        setFiles(prev => [...prev, ...newFiles]);
    };

    const removeFile = (index: number) => {
        setFiles(prev => prev.filter((_, i) => i !== index));
    };

    const handleSelectRequest = (id: string) => {
        markAsSeen(id);
        setRequestId(id);
    };

    const selectedRequest = requests.find(r => r.id === requestId);

    return {
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
        totalItems: pagedRequests?.totalItems || 0,
        totalPages: pagedRequests?.totalPages || 0,
        page,
        setPage,
        isPending: responseMutation.isPending || referralMutation.isPending,
        submissionMode,
        setSubmissionMode,
        targetUserId,
        setTargetUserId,
        setComment,
        setRequestId,
        handleSubmit,
        handleFileChange,
        handleFilesAdd,
        removeFile,
        handleSelectRequest,
        handleViewRequest
    };
};
