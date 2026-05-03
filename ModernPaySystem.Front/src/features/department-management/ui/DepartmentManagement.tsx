import React, { useState, useMemo } from 'react';
import { useDepartmentTree } from '../model/useDepartmentTree';
import { DepartmentMermaidTree } from './DepartmentMermaidTree';
import { SearchableSelect, SearchableSelectOption } from '@/shared/ui/searchable-select';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { GitBranch, GitMerge, GitPullRequest, Plus, RefreshCw, Layers, Trash2 } from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { departmentApi } from '@/entities/department/api/departmentApi';
import { queryKeys } from '@/shared/lib/query-keys';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogDescription } from '@/shared/ui/dialog';
import { DepartmentForm } from './DepartmentForm';
import { useDepartmentActions } from '../model/useDepartmentActions';
import { useUIStore } from '@/app/store/uiStore';
import { useTheme } from '@/app/providers/theme-context';

export const DepartmentManagement: React.FC = () => {
    const [selectedRootId, setSelectedRootId] = useState<string>('');
    const [viewMode, setViewMode] = useState<'full' | 'subtree' | 'children'>('full');
    const [highlightId, setHighlightId] = useState<string>('');
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    
    const { showConfirm } = useUIStore();
    const { theme } = useTheme();
    const isDark = theme === 'dark';
    const { createDepartment, deleteDepartment, isLoading: isActionLoading } = useDepartmentActions();

    // Fetch all departments for selection
    const { data: allDepartments, isLoading: isAllLoading } = useQuery({
        queryKey: queryKeys.department.lists(),
        queryFn: () => departmentApi.search('', 0)
    });

    const departmentOptions: SearchableSelectOption[] = useMemo(() => {
        return (allDepartments || []).map(d => ({
            value: d.id,
            label: d.name,
            order: d.level,
            meta: { level: d.level }
        }));
    }, [allDepartments]);

    const { data: treeData, isLoading: isTreeLoading, refetch } = useDepartmentTree(
        viewMode === 'full' ? undefined : selectedRootId,
        viewMode
    );

    const handleRootChange = (id: string) => {
        setSelectedRootId(id);
        setHighlightId(id);
        if (viewMode === 'full') setViewMode('subtree');
    };

    const handleCreate = async (data: any) => {
        await createDepartment(data);
        setIsCreateOpen(false);
    };

    const handleDelete = () => {
        if (!selectedRootId) return;
        
        showConfirm({
            title: 'تأكيد الحذف',
            message: 'هل أنت متأكد من حذف هذا القسم؟ لا يمكن التراجع عن هذا الإجراء.',
            variant: 'destructive',
            confirmLabel: 'حذف',
            onConfirm: async () => {
                await deleteDepartment(selectedRootId);
                setSelectedRootId('');
                setHighlightId('');
            }
        });
    };

    return (
        <div className="space-y-6 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
                <div>
                    <h2 className="text-2xl font-bold tracking-tight">إدارة الهيكل التنظيمي</h2>
                    <p className="text-muted-foreground">عرض وإدارة أقسام المؤسسة وعلاقاتها الهرمية</p>
                </div>
                <div className="flex gap-2">
                    {selectedRootId && (
                        <Button variant="destructive" size="sm" className="gap-2" onClick={handleDelete}>
                            <Trash2 className="w-4 h-4" />
                            حذف المحدد
                        </Button>
                    )}
                    <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
                        <DialogTrigger asChild>
                            <Button className="gap-2" size="sm">
                                <Plus className="w-4 h-4" />
                                إضافة قسم جديد
                            </Button>
                        </DialogTrigger>
                        <DialogContent className="sm:max-w-[500px]">
                            <DialogHeader>
                                <DialogTitle>إضافة قسم جديد</DialogTitle>
                                <DialogDescription>أدخل تفاصيل القسم الجديد ومكانه في الهيكل التنظيمي</DialogDescription>
                            </DialogHeader>
                            <DepartmentForm 
                                onSubmit={handleCreate} 
                                parentOptions={departmentOptions} 
                                isLoading={isActionLoading}
                            />
                        </DialogContent>
                    </Dialog>
                </div>
            </div>

            <Card className="border-primary/10 shadow-lg bg-background/50 backdrop-blur-sm">
                <CardHeader>
                    <div className="flex justify-between items-center">
                        <div className="space-y-1">
                            <CardTitle>متصفح الأقسام</CardTitle>
                            <CardDescription>استخدم الفلاتر أدناه لتغيير طريقة عرض الشجرة</CardDescription>
                        </div>
                        <Button variant="ghost" size="icon" onClick={() => refetch()} disabled={isTreeLoading}>
                            <RefreshCw className={`w-4 h-4 ${isTreeLoading ? 'animate-spin' : ''}`} />
                        </Button>
                    </div>
                </CardHeader>
                <CardContent className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div className="space-y-2">
                            <label className="text-sm font-medium">القسم المستهدف</label>
                            <SearchableSelect
                                options={departmentOptions}
                                value={selectedRootId}
                                onValueChange={handleRootChange}
                                placeholder="اختر قسماً لتحديده في الشجرة..."
                                isLoading={isAllLoading}
                            />
                        </div>

                        <div className="space-y-2">
                            <label className="text-sm font-medium">طريقة العرض</label>
                            <div className="flex gap-2">
                                <Button 
                                    variant={viewMode === 'full' ? 'default' : 'outline'} 
                                    size="sm" 
                                    className="flex-1 gap-2"
                                    onClick={() => setViewMode('full')}
                                >
                                    <Layers className="w-4 h-4" />
                                    الشجرة الكاملة
                                </Button>
                                <Button 
                                    variant={viewMode === 'subtree' ? 'default' : 'outline'} 
                                    size="sm" 
                                    className="flex-1 gap-2"
                                    disabled={!selectedRootId}
                                    onClick={() => setViewMode('subtree')}
                                >
                                    <GitBranch className="w-4 h-4" />
                                    الشجرة الفرعية
                                </Button>
                                <Button 
                                    variant={viewMode === 'children' ? 'default' : 'outline'} 
                                    size="sm" 
                                    className="flex-1 gap-2"
                                    disabled={!selectedRootId}
                                    onClick={() => setViewMode('children')}
                                >
                                    <GitPullRequest className="w-4 h-4" />
                                    الأبناء فقط
                                </Button>
                            </div>
                        </div>
                    </div>

                    <div className="relative min-h-[400px] border rounded-xl overflow-hidden bg-muted/20">
                        <DepartmentMermaidTree 
                            data={treeData || []} 
                            highlightId={highlightId} 
                            isLoading={isTreeLoading} 
                        />
                    </div>

                    <div className="flex flex-wrap gap-4 text-xs text-muted-foreground pt-2">
                        <div className="flex items-center gap-1.5">
                            <div className={`w-3 h-3 rounded-full ${isDark ? 'bg-[#059669]' : 'bg-[#10b981]'}`} />
                            <span>قسم رئيسي (مستوى 1)</span>
                        </div>
                        <div className="flex items-center gap-1.5">
                            <div className={`w-3 h-3 rounded-full ${isDark ? 'bg-[#3b82f6]' : 'bg-[#2563eb]'}`} />
                            <span>القسم المحدد</span>
                        </div>
                        <div className="flex items-center gap-1.5">
                            <div className={`w-3 h-3 rounded-full ${isDark ? 'bg-[#1f2937]' : 'bg-[#ffffff]'} border border-muted`} />
                            <span>قسم إداري</span>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
};
