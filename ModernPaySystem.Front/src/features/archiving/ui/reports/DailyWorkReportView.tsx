import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Badge } from '@/shared/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { ChevronDown, ChevronLeft, FileText, History, Loader2 } from 'lucide-react';
import type { DailyWorkReportDto } from '../../model/types';

interface DailyWorkReportViewProps {
    data: DailyWorkReportDto | undefined;
    isLoading: boolean;
}

function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('ar-SY', {
        year: 'numeric', month: 'long', day: 'numeric',
    });
}

function formatDateTime(dateStr: string | null | undefined): string {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleString('ar-SY', {
        year: 'numeric', month: '2-digit', day: '2-digit',
        hour: '2-digit', minute: '2-digit',
    });
}

const ACTION_BADGE: Record<string, string> = {
    View: 'bg-blue-500/10 text-blue-600 border-blue-200 dark:border-blue-800',
    Create: 'bg-emerald-500/10 text-emerald-600 border-emerald-200 dark:border-emerald-800',
    Update: 'bg-amber-500/10 text-amber-600 border-amber-200 dark:border-amber-800',
    Delete: 'bg-red-500/10 text-red-600 border-red-200 dark:border-red-800',
    Download: 'bg-indigo-500/10 text-indigo-600 border-indigo-200 dark:border-indigo-800',
    Print: 'bg-purple-500/10 text-purple-600 border-purple-200 dark:border-purple-800',
    AddFiles: 'bg-teal-500/10 text-teal-600 border-teal-200 dark:border-teal-800',
    RemoveFiles: 'bg-orange-500/10 text-orange-600 border-orange-200 dark:border-orange-800',
};

const ACTION_LABEL: Record<string, string> = {
    View: 'عرض',
    Create: 'إنشاء',
    Update: 'تحديث',
    Delete: 'حذف',
    Download: 'تحميل',
    Print: 'طباعة',
    AddFiles: 'إضافة ملفات',
    RemoveFiles: 'حذف ملفات',
};

export function DailyWorkReportView({ data, isLoading }: DailyWorkReportViewProps) {
    const [expandedRecords, setExpandedRecords] = useState<Set<string>>(new Set());

    const toggleRecord = (id: string) => {
        setExpandedRecords(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

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
                    اختر تاريخاً لعرض التقرير اليومي المفصل
                </CardContent>
            </Card>
        );
    }

    return (
        <div className="space-y-6" dir="rtl">
            <Card className="border border-border/40 shadow-sm">
                <CardHeader>
                    <div className="flex items-center gap-2">
                        <History className="w-5 h-5 text-primary" />
                        <CardTitle className="text-lg font-semibold">
                            التقرير اليومي المفصل - {formatDate(data.date)}
                        </CardTitle>
                        <Badge variant="outline" className="mr-2">{data.departmentName}</Badge>
                    </div>
                </CardHeader>
            </Card>

            {/* Audit Logs Section */}
            <Card className="border border-border/40 shadow-sm">
                <CardHeader>
                    <CardTitle className="text-sm font-semibold flex items-center gap-2">
                        <History className="w-4 h-4" />
                        سجل النشاطات
                        <Badge variant="secondary" className="mr-2">{data.auditLogs.length}</Badge>
                    </CardTitle>
                </CardHeader>
                <CardContent className="p-0">
                    {data.auditLogs.length === 0 ? (
                        <div className="p-6 text-center text-muted-foreground text-sm">
                            لا توجد نشاطات في هذا التاريخ
                        </div>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead className="text-right w-10">#</TableHead>
                                    <TableHead className="text-right">رقم السجل</TableHead>
                                    <TableHead className="text-right">المستخدم</TableHead>
                                    <TableHead className="text-right">الإجراء</TableHead>
                                    <TableHead className="text-right">التفاصيل</TableHead>
                                    <TableHead className="text-right">الوقت</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {data.auditLogs.map((log, idx) => (
                                    <TableRow key={log.id}>
                                        <TableCell className="text-muted-foreground">{idx + 1}</TableCell>
                                        <TableCell className="font-medium">{log.archivalNumber}</TableCell>
                                        <TableCell>{log.userName}</TableCell>
                                        <TableCell>
                                            <Badge variant="outline" className={ACTION_BADGE[log.action] || ''}>
                                                {ACTION_LABEL[log.action] || log.action}
                                            </Badge>
                                        </TableCell>
                                        <TableCell className="max-w-xs truncate">{log.details || '-'}</TableCell>
                                        <TableCell>{formatDateTime(log.timestamp)}</TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>

            {/* Archive Records Section */}
            <Card className="border border-border/40 shadow-sm">
                <CardHeader>
                    <CardTitle className="text-sm font-semibold flex items-center gap-2">
                        <FileText className="w-4 h-4" />
                        سجلات الأرشيف
                        <Badge variant="secondary" className="mr-2">{data.archiveRecords.length}</Badge>
                    </CardTitle>
                </CardHeader>
                <CardContent className="p-0">
                    {data.archiveRecords.length === 0 ? (
                        <div className="p-6 text-center text-muted-foreground text-sm">
                            لا توجد سجلات أرشيف في هذا التاريخ
                        </div>
                    ) : (
                        <Table>
                            <TableHeader>
                                <TableRow>
                                    <TableHead className="text-right w-10">#</TableHead>
                                    <TableHead className="text-right">رقم السجل</TableHead>
                                    <TableHead className="text-right">المسار</TableHead>
                                    <TableHead className="text-right">النموذج</TableHead>
                                    <TableHead className="text-right">المستخدم</TableHead>
                                    <TableHead className="text-right">تاريخ الإنشاء</TableHead>
                                    <TableHead className="text-right w-8"></TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {data.archiveRecords.map((rec, idx) => (
                                    <>
                                        <TableRow
                                            key={rec.id}
                                            className="cursor-pointer hover:bg-muted/50"
                                            onClick={() => toggleRecord(rec.id)}
                                        >
                                            <TableCell className="text-muted-foreground">{idx + 1}</TableCell>
                                            <TableCell className="font-medium">{rec.archivalNumber}</TableCell>
                                            <TableCell className="max-w-xs truncate">{rec.folderPath || '-'}</TableCell>
                                            <TableCell>{rec.formName || '-'}</TableCell>
                                            <TableCell>{rec.createdByUserName || '-'}</TableCell>
                                            <TableCell>{formatDateTime(rec.createdAt)}</TableCell>
                                            <TableCell>
                                                {expandedRecords.has(rec.id) ? (
                                                    <ChevronDown className="w-4 h-4 text-muted-foreground" />
                                                ) : (
                                                    <ChevronLeft className="w-4 h-4 text-muted-foreground" />
                                                )}
                                            </TableCell>
                                        </TableRow>
                                        {expandedRecords.has(rec.id) && (
                                            <TableRow key={`${rec.id}-form`}>
                                                <TableCell colSpan={7} className="bg-muted/20 p-4">
                                                    <div className="space-y-2">
                                                        <h4 className="text-xs font-bold text-muted-foreground uppercase tracking-wider">
                                                            بيانات الحقول
                                                        </h4>
                                                        {rec.formValues.length === 0 ? (
                                                            <p className="text-sm text-muted-foreground">لا توجد بيانات حقول</p>
                                                        ) : (
                                                            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
                                                                {rec.formValues.map((fv, i) => (
                                                                    <div key={i} className="flex gap-2 text-sm bg-background rounded-lg p-2 border">
                                                                        <span className="font-semibold text-primary whitespace-nowrap">{fv.key}:</span>
                                                                        <span className="text-muted-foreground">{fv.value || '-'}</span>
                                                                    </div>
                                                                ))}
                                                            </div>
                                                        )}
                                                    </div>
                                                </TableCell>
                                            </TableRow>
                                        )}
                                    </>
                                ))}
                            </TableBody>
                        </Table>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
