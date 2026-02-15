import api from '@/shared/api/baseApi';
import { OcrRequest, OcrResponse, SupportedLanguagesResponse } from '../model/types';

export const extractText = async ({ language, imageFile }: OcrRequest): Promise<OcrResponse> => {
    const formData = new FormData();
    formData.append('imageFile', imageFile);

    const response = await api.post<OcrResponse>(
        `Ocr/extract-text-from-image?language=${language}`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
    );

    return response.data;
};

export const getSupportedLanguages = async (): Promise<SupportedLanguagesResponse> => {
    const response = await api.get<SupportedLanguagesResponse>('Ocr/supported-languages');
    return response.data;
};
