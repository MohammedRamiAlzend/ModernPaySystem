import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { Input } from '@/shared/ui/input';
import { Progress } from '@/shared/ui/progress';
import { archivingService } from '@/features/archiving/api/archivingService';
import { DocumentGallery } from '@/features/archiving/ui/DocumentGallery';
import type { ArchiveRecord, DynamicFormTemplate } from '@/features/archiving/model/types';
import {
    Search,
    SlidersHorizontal,
    Eye,
    Download,
    FileText,
    Hash,
    Layers,
    Loader2,
    X,
    Filter,
    FolderOpen
} from 'lucide-react';

export function ArchiveSearchPage() {
    // Basic Search Fields
    const [searchText, setSearchText] = useState('');
    const [archivalNumber, setArchivalNumber] = useState('');
    const [recordId, setRecordId] = useState('');
    const [logicalOperator, setLogicalOperator] = useState<number>(1); // 1 = AND, 2 = OR
    const [selectedTemplateId, setSelectedTemplateId] = useState<string>('');
    const [dynamicFilters, setDynamicFilters] = useState<Record<string, string>>({});

    // Pagination & Results
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);
    const [searchResults, setSearchResults] = useState<{ items: ArchiveRecord[]; totalItems: number } | null>(null);
    const [isSearching, setIsSearching] = useState(false);

    // Record Gallery Preview
    const [previewingRecord, setPreviewingRecord] = useState<ArchiveRecord | null>(null);

    // ZIP Download State
    const [downloadingZipId, setDownloadingZipId] = useState<string | null>(null);
    const [downloadProgress, setDownloadProgress] = useState(0);

    // Fetch dynamic templates
    const { data: dynamicTemplates = [] } = useQuery<DynamicFormTemplate[]>({
        queryKey: ['archive-dynamic-forms'],
        queryFn: () => archivingService.getAllDynamicForms()
    });

    const activeTemplate = dynamicTemplates.find(t => t.id === selectedTemplateId);

    // Parse fields of selected template
    const templateFields = activeTemplate ? (() => {
        try {
            const parsed = JSON.parse(activeTemplate.contentAsJson);
            return Array.isArray(parsed) ? parsed : [];
        } catch {
            return [];
        }
    })() : [];

    // Clear filters when template changes
    useEffect(() => {
        setDynamicFilters({});
    }, [selectedTemplateId]);

    // Handle dynamic filter input change
    const handleDynamicFilterChange = (fieldLabel: string, value: string) => {
        setDynamicFilters(prev => ({
            ...prev,
            [fieldLabel]: value
        }));
    };

    // Execute Global Search
    const handleSearch = async (targetPage = 1) => {
        setIsSearching(true);
        try {
            // Build input value filters array
            const inputValueFilters = Object.entries(dynamicFilters)
                .filter(([_, value]) => value.trim() !== '')
                .map(([key, value]) => ({ key, value }));

            const filterDto = {
                page: targetPage,
                pageSize,
                searchText: searchText.trim() || undefined,
                archivalNumber: archivalNumber.trim() || undefined,
                recordId: recordId.trim() || undefined,
                logicalOperator,
                inputValueFilters: inputValueFilters.length > 0 ? inputValueFilters : undefined
            };

            const data = await archivingService.getPagedArchiveRecords(filterDto);
            setSearchResults(data);
            setPage(targetPage);
        } catch (e) {
            console.error('Archive search failed:', e);
        } finally {
            setIsSearching(false);
        }
    };

    const handleClearFilters = () => {
        setSearchText('');
        setArchivalNumber('');
        setRecordId('');
        setLogicalOperator(1);
        setSelectedTemplateId('');
        setDynamicFilters({});
        setSearchResults(null);
        setPage(1);
    };

    const handleRecordClick = async (record: ArchiveRecord) => {
        setPreviewingRecord(record);
        try {
            const fullRecord = await archivingService.getArchiveRecordById(record.id);
            setPreviewingRecord(fullRecord);
        } catch (e) {
            console.error('Failed to load full record details:', e);
        }
    };

    const handleDownloadZip = async (record: ArchiveRecord) => {
        setDownloadingZipId(record.id);
        setDownloadProgress(0);
        try {
            const blob = await archivingService.downloadZip(
                record.id,
                { flatten: false, includeMetadata: true },
                (progressEvent) => {
                    const percentCompleted = Math.round((progressEvent.loaded * 100) / (progressEvent.total || 1));
                    setDownloadProgress(percentCompleted);
                }
            );

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `Archive-${record.archivalNumber || record.id}.zip`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
        } catch (e) {
            console.error('ZIP download failed:', e);
        } finally {
            setDownloadingZipId(null);
        }
    };

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6 text-right min-h-screen" dir="rtl">

            {/* Top Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-border pb-6">
                <div className="flex flex-col gap-1">
                    <h1 className="text-3xl font-black text-primary flex items-center gap-2">
                        <SlidersHorizontal className="h-8 w-8 text-primary" />
                        البحث المتقدم في الأرشيف
                    </h1>
                    <p className="text-xs text-muted-foreground font-semibold">
                        ابحث واسترجع المستندات المؤرشفة باستخدام الخصائص والبيانات الديناميكية الخاصة بكل نموذج
                    </p>
                </div>
            </div>

            {/* Advanced Filters Card */}
            <Card className="p-6 bg-card border border-border shadow-lg rounded-3xl space-y-6">
                <h3 className="text-base font-bold flex items-center gap-2 border-b pb-3 text-primary">
                    <Filter className="h-4 w-4" />
                    تحديد معايير البحث والفلترة
                </h3>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                    {/* General Text */}
                    <div className="flex flex-col gap-2">
                        <label className="text-xs font-bold text-muted-foreground">البحث العام</label>
                        <div className="relative">
                            <Input
                                value={searchText}
                                onChange={(e) => setSearchText(e.target.value)}
                                placeholder="بحث في الملاحظات أو محتويات المستند..."
                                className="pl-10 rounded-xl"
                            />
                            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                        </div>
                    </div>

                    {/* Archival Number */}
                    <div className="flex flex-col gap-2">
                        <label className="text-xs font-bold text-muted-foreground">رقم الأرشفة</label>
                        <div className="relative">
                            <Input
                                value={archivalNumber}
                                onChange={(e) => setArchivalNumber(e.target.value)}
                                placeholder="مثال: ARC-12345"
                                className="pl-10 rounded-xl"
                            />
                            <Hash className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                        </div>
                    </div>

                    {/* Record ID */}
                    <div className="flex flex-col gap-2">
                        <label className="text-xs font-bold text-muted-foreground">معرف السجل </label>
                        <div className="relative">
                            <Input
                                value={recordId}
                                onChange={(e) => setRecordId(e.target.value)}
                                placeholder="أدخل رمز الـ UUID الخاص بالسجل..."
                                className="pl-10 rounded-xl"
                            />
                            <Hash className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                        </div>
                    </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-6 items-end">
                    {/* Logical Operator */}
                    <div className="flex flex-col gap-2">
                        <label className="text-xs font-bold text-muted-foreground">نمط الفلترة المنطقية</label>
                        <select
                            value={logicalOperator}
                            onChange={(e) => setLogicalOperator(Number(e.target.value))}
                            className="flex h-10 w-full rounded-xl border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            <option value={1}>مطابقة كل الشروط </option>
                            <option value={2}>مطابقة أي من الشروط </option>
                        </select>
                    </div>

                    {/* Dynamic Template Selector */}
                    <div className="flex flex-col gap-2">
                        <label className="text-xs font-bold text-muted-foreground">نوع المستند (النموذج الديناميكي)</label>
                        <select
                            value={selectedTemplateId}
                            onChange={(e) => setSelectedTemplateId(e.target.value)}
                            className="flex h-10 w-full rounded-xl border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                            <option value="">-- اختر النموذج لمطابقة حقوله --</option>
                            {dynamicTemplates.map(t => (
                                <option key={t.id} value={t.id}>{t.templateFormName}</option>
                            ))}
                        </select>
                    </div>

                    {/* Action Buttons */}
                    <div className="flex gap-3">
                        <Button
                            onClick={() => handleSearch(1)}
                            disabled={isSearching}
                            className="flex-1 rounded-xl font-bold gap-2"
                        >
                            {isSearching ? <Loader2 className="h-4 w-4 animate-spin" /> : <Search className="h-4 w-4" />}
                            <span>بحث</span>
                        </Button>
                        <Button
                            onClick={handleClearFilters}
                            variant="outline"
                            className="rounded-xl font-bold gap-2 border-border"
                        >
                            <X className="h-4 w-4" />
                            <span>تفريغ</span>
                        </Button>
                    </div>
                </div>

                {/* Template Fields Dynamic Inputs */}
                {templateFields.length > 0 && (
                    <div className="mt-6 pt-6 border-t border-dashed border-border space-y-4 animate-in fade-in duration-300">
                        <h4 className="text-xs font-bold text-primary flex items-center gap-1.5">
                            <Layers className="h-3.5 w-3.5" />
                            البحث بموجب خصائص نموذج: {activeTemplate?.templateFormName}
                        </h4>
                        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                            {templateFields.map((field: any) => (
                                <div key={field.id} className="flex flex-col gap-2">
                                    <label className="text-xs font-bold text-muted-foreground">{field.label}</label>
                                    <Input
                                        value={dynamicFilters[field.label] || ''}
                                        onChange={(e) => handleDynamicFilterChange(field.label, e.target.value)}
                                        placeholder={`ابحث بقيمة ${field.label}...`}
                                        className="rounded-xl"
                                    />
                                </div>
                            ))}
                        </div>
                    </div>
                )}
            </Card>

            {/* Results Section */}
            {searchResults !== null && (
                <Card className="p-6 bg-card border border-border shadow-lg rounded-3xl">
                    <div className="flex justify-between items-center mb-6">
                        <h2 className="text-lg font-bold text-primary flex items-center gap-2">
                            <FolderOpen className="h-5 w-5" />
                            نتائج البحث ({searchResults.totalItems} مستند)
                        </h2>
                    </div>

                    <div className="overflow-x-auto custom-scrollbar">
                        <table className="w-full text-right border-collapse">
                            <thead>
                                <tr className="border-b border-border bg-muted/20">
                                    <th className="px-6 py-4 text-xs font-bold text-muted-foreground text-right">رقم الأرشفة</th>
                                    <th className="px-6 py-4 text-xs font-bold text-muted-foreground text-right">نوع النموذج</th>
                                    <th className="px-6 py-4 text-xs font-bold text-muted-foreground text-right">عدد الملفات المرفقة</th>
                                    <th className="px-6 py-4 text-xs font-bold text-muted-foreground text-right">تاريخ الإضافة</th>
                                    <th className="px-6 py-4 text-xs font-bold text-muted-foreground text-center">الإجراءات</th>
                                </tr>
                            </thead>
                            <tbody className="divide-y divide-border/50">
                                {searchResults.items.length > 0 ? (
                                    searchResults.items.map((record) => (
                                        <tr key={record.id} className="hover:bg-muted/10 transition-colors group">
                                            <td className="px-6 py-4 text-sm font-bold text-foreground">
                                                {record.archivalNumber}
                                            </td>
                                            <td className="px-6 py-4 text-xs font-semibold text-muted-foreground">
                                                {dynamicTemplates.find(t => t.id === record.formId)?.templateFormName || 'مستند عام'}
                                            </td>
                                            <td className="px-6 py-4 text-xs font-medium text-foreground">
                                                {record.physicalFiles?.length || 0} ملفات
                                            </td>
                                            <td className="px-6 py-4 text-xs text-muted-foreground font-mono">
                                                {record.createdAt ? new Date(record.createdAt).toLocaleDateString('ar-EG') : '---'}
                                            </td>
                                            <td className="px-6 py-4 text-center">
                                                <div className="flex items-center justify-center gap-2">
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl hover:bg-primary/20 hover:text-primary transition-colors"
                                                        onClick={() => handleRecordClick(record)}
                                                    >
                                                        <Eye className="w-4 h-4" />
                                                    </Button>
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-xl hover:bg-emerald-500/20 hover:text-emerald-600 transition-colors"
                                                        onClick={() => handleDownloadZip(record)}
                                                    >
                                                        <Download className="w-4 h-4" />
                                                    </Button>
                                                </div>
                                            </td>
                                        </tr>
                                    ))
                                ) : (
                                    <tr>
                                        <td colSpan={5} className="py-16 text-center text-muted-foreground">
                                            <div className="flex flex-col items-center justify-center gap-2">
                                                <FileText className="h-10 w-10 opacity-30 text-muted-foreground" />
                                                <p className="font-semibold text-sm">لم يتم العثور على أي مستندات تطابق معايير البحث.</p>
                                            </div>
                                        </td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>

                    {/* Pagination */}
                    {searchResults.totalItems > pageSize && (
                        <div className="flex justify-center items-center gap-2 mt-6 pt-6 border-t border-border">
                            <Button
                                variant="outline"
                                size="sm"
                                disabled={page === 1 || isSearching}
                                onClick={() => handleSearch(page - 1)}
                                className="rounded-xl font-bold"
                            >
                                السابق
                            </Button>
                            <span className="text-xs font-bold text-muted-foreground">
                                صفحة {page} من {Math.ceil(searchResults.totalItems / pageSize)}
                            </span>
                            <Button
                                variant="outline"
                                size="sm"
                                disabled={page * pageSize >= searchResults.totalItems || isSearching}
                                onClick={() => handleSearch(page + 1)}
                                className="rounded-xl font-bold"
                            >
                                التالي
                            </Button>
                        </div>
                    )}
                </Card>
            )}

            {/* Document Preview Gallery Modal */}
            {previewingRecord && (
                <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in animate-duration-300">
                    <div className="bg-card border border-border rounded-3xl w-full max-w-7xl h-[90vh] shadow-2xl flex flex-col overflow-hidden text-right">
                        <div className="p-6 border-b border-border flex items-center justify-between">
                            <button
                                onClick={() => setPreviewingRecord(null)}
                                className="text-muted-foreground hover:text-foreground font-bold p-1 rounded-lg hover:bg-muted transition-all"
                            >
                                إغلاق المعاينة
                            </button>
                            <h2 className="text-base font-bold text-foreground flex items-center gap-2">
                                <FileText className="h-5 w-5 text-primary" />
                                <span>تفاصيل المستند ورقم الأرشفة: {previewingRecord.archivalNumber}</span>
                            </h2>
                        </div>
                        <div className="flex-1 overflow-hidden p-6">
                            <DocumentGallery
                                recordId={previewingRecord.id}
                                files={previewingRecord.physicalFiles || []}
                                record={previewingRecord}
                                formName={dynamicTemplates.find(t => t.id === previewingRecord.formId)?.templateFormName}
                                onFilesChanged={async () => {
                                    try {
                                        const updated = await archivingService.getArchiveRecordById(previewingRecord.id);
                                        setPreviewingRecord(updated);
                                        if (searchResults) {
                                            setSearchResults(prev => {
                                                if (!prev) return null;
                                                return {
                                                    ...prev,
                                                    items: prev.items.map(item => item.id === updated.id ? updated : item)
                                                };
                                            });
                                        }
                                    } catch (e) {
                                        console.error(e);
                                    }
                                }}
                            />
                        </div>
                    </div>
                </div>
            )}

            {/* Zip Download Loading Indicator */}
            {downloadingZipId && (
                <div className="fixed inset-0 bg-black/40 backdrop-blur-[2px] flex items-center justify-center z-[100] animate-in fade-in duration-300">
                    <div className="bg-card border border-border rounded-3xl p-8 max-w-sm w-full shadow-2xl flex flex-col items-center gap-6 text-center">
                        <div className="w-16 h-16 rounded-2xl bg-primary/10 text-primary flex items-center justify-center">
                            <Loader2 className="h-8 w-8 animate-spin" />
                        </div>
                        <div className="flex flex-col gap-2">
                            <h3 className="text-base font-bold text-foreground">جاري تحميل الملفات</h3>
                            <p className="text-xs text-muted-foreground font-medium">يتم الآن تجميع وضغط الملفات وتنزيلها كملف ZIP واحد...</p>
                        </div>
                        <div className="w-full flex flex-col gap-2">
                            <div className="flex justify-between text-xs font-bold text-primary">
                                <span>التقدم:</span>
                                <span>{downloadProgress}%</span>
                            </div>
                            <Progress value={downloadProgress} className="h-2" />
                        </div>
                    </div>
                </div>
            )}
        </AnimatedContainer>
    );
}

export default ArchiveSearchPage;
