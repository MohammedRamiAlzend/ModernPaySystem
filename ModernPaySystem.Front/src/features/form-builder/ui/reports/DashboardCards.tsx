import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Progress } from '@/shared/ui/progress';
import {
    ClipboardList, MessageSquare, Paperclip, Clock, PlayCircle, CheckCircle2,
    Truck, CalendarDays, Calendar, UserCheck, Users, Activity
} from 'lucide-react';
import type { TransactionDashboard } from '../../model/transaction-report-types';

interface DashboardCardsProps {
    dashboard: TransactionDashboard;
}

function StatCard({ icon, label, value, sublabel }: { icon: React.ReactNode; label: string; value: string | number; sublabel?: string }) {
    return (
        <Card className="border border-border/40 shadow-sm hover:shadow-md transition-shadow">
            <CardContent className="pt-6">
                <div className="flex items-center justify-between">
                    <div className="space-y-1">
                        <p className="text-xs text-muted-foreground font-medium">{label}</p>
                        <p className="text-2xl font-bold tracking-tight">{value}</p>
                        {sublabel && <p className="text-xs text-muted-foreground">{sublabel}</p>}
                    </div>
                    <div className="p-3 rounded-xl bg-primary/10 text-primary">
                        {icon}
                    </div>
                </div>
            </CardContent>
        </Card>
    );
}

function StatusBreakdownCard({ breakdown }: { breakdown: Record<string, number> }) {
    const entries = Object.entries(breakdown);
    const total = entries.reduce((sum, [, v]) => sum + v, 0);

    if (entries.length === 0) return null;

    return (
        <Card className="border border-border/40 shadow-sm">
            <CardHeader>
                <CardTitle className="text-sm font-semibold flex items-center gap-2">
                    <Activity className="w-4 h-4 text-primary" />
                    توزيع الطلبات حسب الحالة
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
                {entries.map(([status, count]) => (
                    <div key={status} className="space-y-1">
                        <div className="flex justify-between text-xs">
                            <span className="font-medium">{status}</span>
                            <span className="text-muted-foreground">{count}</span>
                        </div>
                        <Progress value={total > 0 ? (count / total) * 100 : 0} className="h-1.5" />
                    </div>
                ))}
            </CardContent>
        </Card>
    );
}

export function DashboardCards({ dashboard }: DashboardCardsProps) {
    return (
        <div className="space-y-6" dir="rtl">
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <StatCard
                    icon={<ClipboardList className="w-5 h-5" />}
                    label="إجمالي الطلبات"
                    value={dashboard.totalRequests.toLocaleString()}
                />
                <StatCard
                    icon={<MessageSquare className="w-5 h-5" />}
                    label="إجمالي الردود"
                    value={dashboard.totalResponses.toLocaleString()}
                />
                <StatCard
                    icon={<Paperclip className="w-5 h-5" />}
                    label="إجمالي المرفقات"
                    value={dashboard.totalAttachments.toLocaleString()}
                />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <StatCard
                    icon={<Clock className="w-5 h-5" />}
                    label="قيد الانتظار"
                    value={dashboard.pending.toLocaleString()}
                />
                <StatCard
                    icon={<PlayCircle className="w-5 h-5" />}
                    label="قيد المعالجة"
                    value={dashboard.inProcess.toLocaleString()}
                />
                <StatCard
                    icon={<CheckCircle2 className="w-5 h-5" />}
                    label="تمت الإدارة"
                    value={dashboard.managed.toLocaleString()}
                />
                <StatCard
                    icon={<Truck className="w-5 h-5" />}
                    label="تم التسليم"
                    value={dashboard.delivered.toLocaleString()}
                />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                <StatCard
                    icon={<CalendarDays className="w-5 h-5" />}
                    label="طلبات اليوم"
                    value={dashboard.requestsToday}
                />
                <StatCard
                    icon={<CalendarDays className="w-5 h-5" />}
                    label="طلبات هذا الأسبوع"
                    value={dashboard.requestsThisWeek}
                />
                <StatCard
                    icon={<Calendar className="w-5 h-5" />}
                    label="طلبات هذا الشهر"
                    value={dashboard.requestsThisMonth}
                />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                <StatCard
                    icon={<UserCheck className="w-5 h-5" />}
                    label="المستخدمون النشطون اليوم"
                    value={dashboard.activeUsersToday}
                />
                <StatCard
                    icon={<UserCheck className="w-5 h-5" />}
                    label="المستخدمون النشطون هذا الأسبوع"
                    value={dashboard.activeUsersThisWeek}
                />
                <StatCard
                    icon={<Users className="w-5 h-5" />}
                    label="المستخدمون النشطون هذا الشهر"
                    value={dashboard.activeUsersThisMonth}
                />
            </div>

            <StatusBreakdownCard breakdown={dashboard.statusBreakdown} />
        </div>
    );
}
