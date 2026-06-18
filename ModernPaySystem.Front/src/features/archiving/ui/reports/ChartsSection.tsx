import { forwardRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui/card';
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

export const ChartsSection = forwardRef<HTMLDivElement, ChartsSectionProps>(
    function ChartsSection({ data, isLoading }, ref) {
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
        <div ref={ref} className="space-y-6" dir="rtl">
            {data.dailyActivity.length > 0 && (
                <div data-chart-name="dailyActivity">
                    <Card className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">النشاط اليومي</CardTitle>
                            <CardDescription>عدد السجلات التي تم إنشاؤها والإجراءات المتخذة يومياً</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <DailyActivityChart data={data.dailyActivity} />
                        </CardContent>
                    </Card>
                </div>
            )}

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {data.actionTypeBreakdown.length > 0 && (
                    <div data-chart-name="actionTypeBreakdown">
                        <Card className="border border-border/40 shadow-sm">
                            <CardHeader>
                                <CardTitle className="text-sm font-semibold">توزيع الإجراءات</CardTitle>
                                <CardDescription>نسبة الإجراءات حسب نوع النشاط (إضافة، تحديث، حذف)</CardDescription>
                            </CardHeader>
                            <CardContent>
                                <ActionBreakdownChart data={data.actionTypeBreakdown} />
                            </CardContent>
                        </Card>
                    </div>
                )}

                {data.hourlyDistribution.length > 0 && (
                    <div data-chart-name="hourlyDistribution">
                        <Card className="border border-border/40 shadow-sm">
                            <CardHeader>
                                <CardTitle className="text-sm font-semibold">التوزيع الساعي</CardTitle>
                                <CardDescription>توزيع النشاط على مدار ساعات اليوم</CardDescription>
                            </CardHeader>
                            <CardContent>
                                <HourlyDistributionChart data={data.hourlyDistribution} />
                            </CardContent>
                        </Card>
                    </div>
                )}
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                {data.topActiveUsers.length > 0 && (
                    <div data-chart-name="topActiveUsers">
                        <Card className="border border-border/40 shadow-sm">
                            <CardHeader>
                                <CardTitle className="text-sm font-semibold">أكثر المستخدمين نشاطاً</CardTitle>
                                <CardDescription>المستخدمون الأكثر نشاطاً في إدارة السجلات</CardDescription>
                            </CardHeader>
                            <CardContent>
                                <TopUsersChart data={data.topActiveUsers} tooltipLabel="عدد الإجراءات" />
                            </CardContent>
                        </Card>
                    </div>
                )}

                {data.topStorageUsers.length > 0 && (
                    <div data-chart-name="topStorageUsers">
                        <Card className="border border-border/40 shadow-sm">
                            <CardHeader>
                                <CardTitle className="text-sm font-semibold">أكثر المستخدمين استخداماً للتخزين</CardTitle>
                                <CardDescription>المستخدمون الأكثر استهلاكاً لمساحة التخزين</CardDescription>
                            </CardHeader>
                            <CardContent>
                                <TopUsersChart data={data.topStorageUsers} tooltipLabel="المساحة (بالبايت)" />
                            </CardContent>
                        </Card>
                    </div>
                )}
            </div>

            {data.trend7Days.length > 0 && (
                <div data-chart-name="trend7Days">
                    <Card className="border border-border/40 shadow-sm">
                        <CardHeader>
                            <CardTitle className="text-sm font-semibold">اتجاه آخر 7 أيام</CardTitle>
                            <CardDescription>معدل النشاط اليومي خلال الأيام السبعة الماضية</CardDescription>
                        </CardHeader>
                        <CardContent>
                            <TrendChart data={data.trend7Days} />
                        </CardContent>
                    </Card>
                </div>
            )}
        </div>
    );
}
);
