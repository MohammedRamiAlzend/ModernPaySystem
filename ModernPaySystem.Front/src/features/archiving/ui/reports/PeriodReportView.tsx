import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Loader2 } from 'lucide-react';
import type { ArchivePeriodReport } from '../../model/types';
import { DailyBreakdownChart } from './charts/DailyBreakdownChart';
import { resolveUserNames } from '@/shared/utils/resolve-user-names';

interface PeriodReportViewProps {
    data: ArchivePeriodReport | undefined;
    isLoading: boolean;
    periodLabel: string;
}

function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ar-SY', {
        year: 'numeric', month: '2-digit', day: '2-digit',
    });
}

function StatCard({ label, value }: { label: string; value: number }) {
    return (
        <div className="rounded-lg border border-border/40 bg-card p-3 text-center shadow-sm">
            <div className="text-2xl font-bold text-foreground">{value.toLocaleString()}</div>
            <div className="text-xs text-muted-foreground mt-1">{label}</div>
        </div>
    );
}

export function PeriodReportView({ data, isLoading, periodLabel }: PeriodReportViewProps) {
    const [userNames, setUserNames] = useState<Map<string, string>>(new Map());
    const topUsers = data?.topUsers ?? [];

    useEffect(() => {
        if (topUsers.length === 0) return;
        const userIds = topUsers.map((u) => u.userId);
        resolveUserNames(userIds).then(setUserNames);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [data]);

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
                    اختر فترة لعرض التقرير
                </CardContent>
            </Card>
        );
    }

    return (
        <div className="space-y-6" dir="rtl">
            <Card className="border border-border/40 shadow-sm">
                <CardHeader>
                    <CardTitle className="text-lg font-semibold">{periodLabel}</CardTitle>
                    <p className="text-xs text-muted-foreground">
                        من {formatDate(data.periodStart)} إلى {formatDate(data.periodEnd)}
                    </p>
                </CardHeader>
                <CardContent>
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                        <StatCard label="سجلات منشأة" value={data.totalRecordsCreated} />
                        <StatCard label="سجلات محذوفة" value={data.totalRecordsDeleted} />
                        <StatCard label="ملفات مضافة" value={data.totalFilesAdded} />
                        <StatCard label="تنزيلات" value={data.totalDownloads} />
                        <StatCard label="طباعات" value={data.totalPrints} />
                        <StatCard label="مشاهدات" value={data.totalViews} />
                        <StatCard label="مستخدمون فريدون" value={data.uniqueActiveUsers} />
                    </div>
                </CardContent>
            </Card>

            {data.dailyBreakdown.length > 0 && (
                <Card className="border border-border/40 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-sm font-semibold">التوزيع اليومي</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <DailyBreakdownChart dailyBreakdown={data.dailyBreakdown} />
                    </CardContent>
                </Card>
            )}

            {data.dailyBreakdown.length > 0 && (
                <Card className="border border-border/40 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-sm font-semibold">تفاصيل يومية</CardTitle>
                    </CardHeader>
                    <CardContent className="p-0">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead className="text-right">التاريخ</TableHead>
                                    <TableHead className="text-right">سجلات منشأة</TableHead>
                                    <TableHead className="text-right">إجراءات</TableHead>
                                    <TableHead className="text-right">مستخدمون نشطون</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {data.dailyBreakdown.map((d) => (
                                    <TableRow key={d.date}>
                                        <TableCell className="font-medium">{formatDate(d.date)}</TableCell>
                                        <TableCell>{d.recordsCreated}</TableCell>
                                        <TableCell>{d.actions}</TableCell>
                                        <TableCell>{d.activeUsers}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </CardContent>
                </Card>
            )}

            {data.topUsers.length > 0 && (
                <Card className="border border-border/40 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-sm font-semibold">أكثر المستخدمين نشاطاً</CardTitle>
                    </CardHeader>
                    <CardContent className="p-0">
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead className="text-right">المستخدم</TableHead>
                                    <TableHead className="text-right">سجلات منشأة</TableHead>
                                    <TableHead className="text-right">مشاهدات</TableHead>
                                    <TableHead className="text-right">تنزيلات</TableHead>
                                    <TableHead className="text-right">طباعات</TableHead>
                                    <TableHead className="text-right">إجمالي الإجراءات</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {data.topUsers.map((u) => (
                                    <TableRow key={u.userId}>
                                        <TableCell className="font-medium">{userNames.get(u.userId) || u.userName}</TableCell>
                                        <TableCell>{u.recordsCreated}</TableCell>
                                        <TableCell>{u.recordsViewed}</TableCell>
                                        <TableCell>{u.filesDownloaded}</TableCell>
                                        <TableCell>{u.printActions}</TableCell>
                                        <TableCell>
                                            <span className="font-bold">{u.totalActions}</span>
                                        </TableCell>
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
