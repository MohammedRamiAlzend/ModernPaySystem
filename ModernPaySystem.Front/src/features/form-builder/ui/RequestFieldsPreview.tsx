import React from 'react';
import type { FormField } from '@/entities/form/model/types';

interface RequestFieldsPreviewProps {
    /** Raw JSON content string from the request */
    content: string;
    /** Fields from the template schema */
    fields: FormField[];
    /** Maximum number of fields to display */
    maxFields?: number;
    /** Visual variant: 'card' for card-like display, 'inline' for table rows */
    variant?: 'card' | 'inline';
    /** Custom class name for the container */
    className?: string;
}

/**
 * Reusable component to preview the first N fields of a request's content.
 * Used in both IncomingRequestsList and ActionedRequestsPage.
 */
export const RequestFieldsPreview = ({
    content,
    fields,
    maxFields = 3,
    variant = 'card',
    className
}: RequestFieldsPreviewProps) => {
    const data = React.useMemo(() => {
        try {
            return JSON.parse(content) as Record<string, unknown>;
        } catch {
            return null;
        }
    }, [content]);

    const displayFields = React.useMemo(() => {
        if (!data) return [];
        return fields
            .filter(field => data[field.name] !== undefined && data[field.name] !== null && data[field.name] !== '')
            .slice(0, maxFields);
    }, [data, fields, maxFields]);

    // Error state
    if (data === null) {
        return (
            <div className={`text-xs text-muted-foreground ${variant === 'card' ? 'bg-muted/20 p-2 rounded-md font-mono truncate' : 'italic'}`}>
                {variant === 'card' ? content : 'خطأ في البيانات'}
            </div>
        );
    }

    // Empty state
    if (displayFields.length === 0) {
        return (
            <div className={`text-xs text-muted-foreground ${variant === 'card' ? 'bg-muted/20 p-2 rounded-md' : 'italic'}`}>
                لا توجد بيانات لعرضها
            </div>
        );
    }

    return (
        <div className={className || (variant === 'card' ? 'space-y-2' : 'space-y-1.5')}>
            {displayFields.map((field, idx) => (
                <div
                    key={idx}
                    className={`flex items-start gap-2 text-xs ${variant === 'card' ? 'bg-muted/20 p-2 rounded-md' : ''}`}
                >
                    <span className={`${variant === 'card' ? 'font-bold' : 'font-semibold'} text-muted-foreground ${variant === 'inline' ? 'whitespace-nowrap' : 'min-w-[80px]'}`}>
                        {field.label}:
                    </span>
                    <span className="text-foreground font-medium truncate">
                        {String(data[field.name])}
                    </span>
                </div>
            ))}
        </div>
    );
};
