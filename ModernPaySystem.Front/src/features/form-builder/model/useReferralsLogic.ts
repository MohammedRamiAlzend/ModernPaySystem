import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useRequestTransactions, formEndpoints, useTemplateById } from '../api/formEndpoints';
import { useAuthStore } from '@/app/store/authStore';
import { useUIStore } from '@/app/store/uiStore';
import type { TemplateRequest, RequestTransactionDto } from '@/entities/form/model/types';

export const useReferralsLogic = (status: number) => {
    const [page, setPage] = useState(1);
    const pageSize = 10;

    const { data: pagedData, isLoading: isLoadingData } = useRequestTransactions(status, page, pageSize);
    const currentUser = useAuthStore((state) => state.user);
    const { showStatus } = useUIStore();
    const queryClient = useQueryClient();

    // Local state for the process modal
    const [isProcessModalOpen, setIsProcessModalOpen] = useState(false);
    const [selectedRequest, setSelectedRequest] = useState<TemplateRequest | null>(null);
    const [selectedReferral, setSelectedReferral] = useState<RequestTransactionDto | null>(null);
    const [comment, setComment] = useState('');
    const [files, setFiles] = useState<File[]>([]);
    const [submissionMode, setSubmissionMode] = useState<'submit' | 'referral'>('submit');
    const [targetUserId, setTargetUserId] = useState('');

    // Fetch the template specifically by ID for the selected request to ensure permissions
    const { data: selectedTemplate, isLoading: isTemplateLoading } = useTemplateById(selectedRequest?.templateId || null);

    const responseMutation = useMutation({
        mutationFn: formEndpoints.createResponse,
        onSuccess: () => {
            showStatus({ type: 'success', title: 'تمت العملية', message: 'تم إرسال الرد بنجاح' });
            queryClient.invalidateQueries({ queryKey: ['request-transactions'] });
            queryClient.invalidateQueries({ queryKey: ['requests'] });
            handleCloseProcessModal();
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'فشل إرسال الرد' });
        }
    });

    const referralMutation = useMutation({
        mutationFn: formEndpoints.createReferral,
        onSuccess: () => {
            showStatus({ type: 'success', title: 'تمت الإحالة', message: 'تمت إحالة الطلب بنجاح' });
            queryClient.invalidateQueries({ queryKey: ['request-transactions'] });
            queryClient.invalidateQueries({ queryKey: ['requests'] });
            handleCloseProcessModal();
        },
        onError: () => {
            showStatus({ type: 'error', title: 'خطأ', message: 'فشل إحالة الطلب' });
        }
    });

    const handleOpenProcessModal = (referral: RequestTransactionDto) => {
        if (referral.request) {
            setSelectedRequest(referral.request);
            setSelectedReferral(referral);
            setIsProcessModalOpen(true);
        }
    };

    const handleCloseProcessModal = () => {
        setIsProcessModalOpen(false);
        setSelectedRequest(null);
        setSelectedReferral(null);
        setComment('');
        setFiles([]);
        setTargetUserId('');
        setSubmissionMode('submit');
    };

    const handleSubmit = async () => {
        if (!selectedRequest || !currentUser) return;

        if (submissionMode === 'submit') {
            responseMutation.mutate({
                requestId: selectedRequest.id,
                comment,
                respondedByUserId: currentUser.id,
                files: files.length > 0 ? files : undefined
            });
        } else {
            if (!targetUserId) {
                showStatus({ type: 'warning', title: 'تنبيه', message: 'يرجى اختيار المستخدم المحال إليه' });
                return;
            }
            referralMutation.mutate({
                requestId: selectedRequest.id,
                notes: comment,
                parentTransactionId: selectedReferral?.id || selectedRequest.currentTransactionId,
                targetUserId,
                files: files.length > 0 ? files : undefined
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

    return {
        page,
        setPage,
        pagedData,
        isLoadingData,
        isProcessModalOpen,
        selectedRequest,
        selectedReferral,
        comment,
        files,
        submissionMode,
        targetUserId,
        selectedTemplate,
        isTemplateLoading,
        isSubmitting: responseMutation.isPending || referralMutation.isPending,
        setComment,
        setSubmissionMode,
        setTargetUserId,
        handleOpenProcessModal,
        handleCloseProcessModal,
        handleSubmit,
        handleFileChange,
        handleFilesAdd,
        removeFile
    };
};
