import type { FormField } from '@/entities/form/model/types';
import type { FormResponse } from './responses';
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

    // Handle select/radio with options
    if ((field.type === 'select' || field.type === 'radio') && field.dataSource?.options) {
        const option = field.dataSource.options.find(opt => opt.value === value);
        return option ? option.label : String(value);
    }

    // Handle checkbox
    if (field.type === 'checkbox') {
        return value ? 'نعم' : 'لا';
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
