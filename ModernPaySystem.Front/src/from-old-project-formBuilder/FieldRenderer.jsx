import React from 'react';
import TextField from './components/TextField';
import SelectField from './components/SelectField';
import CheckboxField from './components/CheckboxField';
import RadioButtonField from './components/RadioButtonField';
import TextAreaField from './components/TextAreaField';

const FieldRenderer = ({ field, value, onChange, error, disabled, readOnly }) => {
    // Determine which component to render based on field type
    const renderField = () => {
        switch (field.type.toLowerCase()) {
            case 'text':
            case 'email':
            case 'password':
            case 'number':
                return (
                    <TextField
                        field={field}
                        value={value}
                        onChange={onChange}
                        error={error}
                        disabled={disabled}
                        readOnly={readOnly}
                    />
                );

            case 'textarea':
                return (
                    <TextAreaField
                        field={field}
                        value={value}
                        onChange={onChange}
                        error={error}
                        disabled={disabled}
                        readOnly={readOnly}
                    />
                );

            case 'select':
            case 'dropdown':
                return (
                    <SelectField
                        field={field}
                        value={value}
                        onChange={onChange}
                        error={error}
                        disabled={disabled}
                        readOnly={readOnly}
                    />
                );

            case 'checkbox':
                return (
                    <CheckboxField
                        field={field}
                        value={value}
                        onChange={onChange}
                        error={error}
                        disabled={disabled}
                        readOnly={readOnly}
                    />
                );

            case 'radio':
                return (
                    <RadioButtonField
                        field={field}
                        value={value}
                        onChange={onChange}
                        error={error}
                        disabled={disabled}
                        readOnly={readOnly}
                    />
                );

            default:
                console.warn(`Unsupported field type: ${field.type}`);
                return null;
        }
    };

    return (
        <div className="w-full">
            {renderField()}
        </div>
    );
};

export default FieldRenderer;
