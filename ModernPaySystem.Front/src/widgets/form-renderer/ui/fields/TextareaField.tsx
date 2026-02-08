import React from 'react';
import { Textarea } from "@/shared/ui/textarea";
import { Label } from "@/shared/ui/label";
import type { FormField } from "@/entities/form/model/types";

interface TextareaFieldProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const TextareaField: React.FC<TextareaFieldProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    const handleChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
        onChange(e.target.value);
    };

    return (
        <div className="space-y-2">
            <Label htmlFor={field.id} className={error ? "text-red-500" : ""}>
                {field.label}
                {field.validation?.some(v => v.rule === 'required') && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Textarea
                id={field.id}
                value={value || ''}
                onChange={handleChange}
                placeholder={field.placeholder}
                disabled={disabled}
                readOnly={readOnly}
                rows={field.rows || 3}
                className={error ? "border-red-500" : ""}
            />
            {error && <p className="text-sm text-red-500">{error}</p>}
        </div>
    );
};
