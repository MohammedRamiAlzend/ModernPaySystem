import React from 'react';
import { Input } from "@/shared/ui/input";
import { Label } from "@/shared/ui/label";
import type { FormField } from "@/entities/form/model/types";

interface DateFieldProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const DateField: React.FC<DateFieldProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        onChange(e.target.value);
    };

    return (
        <div className="space-y-2 text-right">
            <Label htmlFor={field.id} className={error ? "text-red-500 font-bold" : "font-bold"}>
                {field.label}
                {field.validation?.some(v => v.rule === 'required') && <span className="text-red-500 mr-1">*</span>}
            </Label>
            <Input
                id={field.id}
                type="date"
                value={value || ''}
                onChange={handleChange}
                disabled={disabled}
                readOnly={readOnly}
                className={error ? "border-red-500" : ""}
            />
            {error && <p className="text-sm text-red-500">{error}</p>}
        </div>
    );
};
