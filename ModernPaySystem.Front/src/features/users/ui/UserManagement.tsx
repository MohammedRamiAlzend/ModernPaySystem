import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useUsers, useSubSystems, useUserMutations, User, UserCreateDto } from '../api/usersApi';
import { Button } from '@/shared/ui/button';
import { Card, CardContent } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import {
    Plus,
    Trash2,
    Edit2,
    Loader2,
    Users,
    Filter,
    Shield,
    Building2
} from 'lucide-react';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription
} from '@/shared/ui/dialog';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/ui/select';
import { useUIStore } from '@/app/store/uiStore';
import { useAuthStore } from '@/app/store/authStore';
import { APP_CONFIG } from '@/shared/config/appConfig';
import { AssignDepartmentDialog } from './AssignDepartmentDialog';
import { useDepartments } from '@/entities/department/model/useDepartments';
import { SearchableSelect } from '@/shared/ui/searchable-select';
import { UserForm, UserFormValues } from './UserForm';
import { DepartmentMermaidTree } from '@/features/department-management/ui/DepartmentMermaidTree';
import { useDepartmentTree } from '@/features/department-management/model/useDepartmentTree';

export const UserManagement = () => {
    const { showStatus, showConfirm } = useUIStore();
    const currentUserSubsystem = useAuthStore((state) => state.user?.subsystem);
    const [selectedSubSystem, setSelectedSubSystem] = useState<string>(APP_CONFIG.DEFAULT_SUB_SYSTEM_ID);
    const [selectedDepartmentId, setSelectedDepartmentId] = useState<string>('all');
    const [isUserDialogOpen, setIsUserDialogOpen] = useState(false);
    const [editingUser, setEditingUser] = useState<User | null>(null);
    const [assigningUser, setAssigningUser] = useState<User | null>(null);

    const { data: usersData = [], isLoading: isLoadingUsers } = useUsers(selectedSubSystem);
    const { data: subSystemsData = [], isLoading: isLoadingSubSystems } = useSubSystems();
    const { departmentOptions } = useDepartments();
    const { createUser, updateUser, deleteUser } = useUserMutations();
    const navigate = useNavigate();

    const [selectedDeptForTree, setSelectedDeptForTree] = useState<string | null>(null);
    const { data: treeData, isLoading: isTreeLoading } = useDepartmentTree(undefined, 'full');

    const subSystems = subSystemsData || [];
    const users = usersData || [];

    const filteredUsers = useMemo(() => {
        return users.filter(user => {
            const matchesSubSystem = selectedSubSystem === 'all' || user.subSystem?.toString() === selectedSubSystem;
            const matchesDepartment = selectedDepartmentId === 'all' || user.departmentId === selectedDepartmentId;
            return matchesSubSystem && matchesDepartment;
        });
    }, [users, selectedSubSystem, selectedDepartmentId]);

    const filterDepartmentOptions = useMemo(() => {
        return [{ value: 'all', label: 'كافة الأقسام' }, ...departmentOptions];
    }, [departmentOptions]);

    const handleAddUser = () => {
        setEditingUser(null);
        setIsUserDialogOpen(true);
    };

    const handleEditUser = (user: User) => {
        setEditingUser(user);
        setIsUserDialogOpen(true);
    };

    const handleSaveUser = async (values: UserFormValues) => {
        const targetSubSystemValue = subSystems.find(ss =>
            ss.value.toString() == values.subSystem.toString() || ss.name.toString() == values.subSystem.toString()
        )?.value || "0";

        try {
            const dto: UserCreateDto = {
                userName: values.userName,
                password: values.password || undefined,
                subSystem: parseInt(targetSubSystemValue)
            };

            if (editingUser) {
                await updateUser.mutateAsync({
                    id: editingUser.id,
                    ...dto
                });
                showStatus({ type: 'success', title: 'نجاح', message: 'تم تحديث المستخدم بنجاح' });
            } else {
                await createUser.mutateAsync(dto as Required<UserCreateDto>);
                showStatus({ type: 'success', title: 'نجاح', message: 'تم إضافة المستخدم بنجاح' });
            }
            setIsUserDialogOpen(false);
        } catch {
            showStatus({ type: 'error', title: 'خطأ', message: 'حدث خطأ أثناء حفظ البيانات' });
        }
    };

    const handleDeleteUserClick = (user: User) => {
        showConfirm({
            title: 'حذف المستخدم',
            message: `هل أنت متأكد من حذف المستخدم "${user.userName}"؟ لا يمكن التراجع عن هذا الإجراء.`,
            variant: 'destructive',
            confirmLabel: 'حذف',
            onConfirm: async () => {
                try {
                    await deleteUser.mutateAsync(user.id);
                    showStatus({ type: 'success', title: 'نجاح', message: 'تم حذف المستخدم بنجاح' });
                } catch {
                    showStatus({ type: 'error', title: 'خطأ', message: 'حدث خطأ أثناء الحذف' });
                }
            }
        });
    };

    return (
        <div className="space-y-6" style={{ direction: 'rtl' }}>
            {/* Header and Filters */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex items-center gap-3">
                    <div className="p-2 bg-primary/10 rounded-xl text-primary">
                        <Users className="w-6 h-6" />
                    </div>
                    <div>
                        <h2 className="text-xl font-bold">إدارة المستخدمين</h2>
                        <p className="text-sm text-muted-foreground">عرض وتعديل صلاحيات الوصول للمستخدمين</p>
                    </div>
                </div>

                <div className="flex items-center gap-3">
                    <div className="flex items-center gap-2 bg-muted/30 p-1.5 rounded-xl border border-muted-foreground/10">
                        <Building2 className="w-4 h-4 text-muted-foreground mr-2" />
                        <div className="w-[200px]">
                            <SearchableSelect
                                options={filterDepartmentOptions}
                                value={selectedDepartmentId}
                                onValueChange={setSelectedDepartmentId}
                                placeholder="تصفية حسب القسم"
                                className="h-9 border-none bg-transparent shadow-none focus:ring-0"
                            />
                        </div>
                    </div>

                    {APP_CONFIG.SHOW_SUB_SYSTEM && (
                        <div className="flex items-center gap-2 bg-muted/30 p-1.5 rounded-xl border border-muted-foreground/10">
                            <Filter className="w-4 h-4 text-muted-foreground mr-2" />
                            <Select value={selectedSubSystem} onValueChange={setSelectedSubSystem}>
                                <SelectTrigger className="w-[180px] h-9 border-none bg-transparent shadow-none focus:ring-0">
                                    <SelectValue placeholder={isLoadingSubSystems ? "جاري التحميل..." : "تصفية حسب النظام"} />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">كافة الأنظمة</SelectItem>
                                    {subSystems.map(ss => (
                                        <SelectItem key={ss.value} value={ss.value}>{ss.name}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>
                    )}
                    <Button onClick={handleAddUser} className="rounded-xl px-4 h-10 font-bold shadow-lg shadow-primary/20">
                        <Plus className="w-4 h-4 ml-2" />
                        إضافة مستخدم
                    </Button>
                </div>
            </div>

            {/* Users Table */}
            <Card className="border-none shadow-lg bg-card/50 backdrop-blur-sm overflow-hidden">
                <CardContent className="p-0">
                    <div className="overflow-auto max-h-[600px] custom-scrollbar">
                        <Table>
                            <TableHeader className="bg-muted/50">
                                <TableRow>
                                    <TableHead className="text-right font-bold w-16">#</TableHead>
                                    <TableHead className="text-right font-bold">اسم المستخدم</TableHead>
                                    <TableHead className="text-right font-bold">القسم</TableHead>
                                    {APP_CONFIG.SHOW_SUB_SYSTEM && <TableHead className="text-right font-bold">النظام الفرعي</TableHead>}
                                    <TableHead className="text-right font-bold">تاريخ الإنشاء</TableHead>
                                    <TableHead className="text-left font-bold w-28">الإجراءات</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {isLoadingUsers ? (
                                    <TableRow>
                                        <TableCell colSpan={6} className="h-48 text-center">
                                            <div className="flex flex-col items-center gap-2">
                                                <Loader2 className="w-8 h-8 animate-spin text-primary" />
                                                <span className="text-muted-foreground animate-pulse">جاري تحميل المستخدمين...</span>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ) : filteredUsers.length > 0 ? (
                                    filteredUsers.map((user, index) => (
                                        <TableRow key={user.id} className="hover:bg-muted/30 transition-colors group">
                                            <TableCell className="text-right font-mono text-muted-foreground">{index + 1}</TableCell>
                                            <TableCell className="text-right">
                                                <span className="font-bold">{user.userName}</span>
                                            </TableCell>
                                            <TableCell className="text-right">
                                                {user.departmentName ? (
                                                    <div
                                                        className="flex items-center gap-1.5 text-primary bg-primary/5 px-2 py-1 rounded-md w-fit cursor-pointer hover:bg-primary/10 transition-colors"
                                                        onClick={() => setSelectedDeptForTree(user.departmentId!)}
                                                        title="عرض في الهيكل التنظيمي"
                                                    >
                                                        <Building2 className="w-3.5 h-3.5" />
                                                        <span className="text-xs font-medium">{user.departmentName}</span>
                                                    </div>
                                                ) : (
                                                    <span className="text-xs text-muted-foreground italic">غير معين</span>
                                                )}
                                            </TableCell>
                                            {APP_CONFIG.SHOW_SUB_SYSTEM && (
                                                <TableCell className="text-right">
                                                    <div className="flex items-center gap-1.5">
                                                        <Shield className="w-3.5 h-3.5 text-muted-foreground" />
                                                        <span className="text-sm">
                                                            {subSystems.find(ss => ss.value === user.subSystem?.toString())?.name || user.subSystem || 'لا يوجد'}
                                                        </span>
                                                    </div>
                                                </TableCell>
                                            )}
                                            <TableCell className="text-right text-sm text-muted-foreground">
                                                {user.createdAt ? new Date(user.createdAt).toLocaleDateString('ar-EG') : '-'}
                                            </TableCell>
                                            <TableCell className="text-left">
                                                <div className="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-lg hover:bg-primary/10 hover:text-primary"
                                                        title="تعيين قسم"
                                                        onClick={() => setAssigningUser(user)}
                                                    >
                                                        <Building2 className="w-4 h-4" />
                                                    </Button>
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-lg hover:bg-primary/10 hover:text-primary"
                                                        title="تعديل المستخدم"
                                                        onClick={() => handleEditUser(user)}
                                                    >
                                                        <Edit2 className="w-4 h-4" />
                                                    </Button>
                                                    <Button
                                                        variant="ghost"
                                                        size="icon"
                                                        className="h-8 w-8 rounded-lg hover:bg-destructive/10 hover:text-destructive"
                                                        onClick={() => handleDeleteUserClick(user)}
                                                    >
                                                        <Trash2 className="w-4 h-4" />
                                                    </Button>
                                                </div>
                                            </TableCell>
                                        </TableRow>
                                    ))
                                ) : (
                                    <TableRow>
                                        <TableCell colSpan={6} className="text-center h-48">
                                            <div className="flex flex-col items-center justify-center text-muted-foreground gap-2">
                                                <Users className="w-12 h-12 opacity-20" />
                                                <p className="text-lg font-medium">لا يوجد مستخدمين مطابقين للبحث</p>
                                                <p className="text-sm">حاول تغيير خيارات التصفية أعلاه</p>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                )}
                            </TableBody>
                        </Table>
                    </div>
                </CardContent>
            </Card>

            {/* Add/Edit User Dialog */}
            <Dialog open={isUserDialogOpen} onOpenChange={setIsUserDialogOpen}>
                <DialogContent className="rounded-2xl max-w-md" style={{ direction: 'rtl' }}>
                    <DialogHeader>
                        <DialogTitle className="flex items-center gap-2">
                            {editingUser ? <Edit2 className="w-5 h-5" /> : <Plus className="w-5 h-5" />}
                            {editingUser ? 'تعديل بيانات المستخدم' : 'إضافة مستخدم جديد'}
                        </DialogTitle>
                        <DialogDescription>
                            {editingUser ? `تعديل بيانات المستخدم ${editingUser.userName}` : 'أدخل بيانات المستخدم الجديد وقم بتعيين النظام التابع له'}
                        </DialogDescription>
                    </DialogHeader>

                    <UserForm
                        onSubmit={handleSaveUser}
                        initialData={editingUser}
                        subSystems={subSystems}
                        currentUserSubsystem={currentUserSubsystem}
                        isLoading={createUser.isPending || updateUser.isPending}
                    />
                </DialogContent>
            </Dialog>

            <AssignDepartmentDialog
                user={assigningUser}
                open={!!assigningUser}
                onOpenChange={(open) => !open && setAssigningUser(null)}
                onAssigned={() => setAssigningUser(null)}
            />

            {/* Department Tree Preview Dialog */}
            <Dialog open={!!selectedDeptForTree} onOpenChange={(open) => !open && setSelectedDeptForTree(null)}>
                <DialogContent className="sm:max-w-5xl h-[85vh] flex flex-col p-0 overflow-hidden border-none shadow-2xl" style={{ direction: 'rtl' }}>
                    <DialogHeader className="p-6 bg-primary text-primary-foreground shrink-0">
                        <div className="flex items-center gap-4 justify-start text-right w-full">
                            <div className="p-3 bg-white/10 rounded-2xl">
                                <Building2 className="w-8 h-8" />
                            </div>
                            <div className="flex flex-col gap-1">
                                <DialogTitle className="text-2xl font-black leading-none">موقع القسم في الهيكل التنظيمي</DialogTitle>
                                <DialogDescription className="text-primary-foreground/80 font-medium">
                                    معاينة بصرية كاملة للهيكل الإداري مع تمييز القسم المختار
                                </DialogDescription>
                            </div>
                        </div>
                    </DialogHeader>
                    
                    <div className="flex-1 p-4 bg-muted/30 overflow-hidden relative flex flex-col">
                        <div className="flex-1 bg-background/50 rounded-2xl border-2 border-dashed border-primary/10 overflow-hidden relative">
                            <DepartmentMermaidTree
                                data={treeData || []}
                                highlightId={selectedDeptForTree || undefined}
                                isLoading={isTreeLoading}
                                onNodeClick={(id) => {
                                    setSelectedDepartmentId(id);
                                    setSelectedDeptForTree(null);
                                }}
                            />
                        </div>
                        
                        <div className="mt-4 flex flex-row-reverse justify-between items-center bg-background/80 backdrop-blur-md p-4 rounded-2xl border shadow-lg">
                            <div className="flex flex-row-reverse gap-6 text-sm font-bold">
                                <div className="flex items-center gap-2">
                                    <div className="w-4 h-4 rounded-full bg-blue-600 shadow-sm" />
                                    <span>القسم المختار</span>
                                </div>
                                <div className="flex items-center gap-2">
                                    <div className="w-4 h-4 rounded-full bg-emerald-600 shadow-sm" />
                                    <span>أقسام رئيسية</span>
                                </div>
                            </div>
                            <Button 
                                variant="secondary" 
                                size="sm" 
                                className="font-bold px-6 rounded-xl hover:scale-105 transition-transform"
                                onClick={() => setSelectedDeptForTree(null)}
                            >
                                إغلاق المعاينة
                            </Button>
                        </div>
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    );
};
