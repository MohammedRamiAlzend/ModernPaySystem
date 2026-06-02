import { useState, useMemo } from 'react';
import { DynamicFormTemplate, CreateDynamicFormTemplateDto } from '@/features/archiving/model/types';
import { useInfiniteDynamicForms } from '@/features/archiving/model/queries';
import { useCreateDynamicForm, useUpdateDynamicForm, useDeleteDynamicForm } from '@/features/archiving/model/mutations';
import { ArchivingTemplatesList } from '@/features/archiving/ui/ArchivingTemplatesList';
import { ArchivingTemplateEditor } from '@/features/archiving/ui/ArchivingTemplateEditor';
import { useUIStore } from '@/app/store/uiStore';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { 
    Plus, 
    Loader2, 
    Search,
    ChevronLeft,
    Settings
} from 'lucide-react';
import { useNavigate } from 'react-router-dom';

export default function TemplatesPage() {
    const navigate = useNavigate();
    const { showConfirm } = useUIStore();

    // ---------------------------------------------------------
    // Queries & Mutations (TanStack Query)
    // ---------------------------------------------------------
    const {
        data,
        fetchNextPage,
        hasNextPage,
        isFetchingNextPage,
        isLoading,
        refetch
    } = useInfiniteDynamicForms(10);

    const createMutation = useCreateDynamicForm();
    const updateMutation = useUpdateDynamicForm();
    const deleteMutation = useDeleteDynamicForm();

    const templates = useMemo(() => {
        return data?.pages.flatMap(page => page.items) || [];
    }, [data]);

    // ---------------------------------------------------------
    // Local States (Only UI states)
    // ---------------------------------------------------------
    const [searchTerm, setSearchTerm] = useState('');
    const [showEditor, setShowEditor] = useState(false);
    const [selectedTemplate, setSelectedTemplate] = useState<DynamicFormTemplate | null>(null);

    const isSaving = createMutation.isPending || updateMutation.isPending;

    const handleLoadMore = () => {
        if (hasNextPage) {
            fetchNextPage();
        }
    };

    // ---------------------------------------------------------
    // CRUD Actions
    // ---------------------------------------------------------
    const handleOpenCreate = () => {
        setSelectedTemplate(null);
        setShowEditor(true);
    };

    const handleOpenEdit = (template: DynamicFormTemplate) => {
        setSelectedTemplate(template);
        setShowEditor(true);
    };

    const handleSaveTemplate = async (dto: CreateDynamicFormTemplateDto) => {
        if (selectedTemplate) {
            // Update
            await updateMutation.mutateAsync(
                { id: selectedTemplate.id, dto },
                {
                    onSuccess: () => {
                        setShowEditor(false);
                        refetch();
                    }
                }
            );
        } else {
            // Create
            await createMutation.mutateAsync(
                dto,
                {
                    onSuccess: () => {
                        setShowEditor(false);
                        refetch();
                    }
                }
            );
        }
    };

    const handleDeleteTemplate = (template: DynamicFormTemplate) => {
        showConfirm({
            title: 'تأكيد حذف النموذج الأرشيفي',
            message: `هل أنت متأكد من حذف النموذج "${template.templateFormName}"؟ لن يؤثر هذا على السجلات المؤرشفة مسبقاً، ولكنه سيمنع استخدام هذا النموذج مجدداً في السجلات الجديدة.`,
            variant: 'destructive',
            confirmLabel: 'حذف النموذج',
            onConfirm: async () => {
                await deleteMutation.mutateAsync(template.id, {
                    onSuccess: () => {
                        refetch();
                    }
                });
            }
        });
    };

    // ---------------------------------------------------------
    // Search Filtering (Derived State)
    // ---------------------------------------------------------
    const filteredTemplates = useMemo(() => {
        return templates.filter(t => 
            t.templateFormName.toLowerCase().includes(searchTerm.toLowerCase())
        );
    }, [templates, searchTerm]);

    return (
        <div className="p-4 sm:p-6 flex flex-col gap-6" dir="rtl">
            {/* Breadcrumb / Back button */}
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
                <button 
                    onClick={() => navigate('/archiving')}
                    className="hover:text-primary transition-colors"
                >
                    نظام الأرشفة
                </button>
                <ChevronLeft className="h-4 w-4" />
                <span className="font-semibold text-foreground">إدارة النماذج الأرشيفية</span>
            </div>

            {/* Header section */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-card p-6 rounded-3xl border border-border shadow-sm">
                <div className="flex flex-col gap-1 text-right">
                    <h1 className="text-xl sm:text-2xl font-bold text-foreground tracking-tight flex items-center gap-2">
                        <Settings className="h-6 w-6 text-primary" />
                        <span>إدارة النماذج الأرشيفية (Dynamic Forms)</span>
                    </h1>
                    <p className="text-xs text-muted-foreground font-medium">قم ببناء وتعديل وتخصيص النماذج الأرشيفية التي تظهر للموظفين لتنظيم المستندات.</p>
                </div>

                <div className="flex items-center gap-3">
                    <Button 
                        onClick={handleOpenCreate}
                        className="rounded-2xl py-5 px-6 font-bold shadow-lg shadow-primary/20 flex items-center gap-2"
                    >
                        <Plus className="h-4 w-4" />
                        <span>نموذج جديد</span>
                    </Button>
                </div>
            </div>

            {/* Search Bar */}
            <div className="relative">
                <Search className="absolute right-4 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground pointer-events-none" />
                <Input 
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    placeholder="ابحث عن نموذج بالاسم..."
                    className="pr-12 rounded-2xl border-border bg-card shadow-sm h-11"
                />
            </div>

            {/* List */}
            <div className="bg-card border border-border rounded-3xl p-6 shadow-sm min-h-[400px]">
                {isLoading && templates.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-24 gap-3 text-muted-foreground">
                        <Loader2 className="h-8 w-8 animate-spin text-primary" />
                        <span className="text-sm font-medium">جاري تحميل النماذج...</span>
                    </div>
                ) : (
                    <ArchivingTemplatesList 
                        templates={filteredTemplates}
                        onEdit={handleOpenEdit}
                        onDelete={handleDeleteTemplate}
                        isLoading={isLoading || isFetchingNextPage}
                        hasMore={hasNextPage}
                        onLoadMore={handleLoadMore}
                    />
                )}
            </div>

            {/* Editor Modal */}
            <ArchivingTemplateEditor 
                isOpen={showEditor}
                onClose={() => setShowEditor(false)}
                onSave={handleSaveTemplate}
                template={selectedTemplate}
                isSaving={isSaving}
            />
        </div>
    );
}
