import { useState, useMemo } from 'react';
import { FileText, Search, User, Calendar, Check, AlertCircle, Eye, SlidersHorizontal, X } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Badge } from '@/shared/ui/badge';
import { Label } from '@/shared/ui/label';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/ui/select';
import { SearchableSelect } from '@/shared/ui/searchable-select';
import { useRequestsPaged, useTemplates } from '@/features/form-builder/api/formEndpoints';
import { useRequestDetails } from '@/features/form-builder/model/useRequestDetails';
import { ResponseDetailsModal } from '@/widgets/form-editor/ui/response-details-modal';
import { cn } from '@/shared/lib/utils';
import type { TemplateRequest, FormField } from '@/entities/form/model/types';

interface RequestPickerProps {
    value?: string; // For single select
    onValueChange?: (value: string) => void; // For single select
    multiple?: boolean;
    values?: string[]; // For multi select
    onValuesChange?: (values: string[]) => void; // For multi select
    excludeRequestId?: string;
    placeholder?: string;
}

const getStatusBadge = (status: number) => {
    switch (status) {
        case 0:
            return <Badge variant="outline" className="bg-amber-500/10 text-amber-500 border-amber-500/20 text-[10px] py-0">قيد الانتظار</Badge>;
        case 1:
            return <Badge variant="outline" className="bg-sky-500/10 text-sky-500 border-sky-500/20 text-[10px] py-0">تم التسليم</Badge>;
        case 2:
            return <Badge variant="outline" className="bg-indigo-500/10 text-indigo-500 border-indigo-500/20 text-[10px] py-0">قيد المعالجة</Badge>;
        case 3:
            return <Badge variant="outline" className="bg-emerald-500/10 text-emerald-500 border-emerald-500/20 text-[10px] py-0">تمت إدارتها</Badge>;
        default:
            return <Badge variant="outline" className="text-[10px] py-0">غير معروف</Badge>;
    }
};

const formatDate = (dateStr?: string | null) => {
    if (!dateStr) return '';
    try {
        const date = new Date(dateStr);
        return date.toLocaleDateString('ar-YE', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
        });
    } catch {
        return dateStr;
    }
};

export const RequestPicker = ({
    value,
    onValueChange,
    multiple = false,
    values = [],
    onValuesChange,
    excludeRequestId,
    // placeholder = "اختر الطلب..."
}: RequestPickerProps) => {
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedTemplateId, setSelectedTemplateId] = useState<string>('all');
    const [showAdvancedFilters, setShowAdvancedFilters] = useState(false);

    // Advanced filtering state like RequestFilterPanel.tsx
    const [selectedFieldKeys, setSelectedFieldKeys] = useState<string[]>([]);
    const [filterValues, setFilterValues] = useState<Record<string, string>>({});
    const [appliedFilters, setAppliedFilters] = useState<Record<string, string>>({});

    // Details Modal state
    const { isModalOpen, setIsModalOpen, viewingResponse, handleViewRequest } = useRequestDetails();

    // Fetch all templates to allow filtering by template
    const { data: templates = [], isLoading: isTemplatesLoading } = useTemplates();

    // Parse template fields map for label mapping
    const templateFieldsMap = useMemo(() => {
        const map: Record<string, Record<string, string>> = {};
        templates.forEach(t => {
            try {
                const parsed = JSON.parse(t.contentAsJson);
                const baseSchema = Array.isArray(parsed) ? parsed[0] : parsed;
                if (baseSchema?.fields) {
                    const fieldLabels: Record<string, string> = {};
                    (baseSchema.fields as FormField[]).forEach(f => {
                        fieldLabels[f.name] = f.label;
                    });
                    map[t.id] = fieldLabels;
                }
            } catch {
                // Ignore parsing errors
            }
        });
        return map;
    }, [templates]);

    const getFieldLabel = (templateId: string, key: string) => {
        return templateFieldsMap[templateId]?.[key] || key;
    };

    // Selected template fields for advanced filtering
    const selectedTemplateFields = useMemo((): FormField[] => {
        if (selectedTemplateId === 'all') return [];
        const t = templates.find(temp => temp.id === selectedTemplateId);
        if (!t) return [];
        try {
            const parsed = JSON.parse(t.contentAsJson);
            const baseSchema = Array.isArray(parsed) ? parsed[0] : parsed;
            return baseSchema?.fields || [];
        } catch {
            return [];
        }
    }, [templates, selectedTemplateId]);

    // Handle template selection change
    const handleTemplateChange = (val: string) => {
        setSelectedTemplateId(val);
        setSelectedFieldKeys([]);
        setFilterValues({});
        setAppliedFilters({});
        setShowAdvancedFilters(false);
    };

    // Handle field selection change
    const handleFieldSelectionChange = (keys: string[]) => {
        setSelectedFieldKeys(keys);
        setFilterValues(prev => {
            const next = { ...prev };
            Object.keys(next).forEach(k => {
                if (!keys.includes(k)) delete next[k];
            });
            return next;
        });
    };

    // Handle filter value changes
    const handleFilterValueChange = (key: string, val: string) => {
        setFilterValues(prev => ({
            ...prev,
            [key]: val
        }));
    };

    // Apply Filters
    const applyFilters = () => {
        setAppliedFilters({ ...filterValues });
    };

    // Reset Filters
    const resetFilters = () => {
        setFilterValues({});
        setAppliedFilters({});
        setSelectedFieldKeys([]);
    };

    // Prepare inputs value filters to send to API
    const apiInputValueFilters = useMemo(() => {
        const filters: { key: string; value: string }[] = [];
        Object.entries(appliedFilters).forEach(([key, val]) => {
            if (val.trim()) {
                filters.push({ key, value: val.trim() });
            }
        });
        return filters.length > 0 ? filters : undefined;
    }, [appliedFilters]);

    // Fetch requests. Server filters by templateId and dynamic filters.
    const { data: requestsPaged, isLoading: isRequestsLoading } = useRequestsPaged({
        page: 1,
        pageSize: 100,
        templateId: selectedTemplateId !== 'all' ? selectedTemplateId : undefined,
        inputValueFilters: apiInputValueFilters
    });

    const requestsItems = useMemo(() => requestsPaged?.items || [], [requestsPaged]);

    // Client side filtering for general query (to ensure instant typing feedback)
    const filteredRequests = useMemo(() => {
        return requestsItems.filter((r: TemplateRequest) => {
            if (excludeRequestId && r.id === excludeRequestId) return false;

            if (!searchQuery.trim()) return true;

            const q = searchQuery.toLowerCase().trim();

            const matchesNumber = String(r.requestNumber).includes(q);
            const matchesTemplate = r.template?.templateName?.toLowerCase().includes(q);
            const matchesRequester = r.requester?.userName?.toLowerCase().includes(q);

            // Search inside content values
            const matchesContent = r.content?.some((c) =>
                (c.key || '').toLowerCase().includes(q) || (c.value || '').toLowerCase().includes(q)
            );

            return matchesNumber || matchesTemplate || matchesRequester || matchesContent;
        });
    }, [requestsItems, searchQuery, excludeRequestId]);

    const handleItemClick = (id: string) => {
        if (multiple) {
            if (onValuesChange) {
                if (values.includes(id)) {
                    onValuesChange(values.filter(v => v !== id));
                } else {
                    onValuesChange([...values, id]);
                }
            }
        } else {
            if (onValueChange) {
                onValueChange(id);
            }
        }
    };

    const renderContentPreview = (r: TemplateRequest) => {
        if (!r.content || r.content.length === 0) return null;
        const itemsToShow = r.content
            .filter((c) => c.value && String(c.value).trim() !== '')
            .slice(0, 3);

        if (itemsToShow.length === 0) return null;

        return (
            <div className="mt-2 flex flex-wrap gap-1 text-[10px] text-muted-foreground bg-muted/40 p-1.5 rounded-lg border border-primary/5">
                {itemsToShow.map((item, idx) => (
                    <span key={idx} className="bg-background px-1.5 py-0.5 rounded border border-muted/50 flex items-center gap-1">
                        <span className="font-semibold text-primary/80">
                            {getFieldLabel(r.templateId, item.key)}:
                        </span>
                        <span className="text-foreground max-w-[120px] truncate">{String(item.value)}</span>
                    </span>
                ))}
            </div>
        );
    };

    const hasActiveFilters = Object.values(appliedFilters).some(v => v.trim() !== '');

    return (
        <div className="space-y-3 w-full" dir="rtl">
            {/* Filters bar */}
            <div className="flex gap-2">
                <div className="relative flex-1">
                    <Search className="absolute right-3 top-2.5 w-4 h-4 text-muted-foreground" />
                    <Input
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        placeholder="ابحث برقم الطلب، اسم النموذج، المنشئ، أو الحقول..."
                        className="h-9 pr-9 pl-3 rounded-xl bg-background/50 border-primary/10 text-xs"
                    />
                </div>

                <Select
                    value={selectedTemplateId}
                    onValueChange={handleTemplateChange}
                >
                    <SelectTrigger className="h-9 w-[140px] rounded-xl bg-background/50 border-primary/10 text-xs">
                        <SelectValue placeholder="تصفية حسب النموذج" />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl border-primary/10">
                        <SelectItem value="all" className="text-xs">جميع الخدمات</SelectItem>
                        {templates.map(t => (
                            <SelectItem key={t.id} value={t.id} className="text-xs">
                                {t.templateName}
                            </SelectItem>
                        ))}
                    </SelectContent>
                </Select>

                {selectedTemplateId !== 'all' && selectedTemplateFields.length > 0 && (
                    <Button
                        type="button"
                        variant="outline"
                        size="icon"
                        onClick={() => setShowAdvancedFilters(!showAdvancedFilters)}
                        className={cn(
                            "h-9 w-9 rounded-xl border-primary/10 relative transition-all",
                            showAdvancedFilters || hasActiveFilters
                                ? "bg-primary text-primary-foreground border-primary"
                                : "bg-background/50 hover:bg-primary/5"
                        )}
                        title="تصفية متقدمة حسب الحقول"
                    >
                        <SlidersHorizontal className="w-4 h-4" />
                        {hasActiveFilters && (
                            <span className="absolute -top-1 -left-1 w-2.5 h-2.5 bg-destructive rounded-full border border-background animate-pulse" />
                        )}
                    </Button>
                )}
            </div>

            {/* Advanced Filters Panel matching RequestFilterPanel.tsx */}
            {selectedTemplateId !== 'all' && showAdvancedFilters && selectedTemplateFields.length > 0 && (
                <div className="p-4 rounded-xl bg-muted/40 border border-primary/10 space-y-4 animate-in fade-in slide-in-from-top-1">
                    <div className="space-y-1.5 text-right">
                        <Label className="text-xs font-bold text-muted-foreground mr-1">تخصيص حقول الفلترة للنموذج</Label>
                        <SearchableSelect
                            multiple
                            options={selectedTemplateFields.map(f => ({
                                value: f.name,
                                label: f.label
                            }))}
                            values={selectedFieldKeys}
                            onValuesChange={handleFieldSelectionChange}
                            placeholder="اختر الحقول التي تود الفلترة عليها..."
                            searchPlaceholder="ابحث عن حقل..."
                        />
                    </div>

                    {selectedFieldKeys.length > 0 && (
                        <div className="pt-3 border-t border-dashed border-primary/10 grid grid-cols-1 sm:grid-cols-2 gap-3">
                            {selectedFieldKeys.map(key => {
                                const field = selectedTemplateFields.find(f => f.name === key);
                                return (
                                    <div key={key} className="space-y-1 text-right group">
                                        <Label className="text-[10px] font-bold text-primary/70 group-focus-within:text-primary transition-colors">
                                            {field?.label || key}
                                        </Label>
                                        <div className="relative">
                                            <Input
                                                value={filterValues[key] || ''}
                                                onChange={(e) => handleFilterValueChange(key, e.target.value)}
                                                placeholder={`بحث في ${field?.label || key}...`}
                                                className="h-8 rounded-lg bg-background border-primary/5 focus-visible:ring-primary/20 pl-8 pr-3 text-xs"
                                            />
                                            {filterValues[key] && (
                                                <button
                                                    type="button"
                                                    onClick={() => handleFilterValueChange(key, '')}
                                                    className="absolute left-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-destructive transition-colors"
                                                >
                                                    <X className="w-3.5 h-3.5" />
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    )}

                    <div className="flex items-center justify-end gap-2 pt-2 border-t border-primary/5">
                        <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={resetFilters}
                            className="text-[11px] font-bold text-muted-foreground hover:text-destructive rounded-lg h-8 px-3"
                        >
                            مسح الفلاتر
                        </Button>
                        <Button
                            type="button"
                            size="sm"
                            onClick={applyFilters}
                            className="text-[11px] font-bold rounded-lg px-5 h-8 bg-primary hover:bg-primary/95 text-primary-foreground"
                            disabled={Object.values(filterValues).every(v => v.trim() === '')}
                        >
                            تطبيق الفلتر
                        </Button>
                    </div>
                </div>
            )}

            {/* List */}
            <div className="border border-primary/10 rounded-xl overflow-hidden bg-background/30 backdrop-blur-sm">
                <div className="max-h-[350px] overflow-y-auto p-2 space-y-2">
                    {isRequestsLoading || isTemplatesLoading ? (
                        <div className="flex flex-col items-center justify-center py-8 text-muted-foreground text-xs gap-2">
                            <span className="w-5 h-5 rounded-full border-2 border-primary border-t-transparent animate-spin" />
                            جاري تحميل الطلبات...
                        </div>
                    ) : filteredRequests.length === 0 ? (
                        <div className="flex flex-col items-center justify-center py-8 text-muted-foreground text-xs gap-2">
                            <AlertCircle className="w-5 h-5 text-muted-foreground/60" />
                            لا توجد طلبات مطابقة لمعايير البحث.
                        </div>
                    ) : (
                        filteredRequests.map((r) => {
                            const isSelected = multiple ? values.includes(r.id) : r.id === value;
                            return (
                                <div
                                    key={r.id}
                                    onClick={() => handleItemClick(r.id)}
                                    className={cn(
                                        "p-2.5 rounded-lg border text-right transition-all cursor-pointer select-none",
                                        isSelected
                                            ? "border-primary bg-primary/5 shadow-sm"
                                            : "border-primary/5 hover:border-primary/20 hover:bg-muted/30"
                                    )}
                                >
                                    <div className="flex justify-between items-start gap-2">
                                        <div className="flex items-center gap-2">
                                            <FileText className={cn("w-4 h-4", isSelected ? "text-primary" : "text-primary/60")} />
                                            <span className="text-xs font-bold text-foreground">
                                                طلب #{r.requestNumber} - {r.template?.templateName || 'نموذج'}
                                            </span>
                                        </div>
                                        <div className="flex items-center gap-1.5">
                                            {getStatusBadge(r.status)}

                                            {/* View Details Button */}
                                            <Button
                                                type="button"
                                                variant="ghost"
                                                size="icon"
                                                className="w-6 h-6 rounded-md hover:bg-primary/10 text-muted-foreground hover:text-primary transition-colors"
                                                onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleViewRequest(r);
                                                }}
                                                title="عرض تفاصيل الطلب"
                                            >
                                                <Eye className="w-3.5 h-3.5" />
                                            </Button>

                                            {isSelected && (
                                                <div className="w-4 h-4 rounded-full bg-primary text-primary-foreground flex items-center justify-center p-0.5 animate-in zoom-in">
                                                    <Check className="w-3 h-3" />
                                                </div>
                                            )}
                                        </div>
                                    </div>

                                    {/* Metadata Row */}
                                    <div className="mt-1.5 flex items-center gap-3 text-[10px] text-muted-foreground">
                                        {r.requester?.userName && (
                                            <span className="flex items-center gap-1">
                                                <User className="w-3 h-3 text-muted-foreground/80" />
                                                بواسطة: {r.requester.userName}
                                            </span>
                                        )}
                                        {r.createdAt && (
                                            <span className="flex items-center gap-1">
                                                <Calendar className="w-3 h-3 text-muted-foreground/80" />
                                                بتاريخ: {formatDate(r.createdAt)}
                                            </span>
                                        )}
                                    </div>

                                    {/* Fields preview */}
                                    {renderContentPreview(r)}
                                </div>
                            );
                        })
                    )}
                </div>
            </div>

            {/* Request Details Modal */}
            {viewingResponse && (
                <ResponseDetailsModal
                    isOpen={isModalOpen}
                    onClose={() => setIsModalOpen(false)}
                    response={viewingResponse}
                    schema={viewingResponse.schema}
                />
            )}
        </div>
    );
};
