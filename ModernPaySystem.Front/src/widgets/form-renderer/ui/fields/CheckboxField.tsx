import React from 'react';
import { Label } from "@/shared/ui/label";
import type { FormField } from "@/entities/form/model/types";

interface CheckboxFieldProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const CheckboxField: React.FC<CheckboxFieldProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    return (
        <div className="flex flex-row items-center space-x-3 space-x-reverse space-y-0">
            <input
                type="checkbox"
                id={field.id}
                checked={!!value}
                onChange={(e) => onChange(e.target.checked)}
                disabled={disabled || readOnly}
                className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary"
            />
            <Label htmlFor={field.id} className={error ? "text-red-500" : ""}>
                {field.label}
            </Label>
            {error && <p className="text-sm text-red-500 w-full">{error}</p>}
        </div>
    );
};
