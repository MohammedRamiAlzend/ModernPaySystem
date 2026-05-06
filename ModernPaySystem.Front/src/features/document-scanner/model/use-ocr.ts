import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { extractText, getSupportedLanguages } from '../api/ocr.api';
import { OcrRequest } from './types';

export const useOcr = () => {
    const [ocrResult, setOcrResult] = useState('');

    const { data: languagesData, isLoading: isLoadingLanguages } = useQuery({
        queryKey: ['ocr', 'languages'],
        queryFn: getSupportedLanguages,
    });

    const ocrMutation = useMutation({
        mutationFn: (request: OcrRequest) => extractText(request),
    });

    return {
        extract: ocrMutation.mutateAsync,
        isPending: ocrMutation.isPending,
        error: ocrMutation.error,
        ocrResult,
        setOcrResult,
        languages: languagesData?.languages || [],
        defaultLanguage: languagesData?.defaultLanguage,
        isLoadingLanguages,
    };
};
