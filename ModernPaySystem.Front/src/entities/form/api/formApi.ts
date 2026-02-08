import type { FormSchema } from "../model/types";

export const STORAGE_KEY = 'form_builder_schemas';

// Simulate API delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export const formApi = {
    getForms: async (): Promise<FormSchema[]> => {
        await delay(300);
        const stored = localStorage.getItem(STORAGE_KEY);
        return stored ? JSON.parse(stored) : [];
    },

    getFormById: async (id: string): Promise<FormSchema | null> => {
        await delay(200);
        const forms = await formApi.getForms();
        return forms.find(f => f.id === id) || null;
    },
};
