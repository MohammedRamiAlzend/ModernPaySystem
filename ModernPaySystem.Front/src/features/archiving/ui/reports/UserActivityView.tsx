import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Loader2 } from 'lucide-react';
import type { UserActivityReportItem } from '../../model/types';

interface UserActivityViewProps {
    data: UserActivityReportItem[] | undefined;
    isLoading: boolean;
}

function formatDate(dateStr: string | null): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('ar-SY', {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit',
    });
}

export function UserActivityView({ data, isLoading }: UserActivityViewProps) {
    if (isLoading) {
        return (
            <div className="flex h-64 items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    const list: UserActivityReportItem[] = Array.isArray(data) ? data : (data && Array.isArray((data as any).data) ? (data as any).data : []);

    if (list.length === 0) {
        return (
            <Card>
                <CardContent className="pt-8 text-center text-muted-foreground">
                    لا توجد بيانات نشاط للمستخدمين في الفترة المحددة
                </CardContent>
            </Card>
        );
    }

    return (
        <Card className="border border-border/40 shadow-sm" dir="rtl">
            <CardHeader>
                <CardTitle className="text-sm font-semibold">نشاط المستخدمين</CardTitle>
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
                            <TableHead className="text-right">آخر نشاط</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {list.map((u) => (
                            <TableRow key={u.userId}>
                                <TableCell className="font-medium">{u.userName}</TableCell>
                                <TableCell>{u.recordsCreated}</TableCell>
                                <TableCell>{u.recordsViewed}</TableCell>
                                <TableCell>{u.filesDownloaded}</TableCell>
                                <TableCell>{u.printActions}</TableCell>
                                <TableCell>
                                    <span className="font-bold">{u.totalActions}</span>
                                </TableCell>
                                <TableCell className="text-xs text-muted-foreground">
                                    {formatDate(u.lastActivityDate)}
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </CardContent>
        </Card>
    );
}
