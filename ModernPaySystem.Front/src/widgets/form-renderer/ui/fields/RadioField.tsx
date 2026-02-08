import React from 'react';
import { Label } from "@/shared/ui/label";
import type { FormField } from "@/entities/form/model/types";

interface RadioFieldProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const RadioField: React.FC<RadioFieldProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    const isHorizontal = field.direction === 'horizontal';
    
    return (
        <div className="space-y-3">
            <Label className={error ? "text-destructive" : ""}>
                {field.label}
                {field.validation?.some(v => v.rule === 'required') && <span className="text-destructive ml-1">*</span>}
            </Label>
            <div className={`flex ${isHorizontal ? 'flex-row space-x-4 space-x-reverse' : 'flex-col space-y-2'}`}>
                {(!field.dataSource?.options || field.dataSource.options.length === 0) && (
                    <p className="text-xs text-muted-foreground italic">لا توجد خيارات... أضف خيارات من لوحة الخصائص</p>
                )}
                {field.dataSource?.options?.map((option) => (
                    <div key={option.value} className="flex items-center gap-2">
                        <input
                            type="radio"
                            id={`${field.id}-${option.value}`}
                            name={field.name}
                            value={option.value}
                            checked={value === option.value}
                            onChange={(e) => onChange(e.target.value)}
                            disabled={disabled || readOnly}
                            className="h-4 w-4 border-primary text-primary focus:ring-primary cursor-pointer"
                        />
                        <Label
                            htmlFor={`${field.id}-${option.value}`}
                            className="font-normal cursor-pointer leading-none"
                        >
                            {option.label}
                        </Label>
                    </div>
                ))}
            </div>
            {error && <p className="text-sm text-destructive">{error}</p>}
        </div>
    );
};
