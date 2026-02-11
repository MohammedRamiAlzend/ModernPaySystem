import type { FormSchema } from '@/entities/form/model/types';

export interface FormResponse {
    id: string;
    formId: string;
    submittedAt: string;
    data: Record<string, any>;
    /** The complete form schema at the time of submission */
    schema: FormSchema;
    attachments?: any[];
}

const RESPONSES_KEY = 'form_responses';

export const saveFormResponse = (schema: FormSchema, data: Record<string, any>) => {
    const responses = getAllResponses();
    const newResponse: FormResponse = {
        id: crypto.randomUUID(),
        formId: schema.id,
        submittedAt: new Date().toISOString(),
        data,
        schema,
    };

    responses.push(newResponse);
    localStorage.setItem(RESPONSES_KEY, JSON.stringify(responses));
    return newResponse;
};

export const getResponsesByFormId = (formId: string): FormResponse[] => {
    const responses = getAllResponses();
    return responses.filter(r => r.formId === formId);
};

export const getAllResponses = (): FormResponse[] => {
    const stored = localStorage.getItem(RESPONSES_KEY);
    if (!stored) return [];
    try {
        return JSON.parse(stored);
    } catch (e) {
        console.error('Failed to parse form responses from local storage', e);
        return [];
    }
};

export const deleteResponse = (responseId: string) => {
    const responses = getAllResponses();
    const filtered = responses.filter(r => r.id !== responseId);
    localStorage.setItem(RESPONSES_KEY, JSON.stringify(filtered));
};
