import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Badge } from '@/shared/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Loader2 } from 'lucide-react';
import { resolveUserNames } from '@/shared/utils/resolve-user-names';
import { resolveUserDeptNames } from '@/shared/utils/resolve-user-dept-names';
import type { TransactionActiveUserItem } from '../../model/transaction-report-types';

interface ActiveUsersViewProps {
    data: TransactionActiveUserItem[] | undefined;
    isLoading: boolean;
}

function formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('ar-SY', {
        year: 'numeric', month: '2-digit', day: '2-digit',
    });
}

const ACTION_TRANSLATIONS: Record<string, string> = {
    'Created': 'إنشاء المعاملة',
    'Responded': 'الرد على المعاملة',
    'Transferred': 'تحويل المعاملة',
    'Viewed': 'عرض المعاملة',
    'Updated': 'تحديث المعاملة',
    'Deleted': 'حذف المعاملة',
    'AttachmentAdded': 'إضافة مرفق',
    'AttachmentDownloaded': 'تحميل مرفق',
    'StatusChanged': 'تغيير حالة المعاملة',
};

export function ActiveUsersView({ data, isLoading }: ActiveUsersViewProps) {
    const [userNames, setUserNames] = useState<Map<string, string>>(new Map());
    const [deptNames, setDeptNames] = useState<Map<string, string>>(new Map());

    const list: TransactionActiveUserItem[] = Array.isArray(data) ? data : (data && Array.isArray((data as any).data) ? (data as any).data : []);

    useEffect(() => {
        if (list.length === 0) return;
        const userIds = list.map((u) => u.userId);
        resolveUserNames(userIds).then(setUserNames);
        resolveUserDeptNames(userIds).then(setDeptNames);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [data]);

    if (isLoading) {
        return (
            <div className="flex h-64 items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    if (list.length === 0) {
        return (
            <Card>
                <CardContent className="pt-8 text-center text-muted-foreground">
                    لا يوجد مستخدمون نشطون في الفترة المحددة
                </CardContent>
            </Card>
        );
    }

    return (
        <Card className="border border-border/40 shadow-sm" dir="rtl">
            <CardHeader>
                <CardTitle className="text-sm font-semibold">المستخدمون النشطون في المعاملات</CardTitle>
            </CardHeader>
            <CardContent className="p-0">
                <Table>
                    <TableHeader>
                        <TableRow>
                            <TableHead className="text-right">المستخدم</TableHead>
                            <TableHead className="text-right">القسم</TableHead>
                            <TableHead className="text-right">إجمالي الإجراءات</TableHead>
                            <TableHead className="text-right">أول نشاط</TableHead>
                            <TableHead className="text-right">آخر نشاط</TableHead>
                            <TableHead className="text-right">الإجراءات</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {list.map((u) => (
                            <TableRow key={u.userId}>
                                <TableCell className="font-medium">{userNames.get(u.userId) || u.userName}</TableCell>
                                <TableCell className="text-muted-foreground">{deptNames.get(u.userId) || u.departmentName || '-'}</TableCell>
                                <TableCell>
                                    <span className="font-bold">{u.totalActions}</span>
                                </TableCell>
                                <TableCell className="text-xs text-muted-foreground">
                                    {formatDate(u.firstActionDate)}
                                </TableCell>
                                <TableCell className="text-xs text-muted-foreground">
                                    {formatDate(u.lastActionDate)}
                                </TableCell>
                                <TableCell>
                                    <div className="flex flex-wrap gap-1">
                                        {Array.isArray(u.actionsPerformed) && u.actionsPerformed.map((action, i) => (
                                            <Badge key={i} variant="secondary" className="text-xs">
                                                {ACTION_TRANSLATIONS[action] || action}
                                            </Badge>
                                        ))}
                                    </div>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}
