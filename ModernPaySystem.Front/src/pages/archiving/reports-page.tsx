import { Suspense } from 'react';
import { toPng } from 'html-to-image';
import { useQueryClient } from '@tanstack/react-query';
import { queryKeys } from '@/shared/constants/query-keys';
import { archivingService } from '@/features/archiving/api/archivingService';
import { Card, CardContent } from '@/shared/ui/card';
import { Input } from '@/shared/ui/input';
import { TabsContent } from '@/shared/ui/tabs';
import { ReportsPageLayout } from '@/widgets/reports-page/ui/reports-page-layout';
import { useReportsPage } from '@/widgets/reports-page/model/use-reports-page';
import { ExportButton } from '@/features/archiving/ui/reports/ExportButton';
import {
    useDepartmentDashboard,
    useDailyReport,
    useWeeklyReport,
    useMonthlyReport,
    useUserActivityReport,
    useActiveUsersReport,
    useChartsData,
    useDailyWorkReport,
} from '@/features/archiving/model/queries';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { Calendar, Loader2 } from 'lucide-react';
import type { ReportTabId, ReportTabConfig } from '@/shared/types/reports';

const DashboardCards = lazyWithPreload(() =>
    import('@/features/archiving/ui/reports/DashboardCards').then(m => ({ default: m.DashboardCards }))
);
const DailyReportView = lazyWithPreload(() =>
    import('@/features/archiving/ui/reports/DailyReportView').then(m => ({ default: m.DailyReportView }))
);
const PeriodReportView = lazyWithPreload(() =>
    import('@/features/archiving/ui/reports/PeriodReportView').then(m => ({ default: m.PeriodReportView }))
);
const UserActivityView = lazyWithPreload(() =>
    import('@/features/archiving/ui/reports/UserActivityView').then(m => ({ default: m.UserActivityView }))
);
const ActiveUsersView = lazyWithPreload(() =>
    import('@/features/archiving/ui/reports/ActiveUsersView').then(m => ({ default: m.ActiveUsersView }))
);
const ChartsSection = lazyWithPreload(() =>
    import('@/features/archiving/ui/reports/ChartsSection').then(m => ({ default: m.ChartsSection }))
);
const DailyWorkReportView = lazyWithPreload(() =>
    import('@/features/archiving/ui/reports/DailyWorkReportView').then(m => ({ default: m.DailyWorkReportView }))
);

const fallback = (
    <div className="flex h-64 items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
    </div>
);

const TABS: ReportTabConfig[] = [
    { id: 'dashboard', label: 'لوحة المعلومات' },
    { id: 'daily', label: 'تقرير يومي' },
    { id: 'daily-work', label: 'تقرير يومي مفصل' },
    { id: 'weekly', label: 'تقرير أسبوعي' },
    { id: 'monthly', label: 'تقرير شهري' },
    { id: 'user-activity', label: 'نشاط المستخدمين' },
    { id: 'active-users', label: 'المستخدمون النشطون' },
    { id: 'charts', label: 'الرسوم البيانية' },
];

export default function ReportsPage() {
    const queryClient = useQueryClient();
    const {
        activeTab, setActiveTab,
        fromDate, setFromDate, toDate, setToDate,
        selectedDate, setSelectedDate, workDate, setWorkDate,
        weekStart, setWeekStart,
        reportYear, setReportYear, reportMonth, setReportMonth,
        dailyChartRef, chartsSectionRef,
    } = useReportsPage();

    const { data: dashboard, isLoading: isLoadingDashboard, refetch: refetchDashboard } = useDepartmentDashboard(activeTab === 'dashboard');
    const { data: dailyReport, isLoading: isLoadingDaily, refetch: refetchDaily } = useDailyReport(selectedDate || null, activeTab === 'daily');
    const { data: weeklyReport, isLoading: isLoadingWeekly, refetch: refetchWeekly } = useWeeklyReport(weekStart || null, activeTab === 'weekly');
    const { data: monthlyReport, isLoading: isLoadingMonthly, refetch: refetchMonthly } = useMonthlyReport(reportYear, reportMonth, activeTab === 'monthly');
    const { data: userActivity, isLoading: isLoadingUserActivity, refetch: refetchUserActivity } = useUserActivityReport(fromDate || null, toDate || null, activeTab === 'user-activity');
    const { data: activeUsers, isLoading: isLoadingActiveUsers, refetch: refetchActiveUsers } = useActiveUsersReport(fromDate || null, toDate || null, activeTab === 'active-users');
    const { data: chartsData, isLoading: isLoadingCharts, refetch: refetchCharts } = useChartsData(fromDate || null, toDate || null, activeTab === 'charts');
    const { data: dailyWorkReport, isLoading: isLoadingDailyWork, refetch: refetchDailyWork } = useDailyWorkReport(workDate || null, activeTab === 'daily-work');

    const handlePrefetch = (tab: ReportTabId) => {
        switch (tab) {
            case 'dashboard':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.reports.dashboard(),
                    queryFn: () => archivingService.getDepartmentDashboard(),
                });
                DashboardCards.preload();
                break;
            case 'daily':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.reports.daily(selectedDate || null),
                    queryFn: () => archivingService.getDailyReport(selectedDate || null),
                });
                DailyReportView.preload();
                break;
            case 'weekly':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.reports.weekly(weekStart || null),
                    queryFn: () => archivingService.getWeeklyReport(weekStart || null),
                });
                PeriodReportView.preload();
                break;
            case 'monthly':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.reports.monthly(reportYear, reportMonth),
                    queryFn: () => archivingService.getMonthlyReport(reportYear, reportMonth),
                });
                PeriodReportView.preload();
                break;
            case 'user-activity':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.reports.userActivity(fromDate || null, toDate || null),
                    queryFn: () => archivingService.getUserActivityReport(fromDate || null, toDate || null),
                });
                UserActivityView.preload();
                break;
            case 'active-users':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.reports.activeUsers(fromDate || null, toDate || null),
                    queryFn: () => archivingService.getActiveUsers(fromDate || null, toDate || null),
                });
                ActiveUsersView.preload();
                break;
            case 'charts':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.reports.charts(fromDate || null, toDate || null),
                    queryFn: () => archivingService.getChartsData(fromDate || null, toDate || null),
                });
                ChartsSection.preload();
                break;
            case 'daily-work':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.archiving.dailyWork.detail(workDate || null),
                    queryFn: () => archivingService.getDailyWorkReport(workDate || null),
                });
                DailyWorkReportView.preload();
                break;
            default: break;
        }
    };

    const allRefetch = () => {
        switch (activeTab) {
            case 'dashboard': refetchDashboard(); break;
            case 'daily': refetchDaily(); break;
            case 'weekly': refetchWeekly(); break;
            case 'monthly': refetchMonthly(); break;
            case 'user-activity': refetchUserActivity(); break;
            case 'active-users': refetchActiveUsers(); break;
            case 'charts': refetchCharts(); break;
            case 'daily-work': refetchDailyWork(); break;
            default: break;
        }
    };

    return (
        <ReportsPageLayout
            title="تقارير الأرشيف"
            description="تقارير وإحصائيات شاملة لنظام الأرشفة"
            tabs={TABS}
            activeTab={activeTab}
            onTabChange={setActiveTab}
            onPrefetch={handlePrefetch}
            allRefetch={allRefetch}
        >
            <TabsContent value="dashboard">
                <div className="flex justify-end mb-4">
                    {dashboard && (
                        <ExportButton
                            onExport={async () => {
                                const { exportDashboardToExcel } = await import('@/shared/lib/excel-export');
                                exportDashboardToExcel(dashboard);
                            }}
                            label="تصدير لوحة المعلومات"
                        />
                    )}
                </div>
                {isLoadingDashboard ? (
                    <div className="flex h-64 items-center justify-center">
                        <Loader2 className="h-8 w-8 animate-spin text-primary" />
                    </div>
                ) : dashboard ? (
                    <Suspense fallback={fallback}>
                        <DashboardCards dashboard={dashboard} />
                    </Suspense>
                ) : (
                    <Card>
                        <CardContent className="pt-8 text-center text-muted-foreground">
                            لا توجد بيانات للوحة المعلومات
                        </CardContent>
                    </Card>
                )}
            </TabsContent>

            <TabsContent value="daily">
                <div className="flex items-center justify-between gap-4 mb-4">
                    <Card className="border border-border/40 shadow-sm flex-1">
                        <CardContent className="pt-6">
                            <div className="flex items-center gap-3 max-w-xs">
                                <label className="text-xs font-semibold text-muted-foreground whitespace-nowrap">اختر التاريخ</label>
                                <div className="relative flex-1">
                                    <Input type="date" className="w-full pl-9 text-right" value={selectedDate} onChange={(e) => setSelectedDate(e.target.value)} />
                                    <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    {dailyReport && (
                        <ExportButton
                            onExport={async () => {
                                let chartImageUrl: string | undefined;
                                if (dailyChartRef.current) {
                                    try { chartImageUrl = await toPng(dailyChartRef.current, { quality: 0.95, pixelRatio: 2 }); } catch { /* chart capture failed silently */ }
                                }
                                const { exportDailyReportToExcel } = await import('@/shared/lib/excel-export');
                                await exportDailyReportToExcel(dailyReport, chartImageUrl);
                            }}
                            label="تصدير التقرير"
                        />
                    )}
                </div>
                <Suspense fallback={fallback}>
                    <DailyReportView ref={dailyChartRef} data={dailyReport} isLoading={isLoadingDaily} />
                </Suspense>
            </TabsContent>

            <TabsContent value="weekly">
                <div className="flex items-center justify-between gap-4 mb-4">
                    <Card className="border border-border/40 shadow-sm flex-1">
                        <CardContent className="pt-6">
                            <div className="flex items-center gap-3 max-w-xs">
                                <label className="text-xs font-semibold text-muted-foreground whitespace-nowrap">بداية الأسبوع</label>
                                <div className="relative flex-1">
                                    <Input type="date" className="w-full pl-9 text-right" value={weekStart} onChange={(e) => setWeekStart(e.target.value)} />
                                    <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    {weeklyReport && (
                        <ExportButton
                            onExport={async () => {
                                const { exportPeriodReportToExcel } = await import('@/shared/lib/excel-export');
                                exportPeriodReportToExcel(weeklyReport, 'التقرير الأسبوعي');
                            }}
                            label="تصدير التقرير"
                        />
                    )}
                </div>
                <Suspense fallback={fallback}>
                    <PeriodReportView data={weeklyReport} isLoading={isLoadingWeekly} periodLabel="التقرير الأسبوعي" />
                </Suspense>
            </TabsContent>

            <TabsContent value="monthly">
                <div className="flex items-center justify-between gap-4 mb-4">
                    <Card className="border border-border/40 shadow-sm flex-1">
                        <CardContent className="pt-6">
                            <div className="flex items-center gap-3">
                                <div className="space-y-1.5">
                                    <label className="text-xs font-semibold text-muted-foreground">السنة</label>
                                    <select className="h-10 px-3 bg-background border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary" value={reportYear} onChange={(e) => setReportYear(Number(e.target.value))}>
                                        {Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i).map((y) => (<option key={y} value={y}>{y}</option>))}
                                    </select>
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-xs font-semibold text-muted-foreground">الشهر</label>
                                    <select className="h-10 px-3 bg-background border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary" value={reportMonth} onChange={(e) => setReportMonth(Number(e.target.value))}>
                                        {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (<option key={m} value={m}>{new Date(2000, m - 1).toLocaleDateString('ar-SY', { month: 'long' })}</option>))}
                                    </select>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    {monthlyReport && (
                        <ExportButton
                            onExport={async () => {
                                const { exportPeriodReportToExcel } = await import('@/shared/lib/excel-export');
                                exportPeriodReportToExcel(monthlyReport, 'التقرير الشهري');
                            }}
                            label="تصدير التقرير"
                        />
                    )}
                </div>
                <Suspense fallback={fallback}>
                    <PeriodReportView data={monthlyReport} isLoading={isLoadingMonthly} periodLabel="التقرير الشهري" />
                </Suspense>
            </TabsContent>

            <TabsContent value="user-activity">
                <div className="flex items-center justify-between gap-4 mb-4">
                    <Card className="border border-border/40 shadow-sm flex-1">
                        <CardContent className="pt-6">
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-md">
                                <div className="space-y-1.5">
                                    <label className="text-xs font-semibold text-muted-foreground">من تاريخ</label>
                                    <div className="relative">
                                        <Input type="date" className="w-full pl-9 text-right" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
                                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                    </div>
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-xs font-semibold text-muted-foreground">إلى تاريخ</label>
                                    <div className="relative">
                                        <Input type="date" className="w-full pl-9 text-right" value={toDate} onChange={(e) => setToDate(e.target.value)} />
                                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    {userActivity && userActivity.length > 0 && (
                        <ExportButton
                            onExport={async () => {
                                const { exportUserActivityToExcel } = await import('@/shared/lib/excel-export');
                                exportUserActivityToExcel(userActivity, fromDate, toDate);
                            }}
                            label="تصدير النشاط"
                        />
                    )}
                </div>
                <Suspense fallback={fallback}>
                    <UserActivityView data={userActivity} isLoading={isLoadingUserActivity} />
                </Suspense>
            </TabsContent>

            <TabsContent value="active-users">
                <div className="flex items-center justify-between gap-4 mb-4">
                    <Card className="border border-border/40 shadow-sm flex-1">
                        <CardContent className="pt-6">
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 max-w-md">
                                <div className="space-y-1.5">
                                    <label className="text-xs font-semibold text-muted-foreground">من تاريخ</label>
                                    <div className="relative">
                                        <Input type="date" className="w-full pl-9 text-right" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
                                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                    </div>
                                </div>
                                <div className="space-y-1.5">
                                    <label className="text-xs font-semibold text-muted-foreground">إلى تاريخ</label>
                                    <div className="relative">
                                        <Input type="date" className="w-full pl-9 text-right" value={toDate} onChange={(e) => setToDate(e.target.value)} />
                                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    {activeUsers && activeUsers.length > 0 && (
                        <ExportButton
                            onExport={async () => {
                                const { exportActiveUsersToExcel } = await import('@/shared/lib/excel-export');
                                exportActiveUsersToExcel(activeUsers, fromDate, toDate);
                            }}
                            label="تصدير المستخدمين"
                        />
                    )}
                </div>
                <Suspense fallback={fallback}>
                    <ActiveUsersView data={activeUsers} isLoading={isLoadingActiveUsers} />
                </Suspense>
            </TabsContent>

            <TabsContent value="daily-work">
                <div className="flex items-center justify-between gap-4 mb-4">
                    <Card className="border border-border/40 shadow-sm flex-1">
                        <CardContent className="pt-6">
                            <div className="flex items-center gap-3 max-w-xs">
                                <label className="text-xs font-semibold text-muted-foreground whitespace-nowrap">اختر التاريخ</label>
                                <div className="relative flex-1">
                                    <Input type="date" className="w-full pl-9 text-right" value={workDate} onChange={(e) => setWorkDate(e.target.value)} />
                                    <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                    {dailyWorkReport && (
                        <ExportButton
                            onExport={async () => {
                                const { exportDailyWorkReportToExcel } = await import('@/shared/lib/excel-export');
                                exportDailyWorkReportToExcel(dailyWorkReport);
                            }}
                            label="تصدير Excel متعدد الأوراق"
                        />
                    )}
                </div>
                <Suspense fallback={fallback}>
                    <DailyWorkReportView data={dailyWorkReport} isLoading={isLoadingDailyWork} />
                </Suspense>
            </TabsContent>

            <TabsContent value="charts">
                <div className="flex justify-end mb-4">
                    {chartsData && (
                        <ExportButton
                            onExport={async () => {
                                const chartNames = ['dailyActivity', 'actionTypeBreakdown', 'hourlyDistribution', 'topActiveUsers', 'topStorageUsers', 'trend7Days'] as const;
                                const images: Record<string, string> = {};
                                const container = chartsSectionRef.current;
                                if (container) {
                                    for (const name of chartNames) {
                                        const el = container.querySelector<HTMLElement>(`[data-chart-name="${name}"]`);
                                        if (el) { try { images[name] = await toPng(el, { quality: 0.95, pixelRatio: 2 }); } catch { /* chart capture failed silently */ } }
                                    }
                                }
                                const { exportChartsToExcel } = await import('@/shared/lib/excel-export');
                                await exportChartsToExcel(chartsData, images);
                            }}
                            label="تصدير الرسوم البيانية"
                        />
                    )}
                </div>
                <Suspense fallback={fallback}>
                    <ChartsSection ref={chartsSectionRef} data={chartsData} isLoading={isLoadingCharts} />
                </Suspense>
            </TabsContent>
        </ReportsPageLayout>
    );
}
