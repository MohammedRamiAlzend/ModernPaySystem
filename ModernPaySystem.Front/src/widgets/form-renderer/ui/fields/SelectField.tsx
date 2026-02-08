import React from 'react';
import { Label } from "@/shared/ui/label";
import type { FormField } from "@/entities/form/model/types";

interface selectProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const SelectField: React.FC<selectProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    // For simplicity, using native select to avoid complex Radix Select state management in this migration
    // If strict UI consistency is needed, we would implement Radix Select here.
    // Given "Feature-Based" and "Old Form Builder", native select is safer for dynamic data without refactoring the whole logic.
    // However, I will style it to look decent.

    return (
        <div className="space-y-2">
            <Label htmlFor={field.id} className={error ? "text-red-500" : ""}>
                {field.label}
                {field.validation?.some(v => v.rule === 'required') && <span className="text-red-500 ml-1">*</span>}
            </Label>
            <select
                id={field.id}
                value={value || ''}
                onChange={(e) => onChange(e.target.value)}
                disabled={disabled || readOnly}
                className={`flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${error ? "border-red-500" : ""}`}
            >
                <option value="" disabled>الخيارات ...</option>
                {field.dataSource?.options?.map((opt) => (
                    <option key={opt.value} value={opt.value}>
                        {opt.label}
                    </option>
                ))}
            </select>
            {error && <p className="text-sm text-red-500">{error}</p>}
        </div>
    );
};
