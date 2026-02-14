import React from 'react';
import { Label } from "@/shared/ui/label";
import type { FormField, DataSourceOption } from "@/entities/form/model/types";
import { useLookUpFieldValues } from '@/features/lookup-management/api/lookupApi';

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
    const dataSource = field.dataSource;
    const isLookUp = dataSource?.type === 'lookup' || (!!dataSource?.lookUpFieldId);
    const idToFetch = isLookUp ? (dataSource?.lookUpFieldId || (dataSource as any)?.lookUpFiledId) : null;

    const { data: lookUpValues = [], isLoading } = useLookUpFieldValues(idToFetch);

    const options: DataSourceOption[] = isLookUp
        ? (Array.isArray(lookUpValues) ? lookUpValues.map(v => ({ label: v.desc, value: v.desc })) : [])
        : (dataSource?.options || []);

    return (
        <div className="space-y-3">
            <Label className={error ? "text-destructive" : ""}>
                {field.label}
                {field.validation?.some(v => v.rule === 'required') && <span className="text-destructive ml-1">*</span>}
            </Label>
            <div className={`flex ${isHorizontal ? 'flex-row space-x-4 space-x-reverse' : 'flex-col space-y-2'}`}>
                {isLoading ? (
                    <div className="text-xs text-muted-foreground animate-pulse">جاري جلب الخيارات...</div>
                ) : options.length === 0 ? (
                    <p className="text-xs text-muted-foreground italic">لا توجد خيارات...</p>
                ) : (
                    options.map((option) => (
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
                    ))
                )}
            </div>
            {error && <p className="text-sm text-destructive">{error}</p>}
        </div>
    );
};
