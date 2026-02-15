import api from '../baseApi';

export const utilsService = {
    /**
     * Converts a decimal number to its Arabic spelling (Tafqeet)
     */
    numberSearchSpelling: async (number: number): Promise<string> => {
        const response = await api.post('/NumberSpelling/convert-decimal-to-arabic', number);
        // The API returns { data: "واحد" }
        return response.data?.data;
    }
};
