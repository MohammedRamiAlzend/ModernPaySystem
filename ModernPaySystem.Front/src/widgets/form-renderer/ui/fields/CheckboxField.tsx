import React from 'react';
import { Label } from "@/shared/ui/label";
import type { FormField, DataSourceOption } from "@/entities/form/model/types";
import { useLookUpFieldValues } from '@/features/lookup-management/api/lookupApi';

interface CheckboxFieldProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const CheckboxField: React.FC<CheckboxFieldProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    const dataSource = field.dataSource;
    const isLookUp = dataSource?.type === 'lookup' || (!!dataSource?.lookUpFieldId);
    const idToFetch = isLookUp ? (dataSource?.lookUpFieldId || (dataSource as any)?.lookUpFiledId) : null;

    const { data: lookUpValues = [], isLoading } = useLookUpFieldValues(idToFetch);

    const options: DataSourceOption[] = isLookUp
        ? (Array.isArray(lookUpValues) ? lookUpValues.map(v => ({ label: v.desc, value: v.desc })) : [])
        : (dataSource?.options || []);

    const isHorizontal = field.direction === 'horizontal';

    // For multiple checkboxes, value should be an array
    const currentValue = Array.isArray(value) ? value : (value ? [value] : []);

    const handleToggle = (optionValue: string) => {
        const newValue = currentValue.includes(optionValue)
            ? currentValue.filter(v => v !== optionValue)
            : [...currentValue, optionValue];
        onChange(newValue);
    };

    if (options.length > 0 || isLoading) {
        return (
            <div className="space-y-3">
                <Label className={error ? "text-destructive" : ""}>
                    {field.label}
                    {field.validation?.some(v => v.rule === 'required') && <span className="text-destructive ml-1">*</span>}
                </Label>
                <div className={`flex ${isHorizontal ? 'flex-row flex-wrap gap-4' : 'flex-col space-y-2'}`}>
                    {isLoading ? (
                        <div className="text-xs text-muted-foreground animate-pulse">جاري جلب الخيارات...</div>
                    ) : (
                        options.map((option) => (
                            <div key={option.value} className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    id={`${field.id}-${option.value}`}
                                    checked={currentValue.includes(option.value as any)}
                                    onChange={() => handleToggle(option.value as any)}
                                    disabled={disabled || readOnly}
                                    className="h-4 w-4 rounded border-gray-300 text-primary focus:ring-primary cursor-pointer"
                                />
                                <Label
                                    htmlFor={`${field.id}-${option.value}`}
                                    className="font-normal cursor-pointer leading-none"
                                >
                                    {option.label}
                                </Label>
                            </div>
                        ))
                    )}
                </div>
                {error && <p className="text-sm text-destructive">{error}</p>}
            </div>
        );
    }

    // Default single checkbox behavior
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
            <Label htmlFor={field.id} className={error ? "text-red-500 font-bold" : "font-bold"}>
                {field.label}
                {field.validation?.some(v => v.rule === 'required') && <span className="text-red-500 ml-1">*</span>}
            </Label>
            {error && <p className="text-sm text-red-500 w-full">{error}</p>}
        </div>
    );
};
