import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { formEndpoints, useRequests } from '../api/formEndpoints';
import { useForms } from './useForms';
import { useAppSelector, useAppDispatch } from '@/app/store';
import { selectCurrentUser } from '@/app/store/authSlice';
import { showStatus } from '@/app/store/uiSlice';
import { useRequestDetails } from './useRequestDetails';

export const useResponsePageLogic = () => {
    const dispatch = useAppDispatch();
    const queryClient = useQueryClient();
    const [requestId, setRequestId] = useState('');
    const [comment, setComment] = useState('');
    const [files, setFiles] = useState<File[]>([]);

    const currentUser = useAppSelector(selectCurrentUser);
    const { data: requests = [], isLoading } = useRequests();
    const { data: templates = [] } = useForms();

    const responseMutation = useMutation({
        mutationFn: formEndpoints.createResponse,
        onSuccess: () => {
            dispatch(showStatus({
                type: 'success',
                title: 'تمت العملية',
                message: 'تم إرسال الرد بنجاح'
            }));
            queryClient.invalidateQueries({ queryKey: ['requests'] });
            setComment('');
            setRequestId('');
            setFiles([]);
        },
        onError: () => {
            dispatch(showStatus({
                type: 'error',
                title: 'خطأ',
                message: 'فشل إرسال الرد'
            }));
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
        isPending: responseMutation.isPending,
        setComment,
        handleSubmit,
        handleFileChange,
        removeFile,
        handleSelectRequest,
        handleViewRequest
    };
};
