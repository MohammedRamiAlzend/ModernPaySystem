import { useState, useRef, useEffect } from 'react';
import { validateForm } from '@/shared/lib/form-engine/validation';
import { evaluateLogicRules } from '@/shared/lib/form-engine/logicEngine';
import type { FieldStates } from '@/shared/lib/form-engine/logicEngine';
import type { FormSchema } from '@/entities/form/model/types';
import { saveFormResponse } from '@/shared/lib/form-engine/responses';

export const useFormRenderer = (schema: FormSchema, onSubmit: (data: Record<string, any>) => void, initialData?: Record<string, any>) => {
    const [formData, setFormData] = useState<Record<string, any>>(() => {
        const initial: Record<string, any> = {};
        schema.fields.forEach(field => {
            initial[field.name] = initialData?.[field.name] ?? field.defaultValue ?? '';
        });
        return initial;
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitted, setIsSubmitted] = useState(false);

    const [fieldStates, setFieldStates] = useState<FieldStates>(() => {
        const initial: FieldStates = {};
        schema.fields.forEach(field => {
            const isInitiallyVisible = field.initialVisibility === 'hidden' ? false :
                field.initialVisibility === 'visible' ? true :
                    field.hidden !== true;
            const isInitiallyEnabled = field.initialEnabled === 'disabled' ? false :
                field.initialEnabled === 'enabled' ? true :
                    field.disabled !== true;

            initial[field.name] = {
                visible: isInitiallyVisible,
                enabled: isInitiallyEnabled,
            };
        });
        return initial;
    });

    const fieldStatesRef = useRef<FieldStates>(fieldStates);

    useEffect(() => {
        fieldStatesRef.current = fieldStates;
    }, [fieldStates]);

    const handleChange = (fieldName: string, value: any) => {
        setFormData(prev => {
            const nextData = { ...prev, [fieldName]: value };

            if (schema.logic) {
                const updatedFieldStates = evaluateLogicRules(
                    schema.logic,
                    nextData,
                    fieldStatesRef.current,
                    schema.fields
                );

                schema.fields.forEach(field => {
                    const wasVisible = fieldStatesRef.current[field.name]?.visible !== false;
                    const isVisible = updatedFieldStates[field.name]?.visible !== false;

                    if (wasVisible && !isVisible) {
                        nextData[field.name] = field.defaultValue ?? '';
                    }
                });

                fieldStatesRef.current = updatedFieldStates;
                setFieldStates(updatedFieldStates);
            }

            return nextData;
        });

        if (errors[fieldName]) {
            setErrors(prev => {
                const copy = { ...prev };
                delete copy[fieldName];
                return copy;
            });
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const formErrors = validateForm(schema.fields, formData, fieldStates);
        setErrors(formErrors);

        if (Object.keys(formErrors).length === 0) {
            // Save to local storage
            saveFormResponse(schema, formData);

            setIsSubmitted(true);
            setTimeout(() => setIsSubmitted(false), 3000);

            // Call the callback
            onSubmit(formData);
        }
    };

    return {
        formData,
        errors,
        fieldStates,
        isSubmitted,
        handleChange,
        handleSubmit
    };
};
