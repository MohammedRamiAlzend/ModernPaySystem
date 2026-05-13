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
    if (field.type === 'label') {
        return field.placeholder || '';
    }

    if (value === undefined || value === null || value === '') {
        return '-';
    }

    // Smart parsing for JSON strings (arrays or objects stored as strings)
    let processedValue = value;
    if (typeof value === 'string' && (value.startsWith('[') || value.startsWith('{'))) {
        try {
            processedValue = JSON.parse(value);
        } catch {
            processedValue = value;
        }
    }

    // Handle Checkbox - Can be multiple values (array) or single boolean
    if (field.type === 'checkbox') {
        if (Array.isArray(processedValue)) {
            if (processedValue.length === 0) return '-';
            return processedValue.join(' ، ');
        }
        return processedValue.split(",").join(" ، ") ?? "";
    }

    // Handle select/radio - check if we can find a label in static options
    if ((field.type === 'select' || field.type === 'radio') && field.dataSource?.options) {
        const option = field.dataSource.options.find(opt => opt.value === processedValue);
        if (option) return option.label;
    }

    // If it's a LookUp field or generic
    if (field.dataSource?.type === 'lookup') {
        if (Array.isArray(processedValue)) {
            return processedValue.join(' ، ');
        }
        return String(processedValue);
    }

    // Handle date
    if (field.type === 'date' && processedValue) {
        try {
            return new Date(processedValue).toLocaleDateString('ar-EG');
        } catch {
            return String(processedValue);
        }
    }

    // Handle multiple items in general
    if (Array.isArray(processedValue)) {
        return processedValue.join(' ، ');
    }

    return String(processedValue);
};

/**
 * Prepares field data for printing/PDF export.
 * Only includes visible fields based on logic evaluation.
 */
export const prepareFieldsForPrint = (response: FormResponse): Array<{
    label: string;
    value: string;
    colSpan: number;
    type: string;
}> => {
    const visibleFields = getVisibleFields(response);

    return visibleFields.map(({ field, displayValue }) => ({
        label: field.label,
        value: displayValue,
        colSpan: field.layout?.colSpan || 12,
        type: field.type,
    }));
};
