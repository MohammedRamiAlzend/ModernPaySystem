import api from '@/shared/api/baseApi';
import { OcrRequest, OcrResponse } from '../model/types';

export const extractText = async ({ language, imageFile }: OcrRequest): Promise<OcrResponse> => {
    const formData = new FormData();
    formData.append('language', language);
    formData.append('imageFile', imageFile);

    const response = await api.post<OcrResponse>(
        `Ocr/Ocr/Extract-Text-From-Image?language=${language}`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
    );

    return response.data;
};
