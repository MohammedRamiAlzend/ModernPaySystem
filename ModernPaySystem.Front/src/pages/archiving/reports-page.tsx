import { useState } from 'react';
import { Card, CardContent } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/shared/ui/tabs';
import {
    useDepartmentDashboard,
    useDailyReport,
    useWeeklyReport,
    useMonthlyReport,
    useUserActivityReport,
    useActiveUsersReport,
    useStorageReport,
    useChartsData,
} from '@/features/archiving/model/queries';
import { DashboardCards } from '@/features/archiving/ui/reports/DashboardCards';
import { DailyReportView } from '@/features/archiving/ui/reports/DailyReportView';
import { PeriodReportView } from '@/features/archiving/ui/reports/PeriodReportView';
import { UserActivityView } from '@/features/archiving/ui/reports/UserActivityView';
import { ActiveUsersView } from '@/features/archiving/ui/reports/ActiveUsersView';
import { StorageReportView } from '@/features/archiving/ui/reports/StorageReportView';
import { ChartsSection } from '@/features/archiving/ui/reports/ChartsSection';
import { Calendar, RefreshCw, Loader2 } from 'lucide-react';

type ReportTab = 'dashboard' | 'daily' | 'weekly' | 'monthly' | 'user-activity' | 'active-users' | 'storage' | 'charts';

export default function ReportsPage() {
    const [activeTab, setActiveTab] = useState<ReportTab>('dashboard');
    const [fromDate, setFromDate] = useState('');
    const [toDate, setToDate] = useState('');
    const [selectedDate, setSelectedDate] = useState('');
    const [weekStart, setWeekStart] = useState('');
    const [reportYear, setReportYear] = useState<number>(new Date().getFullYear());
    const [reportMonth, setReportMonth] = useState<number>(new Date().getMonth() + 1);

    const { data: dashboard, isLoading: isLoadingDashboard, refetch: refetchDashboard } = useDepartmentDashboard();
    const { data: dailyReport, isLoading: isLoadingDaily } = useDailyReport(selectedDate || null);
    const { data: weeklyReport, isLoading: isLoadingWeekly } = useWeeklyReport(weekStart || null);
    const { data: monthlyReport, isLoading: isLoadingMonthly } = useMonthlyReport(reportYear, reportMonth);
    const { data: userActivity, isLoading: isLoadingUserActivity } = useUserActivityReport(fromDate || null, toDate || null);
    const { data: activeUsers, isLoading: isLoadingActiveUsers } = useActiveUsersReport(fromDate || null, toDate || null);
    const { data: storageReport, isLoading: isLoadingStorage } = useStorageReport();
    const { data: chartsData, isLoading: isLoadingCharts } = useChartsData(fromDate || null, toDate || null);

    const allRefetch = () => {
        refetchDashboard();
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
        };
        return labels[tab];
    };

    return (
        <div className="space-y-6 max-w-7xl mx-auto px-4 py-6" dir="rtl">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight text-foreground">تقارير الأرشيف</h1>
                    <p className="text-sm text-muted-foreground mt-1">
                        تقارير وإحصائيات شاملة لنظام الأرشفة
                    </p>
                </div>
                <Button variant="outline" size="sm" onClick={allRefetch}>
                    <RefreshCw className="w-4 h-4 ml-2" />
                    <span>تحديث</span>
                </Button>
            </div>

            <Tabs defaultValue="dashboard" value={activeTab} onValueChange={(v) => setActiveTab(v as ReportTab)}>
                <div className="overflow-x-auto pb-2">
                    <TabsList className="w-full justify-start gap-1 bg-muted/50 p-1 rounded-lg">
                        {(['dashboard', 'daily', 'weekly', 'monthly', 'user-activity', 'active-users', 'storage', 'charts'] as ReportTab[]).map((tab) => (
                            <TabsTrigger key={tab} value={tab} className="px-4 py-2 text-sm whitespace-nowrap">
                                {getTabLabel(tab)}
                            </TabsTrigger>
                        ))}
                    </TabsList>
                </div>

                <TabsContent value="dashboard">
                    {isLoadingDashboard ? (
                        <div className="flex h-64 items-center justify-center">
                            <Loader2 className="h-8 w-8 animate-spin text-primary" />
                        </div>
                    ) : dashboard ? (
                        <DashboardCards dashboard={dashboard} />
                    ) : (
                        <Card>
                            <CardContent className="pt-8 text-center text-muted-foreground">
                                لا توجد بيانات للوحة المعلومات
                            </CardContent>
                        </Card>
                    )}
                </TabsContent>

                <TabsContent value="daily">
                    <Card className="border border-border/40 shadow-sm mb-4">
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
                    <DailyReportView data={dailyReport} isLoading={isLoadingDaily} />
                </TabsContent>

                <TabsContent value="weekly">
                    <Card className="border border-border/40 shadow-sm mb-4">
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
                    <PeriodReportView
                        data={weeklyReport}
                        isLoading={isLoadingWeekly}
                        periodLabel="التقرير الأسبوعي"
                    />
                </TabsContent>

                <TabsContent value="monthly">
                    <Card className="border border-border/40 shadow-sm mb-4">
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
                    <PeriodReportView
                        data={monthlyReport}
                        isLoading={isLoadingMonthly}
                        periodLabel="التقرير الشهري"
                    />
                </TabsContent>

                <TabsContent value="user-activity">
                    <Card className="border border-border/40 shadow-sm mb-4">
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
                    <UserActivityView data={userActivity} isLoading={isLoadingUserActivity} />
                </TabsContent>

                <TabsContent value="active-users">
                    <Card className="border border-border/40 shadow-sm mb-4">
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
                    <ActiveUsersView data={activeUsers} isLoading={isLoadingActiveUsers} />
                </TabsContent>

                <TabsContent value="storage">
                    <StorageReportView data={storageReport} isLoading={isLoadingStorage} />
                </TabsContent>

                <TabsContent value="charts">
                    <ChartsSection data={chartsData} isLoading={isLoadingCharts} />
                </TabsContent>
            </Tabs>
        </div>
    );
}
