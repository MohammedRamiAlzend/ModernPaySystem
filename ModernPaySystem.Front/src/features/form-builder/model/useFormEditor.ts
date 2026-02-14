import { useState } from 'react';
import type { FormSchema, FormField } from '@/entities/form/model/types';
import { useSaveForm } from './useForms';

export const useFormEditor = (initialForm?: FormSchema) => {
    const [form, setForm] = useState<FormSchema>(initialForm || {
        id: crypto.randomUUID(),
        title: 'New Form',
        fields: [],
        logic: []
    });

    const updateFormTitle = (title: string) => {
        setForm(prev => ({ ...prev, title }));
    };

    const addField = (type: FormField['type'], lookUpFieldId?: string, customLabel?: string) => {
        const newField: FormField = {
            id: crypto.randomUUID(),
            name: `field_${Date.now()}`,
            type,
            label: customLabel || (lookUpFieldId ? 'حقل من الإعدادات' : `New ${type} Field`),
            validation: [],
            // Initialize dataSource for types that need options
            dataSource: (type === 'select' || type === 'radio' || type === 'checkbox') ? (
                lookUpFieldId ? {
                    type: 'lookup',
                    lookUpFieldId
                } : {
                    type: 'static',
                    options: [
                        { label: 'Option 1', value: 'opt1' },
                        { label: 'Option 2', value: 'opt2' }
                    ]
                }
            ) : undefined
        };
        setForm(prev => ({
            ...prev,
            fields: [...prev.fields, newField]
        }));
    };

    const updateField = (id: string, updates: Partial<FormField>) => {
        setForm(prev => ({
            ...prev,
            fields: prev.fields.map(f => f.id === id ? { ...f, ...updates } : f)
        }));
    };

    const deleteField = (id: string) => {
        setForm(prev => ({
            ...prev,
            fields: prev.fields.filter(f => f.id !== id),
            // Also clean up logic rules pointing to this field
            logic: (prev.logic || []).filter(l => l.when.field !== id && !l.actions.some(a => a.targetField === id))
        }));
    };

    const moveField = (id: string, direction: 'up' | 'down') => {
        setForm(prev => {
            const index = prev.fields.findIndex(f => f.id === id);
            if (index === -1) return prev;

            const newFields = [...prev.fields];
            if (direction === 'up' && index > 0) {
                [newFields[index], newFields[index - 1]] = [newFields[index - 1], newFields[index]];
            } else if (direction === 'down' && index < newFields.length - 1) {
                [newFields[index], newFields[index + 1]] = [newFields[index + 1], newFields[index]];
            }
            return { ...prev, fields: newFields };
        });
    };

    const addLogicRule = () => {
        setForm(prev => ({
            ...prev,
            logic: [
                ...(prev.logic || []),
                {
                    when: { field: '', operator: 'equals', value: '' },
                    actions: [{ targetField: '', effect: 'show' }]
                }
            ]
        }));
    };

    const updateLogicRule = (index: number, updates: any) => {
        setForm(prev => {
            const newLogic = [...(prev.logic || [])];
            newLogic[index] = { ...newLogic[index], ...updates };
            return { ...prev, logic: newLogic };
        });
    };

    const deleteLogicRule = (index: number) => {
        setForm(prev => ({
            ...prev,
            logic: (prev.logic || []).filter((_, i) => i !== index)
        }));
    };

    const { mutateAsync: saveMutation, isPending: isSaving } = useSaveForm();

    const saveForm = async () => {
        try {
            await saveMutation(form);
            return true;
        } catch (error) {
            console.error(error);
            return false;
        }
    };

    return {
        form,
        updateFormTitle,
        addField,
        updateField,
        deleteField,
        moveField,
        addLogicRule,
        updateLogicRule,
        deleteLogicRule,
        saveForm,
        isLoading: isSaving
    };
};
