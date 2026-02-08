// Logic engine for conditional fields
const evaluateCondition = (condition, formData) => {
    const { field, operator, value } = condition;
    const fieldValue = formData[field];

    switch (operator) {
        case 'equals':
        case '==':
            return fieldValue == value;
        case 'notEquals':
        case '!=':
            return fieldValue != value;
        case 'contains':
            return fieldValue && fieldValue.includes && fieldValue.includes(value);
        case 'greaterThan':
        case '>':
            return Number(fieldValue) > Number(value);
        case 'lessThan':
        case '<':
            return Number(fieldValue) < Number(value);
        case 'greaterThanOrEqual':
        case '>=':
            return Number(fieldValue) >= Number(value);
        case 'lessThanOrEqual':
        case '<=':
            return Number(fieldValue) <= Number(value);
        case 'startsWith':
            return fieldValue && fieldValue.startsWith && fieldValue.startsWith(value);
        case 'endsWith':
            return fieldValue && fieldValue.endsWith && fieldValue.endsWith(value);
        default:
            console.warn(`Unknown operator: ${operator}`);
            return false;
    }
};

const evaluateLogicRules = (rules, formData, currentFieldStates, allFields) => {
    // Start with the current field states
    const newFieldStates = { ...currentFieldStates };

    // Process each rule
    rules.forEach(rule => {
        const conditionResult = evaluateCondition(rule.when, formData);

        if (conditionResult) {
            // Apply all actions in the rule
            rule.actions.forEach(action => {
                const { targetField, effect } = action;

                // Ensure the target field exists in our states
                if (!newFieldStates[targetField]) {
                    // Find the field in allFields to get its initial state
                    const field = allFields.find(f => f.name === targetField);
                    newFieldStates[targetField] = {
                        visible: field && field.hidden !== undefined ? !field.hidden : true,
                        enabled: field && field.disabled !== undefined ? !field.disabled : true
                    };
                }

                // Apply the effect
                switch (effect) {
                    case 'show':
                        newFieldStates[targetField].visible = true;
                        break;
                    case 'hide':
                        newFieldStates[targetField].visible = false;
                        break;
                    case 'enable':
                        newFieldStates[targetField].enabled = true;
                        break;
                    case 'disable':
                        newFieldStates[targetField].enabled = false;
                        break;
                    case 'require':
                        newFieldStates[targetField].required = true;
                        break;
                    case 'unrequire':
                        newFieldStates[targetField].required = false;
                        break;
                    default:
                        console.warn(`Unknown effect: ${effect}`);
                        break;
                }
            });
        }
    });

    return newFieldStates;
};

export { evaluateLogicRules };
