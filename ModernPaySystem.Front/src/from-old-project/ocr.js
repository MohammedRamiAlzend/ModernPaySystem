import apiClient from './client';

const ocr = {
    async extractText({ language, imageFile }) {
        const formData = new FormData();
        formData.append('language', language);
        formData.append('imageFile', imageFile);

        const response = await apiClient.post(
             `Ocr/Ocr/Extract-Text-From-Image?language=${language}`,
            formData,
            { headers: { 'Content-Type': 'multipart/form-data' } }
        );

        return response.data;
    },
};

export default ocr;


