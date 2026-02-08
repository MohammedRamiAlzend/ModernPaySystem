import React from 'react';
import type { FormField } from "@/entities/form/model/types";
import { TextField } from './fields/TextField';
import { TextareaField } from './fields/TextareaField';
import { SelectField } from './fields/SelectField';
import { CheckboxField } from './fields/CheckboxField';
import { RadioField } from './fields/RadioField';

interface FieldRendererProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const FieldRenderer: React.FC<FieldRendererProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    const renderField = () => {
        switch (field.type.toLowerCase()) {
            case 'text':
            case 'email':
            case 'password':
            case 'number':
                return <TextField field={field} value={value} onChange={onChange} error={error} disabled={disabled} readOnly={readOnly} />;
            case 'textarea':
                return <TextareaField field={field} value={value} onChange={onChange} error={error} disabled={disabled} readOnly={readOnly} />;
            case 'select':
            case 'dropdown':
                return <SelectField field={field} value={value} onChange={onChange} error={error} disabled={disabled} readOnly={readOnly} />;
            case 'checkbox':
                return <CheckboxField field={field} value={value} onChange={onChange} error={error} disabled={disabled} readOnly={readOnly} />;
            case 'radio':
                return <RadioField field={field} value={value} onChange={onChange} error={error} disabled={disabled} readOnly={readOnly} />;
            default:
                return <div className="text-red-500">Unsupported field: {field.type}</div>;
        }
    };

    return (
        <div className="w-full">
            {renderField()}
        </div>
    );
};
