import React, { useState, useMemo } from 'react';
import { 
    useDepartmentTree, 
    DepartmentMermaidTree, 
    DepartmentForm, 
    useDepartmentActions, 
    DepartmentTemplatesTab 
} from '@/features/department-management';
import { SearchableSelect, SearchableSelectOption } from '@/shared/ui/searchable-select';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui/card';
import { GitBranch, GitPullRequest, Plus, RefreshCw, Layers, Trash2, Crown } from 'lucide-react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { departmentApi } from '@/entities/department/api/departmentApi';
import { queryKeys } from '@/shared/constants/query-keys';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger, DialogDescription } from '@/shared/ui/dialog';
import { useUIStore } from '@/app/store/uiStore';
import { useTheme } from '@/app/providers/theme-context';
import { useSearchParams } from 'react-router-dom';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetDescription } from '@/shared/ui/sheet';
import { Building2 as BuildingIcon, Users as UsersIcon, FileStack } from 'lucide-react';
import { Avatar, AvatarFallback } from '@/shared/ui/avatar';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { User, UserForm, UserFormValues, useUserMutations, useSubSystems, useUsers } from '@/features/users';
import { UserPlus } from 'lucide-react';

export const DepartmentDashboardWidget: React.FC = () => {
    const [selectedRootId, setSelectedRootId] = useState<string>('');
    const [viewMode, setViewMode] = useState<'full' | 'subtree' | 'children'>('full');
    const [highlightId, setHighlightId] = useState<string>('');
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [selectedDeptForUsers, setSelectedDeptForUsers] = useState<string | null>(null);
    const [initialParentId, setInitialParentId] = useState<string>('');
    const [isParentFixed, setIsParentFixed] = useState(false);
    const [isAddUserOpen, setIsAddUserOpen] = useState(false);

    const [searchParams] = useSearchParams();
    const urlHighlightId = searchParams.get('highlightId');

    const { showConfirm, showStatus } = useUIStore();
    const { theme } = useTheme();
    const isDark = theme === 'dark';
    const { createDepartment, deleteDepartment, assignDepartmentHead, isLoading: isActionLoading } = useDepartmentActions();

    // Fetch all departments for selection
    const { data: allDepartments, isLoading: isAllLoading } = useQuery({
        queryKey: queryKeys.department.lists(),
        queryFn: () => departmentApi.search('', 0)
    });

    const { data: subSystems = [] } = useSubSystems();
    const { createUser } = useUserMutations();
    const { assignUserToDepartment } = useDepartmentActions();
    const { refetch: refetchAllUsers } = useUsers();

    const queryClient = useQueryClient();

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

    // Handle deep linking from users page - sync during render
    const [prevUrlHighlightId, setPrevUrlHighlightId] = React.useState(urlHighlightId);
    if (urlHighlightId !== prevUrlHighlightId) {
        setPrevUrlHighlightId(urlHighlightId);
        if (urlHighlightId) {
            setHighlightId(urlHighlightId);
            setSelectedRootId(urlHighlightId);
            setViewMode('subtree');
        }
    }

    // Fetch users for the side panel
    const { data: deptUsers, isLoading: isUsersLoading } = useQuery({
        queryKey: ['department-users', selectedDeptForUsers],
        queryFn: () => selectedDeptForUsers ? departmentApi.getUsers(selectedDeptForUsers) : [],
        enabled: !!selectedDeptForUsers
    });

    // Fetch department details to know the current head
    const { data: currentDepartment } = useQuery({
        queryKey: ['department', selectedDeptForUsers],
        queryFn: () => selectedDeptForUsers ? departmentApi.getById(selectedDeptForUsers) : null,
        enabled: !!selectedDeptForUsers
    });

    const selectedDeptName = useMemo(() => {
        return departmentOptions.find(d => d.value === selectedDeptForUsers)?.label;
    }, [selectedDeptForUsers, departmentOptions]);

    const handleCreate = async (data: any) => {
        await createDepartment(data);
        setIsCreateOpen(false);
        setInitialParentId('');
        setIsParentFixed(false);
    };

    const handleSaveUser = async (values: UserFormValues) => {
        if (!selectedDeptForUsers) return;

        try {
            // 1. Create the user
            await createUser.mutateAsync({
                userName: values.userName,
                password: values.password || undefined,
                subSystem: parseInt(values.subSystem)
            });

            // 2. Fetch fresh users list to find the new ID
            const users = await refetchAllUsers();
            const newUser = users.data?.find(u => u.userName === values.userName);

            if (newUser) {
                // 3. Assign to this department
                await assignUserToDepartment({
                    userId: newUser.id,
                    departmentId: selectedDeptForUsers
                });

                showStatus({
                    type: 'success',
                    title: 'تمت الإضافة',
                    message: `تم إنشاء المستخدم "${values.userName}" وتعيينه للقسم بنجاح`
                });
                setIsAddUserOpen(false);
                // Refresh the department users list
                queryClient.invalidateQueries({ queryKey: ['department-users', selectedDeptForUsers] });
            }
        } catch (error) {
            console.error("Failed to create and assign user:", error);
        }
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
                    <Dialog open={isCreateOpen} onOpenChange={(open) => {
                        setIsCreateOpen(open);
                        if (!open) {
                            setInitialParentId('');
                            setIsParentFixed(false);
                        }
                    }}>
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
                                initialData={initialParentId ? { parentDepartmentId: initialParentId } : undefined}
                                isParentDisabled={isParentFixed}
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
                            onNodeClick={(id: string) => setSelectedDeptForUsers(id)}
                        />
                    </div>

                    <Sheet open={!!selectedDeptForUsers} onOpenChange={(open) => !open && setSelectedDeptForUsers(null)}>
                        <SheetContent className="sm:max-w-md">
                            <SheetHeader className="text-right pr-8">
                                <div className="flex flex-col gap-4">
                                    <SheetTitle className="flex items-center gap-2 justify-end text-xl">
                                        <span>{selectedDeptName}</span>
                                        <BuildingIcon className="w-6 h-6 text-primary" />
                                    </SheetTitle>

                                    <div className="flex gap-2 justify-end">
                                        <Button
                                            size="sm"
                                            variant="destructive"
                                            className="gap-2 h-9"
                                            onClick={() => {
                                                if (selectedDeptForUsers) {
                                                    showConfirm({
                                                        title: 'تأكيد الحذف',
                                                        message: `هل أنت متأكد من حذف قسم "${selectedDeptName}"؟ لا يمكن التراجع عن هذا الإجراء.`,
                                                        variant: 'destructive',
                                                        confirmLabel: 'حذف',
                                                        onConfirm: async () => {
                                                            await deleteDepartment(selectedDeptForUsers);
                                                            setSelectedDeptForUsers(null);
                                                        }
                                                    });
                                                }
                                            }}
                                        >
                                            <Trash2 className="w-4 h-4" />
                                            حذف القسم
                                        </Button>

                                        <Button
                                            size="sm"
                                            variant="outline"
                                            className="gap-2 h-9 border-primary/20 hover:bg-primary/5"
                                            onClick={() => setIsAddUserOpen(true)}
                                        >
                                            <UserPlus className="w-4 h-4" />
                                            إضافة مستخدم
                                        </Button>

                                        <Button
                                            size="sm"
                                            variant="outline"
                                            className="gap-2 h-9 border-primary/20 hover:bg-primary/5"
                                            onClick={() => {
                                                if (selectedDeptForUsers) {
                                                    setInitialParentId(selectedDeptForUsers);
                                                    setIsParentFixed(true);
                                                    setIsCreateOpen(true);
                                                }
                                            }}
                                        >
                                            <Plus className="w-4 h-4" />
                                            إضافة قسم تابع
                                        </Button>
                                    </div>
                                </div>
                                <SheetDescription className="text-right">
                                    إدارة بيانات الموظفين والصلاحيات والهيكل التنظيمي لقسم {selectedDeptName}
                                </SheetDescription>
                            </SheetHeader>

                            <div className="mt-6">
                                <Tabs defaultValue="users" className="w-full">
                                    <TabsList className="grid w-full grid-cols-2">
                                        <TabsTrigger value="users" className="gap-2">
                                            <UsersIcon className="w-4 h-4" /> المستخدمين
                                        </TabsTrigger>
                                        <TabsTrigger value="templates" className="gap-2">
                                            <FileStack className="w-4 h-4" /> النماذج والصلاحيات
                                        </TabsTrigger>
                                    </TabsList>

                                    <TabsContent value="users" className="mt-4">
                                        {isUsersLoading ? (
                                            <div className="flex flex-col items-center justify-center py-20 gap-4">
                                                <RefreshCw className="w-8 h-8 animate-spin text-primary/50" />
                                                <p className="text-sm text-muted-foreground">جاري تحميل قائمة المستخدمين...</p>
                                            </div>
                                        ) : (deptUsers || []).length > 0 ? (
                                            <div className="h-[calc(100vh-280px)] overflow-y-auto pr-4 custom-scrollbar">
                                                <div className="space-y-4">
                                                    {(deptUsers as User[]).map((user) => (
                                                        <div key={user.id} className="flex items-center justify-between p-3 rounded-lg border bg-card hover:shadow-md transition-all group">
                                                            <div className="flex items-center gap-3">
                                                                <Avatar className="h-10 w-10 border-2 border-primary/10 group-hover:border-primary/30 transition-colors">
                                                                    <AvatarFallback className="bg-primary/5 text-primary font-bold">
                                                                        {user.userName.substring(0, 2).toUpperCase()}
                                                                    </AvatarFallback>
                                                                </Avatar>
                                                                <div className="text-right">
                                                                    <p className="text-sm font-bold">{user.userName}</p>
                                                                    <div className="flex items-center gap-1 text-[10px] text-muted-foreground">
                                                                        <BuildingIcon className="w-3 h-3" />
                                                                        <span>{selectedDeptName}</span>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                            <div className="flex items-center gap-2">
                                                                <Button variant="ghost" size="sm" className="h-8 text-xs gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                                                    <RefreshCw className="w-3 h-3" />
                                                                    ملف المستخدم
                                                                </Button>

                                                            {currentDepartment?.departmentHeadId === user.id ? (
                                                                <div className="flex items-center gap-1 text-amber-500 bg-amber-500/10 px-2 py-1 rounded text-[10px] font-bold">
                                                                    <Crown className="w-3 h-3" />
                                                                    رئيس القسم
                                                                </div>
                                                            ) : (
                                                                <Button 
                                                                    variant="ghost" 
                                                                    size="sm" 
                                                                    className="h-8 text-xs gap-1 opacity-0 group-hover:opacity-100 transition-opacity hover:text-amber-500 hover:bg-amber-500/10"
                                                                    onClick={() => {
                                                                        showConfirm({
                                                                            title: 'تعيين رئيس قسم',
                                                                            message: `هل أنت متأكد من تعيين "${user.userName}" رئيساً لقسم "${selectedDeptName}"؟`,
                                                                            variant: 'warning',
                                                                            confirmLabel: 'تعيين كرئيس',
                                                                            onConfirm: async () => {
                                                                                await assignDepartmentHead({
                                                                                    departmentId: selectedDeptForUsers!,
                                                                                    userId: user.id
                                                                                });
                                                                                queryClient.invalidateQueries({ queryKey: ['department', selectedDeptForUsers] });
                                                                            }
                                                                        });
                                                                    }}
                                                                >
                                                                    <Crown className="w-3 h-3" />
                                                                    تعيين كرئيس
                                                                </Button>
                                                            )}
                                                            </div>
                                                        </div>
                                                    ))}
                                                </div>
                                            </div>
                                        ) : (
                                            <div className="flex flex-col items-center justify-center py-20 text-center gap-4 border-2 border-dashed rounded-xl">
                                                <UsersIcon className="w-12 h-12 text-muted-foreground/20" />
                                                <div className="space-y-1">
                                                    <p className="font-medium text-muted-foreground">لا يوجد مستخدمين</p>
                                                    <p className="text-xs text-muted-foreground/60">لم يتم تعيين أي موظف في هذا القسم حالياً</p>
                                                </div>
                                            </div>
                                        )}
                                    </TabsContent>

                                    <TabsContent value="templates" className="mt-4">
                                        <div className="h-[calc(100vh-280px)] overflow-y-auto pr-2 custom-scrollbar">
                                            {selectedDeptForUsers && (
                                                <DepartmentTemplatesTab
                                                    departmentId={selectedDeptForUsers}
                                                    departmentName={selectedDeptName}
                                                />
                                            )}
                                        </div>
                                    </TabsContent>
                                </Tabs>
                            </div>
                        </SheetContent>
                    </Sheet>

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

            {/* Add User to Department Dialog */}
            <Dialog open={isAddUserOpen} onOpenChange={setIsAddUserOpen}>
                <DialogContent className="rounded-2xl max-w-md" style={{ direction: 'rtl' }}>
                    <DialogHeader>
                        <DialogTitle className="flex items-center gap-2">
                            <UserPlus className="w-5 h-5" />
                            إضافة مستخدم إلى {selectedDeptName}
                        </DialogTitle>
                        <DialogDescription>
                            سيتم إنشاء حساب المستخدم وتعيينه تلقائياً لهذا القسم
                        </DialogDescription>
                    </DialogHeader>

                    <UserForm
                        onSubmit={handleSaveUser}
                        subSystems={subSystems as any}
                        isLoading={createUser.isPending}
                    />
                </DialogContent>
            </Dialog>
        </div>
    );
};
