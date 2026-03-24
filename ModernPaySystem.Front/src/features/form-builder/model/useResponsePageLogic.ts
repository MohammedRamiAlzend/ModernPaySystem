import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useQueryState, parseAsInteger } from 'nuqs';
import { formEndpoints, useRequests } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useAuthStore } from '@/app/store/authStore';
import { useUIStore } from '@/app/store/uiStore';
import { useRequestDetails } from './useRequestDetails';

export const useResponsePageLogic = () => {
    const { showStatus } = useUIStore();
    const queryClient = useQueryClient();
    const [requestId, setRequestId] = useState('');
    const [comment, setComment] = useState('');
    const [files, setFiles] = useState<File[]>([]);

    const currentUser = useAuthStore((state) => state.user);
    const [page, setPage] = useQueryState('page', parseAsInteger.withDefault(1));
    const { data: pagedRequests, isLoading } = useRequests(false, page, 15);
    const requests = pagedRequests?.items || [];
    const { data: templates = [] } = useForms();

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

    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest } = useRequestDetails(templates);

    const handleSubmit = () => {
        if (!requestId || !currentUser) return;
        responseMutation.mutate({
            requestId,
            comment,
            respondedByUserId: currentUser.id,
            files: files.length > 0 ? files : undefined
        });
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            setFiles(prev => [...prev, ...Array.from(e.target.files!)]);
        }
    };

    const removeFile = (index: number) => {
        setFiles(prev => prev.filter((_, i) => i !== index));
    };

    const handleSelectRequest = (id: string) => {
        setRequestId(id);
    };

    return {
        requestId,
        comment,
        files,
        isModalOpen,
        setIsModalOpen,
        viewingResponse,
        currentUser,
        requests,
        isLoading,
        templates,
        totalItems: pagedRequests?.totalItems || 0,
        totalPages: pagedRequests?.totalPages || 0,
        page,
        setPage,
        isPending: responseMutation.isPending,
        setComment,
        handleSubmit,
        handleFileChange,
        removeFile,
        handleSelectRequest,
        handleViewRequest
    };
};
