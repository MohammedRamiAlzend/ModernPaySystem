import React, { useState, useEffect } from 'react';
import FieldRenderer from './FieldRenderer';
import { validateForm } from './utils/validation';
import { evaluateLogicRules } from './utils/logicEngine';
import Button from '../components/common/Button';

const FormRenderer = ({ schema, onSubmit }) => {
    // Initialize form state
    const [formData, setFormData] = useState(() => {
        const initialState = {};
        schema.fields.forEach(field => {
            initialState[field.name] = field.defaultValue || '';
        });
        return initialState;
    });

    // Initialize errors state
    const [errors, setErrors] = useState({});

    // Initialize field visibility/enablement based on logic rules
    const [fieldStates, setFieldStates] = useState(() => {
        const initialStates = {};
        schema.fields.forEach(field => {
            initialStates[field.name] = {
                visible: field.hidden !== undefined ? !field.hidden : true,
                enabled: field.disabled !== undefined ? !field.disabled : true
            };
        });
        return initialStates;
    });

    // Handle form field changes
    const handleChange = (fieldName, value) => {
        setFormData(prev => ({
            ...prev,
            [fieldName]: value
        }));

        // Clear error for this field when value changes
        if (errors[fieldName]) {
            setErrors(prev => {
                const newErrors = { ...prev };
                delete newErrors[fieldName];
                return newErrors;
            });
        }
    };

    // Handle form submission
    const handleSubmit = (e) => {
        e.preventDefault();

        // Validate entire form
        const formErrors = validateForm(schema.fields, formData, fieldStates);
        setErrors(formErrors);

        // Submit only if no errors
        if (Object.keys(formErrors).length === 0) {
            onSubmit(formData);
        }
    };

    // Evaluate logic rules and clean data when fields become hidden
    useEffect(() => {
        if (schema.logic) {
            const updatedFieldStates = evaluateLogicRules(schema.logic, formData, fieldStates, schema.fields);

            // Data Integrity Check: If a field becomes hidden, reset its value
            const fieldsToClear = [];
            schema.fields.forEach(field => {
                const wasVisible = fieldStates[field.name]?.visible !== false;
                const isVisible = updatedFieldStates[field.name]?.visible !== false;

                if (wasVisible && !isVisible) {
                    fieldsToClear.push(field);
                }
            });

            if (fieldsToClear.length > 0) {
                setFormData(prev => {
                    const newData = { ...prev };
                    fieldsToClear.forEach(field => {
                        newData[field.name] = field.defaultValue || '';
                    });
                    return newData;
                });
            }

            setFieldStates(updatedFieldStates);
        }
    }, [formData]);

    // Render form fields based on schema
    const renderFields = () => {
        return schema.fields
            .filter(field => fieldStates[field.name]?.visible)
            .map((field) => {
                const fieldError = errors[field.name];
                const isEnabled = fieldStates[field.name]?.enabled && !field.readOnly;

                return (
                    <div key={field.id} className="mb-6 animate-slide-up">
                        <FieldRenderer
                            field={field}
                            value={formData[field.name]}
                            onChange={(value) => handleChange(field.name, value)}
                            error={fieldError}
                            disabled={!isEnabled}
                            readOnly={field.readOnly}
                        />
                    </div>
                );
            });
    };

    return (
        <form onSubmit={handleSubmit} className="space-y-2">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-x-6">
                {renderFields()}
            </div>
            <div className="flex justify-end mt-8 pt-6 border-t border-border">
                <Button
                    type="submit"
                    variant="primary"
                    size="lg"
                    className="min-w-[150px] shadow-lg hover:scale-105 transition-transform"
                >
                    حفظ البيانات
                </Button>
            </div>
        </form>
    );
};

export default FormRenderer;
