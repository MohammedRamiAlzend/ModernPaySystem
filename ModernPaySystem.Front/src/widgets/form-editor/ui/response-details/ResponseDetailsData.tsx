import React from 'react';
import { FileArchive, History } from 'lucide-react';
// import { FileArchive, History, CheckCircle2 } from 'lucide-react';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/shared/ui/tabs';
import { RequestTransactionsHistory } from '@/features/form-builder/ui/RequestTransactionsHistory';
import { CurrentLocationBadge } from '@/features/form-builder/ui/CurrentLocationBadge';
// import { useRequestResponses } from '@/features/form-builder/api/formEndpoints';
// import { UserDisplay } from '@/features/users/ui/UserDisplay';

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

    // Fetch responses to display the final response if it exists
    // const { data: responses = [] } = useRequestResponses(requestId || null);
    // const finalResponse = responses.length > 0 ? responses[responses.length - 1] : null;

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

                        {/* {finalResponse && (
                            <div className="mt-8 p-5 bg-primary/5 border border-primary/10 rounded-2xl relative overflow-hidden text-right" dir="rtl">
                                <div className="absolute top-0 right-0 w-1.5 h-full bg-primary" />
                                <div className="flex items-center justify-between mb-3 pr-3">
                                    <div className="flex items-center gap-2">
                                        <CheckCircle2 className="w-4 h-4 text-primary" />
                                        <span className="text-xs font-bold text-primary bg-primary/10 px-2 py-0.5 rounded-full">
                                            الرد
                                        </span>
                                        <UserDisplay userId={finalResponse.respondedByUserId} className="text-xs font-bold text-foreground" showIcon />
                                    </div>
                                    {finalResponse.createdAt && (
                                        <span className="text-[10px] text-muted-foreground font-mono">
                                            {new Date(finalResponse.createdAt).toLocaleString('ar-EG')}
                                        </span>
                                    )}
                                </div>
                                <p className="text-sm font-semibold text-foreground pr-3 leading-relaxed whitespace-pre-wrap">
                                    {finalResponse.comment}
                                </p>
                            </div>
                        )} */}
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

                    {/* {finalResponse && (
                        <div className="mt-8 p-5 bg-primary/5 border border-primary/10 rounded-2xl relative overflow-hidden text-right" dir="rtl">
                            <div className="absolute top-0 right-0 w-1.5 h-full bg-primary" />
                            <div className="flex items-center justify-between mb-3 pr-3">
                                <div className="flex items-center gap-2">
                                    <CheckCircle2 className="w-4 h-4 text-primary" />
                                    <span className="text-xs font-bold text-primary bg-primary/10 px-2 py-0.5 rounded-full">
                                        الرد
                                    </span>
                                    <UserDisplay userId={finalResponse.respondedByUserId} className="text-xs font-bold text-foreground" showIcon />
                                </div>
                                {finalResponse.createdAt && (
                                    <span className="text-[10px] text-muted-foreground font-mono">
                                        {new Date(finalResponse.createdAt).toLocaleString('ar-EG')}
                                    </span>
                                )}
                            </div>
                            <p className="text-sm font-semibold text-foreground pr-3 leading-relaxed whitespace-pre-wrap">
                                {finalResponse.comment}
                            </p>
                        </div>
                    )} */}
                </>
            )}
        </div>
    );
};
