import type { FormField } from '../../../entities/form/model/types';

export const validateField = (field: FormField, value: any, isRequired: boolean = false): string | null => {
    // Check if field is required either by validation rules or by logic rules
    const isFieldRequired = isRequired || (field.validation && field.validation.some(v => v.rule === 'required'));

    const isEmpty = value === null || value === undefined || value.toString().trim() === '';

    // Required validation
    if (isFieldRequired && isEmpty) {
        return field.validation?.find(v => v.rule === 'required')?.message || `${field.label || field.name} مطلوب`;
    }

    // Only run other validations if field has a value
    if (!isEmpty && field.validation) {
        for (const rule of field.validation) {
            switch (rule.rule) {
                case 'minLength':
                    if (typeof value === 'string' && value.length < Number(rule.value)) {
                        return rule.message || `${field.label || field.name} يجب أن يكون على الأقل ${rule.value} حروف`;
                    }
                    break;

                case 'maxLength':
                    if (typeof value === 'string' && value.length > Number(rule.value)) {
                        return rule.message || `${field.label || field.name} يجب أن لا يتجاوز ${rule.value} حروف`;
                    }
                    break;

                case 'pattern':
                    if (rule.value) {
                        const regex = new RegExp(String(rule.value));
                        if (!regex.test(value)) {
                            return rule.message || `قيمة الحقل ${field.label || field.name} غير صحيحة`;
                        }
                    }
                    break;

                case 'minValue':
                    if (Number(value) < Number(rule.value)) {
                        return rule.message || `قيمة ${field.label || field.name} يجب أن تكون على الأقل ${rule.value}`;
                    }
                    break;

                case 'maxValue':
                    if (Number(value) > Number(rule.value)) {
                        return rule.message || `قيمة ${field.label || field.name} يجب أن لا تتجاوز ${rule.value}`;
                    }
                    break;
            }
        }
    }

    return null;
};

export const validateForm = (fields: FormField[], formData: Record<string, any>, fieldStates: Record<string, any> = {}): Record<string, string> => {
    const errors: Record<string, string> = {};

    fields.forEach(field => {
        // Only validate visible fields
        const isVisible = fieldStates[field.name]?.visible !== false;
        if (isVisible) {
            const error = validateField(field, formData[field.name], fieldStates[field.name]?.required);
            if (error) {
                errors[field.name] = error;
            }
        }
    });

    return errors;
};
