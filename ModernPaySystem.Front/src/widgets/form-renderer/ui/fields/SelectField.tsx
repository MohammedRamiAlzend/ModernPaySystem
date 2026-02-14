import React from 'react';
import { Label } from "@/shared/ui/label";
import type { FormField, DataSourceOption } from "@/entities/form/model/types";
import { useLookUpFieldValues } from '@/features/lookup-management/api/lookupApi';

interface selectProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const SelectField: React.FC<selectProps> = ({ field, value, onChange, error, disabled, readOnly }) => {
    const dataSource = field.dataSource;
    const isLookUp = dataSource?.type === 'lookup' || (!!dataSource?.lookUpFieldId);
    const idToFetch = isLookUp ? (dataSource?.lookUpFieldId || (dataSource as any)?.lookUpFiledId) : null;

    const { data: lookUpValues = [], isLoading } = useLookUpFieldValues(idToFetch);

    const options: DataSourceOption[] = isLookUp
        ? (Array.isArray(lookUpValues) ? lookUpValues.map(v => ({ label: v.desc, value: v.desc })) : [])
        : (dataSource?.options || []);

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
                disabled={disabled || readOnly || isLoading}
                className={`flex h-10 w-full items-center justify-between rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 ${error ? "border-red-500" : ""}`}
            >
                <option value="" disabled>{isLoading ? 'جاري التحميل...' : 'الخيارات ...'}</option>
                {options.map((opt) => (
                    <option key={opt.value} value={opt.value}>
                        {opt.label}
                    </option>
                ))}
            </select>
            {error && <p className="text-sm text-red-500">{error}</p>}
        </div>
    );
};
