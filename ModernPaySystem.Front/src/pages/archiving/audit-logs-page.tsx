import { useState } from 'react';
import { Card, CardTitle, CardContent } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import { Badge } from '@/shared/ui/badge';
import { useLedDepartments, useArchiveAuditLogs, useArchiveRecord, useAllDynamicForms } from '@/features/archiving/model/queries';
import { useUsers } from '@/entities/user/api/userEndpoints';
import { AuditAction, AUDIT_ACTION_LABELS } from '@/features/archiving/model/types';
import { DocumentGalleryModal } from '@/features/archiving/ui/DocumentGalleryModal';
import { 
    Calendar, 
    ShieldAlert, 
    Download, 
    Printer, 
    Eye, 
    Trash2, 
    Plus, 
    Edit, 
    FileUp, 
    FileDown,
    Loader2,
    RefreshCw,
    ChevronLeft,
    ChevronRight
} from 'lucide-react';

export default function AuditLogsPage() {
    const [departmentId, setDepartmentId] = useState<string>('');
    const [action, setAction] = useState<number | null>(null);
    const [fromDate, setFromDate] = useState<string>('');
    const [toDate, setToDate] = useState<string>('');
    const [page, setPage] = useState(1);
    const pageSize = 10;

    const [activePreviewRecordId, setActivePreviewRecordId] = useState<string | null>(null);

    // Queries
    const { data: departments = [], isLoading: isLoadingDeps } = useLedDepartments();
    const { data: users = [] } = useUsers();
    const { data: previewRecord, isLoading: isLoadingPreviewRecord } = useArchiveRecord(activePreviewRecordId);
    const { data: dynamicTemplates = [] } = useAllDynamicForms();

    // Set default department
    const [prevDepsLength, setPrevDepsLength] = useState(0);
    if (departments.length !== prevDepsLength) {
        setPrevDepsLength(departments.length);
        if (departments.length > 0 && !departmentId) {
            setDepartmentId(departments[0].id);
        }
    }

    const { data, isLoading: isLoadingLogs, isFetching, refetch } = useArchiveAuditLogs({
        page,
        pageSize,
        action,
        fromDate: fromDate ? new Date(fromDate).toISOString() : null,
        toDate: toDate ? new Date(toDate).toISOString() : null,
        departmentId: departmentId || null,
    });

    const logs = data?.items || [];
    const totalItems = data?.totalItems || 0;
    const totalPages = Math.ceil(totalItems / pageSize);

    // Map user ID to user name
    const getUserName = (userId: string) => {
        const user = users.find(u => u.id === userId || u.userName === userId);
        return user ? user.userName : userId;
    };

    // Style for action badges
    const getActionBadge = (action: AuditAction) => {
        switch (action) {
            case AuditAction.View:
                return (
                    <Badge className="bg-blue-500/10 text-blue-500 hover:bg-blue-500/20 border-blue-500/20 flex items-center gap-1 w-fit">
                        <Eye className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            case AuditAction.Create:
                return (
                    <Badge className="bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500/20 border-emerald-500/20 flex items-center gap-1 w-fit">
                        <Plus className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            case AuditAction.Update:
                return (
                    <Badge className="bg-amber-500/10 text-amber-500 hover:bg-amber-500/20 border-amber-500/20 flex items-center gap-1 w-fit">
                        <Edit className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            case AuditAction.Delete:
                return (
                    <Badge className="bg-red-500/10 text-red-500 hover:bg-red-500/20 border-red-500/20 flex items-center gap-1 w-fit">
                        <Trash2 className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            case AuditAction.Download:
                return (
                    <Badge className="bg-indigo-500/10 text-indigo-500 hover:bg-indigo-500/20 border-indigo-500/20 flex items-center gap-1 w-fit">
                        <Download className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            case AuditAction.Print:
                return (
                    <Badge className="bg-purple-500/10 text-purple-500 hover:bg-purple-500/20 border-purple-500/20 flex items-center gap-1 w-fit">
                        <Printer className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            case AuditAction.AddFiles:
                return (
                    <Badge className="bg-teal-500/10 text-teal-500 hover:bg-teal-500/20 border-teal-500/20 flex items-center gap-1 w-fit">
                        <FileUp className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            case AuditAction.RemoveFiles:
                return (
                    <Badge className="bg-rose-500/10 text-rose-500 hover:bg-rose-500/20 border-rose-500/20 flex items-center gap-1 w-fit">
                        <FileDown className="w-3.5 h-3.5" />
                        <span>{AUDIT_ACTION_LABELS[action]}</span>
                    </Badge>
                );
            default:
                return <Badge variant="secondary">{AUDIT_ACTION_LABELS[action] || action}</Badge>;
        }
    };

    if (isLoadingDeps) {
        return (
            <div className="flex h-[400px] items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
            </div>
        );
    }

    if (departments.length === 0) {
        return (
            <Card className="max-w-2xl mx-auto my-12 border-dashed border-red-200/50 bg-red-50/5 dark:bg-red-950/5">
                <CardContent className="pt-8 text-center flex flex-col items-center">
                    <div className="p-3 rounded-full bg-red-100 dark:bg-red-950/50 text-red-500 mb-4">
                        <ShieldAlert className="w-12 h-12" />
                    </div>
                    <CardTitle className="text-xl mb-2 text-foreground font-semibold">غير مصرح</CardTitle>
                    <p className="text-muted-foreground text-sm max-w-md">
                        عذراً، يجب أن تكون مسؤول أرشيف (Archive Leader) لقسم واحد على الأقل لعرض سجلات النشاط.
                    </p>
                </CardContent>
            </Card>
        );
    }

    return (
        <div className="space-y-6 max-w-7xl mx-auto px-4 py-6" dir="rtl">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold tracking-tight text-foreground">سجلات النشاط (Audit Logs)</h1>
                    <p className="text-sm text-muted-foreground mt-1">
                        تتبع عمليات الوصول والتحميل والطباعة ومختلف التغييرات على سجلات الأرشيف.
                    </p>
                </div>
                <Button 
                    variant="outline" 
                    size="sm" 
                    onClick={() => refetch()}
                    disabled={isFetching}
                >
                    <RefreshCw className={`w-4 h-4 ml-2 ${isFetching ? 'animate-spin' : ''}`} />
                    <span>تحديث</span>
                </Button>
            </div>

            {/* Filters */}
            <Card className="border border-border/40 bg-card/60 backdrop-blur-sm shadow-md">
                <CardContent className="pt-6">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                        {/* Department Select */}
                        <div className="space-y-1.5">
                            <label className="text-xs font-semibold text-muted-foreground">القسم</label>
                            <select
                                className="w-full h-10 px-3 py-2 bg-background border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent text-right"
                                value={departmentId}
                                onChange={(e) => {
                                    setDepartmentId(e.target.value);
                                    setPage(1);
                                }}
                            >
                                {departments.map((d) => (
                                    <option key={d.id} value={d.id}>
                                        {d.name}
                                    </option>
                                ))}
                            </select>
                        </div>

                        {/* Action Select */}
                        <div className="space-y-1.5">
                            <label className="text-xs font-semibold text-muted-foreground">العملية / الإجراء</label>
                            <select
                                className="w-full h-10 px-3 py-2 bg-background border border-input rounded-md text-sm focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent text-right"
                                value={action ?? ''}
                                onChange={(e) => {
                                    const val = e.target.value;
                                    setAction(val ? Number(val) : null);
                                    setPage(1);
                                }}
                            >
                                <option value="">كل العمليات</option>
                                <option value={AuditAction.View}>عرض</option>
                                <option value={AuditAction.Create}>إنشاء</option>
                                <option value={AuditAction.Update}>تعديل</option>
                                <option value={AuditAction.Delete}>حذف</option>
                                <option value={AuditAction.Download}>تنزيل</option>
                                <option value={AuditAction.Print}>طباعة</option>
                                <option value={AuditAction.AddFiles}>إضافة ملفات</option>
                                <option value={AuditAction.RemoveFiles}>حذف ملفات</option>
                            </select>
                        </div>

                        {/* From Date */}
                        <div className="space-y-1.5">
                            <label className="text-xs font-semibold text-muted-foreground">من تاريخ</label>
                            <div className="relative">
                                <Input
                                    type="date"
                                    className="w-full pl-9 text-right"
                                    value={fromDate}
                                    onChange={(e) => {
                                        setFromDate(e.target.value);
                                        setPage(1);
                                    }}
                                />
                                <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                            </div>
                        </div>

                        {/* To Date */}
                        <div className="space-y-1.5">
                            <label className="text-xs font-semibold text-muted-foreground">إلى تاريخ</label>
                            <div className="relative">
                                <Input
                                    type="date"
                                    className="w-full pl-9 text-right"
                                    value={toDate}
                                    onChange={(e) => {
                                        setToDate(e.target.value);
                                        setPage(1);
                                    }}
                                />
                                <Calendar className="absolute left-3 top-3 h-4 w-4 text-muted-foreground pointer-events-none" />
                            </div>
                        </div>
                    </div>
                </CardContent>
            </Card>

            {/* Audit Logs Table */}
            <Card className="border border-border/40 shadow-md">
                <CardContent className="p-0">
                    <Table>
                        <TableHeader>
                            <TableRow className="hover:bg-transparent">
                                <TableHead className="text-right font-bold w-[120px]">نوع العملية</TableHead>
                                <TableHead className="text-right font-bold w-[180px]">الوقت</TableHead>
                                <TableHead className="text-right font-bold w-[180px]">المستخدم</TableHead>
                                <TableHead className="text-right font-bold">التفاصيل</TableHead>
                                <TableHead className="text-right font-bold w-[120px]">المستند</TableHead>
                                <TableHead className="text-right font-bold w-[140px]">عنوان IP</TableHead>
                                <TableHead className="text-right font-bold w-[200px]">متصفح المستخدم</TableHead>
                            </TableRow>
                        </TableHeader>
                        <TableBody>
                            {isLoadingLogs ? (
                                <TableRow>
                                    <TableCell colSpan={6} className="h-64 text-center">
                                        <div className="flex items-center justify-center gap-2">
                                            <Loader2 className="h-6 w-6 animate-spin text-primary" />
                                            <span className="text-muted-foreground text-sm">جاري تحميل السجلات...</span>
                                        </div>
                                    </TableCell>
                                </TableRow>
                            ) : logs.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={6} className="h-48 text-center text-muted-foreground text-sm">
                                        لا توجد سجلات نشاط تطابق خيارات التصفية المحددة.
                                    </TableCell>
                                </TableRow>
                            ) : (
                                logs.map((log) => (
                                    <TableRow key={log.id} className="hover:bg-muted/40 transition-colors">
                                        <TableCell className="py-3.5">{getActionBadge(log.action)}</TableCell>
                                        <TableCell className="text-muted-foreground text-sm py-3.5">
                                            {new Date(log.timestamp).toLocaleString('ar-SY', {
                                                year: 'numeric',
                                                month: '2-digit',
                                                day: '2-digit',
                                                hour: '2-digit',
                                                minute: '2-digit',
                                                second: '2-digit'
                                            })}
                                        </TableCell>
                                        <TableCell className="font-medium text-sm py-3.5">
                                            {getUserName(log.userId)}
                                        </TableCell>
                                        <TableCell className="text-sm py-3.5">
                                            <span className="text-foreground">{log.details || '-'}</span>
                                        </TableCell>
                                        <TableCell className="py-3.5">
                                            {log.archiveRecordId && log.archiveRecordId !== '00000000-0000-0000-0000-000000000000' ? (
                                                <Button
                                                    variant="outline"
                                                    size="sm"
                                                    onClick={() => setActivePreviewRecordId(log.archiveRecordId)}
                                                    className="flex items-center gap-1.5 h-8 px-2.5 border-primary/20 hover:border-primary text-primary bg-primary/5 hover:bg-primary/10 transition-colors"
                                                >
                                                    <Eye className="w-3.5 h-3.5" />
                                                    <span>معاينة</span>
                                                </Button>
                                            ) : (
                                                <span className="text-muted-foreground">-</span>
                                            )}
                                        </TableCell>
                                        <TableCell className="text-muted-foreground text-sm font-mono py-3.5">
                                            {log.ipAddress || '-'}
                                        </TableCell>
                                        <TableCell className="text-muted-foreground text-xs truncate max-w-[200px] py-3.5" title={log.userAgent || ''}>
                                            {log.userAgent || '-'}
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>

                    {/* Pagination Footer */}
                    {totalPages > 1 && (
                        <div className="flex justify-between items-center px-6 py-4 border-t border-border">
                            <span className="text-sm text-muted-foreground">
                                عرض صفحة {page} من {totalPages} (إجمالي {totalItems} سجل)
                            </span>
                            <div className="flex gap-2">
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setPage(p => Math.min(p + 1, totalPages))}
                                    disabled={page === totalPages || isLoadingLogs}
                                    className="flex items-center gap-1"
                                >
                                    <ChevronRight className="w-4 h-4" />
                                    <span>التالي</span>
                                </Button>
                                <Button
                                    variant="outline"
                                    size="sm"
                                    onClick={() => setPage(p => Math.max(p - 1, 1))}
                                    disabled={page === 1 || isLoadingLogs}
                                    className="flex items-center gap-1"
                                >
                                    <span>السابق</span>
                                    <ChevronLeft className="w-4 h-4" />
                                </Button>
                            </div>
                        </div>
                    )}
                </CardContent>
            </Card>

            {/* Document Gallery Modal Preview */}
            {activePreviewRecordId && previewRecord && (
                <DocumentGalleryModal
                    record={previewRecord}
                    dynamicTemplates={dynamicTemplates}
                    onClose={() => setActivePreviewRecordId(null)}
                />
            )}

            {/* Loading Overlay */}
            {isLoadingPreviewRecord && (
                <div className="fixed inset-0 bg-black/40 backdrop-blur-xs flex items-center justify-center z-[100]">
                    <div className="bg-card p-6 rounded-2xl shadow-xl flex items-center gap-3 border border-border">
                        <Loader2 className="h-6 w-6 animate-spin text-primary" />
                        <span className="text-sm font-semibold">جاري تحميل المستند...</span>
                    </div>
                </div>
            )}
        </div>
    );
}
