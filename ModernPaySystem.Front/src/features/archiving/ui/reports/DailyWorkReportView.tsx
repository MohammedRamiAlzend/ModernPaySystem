import { useState, Fragment } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui/card';
import { Badge } from '@/shared/ui/badge';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Button } from '@/shared/ui/button';
import { ChevronDown, ChevronLeft, ChevronRight, FileText, History, Loader2 } from 'lucide-react';
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

function translateDetails(details: string | null | undefined): string {
    if (!details) return '-';

    const mappings: Record<string, string> = {
        'Viewed archive record': 'عرض السجل الأرشيفي',
        'Created archive record': 'إنشاء السجل الأرشيفي',
        'Updated archive record': 'تحديث السجل الأرشيفي',
        'Deleted archive record': 'حذف السجل الأرشيفي',
        'Downloaded archive record as ZIP': 'تحميل السجل الأرشيفي كملف ZIP',
        'Printed archive record': 'طباعة السجل الأرشيفي',
        'Submitted delete request for archive record': 'تقديم طلب حذف سجل أرشيفي',
        'Approved delete request for archive record': 'الموافقة على طلب حذف سجل أرشيفي',
        'Rejected delete request for archive record': 'رفض طلب حذف سجل أرشيفي',
        'Submitted edit request for archive record': 'تقديم طلب تعديل سجل أرشيفي',
        'Approved edit request for archive record': 'الموافقة على طلب تعديل سجل أرشيفي',
        'Rejected edit request for archive record': 'رفض طلب تعديل سجل أرشيفي',
        'Downloaded archive record file': 'تحميل ملف من السجل الأرشيفي',
        'Viewed archive record file': 'عرض ملف من السجل الأرشيفي',
    };

    if (mappings[details]) {
        return mappings[details];
    }

    let match;

    match = details.match(/Moved from folder '(.*)' to folder '(.*)'/);
    if (match) {
        return `نُقل من المجلد "${match[1]}" إلى المجلد "${match[2]}"`;
    }

    match = details.match(/Added (\d+) file\(s\) to archive record/);
    if (match) {
        return `إضافة ${match[1]} ملف(ات) إلى السجل الأرشيفي`;
    }

    match = details.match(/Removed file '(.*)' from archive record/);
    if (match) {
        return `إزالة الملف "${match[1]}" من السجل الأرشيفي`;
    }

    match = details.match(/Downloaded file: (.*)/);
    if (match) {
        return `تحميل الملف: ${match[1]}`;
    }

    match = details.match(/Viewed file: (.*)/);
    if (match) {
        return `عرض الملف: ${match[1]}`;
    }

    match = details.match(/Approved delete request for archive record: (.*)/);
    if (match) {
        return `الموافقة على طلب حذف سجل أرشيفي: ${match[1]}`;
    }

    match = details.match(/Rejected delete request for archive record: (.*)/);
    if (match) {
        return `رفض طلب حذف سجل أرشيفي: ${match[1]}`;
    }

    match = details.match(/Approved edit request for archive record: (.*)/);
    if (match) {
        return `الموافقة على طلب تعديل سجل أرشيفي: ${match[1]}`;
    }

    match = details.match(/Rejected edit request for archive record: (.*)/);
    if (match) {
        return `رفض طلب تعديل سجل أرشيفي: ${match[1]}`;
    }

    return details;
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
    ApproveEdit: 'bg-emerald-500/10 text-emerald-600 border-emerald-200 dark:border-emerald-800',
    RejectEdit: 'bg-red-500/10 text-red-600 border-red-200 dark:border-red-800',
    ApproveDelete: 'bg-emerald-500/10 text-emerald-600 border-emerald-200 dark:border-emerald-800',
    RejectDelete: 'bg-red-500/10 text-red-600 border-red-200 dark:border-red-800',
    SubmitEditRequest: 'bg-sky-500/10 text-sky-600 border-sky-200 dark:border-sky-800',
    SubmitDeleteRequest: 'bg-orange-500/10 text-orange-600 border-orange-200 dark:border-orange-800',
    Move: 'bg-violet-500/10 text-violet-600 border-violet-200 dark:border-violet-800',
};

const ACTION_LABEL: Record<string, string> = {
    View: 'عرض مجلد',
    Create: 'إنشاء مجلد',
    Update: 'تحديث بيانات',
    Delete: 'حذف',
    Download: 'تنزيل ملف',
    Print: 'طباعة ملف',
    AddFiles: 'إضافة ملفات لل',
    RemoveFiles: 'حذف ملفات من ا',
    ApproveEdit: 'الموافقة على طلب تعديل',
    RejectEdit: 'رفض طلب تعديل',
    ApproveDelete: 'الموافقة على طلب حذف',
    RejectDelete: 'رفض طلب حذف',
    SubmitEditRequest: 'طلب تعديل',
    SubmitDeleteRequest: 'طلب حذف',
    Move: 'نقل مجلد',
};

export function DailyWorkReportView({ data, isLoading }: DailyWorkReportViewProps) {
    const [expandedRecords, setExpandedRecords] = useState<Set<string>>(new Set());
    const [recordsPage, setRecordsPage] = useState(1);
    const recordsPerPage = 10;
    const [auditPage, setAuditPage] = useState(1);
    const auditPerPage = 10;

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

    const totalRecords = data.archiveRecords.length;
    const totalRecordPages = Math.ceil(totalRecords / recordsPerPage);
    const paginatedRecords = data.archiveRecords.slice(
        (recordsPage - 1) * recordsPerPage,
        recordsPage * recordsPerPage
    );

    const totalAuditLogs = data.auditLogs.length;
    const totalAuditPages = Math.ceil(totalAuditLogs / auditPerPage);
    const paginatedAuditLogs = data.auditLogs.slice(
        (auditPage - 1) * auditPerPage,
        auditPage * auditPerPage
    );

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
                    {totalAuditLogs === 0 ? (
                        <div className="p-6 text-center text-muted-foreground text-sm">
                            لا توجد نشاطات في هذا التاريخ
                        </div>
                    ) : (
                        <>
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
                                    {paginatedAuditLogs.map((log, idx) => {
                                        const globalIdx = (auditPage - 1) * auditPerPage + idx;
                                        return (
                                            <TableRow key={log.id}>
                                                <TableCell className="text-muted-foreground">{globalIdx + 1}</TableCell>
                                                <TableCell className="font-medium">{log.id.slice(0, 8)}</TableCell>
                                                <TableCell>{log.userName}</TableCell>
                                                <TableCell>
                                                    <Badge variant="outline" className={ACTION_BADGE[log.action] || ''}>
                                                        {ACTION_LABEL[log.action] || log.action}
                                                    </Badge>
                                                </TableCell>
                                                <TableCell className="max-w-xs truncate">{translateDetails(log.details)}</TableCell>
                                                <TableCell>{formatDateTime(log.timestamp)}</TableCell>
                                            </TableRow>
                                        );
                                    })}
                                </TableBody>
                            </Table>

                            {totalAuditLogs > 0 && (
                                <div className="flex justify-between items-center px-6 py-4 border-t border-border">
                                    <span className="text-sm text-muted-foreground">
                                        عرض صفحة {auditPage} من {totalAuditPages} (إجمالي {totalAuditLogs} سجل)
                                    </span>
                                    <div className="flex gap-2">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setAuditPage(p => Math.min(p + 1, totalAuditPages))}
                                            disabled={auditPage === totalAuditPages}
                                            className="flex items-center gap-1"
                                        >
                                            <ChevronRight className="w-4 h-4" />
                                            <span>التالي</span>
                                        </Button>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setAuditPage(p => Math.max(p - 1, 1))}
                                            disabled={auditPage === 1}
                                            className="flex items-center gap-1"
                                        >
                                            <span>السابق</span>
                                            <ChevronLeft className="w-4 h-4" />
                                        </Button>
                                    </div>
                                </div>
                            )}
                        </>
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
                    {totalRecords === 0 ? (
                        <div className="p-6 text-center text-muted-foreground text-sm">
                            لا توجد سجلات أرشيف في هذا التاريخ
                        </div>
                    ) : (
                        <>
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
                                    {paginatedRecords.map((rec, idx) => {
                                        const globalIdx = (recordsPage - 1) * recordsPerPage + idx;
                                        return (
                                            <Fragment key={rec.id}>
                                                <TableRow
                                                    className="cursor-pointer hover:bg-muted/50"
                                                    onClick={() => toggleRecord(rec.id)}
                                                >
                                                    <TableCell className="text-muted-foreground">{globalIdx + 1}</TableCell>
                                                    <TableCell className="font-medium">{rec.id.slice(0, 8)}</TableCell>
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
                                            </Fragment>
                                        );
                                    })}
                                </TableBody>
                            </Table>

                            {totalRecords > 0 && (
                                <div className="flex justify-between items-center px-6 py-4 border-t border-border">
                                    <span className="text-sm text-muted-foreground">
                                        عرض صفحة {recordsPage} من {totalRecordPages} (إجمالي {totalRecords} سجل)
                                    </span>
                                    <div className="flex gap-2">
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setRecordsPage(p => Math.min(p + 1, totalRecordPages))}
                                            disabled={recordsPage === totalRecordPages}
                                            className="flex items-center gap-1"
                                        >
                                            <ChevronRight className="w-4 h-4" />
                                            <span>التالي</span>
                                        </Button>
                                        <Button
                                            variant="outline"
                                            size="sm"
                                            onClick={() => setRecordsPage(p => Math.max(p - 1, 1))}
                                            disabled={recordsPage === 1}
                                            className="flex items-center gap-1"
                                        >
                                            <span>السابق</span>
                                            <ChevronLeft className="w-4 h-4" />
                                        </Button>
                                    </div>
                                </div>
                            )}
                        </>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
