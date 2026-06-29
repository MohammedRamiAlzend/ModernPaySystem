import { useState, useRef, Suspense } from 'react';
import { toPng } from 'html-to-image';
import { useQueryClient } from '@tanstack/react-query';
import { Card, CardContent } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/shared/ui/tabs';
import { useAuthStore } from '@/app/store/authStore';
import { queryKeys } from '@/shared/constants/query-keys';
import { formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { lazyWithPreload } from '@/shared/utils/lazy-with-preload';
import { ExportButton } from '@/features/form-builder/ui/reports/ExportButton';
import {
    useTransactionDashboard,
    useTransactionDailyReport,
    useTransactionWeeklyReport,
    useTransactionMonthlyReport,
    useTransactionUserActivityReport,
    useTransactionActiveUsersReport,
    useTransactionStorageReport,
    useTransactionChartsData,
    useTransactionDailyWorkReport,
} from '@/features/form-builder/api/formEndpoints';
import {
    exportTransactionDashboardToExcel,
    exportTransactionDailyReportToExcel,
    exportTransactionPeriodReportToExcel,
    exportTransactionUserActivityToExcel,
    exportTransactionActiveUsersToExcel,
    exportTransactionStorageReportToExcel,
    exportTransactionDailyWorkReportToExcel,
    exportTransactionChartsToExcel,
} from '@/features/form-builder/model/transaction-excel-export';
import { Calendar, RefreshCw, Loader2, Ban } from 'lucide-react';

const DashboardCards = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/DashboardCards').then(m => ({ default: m.DashboardCards }))
);
const DailyReportView = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/DailyReportView').then(m => ({ default: m.DailyReportView }))
);
const PeriodReportView = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/PeriodReportView').then(m => ({ default: m.PeriodReportView }))
);
const UserActivityView = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/UserActivityView').then(m => ({ default: m.UserActivityView }))
);
const ActiveUsersView = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/ActiveUsersView').then(m => ({ default: m.ActiveUsersView }))
);
const StorageReportView = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/StorageReportView').then(m => ({ default: m.StorageReportView }))
);
const ChartsSection = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/ChartsSection').then(m => ({ default: m.ChartsSection }))
);
const DailyWorkReportView = lazyWithPreload(() =>
    import('@/features/form-builder/ui/reports/DailyWorkReportView').then(m => ({ default: m.DailyWorkReportView }))
);

const fallback = (
    <div className="flex h-64 items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
    </div>
);

type ReportTab = 'dashboard' | 'daily' | 'weekly' | 'monthly' | 'user-activity' | 'active-users' | 'storage' | 'charts' | 'daily-work';

export default function ReportsPage() {
    const currentUser = useAuthStore((state) => state.user);
    const queryClient = useQueryClient();
    const [activeTab, setActiveTab] = useState<ReportTab>('dashboard');
    const [fromDate, setFromDate] = useState('');
    const [toDate, setToDate] = useState('');
    const [selectedDate, setSelectedDate] = useState('');
    const [workDate, setWorkDate] = useState('');
    const dailyChartRef = useRef<HTMLDivElement>(null);
    const chartsSectionRef = useRef<HTMLDivElement>(null);
    const [weekStart, setWeekStart] = useState('');
    const [reportYear, setReportYear] = useState<number>(new Date().getFullYear());
    const [reportMonth, setReportMonth] = useState<number>(new Date().getMonth() + 1);

    const { data: dashboard, isLoading: isLoadingDashboard, refetch: refetchDashboard } = useTransactionDashboard(activeTab === 'dashboard');
    const { data: dailyReport, isLoading: isLoadingDaily } = useTransactionDailyReport(selectedDate || null, activeTab === 'daily');
    const { data: weeklyReport, isLoading: isLoadingWeekly } = useTransactionWeeklyReport(weekStart || null, activeTab === 'weekly');
    const { data: monthlyReport, isLoading: isLoadingMonthly } = useTransactionMonthlyReport(reportYear, reportMonth, activeTab === 'monthly');
    const { data: userActivity, isLoading: isLoadingUserActivity } = useTransactionUserActivityReport(fromDate || null, toDate || null, activeTab === 'user-activity');
    const { data: activeUsers, isLoading: isLoadingActiveUsers } = useTransactionActiveUsersReport(fromDate || null, toDate || null, activeTab === 'active-users');
    const { data: storageReport, isLoading: isLoadingStorage } = useTransactionStorageReport(activeTab === 'storage');
    const { data: chartsData, isLoading: isLoadingCharts } = useTransactionChartsData(fromDate || null, toDate || null, activeTab === 'charts');
    const { data: dailyWorkReport, isLoading: isLoadingDailyWork } = useTransactionDailyWorkReport(workDate || null, activeTab === 'daily-work');

    if (!currentUser?.isDepartmentHead) {
        return (
            <div className="flex h-[60vh] items-center justify-center" dir="rtl">
                <Card className="max-w-md w-full border-destructive/20">
                    <CardContent className="pt-12 pb-12 text-center space-y-4">
                        <Ban className="w-16 h-16 mx-auto text-destructive/60" />
                        <h2 className="text-2xl font-bold text-foreground">غير مصرح بالوصول</h2>
                        <p className="text-muted-foreground">
                            هذه الصفحة متاحة فقط لمديري الأقسام. يرجى التواصل مع المشرف إذا كنت بحاجة إلى صلاحية الوصول.
                        </p>
                    </CardContent>
                </Card>
            </div>
        );
    }

    const handlePrefetch = (tab: ReportTab) => {
        switch (tab) {
            case 'dashboard':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.dashboard,
                    queryFn: () => formEndpoints.getTransactionDashboard(),
                });
                DashboardCards.preload();
                break;
            case 'daily':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.daily(selectedDate || null),
                    queryFn: () => formEndpoints.getTransactionDailyReport(selectedDate || null),
                });
                DailyReportView.preload();
                break;
            case 'weekly':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.weekly(weekStart || null),
                    queryFn: () => formEndpoints.getTransactionWeeklyReport(weekStart || null),
                });
                PeriodReportView.preload();
                break;
            case 'monthly':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.monthly(reportYear, reportMonth),
                    queryFn: () => formEndpoints.getTransactionMonthlyReport(reportYear, reportMonth),
                });
                PeriodReportView.preload();
                break;
            case 'user-activity':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.userActivity(fromDate || null, toDate || null),
                    queryFn: () => formEndpoints.getTransactionUserActivity(fromDate || null, toDate || null),
                });
                UserActivityView.preload();
                break;
            case 'active-users':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.activeUsers(fromDate || null, toDate || null),
                    queryFn: () => formEndpoints.getTransactionActiveUsers(fromDate || null, toDate || null),
                });
                ActiveUsersView.preload();
                break;
            case 'storage':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.storage,
                    queryFn: () => formEndpoints.getTransactionStorageReport(),
                });
                StorageReportView.preload();
                break;
            case 'charts':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.charts(fromDate || null, toDate || null),
                    queryFn: () => formEndpoints.getTransactionChartsData(fromDate || null, toDate || null),
                });
                ChartsSection.preload();
                break;
            case 'daily-work':
                queryClient.prefetchQuery({
                    queryKey: queryKeys.transactionReports.dailyWork(workDate || null),
                    queryFn: () => formEndpoints.getTransactionDailyWork(workDate || null),
                });
                DailyWorkReportView.preload();
                break;
        }
    };

    const getTabLabel = (tab: ReportTab): string => {
        const labels: Record<ReportTab, string> = {
            dashboard: 'لوحة المعلومات',
            daily: 'تقرير يومي',
            weekly: 'تقرير أسبوعي',
            monthly: 'تقرير شهري',
            'user-activity': 'نشاط المستخدمين',
            'active-users': 'المستخدمون النشطون',
            storage: 'التخزين',
            charts: 'الرسوم البيانية',
            'daily-work': 'تقرير يومي مفصل',
        };
        return labels[tab];
    };

    return (
        <div className="space-y-6 max-w-7xl mx-auto px-4 py-6" dir="rtl">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight text-foreground">تقارير المعاملات</h1>
                    <p className="text-sm text-muted-foreground mt-1">
                        تقارير وإحصائيات شاملة لنظام المعاملات
                    </p>
                </div>
                <Button variant="outline" size="sm" onClick={() => refetchDashboard()}>
                    <RefreshCw className="w-4 h-4 ml-2" />
                    <span>تحديث</span>
                </Button>
            </div>

            <Tabs defaultValue="dashboard" value={activeTab} onValueChange={(v) => setActiveTab(v as ReportTab)}>
                <div className="overflow-x-auto pb-2">
                    <TabsList className="w-full justify-start gap-1 bg-muted/50 p-1 rounded-lg">
                        {(['dashboard', 'daily', 'daily-work', 'weekly', 'monthly', 'user-activity', 'active-users', 'storage', 'charts'] as ReportTab[]).map((tab) => (
                            <TabsTrigger
                                key={tab}
                                value={tab}
                                className="px-4 py-2 text-sm whitespace-nowrap"
                                onMouseEnter={() => handlePrefetch(tab)}
                                onFocus={() => handlePrefetch(tab)}
                            >
                                {getTabLabel(tab)}
                            </TabsTrigger>
                        ))}
                    </TabsList>
                </div>

                <TabsContent value="dashboard">
                    <div className="flex justify-end mb-4">
                        {dashboard && (
                            <ExportButton
                                onExport={() => exportTransactionDashboardToExcel(dashboard)}
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
                                        <Input
                                            type="date"
                                            className="w-full pl-9 text-right"
                                            value={selectedDate}
                                            onChange={(e) => setSelectedDate(e.target.value)}
                                        />
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
                                        try {
                                            chartImageUrl = await toPng(dailyChartRef.current, { quality: 0.95, pixelRatio: 2 });
                                        } catch { /* chart capture failed */ }
                                    }
                                    await exportTransactionDailyReportToExcel(dailyReport, chartImageUrl);
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
                                        <Input
                                            type="date"
                                            className="w-full pl-9 text-right"
                                            value={weekStart}
                                            onChange={(e) => setWeekStart(e.target.value)}
                                        />
                                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                        {weeklyReport && (
                            <ExportButton
                                onExport={() => exportTransactionPeriodReportToExcel(weeklyReport, 'التقرير الأسبوعي')}
                                label="تصدير التقرير"
                            />
                        )}
                    </div>
                    <Suspense fallback={fallback}>
                        <PeriodReportView
                            data={weeklyReport}
                            isLoading={isLoadingWeekly}
                            periodLabel="التقرير الأسبوعي"
                        />
                    </Suspense>
                </TabsContent>

                <TabsContent value="monthly">
                    <div className="flex items-center justify-between gap-4 mb-4">
                        <Card className="border border-border/40 shadow-sm flex-1">
                            <CardContent className="pt-6">
                                <div className="flex items-center gap-3">
                                    <div className="space-y-1.5">
                                        <label className="text-xs font-semibold text-muted-foreground">السنة</label>
                                        <select
                                            className="h-10 px-3 bg-background border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary"
                                            value={reportYear}
                                            onChange={(e) => setReportYear(Number(e.target.value))}
                                        >
                                            {Array.from({ length: 5 }, (_, i) => new Date().getFullYear() - i).map((y) => (
                                                <option key={y} value={y}>{y}</option>
                                            ))}
                                        </select>
                                    </div>
                                    <div className="space-y-1.5">
                                        <label className="text-xs font-semibold text-muted-foreground">الشهر</label>
                                        <select
                                            className="h-10 px-3 bg-background border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary"
                                            value={reportMonth}
                                            onChange={(e) => setReportMonth(Number(e.target.value))}
                                        >
                                            {Array.from({ length: 12 }, (_, i) => i + 1).map((m) => (
                                                <option key={m} value={m}>
                                                    {new Date(2000, m - 1).toLocaleDateString('ar-SY', { month: 'long' })}
                                                </option>
                                            ))}
                                        </select>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                        {monthlyReport && (
                            <ExportButton
                                onExport={() => exportTransactionPeriodReportToExcel(monthlyReport, 'التقرير الشهري')}
                                label="تصدير التقرير"
                            />
                        )}
                    </div>
                    <Suspense fallback={fallback}>
                        <PeriodReportView
                            data={monthlyReport}
                            isLoading={isLoadingMonthly}
                            periodLabel="التقرير الشهري"
                        />
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
                                            <Input
                                                type="date"
                                                className="w-full pl-9 text-right"
                                                value={fromDate}
                                                onChange={(e) => setFromDate(e.target.value)}
                                            />
                                            <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                        </div>
                                    </div>
                                    <div className="space-y-1.5">
                                        <label className="text-xs font-semibold text-muted-foreground">إلى تاريخ</label>
                                        <div className="relative">
                                            <Input
                                                type="date"
                                                className="w-full pl-9 text-right"
                                                value={toDate}
                                                onChange={(e) => setToDate(e.target.value)}
                                            />
                                            <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                        </div>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                        {userActivity && userActivity.length > 0 && (
                            <ExportButton
                                onExport={() => exportTransactionUserActivityToExcel(userActivity, fromDate, toDate)}
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
                                            <Input
                                                type="date"
                                                className="w-full pl-9 text-right"
                                                value={fromDate}
                                                onChange={(e) => setFromDate(e.target.value)}
                                            />
                                            <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                        </div>
                                    </div>
                                    <div className="space-y-1.5">
                                        <label className="text-xs font-semibold text-muted-foreground">إلى تاريخ</label>
                                        <div className="relative">
                                            <Input
                                                type="date"
                                                className="w-full pl-9 text-right"
                                                value={toDate}
                                                onChange={(e) => setToDate(e.target.value)}
                                            />
                                            <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                        </div>
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                        {activeUsers && activeUsers.length > 0 && (
                            <ExportButton
                                onExport={() => exportTransactionActiveUsersToExcel(activeUsers, fromDate, toDate)}
                                label="تصدير المستخدمين"
                            />
                        )}
                    </div>
                    <Suspense fallback={fallback}>
                        <ActiveUsersView data={activeUsers} isLoading={isLoadingActiveUsers} />
                    </Suspense>
                </TabsContent>

                <TabsContent value="storage">
                    <div className="flex justify-end mb-4">
                        {storageReport && (
                            <ExportButton
                                onExport={() => exportTransactionStorageReportToExcel(storageReport)}
                                label="تصدير التقرير"
                            />
                        )}
                    </div>
                    <Suspense fallback={fallback}>
                        <StorageReportView data={storageReport} isLoading={isLoadingStorage} />
                    </Suspense>
                </TabsContent>

                <TabsContent value="daily-work">
                    <div className="flex items-center justify-between gap-4 mb-4">
                        <Card className="border border-border/40 shadow-sm flex-1">
                            <CardContent className="pt-6">
                                <div className="flex items-center gap-3 max-w-xs">
                                    <label className="text-xs font-semibold text-muted-foreground whitespace-nowrap">اختر التاريخ</label>
                                    <div className="relative flex-1">
                                        <Input
                                            type="date"
                                            className="w-full pl-9 text-right"
                                            value={workDate}
                                            onChange={(e) => setWorkDate(e.target.value)}
                                        />
                                        <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                                    </div>
                                </div>
                            </CardContent>
                        </Card>
                        {dailyWorkReport && (
                            <ExportButton
                                onExport={() => exportTransactionDailyWorkReportToExcel(dailyWorkReport)}
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
                                            if (el) {
                                                try {
                                                    images[name] = await toPng(el, { quality: 0.95, pixelRatio: 2 });
                                                } catch { /* chart capture failed */ }
                                            }
                                        }
                                    }
                                    await exportTransactionChartsToExcel(chartsData, images);
                                }}
                                label="تصدير الرسوم البيانية"
                            />
                        )}
                    </div>
                    <Suspense fallback={fallback}>
                        <ChartsSection ref={chartsSectionRef} data={chartsData} isLoading={isLoadingCharts} />
                    </Suspense>
                </TabsContent>
            </Tabs>
        </div>
    );
}
