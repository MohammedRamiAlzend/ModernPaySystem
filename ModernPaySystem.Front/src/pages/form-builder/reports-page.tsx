import React, { useState, useMemo } from 'react';
import { Card } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { useUIStore } from '@/app/store/uiStore';
import { useAuthStore } from '@/app/store/authStore';
import {
    BarChart3,
    Filter,
    Download,
    CheckCircle2,
    Clock,
    AlertCircle,
    Layers,
    ChevronLeft,
    ChevronRight,
    Users,
    FileSpreadsheet
} from 'lucide-react';
import {
    useRequestsReport,
    useResponsesReport,
    formEndpoints
} from '@/features/form-builder/api/formEndpoints';
import { useForms } from '@/features/form-builder/model/useForms';
import { generateReportPDF } from '@/shared/lib/pdf-generator';
import { UserDisplay } from '@/features/users/ui/UserDisplay';

export const ReportsPage: React.FC = () => {
    const { showStatus } = useUIStore();
    const currentUser = useAuthStore((state) => state.user);
    const { data: templates = [] } = useForms();

    // 1. Local Input States (for filter inputs)
    const [reportType, setReportType] = useState<'requests' | 'responses'>('requests');
    const [startDate, setStartDate] = useState<string>(() =>
        new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0] // Default to 30 days ago
    );
    const [endDate, setEndDate] = useState<string>(() =>
        new Date().toISOString().split('T')[0] // Default to today
    );
    const [forCurrentDepartment, setForCurrentDepartment] = useState<boolean>(false);

    // 2. Applied Filter States (drives the TanStack Query, prevents useEffect cascading renders)
    const [appliedFilters, setAppliedFilters] = useState(() => {
        const defaultStart = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];
        const defaultEnd = new Date().toISOString().split('T')[0];
        return {
            reportType: 'requests' as 'requests' | 'responses',
            startDate: defaultStart,
            endDate: defaultEnd,
            forCurrentDepartment: false,
        };
    });

    const [page, setPage] = useState<number>(1);
    const pageSize = 10;
    const [isGeneratingPdf, setIsGeneratingPdf] = useState(false);

    // Queries activation
    const isRequests = appliedFilters.reportType === 'requests';

    const {
        data: requestsData,
        isLoading: isRequestsLoading,
        isFetching: isRequestsFetching
    } = useRequestsReport(
        page,
        pageSize,
        appliedFilters.startDate,
        appliedFilters.endDate,
        appliedFilters.forCurrentDepartment,
        isRequests
    );

    const {
        data: responsesData,
        isLoading: isResponsesLoading,
        isFetching: isResponsesFetching
    } = useResponsesReport(
        page,
        pageSize,
        appliedFilters.startDate,
        appliedFilters.endDate,
        appliedFilters.forCurrentDepartment,
        !isRequests
    );

    // Dynamic derivation of loading/pagination state based on chosen report type
    const isLoading = isRequests ? isRequestsLoading || isRequestsFetching : isResponsesLoading || isResponsesFetching;
    const totalItems = isRequests ? requestsData?.totalItems || 0 : responsesData?.totalItems || 0;
    const totalPages = isRequests ? requestsData?.totalPages || 1 : responsesData?.totalPages || 1;

    // Handle filter application
    const handleApplyFilters = (e: React.FormEvent) => {
        e.preventDefault();

        if (startDate && endDate && new Date(startDate) > new Date(endDate)) {
            showStatus({
                type: 'warning',
                title: 'تاريخ غير صالح',
                message: 'يجب أن يكون تاريخ البدء أقدم من تاريخ الانتهاء.'
            });
            return;
        }

        setPage(1);
        setAppliedFilters({
            reportType,
            startDate,
            endDate,
            forCurrentDepartment
        });

        showStatus({
            type: 'info',
            title: 'تم تحديث التصفية',
            message: 'تم تطبيق خيارات الفلترة وتحديث المعاينة بنجاح.'
        });
    };

    // Quick calculations for statistics cards
    const stats = useMemo(() => {
        if (isRequests) {
            const items = requestsData?.items || [];
            const pendingCount = items.filter((i: any) => String(i.status) === '0').length;
            const processedCount = items.filter((i: any) => String(i.status) === '2').length;
            const completedCount = items.filter((i: any) => String(i.status) === '3' || String(i.status) === '1').length;
            return {
                total: totalItems,
                pending: pendingCount,
                processed: processedCount,
                completed: completedCount,
                attachments: items.reduce((sum: number, i: any) => sum + (i.requestAttachmentDtos?.length || 0), 0)
            };
        } else {
            const items = responsesData?.items || [];
            return {
                total: totalItems,
                attachments: items.reduce((sum: number, i: any) => sum + (i.responseAttachments?.length || 0), 0)
            };
        }
    }, [isRequests, requestsData, responsesData, totalItems]);

    // Handle PDF Generation (Background full-load fetch)
    const handleExportPdf = async () => {
        setIsGeneratingPdf(true);
        try {
            showStatus({
                type: 'info',
                title: 'جاري التحضير',
                message: 'جاري جلب كامل البيانات وتوليد التقرير بصيغة PDF...'
            });

            const title = appliedFilters.reportType === 'requests'
                ? 'تقرير المعاملات والطلبات المودعة'
                : 'تقرير الردود والإجراءات المنجزة';

            let items: any[] = [];

            if (appliedFilters.reportType === 'requests') {
                const response = await formEndpoints.getRequestsReport(
                    1,
                    100,
                    appliedFilters.startDate,
                    appliedFilters.endDate,
                    appliedFilters.forCurrentDepartment
                );

                const responseData = (response as any).data || response;
                const unwrappedData = responseData?.value || responseData;
                const itemsList = Array.isArray(unwrappedData) ? unwrappedData : (unwrappedData?.items || []);

                items = itemsList.map((item: any) => {
                    const templateName = templates.find(t => t.id === item.templateId)?.title || 'طلب خدمة';
                    let statusLabel = 'قيد الانتظار';
                    if (String(item.status) === '2') statusLabel = 'قيد المعالجة';
                    if (String(item.status) === '3') statusLabel = 'تمت إدارتها';
                    if (String(item.status) === '1') statusLabel = 'تم التسليم';

                    return {
                        id: item.id,
                        title: templateName,
                        ownerName: item.requester?.userName || 'غير معروف',
                        date: item.createdAt ? new Date(item.createdAt).toLocaleDateString('ar-EG') : '---',
                        status: statusLabel,
                        attachmentsCount: item.requestAttachmentDtos?.length || 0
                    };
                });
            } else {
                const response = await formEndpoints.getResponsesReport(
                    1,
                    100,
                    appliedFilters.startDate,
                    appliedFilters.endDate,
                    appliedFilters.forCurrentDepartment
                );

                const responseData = (response as any).data || response;
                const unwrappedData = responseData?.value || responseData;
                const itemsList = Array.isArray(unwrappedData) ? unwrappedData : (unwrappedData?.items || []);

                items = itemsList.map((item: any) => {
                    const templateName = templates.find(t => t.id === item.request?.templateId)?.title || 'الخدمة الأصلية';
                    return {
                        id: item.id,
                        title: templateName,
                        ownerName: item.request?.requester?.userName || 'غير معروف',
                        date: item.respondedAt || item.createdAt ? new Date(item.respondedAt || item.createdAt).toLocaleDateString('ar-EG') : '---',
                        status: 'مكتمل',
                        attachmentsCount: item.responseAttachments?.length || 0,
                        extraContent: item.comment || 'لا يوجد تعليق مضاف'
                    };
                });
            }

            if (items.length === 0) {
                showStatus({
                    type: 'warning',
                    title: 'تقرير فارغ',
                    message: 'لا توجد معاملات أو بيانات مطابقة للفترة المحددة لتوليد ملف PDF.'
                });
                return;
            }

            await generateReportPDF(title, items, {
                reportType: appliedFilters.reportType,
                startDate: appliedFilters.startDate,
                endDate: appliedFilters.endDate,
                generatedBy: (currentUser as any)?.fullName || (currentUser as any)?.userName || currentUser?.username || 'الموظف المسؤول',
                departmentName: (currentUser as any)?.departmentName || undefined
            });

            showStatus({
                type: 'success',
                title: 'تم التنزيل بنجاح',
                message: 'تم توليد وحفظ ملف التقرير بصيغة PDF بنجاح.'
            });
        } catch (error) {
            console.error('Error generating PDF:', error);
            showStatus({
                type: 'error',
                title: 'خطأ أثناء التوليد',
                message: 'حدث خطأ غير متوقع أثناء معالجة ملف الـ PDF. يرجى المحاولة لاحقاً.'
            });
        } finally {
            setIsGeneratingPdf(false);
        }
    };

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6 max-w-7xl" dir="rtl">

            {/* Header section (replaces flashy banners with standard enterprise look) */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b pb-6">
                <div className="space-y-1.5">
                    <h1 className="text-3xl font-bold flex items-center gap-2 text-foreground">
                        <BarChart3 className="w-8 h-8 text-primary" />
                        التقارير والإحصائيات
                    </h1>
                    <p className="text-muted-foreground text-sm">
                        توليد وتصدير التقارير الرسمية لمعاملات الطلبات والردود مصحوبة بالمؤشرات الإحصائية.
                    </p>
                </div>
                <div className="flex items-center gap-3">
                    <Button
                        onClick={handleExportPdf}
                        disabled={isLoading || isGeneratingPdf}
                        className="rounded-xl font-bold px-5 py-4 flex items-center gap-2 transition-all shadow-sm"
                    >
                        {isGeneratingPdf ? (
                            <div className="w-4 h-4 border-2 border-primary-foreground border-t-transparent rounded-full animate-spin" />
                        ) : (
                            <Download className="w-4 h-4" />
                        )}
                        {isGeneratingPdf ? 'جاري التحميل...' : 'تحميل كتقرير PDF'}
                    </Button>
                </div>
            </div>

            {/* Main Layout Grid */}
            <div className="grid grid-cols-1 lg:grid-cols-4 gap-6 items-start">

                {/* Right Column: Search Filters Panel */}
                <Card className="lg:col-span-1 p-6 border border-border shadow-sm rounded-2xl bg-card space-y-6">
                    <div className="flex items-center gap-2 border-b pb-4">
                        <Filter className="w-4 h-4 text-primary" />
                        <h2 className="text-sm font-bold text-foreground">خيارات الفلترة</h2>
                    </div>

                    <form onSubmit={handleApplyFilters} className="space-y-5">
                        {/* Report Type */}
                        <div className="space-y-2">
                            <label className="text-xs font-bold text-muted-foreground">نوع التقرير</label>
                            <select
                                value={reportType}
                                onChange={(e) => setReportType(e.target.value as any)}
                                className="w-full rounded-xl border border-input bg-background px-3 py-2.5 text-sm font-bold text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary"
                            >
                                <option value="requests">تقرير الطلبات والمعاملات</option>
                                <option value="responses">تقرير الردود والإجراءات</option>
                            </select>
                        </div>

                        {/* Start Date */}
                        <div className="space-y-2">
                            <label className="text-xs font-bold text-muted-foreground">تاريخ البدء</label>
                            <input
                                type="date"
                                value={startDate}
                                onChange={(e) => setStartDate(e.target.value)}
                                className="w-full rounded-xl border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary font-bold"
                                dir="ltr"
                            />
                        </div>

                        {/* End Date */}
                        <div className="space-y-2">
                            <label className="text-xs font-bold text-muted-foreground">تاريخ الانتهاء</label>
                            <input
                                type="date"
                                value={endDate}
                                onChange={(e) => setEndDate(e.target.value)}
                                className="w-full rounded-xl border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary font-bold"
                                dir="ltr"
                            />
                        </div>

                        {/* Department check */}
                        <div className="flex items-center gap-2 py-2">
                            <input
                                id="dept-filter"
                                type="checkbox"
                                checked={forCurrentDepartment}
                                onChange={(e) => setForCurrentDepartment(e.target.checked)}
                                className="w-4 h-4 rounded border-input text-primary focus:ring-primary cursor-pointer"
                            />
                            <label htmlFor="dept-filter" className="text-xs font-bold text-foreground cursor-pointer select-none">
                                تصفية للقسم المعني فقط
                            </label>
                        </div>

                        {/* Submit */}
                        <Button
                            type="submit"
                            disabled={isLoading}
                            className="w-full font-bold py-3 rounded-xl transition-all flex items-center justify-center gap-2 shadow-sm"
                        >
                            تحديث المعاينة
                        </Button>
                    </form>
                </Card>

                {/* Left Column: Visual Metrics & Preview Grid */}
                <div className="lg:col-span-3 space-y-6">

                    {/* Visual Analytics Quick Metrics */}
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                        <Card className="p-5 border border-border shadow-sm flex flex-col justify-between min-h-[105px] bg-card">
                            <span className="text-muted-foreground text-xs font-bold flex items-center gap-1.5">
                                <Layers className="w-4 h-4 text-primary" />
                                إجمالي السجلات
                            </span>
                            <span className="text-3xl font-black text-foreground mt-2">{stats.total}</span>
                        </Card>

                        {isRequests ? (
                            <>
                                <Card className="p-5 border border-border shadow-sm flex flex-col justify-between min-h-[105px] bg-card">
                                    <span className="text-red-500 text-xs font-bold flex items-center gap-1.5">
                                        <Clock className="w-4 h-4 text-red-500" />
                                        قيد الانتظار
                                    </span>
                                    <span className="text-3xl font-black text-red-600 mt-2">{(stats as any).pending}</span>
                                </Card>
                                <Card className="p-5 border border-border shadow-sm flex flex-col justify-between min-h-[105px] bg-card">
                                    <span className="text-amber-500 text-xs font-bold flex items-center gap-1.5">
                                        <AlertCircle className="w-4 h-4 text-amber-500" />
                                        قيد المعالجة
                                    </span>
                                    <span className="text-3xl font-black text-amber-600 mt-2">{(stats as any).processed}</span>
                                </Card>
                                <Card className="p-5 border border-border shadow-sm flex flex-col justify-between min-h-[105px] bg-card">
                                    <span className="text-emerald-500 text-xs font-bold flex items-center gap-1.5">
                                        <CheckCircle2 className="w-4 h-4 text-emerald-500" />
                                        منجز / مسلم
                                    </span>
                                    <span className="text-3xl font-black text-emerald-600 mt-2">{(stats as any).completed}</span>
                                </Card>
                            </>
                        ) : (
                            <Card className="col-span-3 p-5 border border-border shadow-sm flex flex-col justify-between min-h-[105px] bg-card">
                                <span className="text-muted-foreground text-xs font-bold flex items-center gap-1.5">
                                    <Users className="w-4 h-4 text-primary" />
                                    مرفقات ومستندات الردود
                                </span>
                                <span className="text-3xl font-black text-foreground mt-2">{stats.attachments}</span>
                            </Card>
                        )}
                    </div>

                    {/* Data Table Preview */}
                    <Card className="p-6 overflow-hidden flex flex-col min-h-[500px] border border-border shadow-sm rounded-2xl bg-card">

                        <div className="flex items-center justify-between mb-6 border-b pb-4">
                            <h3 className="text-lg font-bold flex items-center gap-2 text-foreground">
                                <FileSpreadsheet className="w-5 h-5 text-primary" />
                                معاينة المعاملات الحالية
                            </h3>
                            <span className="px-3 py-1 bg-primary/10 text-primary text-xs font-bold rounded-full border border-primary/20">
                                {totalItems} سجل متاح
                            </span>
                        </div>

                        {/* Responsive Table */}
                        <div className="overflow-x-auto flex-1 custom-scrollbar">
                            <table className="w-full text-right border-collapse">
                                <thead className="bg-muted/30 sticky top-0 z-10 backdrop-blur-sm">
                                    {isRequests ? (
                                        <tr>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">نوع الخدمة</th>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">صاحب الطلب</th>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-center">تاريخ التقديم</th>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-center">الحالة</th>
                                            {/* <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-center">المرفقات</th> */}
                                        </tr>
                                    ) : (
                                        <tr>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">صاحب الرد</th>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">نوع الخدمة الأصلية</th>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-right">الرد / الإجراء المضاف</th>
                                            <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-center">تاريخ الرد</th>
                                            {/* <th className="px-6 py-4 font-bold text-sm text-muted-foreground border-b text-center">المرفقات</th> */}
                                        </tr>
                                    )}
                                </thead>
                                <tbody className="divide-y divide-border/50">
                                    {isLoading ? (
                                        Array.from({ length: 5 }).map((_, i) => (
                                            <tr key={i} className="animate-pulse">
                                                <td colSpan={6} className="px-6 py-4">
                                                    <div className="h-10 bg-muted/20 rounded-xl w-full" />
                                                </td>
                                            </tr>
                                        ))
                                    ) : isRequests ? (
                                        (requestsData?.items || []).length > 0 ? (
                                            (requestsData?.items || []).map((request: any) => {
                                                const templateTitle = templates.find(t => t.id === request.templateId)?.title || "طلب معاملة";

                                                let statusText = 'قيد الانتظار';
                                                let statusClass = 'bg-red-500/10 text-red-600 border-red-500/20';
                                                if (String(request.status) === '2') {
                                                    statusText = 'قيد المعالجة';
                                                    statusClass = 'bg-amber-500/10 text-amber-600 border-amber-500/20';
                                                } else if (String(request.status) === '3') {
                                                    statusText = 'تمت إدارتها';
                                                    statusClass = 'bg-green-500/10 text-green-600 border-green-500/20';
                                                } else if (String(request.status) === '1') {
                                                    statusText = 'تم التسليم';
                                                    statusClass = 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20';
                                                }

                                                return (
                                                    <tr key={request.id} className="hover:bg-muted/10 transition-colors">
                                                        <td className="px-6 py-4 font-bold text-sm text-foreground">{templateTitle}</td>
                                                        <td className="px-6 py-4 text-sm">
                                                            <UserDisplay userId={request.requesterId} className="text-sm font-bold" />
                                                        </td>
                                                        <td className="px-6 py-4 text-center text-xs text-muted-foreground">
                                                            {request.createdAt ? new Date(request.createdAt).toLocaleDateString('ar-EG') : '---'}
                                                        </td>
                                                        <td className="px-6 py-4 text-center">
                                                            <span className={`px-2.5 py-1 text-xs font-bold rounded-lg border ${statusClass}`}>
                                                                {statusText}
                                                            </span>
                                                        </td>
                                                        {/* <td className="px-6 py-4 text-center text-sm font-bold text-muted-foreground">
                                                            {request.requestAttachmentDtos?.length || 0}
                                                        </td> */}
                                                    </tr>
                                                );
                                            })
                                        ) : (
                                            <tr>
                                                <td colSpan={5} className="py-20 text-center">
                                                    <div className="flex flex-col items-center justify-center text-muted-foreground">
                                                        <AlertCircle className="w-12 h-12 mb-3 opacity-20 text-primary" />
                                                        <p className="font-bold">لا توجد سجلات مطابقة لخيارات الفلترة حالياً</p>
                                                    </div>
                                                </td>
                                            </tr>
                                        )
                                    ) : (
                                        (responsesData?.items || []).length > 0 ? (
                                            (responsesData?.items || []).map((response: any) => {
                                                const originalTitle = templates.find(t => t.id === response.request?.templateId)?.title || "طلب معاملة";

                                                return (
                                                    <tr key={response.id} className="hover:bg-muted/10 transition-colors">
                                                        <td className="px-6 py-4 text-sm">
                                                            <UserDisplay userId={response.respondedByUserId} className="text-sm font-bold" />
                                                        </td>
                                                        <td className="px-6 py-4 font-bold text-sm text-foreground">{originalTitle}</td>
                                                        <td className="px-6 py-4 text-sm text-muted-foreground max-w-xs truncate" title={response.comment || ''}>
                                                            {response.comment || 'لا يوجد تعليق مضاف'}
                                                        </td>
                                                        <td className="px-6 py-4 text-center text-xs text-muted-foreground">
                                                            {response.respondedAt || response.createdAt ? new Date(response.respondedAt || response.createdAt).toLocaleDateString('ar-EG') : '---'}
                                                        </td>
                                                        {/* <td className="px-6 py-4 text-center text-sm font-bold text-muted-foreground">
                                                            {response.responseAttachments?.length || 0}
                                                        </td> */}
                                                    </tr>
                                                );
                                            })
                                        ) : (
                                            <tr>
                                                <td colSpan={5} className="py-20 text-center">
                                                    <div className="flex flex-col items-center justify-center text-muted-foreground">
                                                        <AlertCircle className="w-12 h-12 mb-3 opacity-20 text-primary" />
                                                        <p className="font-bold">لا توجد ردود مطابقة لخيارات الفلترة حالياً</p>
                                                    </div>
                                                </td>
                                            </tr>
                                        )
                                    )}
                                </tbody>
                            </table>
                        </div>

                        {/* Pagination Area */}
                        {totalPages > 1 && (
                            <div className="flex justify-between items-center mt-6 pt-4 border-t border-border/50">
                                <span className="text-xs text-muted-foreground font-bold">
                                    الصفحة {page} من {totalPages}
                                </span>
                                <div className="flex items-center gap-2">
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        disabled={page === 1 || isLoading}
                                        onClick={() => setPage(prev => Math.max(prev - 1, 1))}
                                        className="h-9 px-3 rounded-xl border-border hover:bg-primary hover:text-white transition-all shadow-sm"
                                    >
                                        <ChevronRight className="w-4 h-4 ml-1" />
                                        السابق
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        disabled={page === totalPages || isLoading}
                                        onClick={() => setPage(prev => Math.min(prev + 1, totalPages))}
                                        className="h-9 px-3 rounded-xl border-border hover:bg-primary hover:text-white transition-all shadow-sm"
                                    >
                                        التالي
                                        <ChevronLeft className="w-4 h-4 mr-1" />
                                    </Button>
                                </div>
                            </div>
                        )}

                    </Card>

                </div>

            </div>

        </AnimatedContainer>
    );
};

export default ReportsPage;
