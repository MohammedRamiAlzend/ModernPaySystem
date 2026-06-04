import React from 'react';
import { DynamicFormTemplate } from '../model/types';
import { Button } from '@/shared/ui/button';
import { FileEdit, Trash2, Calendar, ClipboardList } from 'lucide-react';

interface ArchivingTemplatesListProps {
    templates: DynamicFormTemplate[];
    onEdit: (template: DynamicFormTemplate) => void;
    onDelete: (template: DynamicFormTemplate) => void;
    isLoading: boolean;
    hasMore?: boolean;
    onLoadMore?: () => void;
}

export const ArchivingTemplatesList: React.FC<ArchivingTemplatesListProps> = ({
    templates,
    onEdit,
    onDelete,
    isLoading,
    hasMore = false,
    onLoadMore
}) => {
    const getFieldCount = (contentAsJson: string) => {
        try {
            const parsed = JSON.parse(contentAsJson);
            return Array.isArray(parsed) ? parsed.length : 0;
        } catch {
            return 0;
        }
    };

    return (
        <div className="flex flex-col gap-4">
            <div className="overflow-x-auto rounded-3xl border border-border bg-card shadow-sm hover:shadow-md transition-all duration-300">
                <table className="w-full text-right text-sm">
                    <thead className="bg-muted/40 text-muted-foreground border-b border-border text-xs">
                        <tr>
                            <th className="p-4 font-semibold">اسم النموذج</th>
                            <th className="p-4 font-semibold text-center">عدد الحقول المخصصة</th>
                            <th className="p-4 font-semibold">تاريخ الإنشاء</th>
                            <th className="p-4 font-semibold text-center">الإجراءات</th>
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-border text-foreground">
                        {isLoading && templates.length === 0 ? (
                            // Skeleton loading rows
                            Array.from({ length: 3 }).map((_, idx) => (
                                <tr key={idx} className="animate-pulse">
                                    <td className="p-4">
                                        <div className="flex items-center gap-3">
                                            <div className="w-8 h-8 rounded-lg bg-muted" />
                                            <div className="h-4 bg-muted rounded w-36" />
                                        </div>
                                    </td>
                                    <td className="p-4">
                                        <div className="flex justify-center">
                                            <div className="h-6 bg-muted rounded-full w-16" />
                                        </div>
                                    </td>
                                    <td className="p-4">
                                        <div className="h-4 bg-muted rounded w-24" />
                                    </td>
                                    <td className="p-4">
                                        <div className="flex justify-center gap-2">
                                            <div className="w-8 h-8 rounded-lg bg-muted" />
                                            <div className="w-8 h-8 rounded-lg bg-muted" />
                                        </div>
                                    </td>
                                </tr>
                            ))
                        ) : templates.length === 0 ? (
                            <tr>
                                <td colSpan={4} className="p-12 text-center text-muted-foreground">
                                    <div className="flex flex-col items-center gap-3 py-6">
                                        <div className="w-16 h-16 rounded-2xl bg-muted flex items-center justify-center text-muted-foreground/30">
                                            <ClipboardList className="h-8 w-8 stroke-[1.2]" />
                                        </div>
                                        <span className="text-base font-semibold text-foreground">لا توجد نماذج مضافة بعد</span>
                                        <span className="text-xs text-muted-foreground max-w-[280px]">
                                            قم بالضغط على زر "نموذج جديد" لبناء أول نموذج أرشفة وتحديد الحقول المخصصة.
                                        </span>
                                    </div>
                                </td>
                            </tr>
                        ) : (
                            templates.map((template) => (
                                <tr key={template.id} className="hover:bg-muted/20 transition-all duration-200 group">
                                    <td className="p-4 font-bold">
                                        <div className="flex items-center gap-2">
                                            <div className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center group-hover:scale-105 transition-transform duration-200">
                                                <ClipboardList className="h-4 w-4" />
                                            </div>
                                            <span className="group-hover:text-primary transition-colors">{template.templateFormName}</span>
                                        </div>
                                    </td>
                                    <td className="p-4 text-center font-semibold">
                                        <span className="inline-flex items-center justify-center bg-muted/60 px-3 py-1 rounded-full text-xs font-bold text-muted-foreground group-hover:bg-primary/5 group-hover:text-primary transition-colors">
                                            {getFieldCount(template.contentAsJson)} حقول
                                        </span>
                                    </td>
                                    <td className="p-4 text-muted-foreground">
                                        <span className="inline-flex items-center gap-1.5 text-xs font-medium">
                                            <Calendar className="h-3.5 w-3.5 text-muted-foreground/40" />
                                            {template.createdAt ? new Date(template.createdAt).toLocaleDateString('ar-SY') : '-'}
                                        </span>
                                    </td>
                                    <td className="p-4">
                                        <div className="flex items-center justify-center gap-2">
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-8 w-8 rounded-xl text-muted-foreground hover:text-amber-500 hover:bg-amber-500/10 border border-transparent hover:border-amber-500/20 transition-all duration-200"
                                                onClick={() => onEdit(template)}
                                                title="تعديل النموذج"
                                            >
                                                <FileEdit className="h-4 w-4" />
                                            </Button>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-8 w-8 rounded-xl text-muted-foreground hover:text-destructive hover:bg-destructive/10 border border-transparent hover:border-destructive/20 transition-all duration-200"
                                                onClick={() => onDelete(template)}
                                                title="حذف النموذج"
                                            >
                                                <Trash2 className="h-4 w-4" />
                                            </Button>
                                        </div>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {hasMore && onLoadMore && (
                <div className="flex justify-center py-4">
                    <Button
                        variant="outline"
                        onClick={onLoadMore}
                        disabled={isLoading}
                        className="rounded-xl px-8 border-border text-foreground hover:bg-muted font-bold transition-all hover:scale-[1.02]"
                    >
                        {isLoading ? 'جاري التحميل...' : 'تحميل المزيد'}
                    </Button>
                </div>
            )}
        </div>
    );
};
