import React, { useState } from 'react';
import { useTemplatesByDepartment, useAddTemplateOwnership, useRemoveTemplateOwnership } from '@/features/form-builder/model/useTemplateOwnerships';
import { useForms } from '@/features/form-builder/model/useForms';
import { Button } from '@/shared/ui/button';
import { RefreshCw, FileStack, Plus, Trash2 } from 'lucide-react';
import { SearchableSelect } from '@/shared/ui/searchable-select';
import { useUIStore } from '@/app/store/uiStore';

interface DepartmentTemplatesTabProps {
    departmentId: string;
    departmentName?: string;
}

export const DepartmentTemplatesTab: React.FC<DepartmentTemplatesTabProps> = ({ departmentId, departmentName }) => {
    const { showConfirm } = useUIStore();

    // Fetch assigned templates
    const { data: assignedTemplates = [], isLoading } = useTemplatesByDepartment(departmentId);

    // Fetch all templates for assignment
    const { data: allTemplates = [] } = useForms();

    const addMut = useAddTemplateOwnership();
    const removeMut = useRemoveTemplateOwnership();

    const [selectedTemplate, setSelectedTemplate] = useState<string>('');

    const unassignedTemplates = allTemplates.filter(t => !assignedTemplates.some(at => at.id === t.id));
    const templateOptions = unassignedTemplates.map(t => ({ value: t.id, label: t.title }));

    const handleAssign = () => {
        if (!selectedTemplate) return;
        addMut.mutate({ templateId: selectedTemplate, departmentId }, {
            onSuccess: () => setSelectedTemplate('')
        });
    };

    const handleRemove = (templateId: string, templateTitle: string) => {
        showConfirm({
            title: 'إزالة الخدمة ',
            message: `هل أنت متأكد من إزالة الصلاحية للخدمة  (${templateTitle}) من قسم ${departmentName}؟`,
            variant: 'destructive',
            confirmLabel: 'إزالة',
            onConfirm: () => removeMut.mutate({ templateId, departmentId })
        });
    };

    return (
        <div className="space-y-6">
            <div className="flex gap-2 items-end">
                <div className="flex-1">
                    <label className="text-sm font-medium mb-1 block">إسناد خدمة جديد للقسم</label>
                    <SearchableSelect
                        options={templateOptions}
                        value={selectedTemplate}
                        onValueChange={setSelectedTemplate}
                        placeholder="ابحث عن خدمة..."
                    />
                </div>
                <Button
                    onClick={handleAssign}
                    disabled={!selectedTemplate || addMut.isPending}
                    className="gap-2"
                >
                    <Plus className="w-4 h-4" /> إضافة
                </Button>
            </div>

            <div className="border rounded-md">
                {isLoading ? (
                    <div className="flex flex-col items-center justify-center py-10 gap-4">
                        <RefreshCw className="w-6 h-6 animate-spin text-primary/50" />
                        <p className="text-sm text-muted-foreground">جاري تحميل النماذج...</p>
                    </div>
                ) : assignedTemplates.length > 0 ? (
                    <ul className="divide-y">
                        {assignedTemplates.map(template => (
                            <li key={template.id} className="flex items-center justify-between p-3 hover:bg-muted/50">
                                <div className="flex items-center gap-3">
                                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
                                        <FileStack className="w-4 h-4 text-primary" />
                                    </div>
                                    <span className="font-medium">{template.title}</span>
                                </div>
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="text-destructive hover:text-destructive/90 hover:bg-destructive/10"
                                    onClick={() => handleRemove(template.id, template.title)}
                                >
                                    <Trash2 className="w-4 h-4" />
                                </Button>
                            </li>
                        ))}
                    </ul>
                ) : (
                    <div className="p-8 text-center text-muted-foreground">
                        <FileStack className="w-12 h-12 mx-auto mb-2 opacity-20" />
                        لا توجد نماذج مسندة لهذا القسم بعد.
                    </div>
                )}
            </div>
        </div>
    );
};
