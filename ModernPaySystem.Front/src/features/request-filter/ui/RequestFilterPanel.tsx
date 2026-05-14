import { Filter, X, RefreshCw, ChevronDown, ChevronUp, Settings2 } from 'lucide-react';
import { useState } from 'react';
import { Card } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { SearchableSelect } from '@/shared/ui/searchable-select';
import { cn } from '@/shared/lib/utils';
import type { useRequestFilter } from '../model/useRequestFilter';

interface RequestFilterPanelProps {
    filter: ReturnType<typeof useRequestFilter>;
    className?: string;
}

export const RequestFilterPanel = ({ filter, className }: RequestFilterPanelProps) => {
    const [isExpanded, setIsExpanded] = useState(false);
    const {
        visitedTemplates,
        isTemplatesLoading,
        selectedTemplateId,
        availableFields,
        selectedFieldKeys,
        filterValues,
        handleTemplateChange,
        handleFieldSelectionChange,
        handleFilterValueChange,
        applyFilters,
        resetFilters,
        hasActiveFilters
    } = filter;

    const templateOptions = visitedTemplates.map(t => ({
        value: t.id,
        label: t.title
    }));

    const fieldOptions = availableFields.map(f => ({
        value: f.name,
        label: f.label
    }));

    return (
        <Card className={cn("overflow-hidden border-primary/10 shadow-sm", className)}>
            <div
                className="p-4 bg-muted/30 flex items-center justify-between cursor-pointer hover:bg-muted/50 transition-colors"
                onClick={() => setIsExpanded(!isExpanded)}
            >
                <div className="flex items-center gap-2">
                    <Filter className="w-5 h-5 text-primary" />
                    <h3 className="font-bold text-sm">أدوات الفلترة والبحث</h3>
                </div>
                <div className="flex items-center gap-2">
                    {hasActiveFilters && (
                        <span className="px-2 py-0.5 bg-primary text-white text-[10px] font-bold rounded-full animate-in zoom-in">
                            فلتر نشط
                        </span>
                    )}
                    <Button variant="ghost" size="sm" className="h-8 w-8 p-0 rounded-full">
                        {isExpanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
                    </Button>
                </div>
            </div>

            {isExpanded && (
                <div className="p-6 space-y-6 animate-in slide-in-from-top-2 duration-300">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                        <div className="space-y-2">
                            <Label className="text-xs font-bold text-muted-foreground mr-1">اختيار الخدمة</Label>
                            <SearchableSelect
                                options={templateOptions}
                                value={selectedTemplateId}
                                onValueChange={handleTemplateChange}
                                placeholder="اختر الخدمة أولاً..."
                                searchPlaceholder="ابحث عن الخدمة..."
                                isLoading={isTemplatesLoading}
                            />
                        </div>

                        {selectedTemplateId && (
                            <div className="space-y-2 animate-in fade-in slide-in-from-right-4">
                                <Label className="text-xs font-bold text-muted-foreground mr-1 flex items-center gap-1">
                                    <Settings2 className="w-3 h-3" />
                                    تخصيص حقول الفلترة (تفضيلات)
                                </Label>
                                <SearchableSelect
                                    multiple
                                    options={fieldOptions}
                                    values={selectedFieldKeys}
                                    onValuesChange={handleFieldSelectionChange}
                                    placeholder="اختر الحقول التي تود الفلترة عليها..."
                                    searchPlaceholder="ابحث عن حقل..."
                                />
                            </div>
                        )}
                    </div>

                    {selectedFieldKeys.length > 0 && (
                        <div className="pt-4 border-t border-dashed border-primary/10 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4 animate-in fade-in">
                            {selectedFieldKeys.map(key => {
                                const field = availableFields.find(f => f.name === key);
                                return (
                                    <div key={key} className="space-y-1.5 group">
                                        <Label className="text-[11px] font-bold text-primary/70 group-focus-within:text-primary transition-colors">
                                            {field?.label || key}
                                        </Label>
                                        <div className="relative">
                                            <Input
                                                value={filterValues[key] || ''}
                                                onChange={(e) => handleFilterValueChange(key, e.target.value)}
                                                placeholder={`بحث في ${field?.label}...`}
                                                className="h-9 rounded-xl bg-background/50 border-primary/10 focus-visible:ring-primary/20 pr-8"
                                            />
                                            {filterValues[key] && (
                                                <button
                                                    onClick={() => handleFilterValueChange(key, '')}
                                                    className="absolute left-2 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-destructive transition-colors"
                                                >
                                                    <X className="w-3 h-3" />
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    )}

                    <div className="flex items-center justify-end gap-3 pt-2">
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={resetFilters}
                            className="text-xs font-bold text-muted-foreground hover:text-destructive gap-2 rounded-xl"
                        >
                            <RefreshCw className="w-3 h-3" />
                            مسح الفلاتر
                        </Button>
                        <Button
                            size="sm"
                            onClick={applyFilters}
                            className="text-xs font-bold gap-2 rounded-xl px-6"
                            disabled={!selectedTemplateId || Object.values(filterValues).every(v => v.trim() === '')}
                        >
                            <RefreshCw className="w-3 h-3" />
                            تطبيق الفلتر
                        </Button>
                    </div>
                </div>
            )}
        </Card>
    );
};
