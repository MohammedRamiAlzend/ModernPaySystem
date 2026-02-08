import type { LogicRule, FormField } from '../../../entities/form/model/types';

const evaluateCondition = (condition: LogicRule['when'], formData: Record<string, any>): boolean => {
    const { field, operator, value } = condition;
    const fieldValue = formData[field];

    // Helper to safely compare
    const num = (v: any) => Number(v);

    switch (operator) {
        case 'equals':
            // eslint-disable-next-line eqeqeq
            return fieldValue == value;
        case 'notEquals':
            // eslint-disable-next-line eqeqeq
            return fieldValue != value;
        case 'contains':
            return fieldValue && typeof fieldValue === 'string' && fieldValue.includes(value);
        case 'greaterThan':
            return num(fieldValue) > num(value);
        case 'lessThan':
            return num(fieldValue) < num(value);
        case 'greaterThanOrEqual':
            return num(fieldValue) >= num(value);
        case 'lessThanOrEqual':
            return num(fieldValue) <= num(value);
        case 'startsWith':
            return fieldValue && typeof fieldValue === 'string' && fieldValue.startsWith(value);
        case 'endsWith':
            return fieldValue && typeof fieldValue === 'string' && fieldValue.endsWith(value);
        default:
            return false;
    }
};

export interface FieldState {
    visible: boolean;
    enabled: boolean;
    required?: boolean;
}

export type FieldStates = Record<string, FieldState>;

export const evaluateLogicRules = (
    rules: LogicRule[],
    formData: Record<string, any>,
    _unusedCurrentFieldStates: FieldStates,
    allFields: FormField[]
): FieldStates => {
    // Start with the BASE field states from the field definitions
    // This allows rules to be additive/overriding on top of a clean slate every time
    const newFieldStates: FieldStates = {};

    allFields.forEach(field => {
        // Use initialVisibility/initialEnabled if set, otherwise fall back to hidden/disabled
        const isInitiallyVisible = field.initialVisibility === 'hidden' ? false : 
                                   field.initialVisibility === 'visible' ? true :
                                   field.hidden !== true;
        const isInitiallyEnabled = field.initialEnabled === 'disabled' ? false :
                                    field.initialEnabled === 'enabled' ? true :
                                    field.disabled !== true;
        
        newFieldStates[field.name] = {
            visible: isInitiallyVisible,
            enabled: isInitiallyEnabled,
            required: field.validation?.some(v => v.rule === 'required') || false
        };
    });

    // Process each rule
    rules.forEach(rule => {
        const conditionResult = evaluateCondition(rule.when, formData);

        if (conditionResult) {
            // Apply all actions in the rule
            rule.actions.forEach(action => {
                const { targetField, effect } = action;

                // Skip if target field doesn't exist in our schema
                if (!newFieldStates[targetField]) return;

                const targetState = newFieldStates[targetField];

                // Apply the effect
                switch (effect) {
                    case 'show':
                        targetState.visible = true;
                        break;
                    case 'hide':
                        targetState.visible = false;
                        break;
                    case 'enable':
                        targetState.enabled = true;
                        break;
                    case 'disable':
                        targetState.enabled = false;
                        break;
                    case 'require':
                        targetState.required = true;
                        break;
                    case 'unrequire':
                        targetState.required = false;
                        break;
                }
            });
        }
    });

    return newFieldStates;
};
