import React from 'react';
import { Label } from "@/shared/ui/label";
import type { FormField } from "@/entities/form/model/types";
import { cn } from '@/shared/lib/utils';

interface LabelFieldProps {
    field: FormField;
    value: any;
    onChange: (value: any) => void;
    error?: string;
    disabled?: boolean;
    readOnly?: boolean;
}

export const LabelField: React.FC<LabelFieldProps> = ({ field }) => {
    return (
        <div className="py-2 border-b border-gray-100 mb-2">
           <Label className={cn("text-lg font-bold text-primary", field.layout?.className)}>
                {field.label}
            </Label>
            {field.placeholder && (
                <p className="text-sm text-muted-foreground mt-1 italic">
                    {field.placeholder}
                </p>
            )}
        </div>
    );
};
