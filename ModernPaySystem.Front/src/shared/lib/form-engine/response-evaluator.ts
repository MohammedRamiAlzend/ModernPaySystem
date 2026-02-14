import type { FormField } from '@/entities/form/model/types';
import type { FormResponse } from '@/entities/form/model/types';
import { evaluateLogicRules, type FieldStates } from './logicEngine';

export interface EvaluatedField {
    field: FormField;
    value: any;
    displayValue: string;
    isVisible: boolean;
    isEnabled: boolean;
}

/**
 * Evaluates a form response against its saved schema and logic rules.
 * Returns only the fields that should be visible based on the saved data.
 */
export const evaluateFormResponse = (response: FormResponse): EvaluatedField[] => {
    const schema = response.schema;
    const data = response.data;

    if (!schema || !schema.fields) {
        return [];
    }

    // Calculate initial field states
    const initialFieldStates: FieldStates = {};
    schema.fields.forEach(field => {
        const isInitiallyVisible = field.initialVisibility === 'hidden' ? false :
            field.initialVisibility === 'visible' ? true :
                field.hidden !== true;
        const isInitiallyEnabled = field.initialEnabled === 'disabled' ? false :
            field.initialEnabled === 'enabled' ? true :
                field.disabled !== true;

        initialFieldStates[field.name] = {
            visible: isInitiallyVisible,
            enabled: isInitiallyEnabled,
        };
    });

    // Apply logic rules if they exist
    const fieldStates = schema.logic
        ? evaluateLogicRules(schema.logic, data, initialFieldStates, schema.fields)
        : initialFieldStates;

    // Build the evaluated fields array
    return schema.fields.map(field => {
        const state = fieldStates[field.name];
        const value = data[field.name];

        return {
            field,
            value,
            displayValue: getDisplayValue(field, value),
            isVisible: state?.visible !== false,
            isEnabled: state?.enabled !== false,
        };
    });
};

/**
 * Gets the visible fields only from an evaluated response.
 */
export const getVisibleFields = (response: FormResponse): EvaluatedField[] => {
    return evaluateFormResponse(response).filter(f => f.isVisible);
};

/**
 * Formats a field value for display based on field type.
 */
const getDisplayValue = (field: FormField, value: any): string => {
    if (value === undefined || value === null || value === '') {
        return '-';
    }

    // Handle Checkbox - Can be multiple values (array) or single boolean
    if (field.type === 'checkbox') {
        if (Array.isArray(value)) {
            if (value.length === 0) return '-';
            // Join array values. If it's a LookUp, the values themselves are the 'desc' (labels)
            return value.join(' ، ');
        }
        return value ? 'نعم' : 'لا';
    }

    // Handle select/radio - check if we can find a label in static options
    if ((field.type === 'select' || field.type === 'radio') && field.dataSource?.options) {
        const option = field.dataSource.options.find(opt => opt.value === value);
        if (option) return option.label;
    }

    // If it's a LookUp field, the stored value is already the 'desc' (as per implementation in SelectField/RadioField)
    // So if we didn't find it in options (which might be empty in the saved schema), just return the value
    if (field.dataSource?.type === 'lookup') {
        return String(value);
    }

    // Handle date
    if (field.type === 'date' && value) {
        try {
            return new Date(value).toLocaleDateString('ar-EG');
        } catch {
            return String(value);
        }
    }

    return String(value);
};

/**
 * Prepares field data for printing/PDF export.
 * Only includes visible fields based on logic evaluation.
 */
export const prepareFieldsForPrint = (response: FormResponse): Array<{
    label: string;
    value: string;
    colSpan: number;
}> => {
    const visibleFields = getVisibleFields(response);

    return visibleFields.map(({ field, displayValue }) => ({
        label: field.label,
        value: displayValue,
        colSpan: field.layout?.colSpan || 12,
    }));
};
