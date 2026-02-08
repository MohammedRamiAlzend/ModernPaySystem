import React from 'react';
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import type { FormField } from "@/entities/form/model/types";

interface TextFieldProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const TextField: React.FC<TextFieldProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        onChange(e.target.value);
    };

    const inputType = () => {
        switch (field.type) {
            case 'email': return 'email';
            case 'password': return 'password';
            case 'number': return 'number';
            default: return 'text';
        }
    };

    return (
        <div className="space-y-2">
            <Label htmlFor={field.id} className={error ? "text-red-500" : ""}>
                {field.label}
                {field.validation?.some(v => v.rule === 'required') && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <Input
                id={field.id}
                type={inputType()}
                value={value || ''}
                onChange={handleChange}
                placeholder={field.placeholder}
                disabled={disabled}
                readOnly={readOnly}
                className={error ? "border-red-500" : ""}
            />
            {error && <p className="text-sm text-red-500">{error}</p>}
        </div>
    );
};
