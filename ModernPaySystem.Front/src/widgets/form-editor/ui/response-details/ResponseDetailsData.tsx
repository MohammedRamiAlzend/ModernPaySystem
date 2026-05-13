import React from 'react';
import { FileArchive, History } from 'lucide-react';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/shared/ui/tabs';
import { RequestTransactionsHistory } from '@/features/form-builder/ui/RequestTransactionsHistory';
import { CurrentLocationBadge } from '@/features/form-builder/ui/CurrentLocationBadge';

interface ResponseDetailsDataProps {
    requestId?: string;
    hideTabs?: boolean;
    visibleFields: Array<{
        field: any;
        displayValue: string;
    }>;
}

export const ResponseDetailsData: React.FC<ResponseDetailsDataProps> = ({
    requestId,
    hideTabs = false,
    visibleFields
}) => {
    const showTabs = requestId && !hideTabs;

    // Logic for rendering fields extracted to avoid duplication
    const renderFieldsGrid = () => (
        <>
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
                                    <span className="text-lg font-bold text-primary">{field.label}</span>
                                    {displayValue && <span className="text-sm text-muted-foreground italic">{displayValue}</span>}
                                </div>
                            ) : (
                                <div className="flex flex-col gap-1 text-right">
                                    <span className="text-xs font-bold text-muted-foreground">{field.label}</span>
                                    <span className="text-base font-semibold text-foreground break-words">{displayValue || '-'}</span>
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
            {visibleFields.length === 0 && (
                <div className="text-center py-8 text-gray-400">لا توجد بيانات لعرضها</div>
            )}
        </>
    );

    return (
        <div className="bg-muted/10 rounded-3xl p-6 border border-muted-foreground/10">
            {showTabs ? (
                <Tabs defaultValue="data" className="w-full">
                    <TabsList className="w-full justify-between border-b border-muted-foreground/10 rounded-none p-0 h-auto bg-transparent mb-6">
                        <div className="flex">
                            <TabsTrigger
                                value="data"
                                className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 py-2 font-bold flex items-center gap-2"
                            >
                                <FileArchive className="w-4 h-4" />
                                بيانات النموذج
                            </TabsTrigger>
                            <TabsTrigger
                                value="referrals"
                                className="rounded-none border-b-2 border-transparent data-[state=active]:border-primary data-[state=active]:bg-transparent px-4 py-2 font-bold flex items-center gap-2"
                            >
                                <History className="w-4 h-4" />
                                متابعة الإحالات
                            </TabsTrigger>
                        </div>

                        <CurrentLocationBadge
                            name={null}
                            department={null}
                            className="mb-1"
                        />
                    </TabsList>

                    <TabsContent value="data" className="mt-0 outline-none">
                        <CurrentLocationBadge
                            name={null}
                            department={null}
                            className="mb-1"
                        />
                        {renderFieldsGrid()}
                    </TabsContent>

                    <TabsContent value="referrals" className="mt-0 outline-none">
                        <RequestTransactionsHistory requestId={requestId!} />
                    </TabsContent>
                </Tabs>
            ) : (
                <>
                    <div className="flex items-center justify-between mb-6">
                        <h3 className="text-xl font-bold flex items-center gap-2 text-primary">
                            <FileArchive className="w-5 h-5" />
                            بيانات النموذج
                        </h3>

                    </div>
                    {renderFieldsGrid()}
                </>
            )}
        </div>
    );
};
