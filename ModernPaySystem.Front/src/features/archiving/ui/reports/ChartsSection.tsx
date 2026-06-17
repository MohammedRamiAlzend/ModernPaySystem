import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Loader2 } from 'lucide-react';
import type { DepartmentChartsData } from '../../model/types';
import { DailyActivityChart } from './charts/DailyActivityChart';
import { ActionBreakdownChart } from './charts/ActionBreakdownChart';
import { HourlyDistributionChart } from './charts/HourlyDistributionChart';
import { TopUsersChart } from './charts/TopUsersChart';
import { TrendChart } from './charts/TrendChart';

interface ChartsSectionProps {
    data: DepartmentChartsData | undefined;
    isLoading: boolean;
}

export function ChartsSection({ data, isLoading }: ChartsSectionProps) {
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
                    لا توجد بيانات رسوم بيانية متاحة
                </CardContent>
            </Card>
        );
    }

    return (
        <div className="space-y-6" dir="rtl">
            {data.dailyActivity.length > 0 && (
                <Card className="border border-border/40 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-sm font-semibold">النشاط اليومي</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <DailyActivityChart data={data.dailyActivity} />
                    </CardContent>
                </Card>
            )}

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {data.actionTypeBreakdown.length > 0 && (
                    <Card className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">توزيع الإجراءات</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <ActionBreakdownChart data={data.actionTypeBreakdown} />
                        </CardContent>
                    </Card>
                )}

                {data.hourlyDistribution.length > 0 && (
                    <Card className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">التوزيع الساعي</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <HourlyDistributionChart data={data.hourlyDistribution} />
                        </CardContent>
                    </Card>
                )}
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {data.topActiveUsers.length > 0 && (
                    <Card className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">أكثر المستخدمين نشاطاً</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <TopUsersChart data={data.topActiveUsers} />
                        </CardContent>
                    </Card>
                )}

                {data.topStorageUsers.length > 0 && (
                    <Card className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">أكثر المستخدمين استخداماً للتخزين</CardTitle>
                        </CardHeader>
                        <CardContent>
                            <TopUsersChart data={data.topStorageUsers} />
                        </CardContent>
                    </Card>
                )}
            </div>

            {data.trend7Days.length > 0 && (
                <Card className="border border-border/40 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-sm font-semibold">اتجاه آخر 7 أيام</CardTitle>
                    </CardHeader>
                    <CardContent>
                        <TrendChart data={data.trend7Days} />
                    </CardContent>
                </Card>
            )}
        </div>
    );
}
