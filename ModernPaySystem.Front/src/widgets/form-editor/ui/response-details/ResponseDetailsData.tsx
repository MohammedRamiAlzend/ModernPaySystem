import React from 'react';
import { FileArchive } from 'lucide-react';

interface ResponseDetailsDataProps {
    visibleFields: Array<{
        field: any;
        displayValue: string;
    }>;
}

export const ResponseDetailsData: React.FC<ResponseDetailsDataProps> = ({ visibleFields }) => {
    return (
        <div className="bg-muted/10 rounded-3xl p-6 border border-muted-foreground/10">
            <h3 className="text-xl font-bold mb-6 flex items-center gap-2 text-primary">
                <FileArchive className="w-5 h-5" />
                بيانات النموذج
            </h3>
            <div className="grid grid-cols-12 gap-x-8 gap-y-6">
                {visibleFields.map(({ field, displayValue }) => {
                    const colSpan = field.layout?.colSpan || 12;

                    const spanMap = {
                        12: 'col-span-12',
                        6: 'col-span-12 md:col-span-6',
                        4: 'col-span-12 md:col-span-4',
                        3: 'col-span-12 md:col-span-3',
                    };

                    const spanClass = spanMap[colSpan as keyof typeof spanMap] || 'col-span-12';

                    return (
                        <div key={field.id} className={`${spanClass} border-b border-gray-100/50 pb-2`}>
                            {field.type === 'label' ? (
                                <div className="flex flex-col gap-1 text-right py-1">
                                    <span className="text-lg font-bold text-primary">
                                        {field.label}
                                    </span>
                                    {displayValue && (
                                        <span className="text-sm text-muted-foreground italic">
                                            {displayValue}
                                        </span>
                                    )}
                                </div>
                            ) : (
                                <div className="flex flex-col gap-1 text-right">
                                    <span className="text-xs font-bold text-muted-foreground">
                                        {field.label}
                                    </span>
                                    <span className="text-base font-semibold text-foreground break-words">
                                        {displayValue || '-'}
                                    </span>
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
            {visibleFields.length === 0 && (
                <div className="text-center py-8 text-gray-400">
                    لا توجد بيانات لعرضها
                </div>
            )}
        </div>
    );
};
