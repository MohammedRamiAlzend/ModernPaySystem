import type { FormSchema } from "@/entities/form/model/types";
import { formApi, STORAGE_KEY } from "@/entities/form/api/formApi";

// Simulate API delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

/**
 * Feature-level service for Actions (Write/Update/Delete)
 */
export const formService = {
    saveForm: async (form: FormSchema): Promise<FormSchema> => {
        await delay(500);
        const forms = await formApi.getForms();
        const index = forms.findIndex(f => f.id === form.id);

        if (index !== -1) {
            forms[index] = form;
        } else {
            forms.push(form);
        }

        localStorage.setItem(STORAGE_KEY, JSON.stringify(forms));
        return form;
    },

    deleteForm: async (id: string): Promise<void> => {
        await delay(300);
        const forms = await formApi.getForms();
        const newForms = forms.filter(f => f.id !== id);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(newForms));
    }
};
