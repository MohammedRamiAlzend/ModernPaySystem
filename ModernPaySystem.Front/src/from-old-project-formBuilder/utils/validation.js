// Validation utilities
const validateField = (field, value, isRequired = false) => {
    // Check if field is required either by validation rules or by logic rules
    const isFieldRequired = isRequired || (field.validation && field.validation.some(v => v.rule === 'required'));

    // Required validation
    if (isFieldRequired && (!value || value.toString().trim() === '')) {
        return field.validation?.find(v => v.rule === 'required')?.message || `${field.label || field.name} is required`;
    }

    // Only run other validations if field has a value (to avoid validating empty fields unnecessarily)
    if (value && field.validation) {
        for (const rule of field.validation) {
            switch (rule.rule) {
                case 'required':
                    // Already handled above
                    break;

                case 'minLength':
                    if (value.length < rule.value) {
                        return rule.message || `${field.label || field.name} must be at least ${rule.value} characters`;
                    }
                    break;

                case 'maxLength':
                    if (value.length > rule.value) {
                        return rule.message || `${field.label || field.name} must be no more than ${rule.value} characters`;
                    }
                    break;

                case 'pattern':
                    const regex = new RegExp(rule.value);
                    if (!regex.test(value)) {
                        return rule.message || `${field.label || field.name} is invalid`;
                    }
                    break;

                case 'minValue':
                    if (Number(value) < rule.value) {
                        return rule.message || `${field.label || field.name} must be at least ${rule.value}`;
                    }
                    break;

                case 'maxValue':
                    if (Number(value) > rule.value) {
                        return rule.message || `${field.label || field.name} must not exceed ${rule.value}`;
                    }
                    break;

                default:
                    console.warn(`Unknown validation rule: ${rule.rule}`);
                    break;
            }
        }
    }

    return null;
};

const validateForm = (fields, formData, fieldStates = {}) => {
    const errors = {};

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

export { validateField, validateForm };
