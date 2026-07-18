import { useState, useEffect, forwardRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui/card';
import { Loader2 } from 'lucide-react';
import { resolveUserNames } from '@/shared/utils/resolve-user-names';
import type { TransactionChartsData, ChartDataPoint } from '../../model/transaction-report-types';
import { DailyActivityChart } from './charts/DailyActivityChart';
import { ActionBreakdownChart } from './charts/ActionBreakdownChart';
import { HourlyDistributionChart } from './charts/HourlyDistributionChart';
import { TopUsersChart } from './charts/TopUsersChart';
import { TrendChart } from './charts/TrendChart';

interface ChartsSectionProps {
    data: TransactionChartsData | undefined;
    isLoading: boolean;
}

export const ChartsSection = forwardRef<HTMLDivElement, ChartsSectionProps>(
    function ChartsSection({ data, isLoading }, ref) {
        const [userNames, setUserNames] = useState<Map<string, string>>(new Map());

        const processedData = (data as any)?.data ? (data as any).data : data;

        const dailyActivity = Array.isArray(processedData?.dailyActivity) ? processedData.dailyActivity : (Array.isArray(processedData?.DailyActivity) ? processedData.DailyActivity : []);
        const actionTypeBreakdown = Array.isArray(processedData?.actionTypeBreakdown) ? processedData.actionTypeBreakdown : (Array.isArray(processedData?.ActionTypeBreakdown) ? processedData.ActionTypeBreakdown : []);
        const hourlyDistribution = Array.isArray(processedData?.hourlyDistribution) ? processedData.hourlyDistribution : (Array.isArray(processedData?.HourlyDistribution) ? processedData.HourlyDistribution : []);
        const rawTopActiveUsers: ChartDataPoint[] = Array.isArray(processedData?.topActiveUsers) ? processedData.topActiveUsers : (Array.isArray(processedData?.TopActiveUsers) ? processedData.TopActiveUsers : []);
        const rawTopStorageUsers: ChartDataPoint[] = Array.isArray(processedData?.topStorageUsers) ? processedData.topStorageUsers : (Array.isArray(processedData?.TopStorageUsers) ? processedData.TopStorageUsers : []);
        const trend7Days = Array.isArray(processedData?.trend7Days) ? processedData.trend7Days : (Array.isArray(processedData?.Trend7Days) ? processedData.Trend7Days : []);

        useEffect(() => {
            const chartUserIds = [...rawTopActiveUsers, ...rawTopStorageUsers]
                .map(p => p.label)
                .filter(id => !!id);
            if (chartUserIds.length > 0) {
                resolveUserNames(chartUserIds).then(setUserNames);
            }
        // eslint-disable-next-line react-hooks/exhaustive-deps
        }, [data]);

        if (isLoading) {
            return (
                <div className="flex h-64 items-center justify-center">
                    <Loader2 className="h-8 w-8 animate-spin text-primary" />
                </div>
            );
        }

        if (!processedData) {
            return (
                <Card>
                    <CardContent className="pt-8 text-center text-muted-foreground">
                        لا توجد بيانات رسوم بيانية متاحة
                    </CardContent>
                </Card>
            );
        }

        const topActiveUsers = rawTopActiveUsers.map(p => ({
            ...p,
            label: userNames.get(p.label) || p.label,
        }));

        const topStorageUsers = rawTopStorageUsers.map(p => ({
            ...p,
            label: userNames.get(p.label) || p.label,
        }));

        return (
            <div ref={ref} className="space-y-6" dir="rtl">
                {dailyActivity.length > 0 && (
                    <div data-chart-name="dailyActivity">
                        <Card className="border border-border/40 shadow-sm">
                            <CardHeader>
                                <CardTitle className="text-sm font-semibold">النشاط اليومي</CardTitle>
                                <CardDescription>عدد الطلبات والردود المنشأة يومياً</CardDescription>
                            </CardHeader>
                            <CardContent>
                                <DailyActivityChart data={dailyActivity} />
                            </CardContent>
                        </Card>
                    </div>
                )}

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {actionTypeBreakdown.length > 0 && (
                        <div data-chart-name="actionTypeBreakdown">
                            <Card className="border border-border/40 shadow-sm">
                                <CardHeader>
                                    <CardTitle className="text-sm font-semibold">توزيع الإجراءات</CardTitle>
                                    <CardDescription>نسبة الإجراءات حسب نوع النشاط</CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <ActionBreakdownChart data={actionTypeBreakdown} />
                                </CardContent>
                            </Card>
                        </div>
                    )}

                    {hourlyDistribution.length > 0 && (
                        <div data-chart-name="hourlyDistribution">
                            <Card className="border border-border/40 shadow-sm">
                                <CardHeader>
                                    <CardTitle className="text-sm font-semibold">التوزيع الساعي</CardTitle>
                                    <CardDescription>توزيع النشاط على مدار ساعات اليوم</CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <HourlyDistributionChart data={hourlyDistribution} />
                                </CardContent>
                            </Card>
                        </div>
                    )}
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    {topActiveUsers.length > 0 && (
                        <div data-chart-name="topActiveUsers">
                            <Card className="border border-border/40 shadow-sm">
                                <CardHeader>
                                    <CardTitle className="text-sm font-semibold">أكثر المستخدمين نشاطاً</CardTitle>
                                    <CardDescription>المستخدمون الأكثر نشاطاً في المعاملات</CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <TopUsersChart data={topActiveUsers} tooltipLabel="عدد الإجراءات" />
                                </CardContent>
                            </Card>
                        </div>
                    )}

                    {topStorageUsers.length > 0 && (
                        <div data-chart-name="topStorageUsers">
                            <Card className="border border-border/40 shadow-sm">
                                <CardHeader>
                                    <CardTitle className="text-sm font-semibold">أكثر المستخدمين استخداماً للتخزين</CardTitle>
                                    <CardDescription>المستخدمون الأكثر استهلاكاً لمساحة التخزين</CardDescription>
                                </CardHeader>
                                <CardContent>
                                    <TopUsersChart data={topStorageUsers} tooltipLabel="المساحة (بالبايت)" />
                                </CardContent>
                            </Card>
                        </div>
                    )}
                </div>

                {trend7Days.length > 0 && (
                    <div data-chart-name="trend7Days">
                        <Card className="border border-border/40 shadow-sm">
                            <CardHeader>
                                <CardTitle className="text-sm font-semibold">اتجاه آخر 7 أيام</CardTitle>
                                <CardDescription>معدل النشاط اليومي خلال الأيام السبعة الماضية</CardDescription>
                            </CardHeader>
                            <CardContent>
                                <TrendChart data={trend7Days} />
                            </CardContent>
                        </Card>
                    </div>
                )}
            </div>
        );
    }
);
