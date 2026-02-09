import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { extractText } from '../api/ocr.api';
import { OcrRequest } from './types';

export const useOcr = () => {
    const [ocrResult, setOcrResult] = useState('');

    const ocrMutation = useMutation({
        mutationFn: (request: OcrRequest) => extractText(request),
        onSuccess: (data) => {
            if (data.success) {
                setOcrResult((prev) => prev ? `${prev}\n\n${data.extractedText}` : data.extractedText);
            }
        },
    });

    return {
        extract: ocrMutation.mutateAsync,
        isPending: ocrMutation.isPending,
        error: ocrMutation.error,
        ocrResult,
        setOcrResult,
    };
};
