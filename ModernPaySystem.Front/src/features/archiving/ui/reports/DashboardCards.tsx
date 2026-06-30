import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Progress } from '@/shared/ui/progress';
import {
    Archive, Users, FolderOpen, FileText, HardDrive,
    CalendarDays, Calendar,
    UserCheck, Activity
} from 'lucide-react';
import type { DepartmentArchiveDashboard } from '../../model/types';

interface DashboardCardsProps {
    dashboard: DepartmentArchiveDashboard;
}

function formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
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

const ACTION_TRANSLATIONS: Record<string, string> = {
    'View': 'عرض مجلد/سجل',
    'Update': 'تحديث بيانات',
    'Download': 'تنزيل ملف',
    'Print': 'طباعة ملف',
    'Create': 'إنشاء مجلد/سجل',
    'Delete': 'حذف سجل',
    'Export': 'تصدير بيانات',
    'Upload': 'رفع ملفات',
    'AddFiles': 'إضافة ملفات للسجل',
    'RemoveFiles': 'حذف ملفات من السجل',
    'ApproveEdit': 'موافقة على طلب تعديل',
    'RejectEdit': 'رفض طلب تعديل',
    'ApproveDelete': 'موافقة على طلب حذف',
    'RejectDelete': 'رفض طلب حذف',
    'SubmitEditRequest': 'طلب تعديل سجل',
    'SubmitDeleteRequest': 'طلب حذف سجل',
    'Move': 'نقل مجلد/سجل'
};

function ActionBreakdownCard({ breakdown }: { breakdown: Record<string, number> }) {
    const entries = Object.entries(breakdown);
    const total = entries.reduce((sum, [, v]) => sum + v, 0);

    if (entries.length === 0) return null;

    return (
        <Card className="border border-border/40 shadow-sm">
            <CardHeader>
                <CardTitle className="text-sm font-semibold flex items-center gap-2">
                    <Activity className="w-4 h-4 text-primary" />
                    توزيع الإجراءات
                </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
                {entries.map(([action, count]) => (
                    <div key={action} className="space-y-1">
                        <div className="flex justify-between text-xs">
                            <span className="font-medium">{ACTION_TRANSLATIONS[action] || action}</span>
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
                    icon={<Archive className="w-5 h-5" />}
                    label="إجمالي السجلات"
                    value={dashboard.totalArchiveRecords.toLocaleString()}
                    sublabel={`${dashboard.departmentName}`}
                />
                <StatCard
                    icon={<FolderOpen className="w-5 h-5" />}
                    label="إجمالي المجلدات"
                    value={dashboard.totalFolders.toLocaleString()}
                />
                <StatCard
                    icon={<FileText className="w-5 h-5" />}
                    label="الملفات المرفوعة"
                    value={dashboard.totalPhysicalFiles.toLocaleString()}
                />
                <StatCard
                    icon={<HardDrive className="w-5 h-5" />}
                    label="مساحة التخزين"
                    value={formatBytes(dashboard.totalStorageBytes)}
                />
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                <StatCard
                    icon={<CalendarDays className="w-5 h-5" />}
                    label="سجلات اليوم"
                    value={dashboard.recordsCreatedToday}
                />
                <StatCard
                    icon={<CalendarDays className="w-5 h-5" />}
                    label="سجلات هذا الأسبوع"
                    value={dashboard.recordsCreatedThisWeek}
                />
                <StatCard
                    icon={<Calendar className="w-5 h-5" />}
                    label="سجلات هذا الشهر"
                    value={dashboard.recordsCreatedThisMonth}
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
                    label="إجمالي المستخدمين"
                    value={dashboard.totalUsers}
                />
            </div>

            <ActionBreakdownCard breakdown={dashboard.actionTypeBreakdown} />
        </div>
    );
}
