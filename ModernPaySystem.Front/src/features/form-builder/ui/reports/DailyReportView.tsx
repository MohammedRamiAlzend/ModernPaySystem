import { forwardRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Loader2 } from 'lucide-react';
import type { TransactionDailyReport } from '../../model/transaction-report-types';
import { DailyChart } from './charts/DailyChart';

interface DailyReportViewProps {
    data: TransactionDailyReport | undefined;
    isLoading: boolean;
}

function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ar-SY', {
        year: 'numeric', month: 'long', day: 'numeric',
    });
}

export const DailyReportView = forwardRef<HTMLDivElement, DailyReportViewProps>(
    function DailyReportView({ data, isLoading }, ref) {
        if (isLoading) {
            return (
                <div className="flex h-64 items-center justify-center">
                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
            );
        }

        if (!data) {
            return (
                <Card>
                    <CardContent className="pt-8 text-center text-muted-foreground">
                        اختر تاريخاً لعرض التقرير اليومي
                    </CardContent>
                </Card>
            );
        }

        return (
            <div className="space-y-6" dir="rtl">
                <Card className="border border-border/40 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-lg font-semibold">
                            التقرير اليومي للمعاملات - {formatDate(data.date)}
                        </CardTitle>
                    </CardHeader>
                    <CardContent>
                        <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
                            <StatBadge label="طلبات منشأة" value={data.requestsCreated} color="emerald" />
                            <StatBadge label="ردود مضافة" value={data.responsesMade} color="blue" />
                            <StatBadge label="مرفقات مضافة" value={data.attachmentsAdded} color="indigo" />
                            <StatBadge label="مشاهدات" value={data.views} color="amber" />
                            <StatBadge label="مستخدمون نشطون" value={data.activeUsers} color="teal" />
                        </div>
                    </CardContent>
                </Card>

                {data.hourlyBreakdown && data.hourlyBreakdown.length > 0 && (
                    <Card ref={ref} className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">التوزيع الساعي</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <DailyChart hourlyBreakdown={data.hourlyBreakdown} />
                        </CardContent>
                    </Card>
                )}

                {data.hourlyBreakdown && data.hourlyBreakdown.length > 0 && (
                    <Card className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">تفاصيل التوزيع الساعي</CardTitle>
                        </CardHeader>
                        <CardContent className="p-0">
                            <Table>
                                <TableHeader>
                                    <TableRow>
                                        <TableHead className="text-right">الساعة</TableHead>
                                        <TableHead className="text-right">سجلات منشأة</TableHead>
                                        <TableHead className="text-right">إجراءات</TableHead>
                                    </TableRow>
                                </TableHeader>
                                <TableBody>
                                    {data.hourlyBreakdown.map((h) => (
                                        <TableRow key={h.hour}>
                                            <TableCell className="font-medium">{h.hour}:00</TableCell>
                                            <TableCell>{h.recordsCreated}</TableCell>
                                            <TableCell>{h.actions}</TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </CardContent>
                    </Card>
                )}
            </div>
        );
    }
);

function StatBadge({ label, value, color }: { label: string; value: number; color: string }) {
    const colorMap: Record<string, string> = {
        emerald: 'bg-emerald-500/10 text-emerald-600 border-emerald-200 dark:border-emerald-800',
        red: 'bg-red-500/10 text-red-600 border-red-200 dark:border-red-800',
        blue: 'bg-blue-500/10 text-blue-600 border-blue-200 dark:border-blue-800',
        indigo: 'bg-indigo-500/10 text-indigo-600 border-indigo-200 dark:border-indigo-800',
        purple: 'bg-purple-500/10 text-purple-600 border-purple-200 dark:border-purple-800',
        amber: 'bg-amber-500/10 text-amber-600 border-amber-200 dark:border-amber-800',
        teal: 'bg-teal-500/10 text-teal-600 border-teal-200 dark:border-teal-800',
    };

    return (
        <div className={`rounded-lg border p-3 text-center ${colorMap[color] || colorMap.emerald}`}>
            <div className="text-2xl font-bold">{value}</div>
            <div className="text-xs mt-1 opacity-80">{label}</div>
        </div>
    );
}
