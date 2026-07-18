import { useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui/card';
import { Progress } from '@/shared/ui/progress';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Loader2, HardDrive, FileText } from 'lucide-react';
import type { StorageConsumptionReport } from '../../model/types';
import { StorageChart } from './charts/StorageChart';
import { resolveUserNames } from '@/shared/utils/resolve-user-names';

interface StorageReportViewProps {
    data: StorageConsumptionReport | undefined;
    isLoading: boolean;
}

function formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

export function StorageReportView({ data, isLoading }: StorageReportViewProps) {
    const reportData = (data as any)?.data ? (data as any).data : data;
    const perUser: any[] = Array.isArray(reportData?.perUser) ? reportData.perUser : (Array.isArray((reportData as any)?.PerUser) ? (reportData as any).PerUser : []);

    const [userNames, setUserNames] = useState<Map<string, string>>(new Map());

    useEffect(() => {
        if (perUser.length === 0) return;
        const userIds = perUser.map((u: any) => u.userId);
        resolveUserNames(userIds).then(setUserNames);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [data]);

    if (isLoading) {
        return (
            <div className="flex h-64 items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    if (!reportData) {
        return (
            <Card>
                <CardContent className="pt-8 text-center text-muted-foreground">
                    لا توجد بيانات تخزين متاحة
                </CardContent>
            </Card>
        );
    }

    const totalStorageBytes = reportData.totalStorageBytes ?? reportData.TotalStorageBytes ?? 0;
    const totalFiles = reportData.totalFiles ?? reportData.TotalFiles ?? 0;
    const fileTypeBreakdown: any[] = Array.isArray(reportData.fileTypeBreakdown) ? reportData.fileTypeBreakdown : (Array.isArray(reportData.FileTypeBreakdown) ? reportData.FileTypeBreakdown : []);

    return (
        <div className="space-y-6" dir="rtl">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <Card className="border border-border/40 shadow-sm">
                    <CardContent className="pt-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-xs text-muted-foreground font-medium">إجمالي مساحة التخزين</p>
                                <p className="text-2xl font-bold tracking-tight">{formatBytes(totalStorageBytes)}</p>
                            </div>
                            <div className="p-3 rounded-xl bg-primary/10 text-primary">
                                <HardDrive className="w-5 h-5" />
                            </div>
                        </div>
                    </CardContent>
                </Card>
                <Card className="border border-border/40 shadow-sm">
                    <CardContent className="pt-6">
                        <div className="flex items-center justify-between">
                            <div>
                                <p className="text-xs text-muted-foreground font-medium">إجمالي الملفات</p>
                                <p className="text-2xl font-bold tracking-tight">{totalFiles.toLocaleString()}</p>
                            </div>
                            <div className="p-3 rounded-xl bg-primary/10 text-primary">
                                <FileText className="w-5 h-5" />
                            </div>
                        </div>
                    </CardContent>
                </Card>
            </div>

            {fileTypeBreakdown.length > 0 && (
                <Card className="border border-border/40 shadow-sm">
                    <CardHeader>
                        <CardTitle className="text-sm font-semibold">توزيع الملفات حسب النوع</CardTitle>
                        <CardDescription>النسبة المئوية لكل نوع من الملفات المخزنة في النظام</CardDescription>
                    </CardHeader>
                    <CardContent>
                        <StorageChart fileTypeBreakdown={fileTypeBreakdown} />
                    </CardContent>
                </Card>
            )}

            <Card className="border border-border/40 shadow-sm">
                <CardHeader>
                    <CardTitle className="text-sm font-semibold">التخزين لكل مستخدم</CardTitle>
                    <CardDescription>مساحة التخزين المستخدمة لكل مستخدم ونسبة الاستخدام</CardDescription>
                </CardHeader>
                <CardContent className="p-0">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead className="text-right">المستخدم</TableHead>
                                <TableHead className="text-right">الملفات</TableHead>
                                <TableHead className="text-right">المساحة</TableHead>
                                <TableHead className="text-right">النسبة المئوية</TableHead>
                                <TableHead className="text-right">آخر إضافة</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {perUser.map((u) => (
                                <TableRow key={u.userId}>
                                    <TableCell className="font-medium">{userNames.get(u.userId) || u.userName}</TableCell>
                                    <TableCell>{u.totalFiles}</TableCell>
                                    <TableCell>{formatBytes(u.totalBytes)}</TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            <Progress value={u.percentageOfTotal} className="h-2 w-20" />
                                            <span className="text-xs text-muted-foreground">{u.percentageOfTotal.toFixed(1)}%</span>
                                        </div>
                                    </TableCell>
                                    <TableCell className="text-xs text-muted-foreground">
                                        {u.lastFileAddedAt
                                            ? new Date(u.lastFileAddedAt).toLocaleDateString('ar-SY')
                                            : '-'}
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>

            <Card className="border border-border/40 shadow-sm">
                <CardHeader>
                    <CardTitle className="text-sm font-semibold">توزيع الملفات حسب الامتداد</CardTitle>
                    <CardDescription>تفاصيل الملفات المخزنة حسب الامتداد مع العدد والمساحة</CardDescription>
                </CardHeader>
                <CardContent className="p-0">
                    <Table>
                        <TableHeader>
                            <TableRow>
                                <TableHead className="text-right">الامتداد</TableHead>
                                <TableHead className="text-right">العدد</TableHead>
                                <TableHead className="text-right">المساحة</TableHead>
                                <TableHead className="text-right">النسبة المئوية</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {fileTypeBreakdown.map((t) => (
                                <TableRow key={t.extension}>
                                    <TableCell className="font-medium">.{t.extension}</TableCell>
                                    <TableCell>{t.count}</TableCell>
                                    <TableCell>{formatBytes(t.totalBytes)}</TableCell>
                                    <TableCell>
                                        <div className="flex items-center gap-2">
                                            <Progress value={t.percentageOfTotal} className="h-2 w-20" />
                                            <span className="text-xs text-muted-foreground">{t.percentageOfTotal.toFixed(1)}%</span>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </CardContent>
            </Card>
        </div>
    );
}
