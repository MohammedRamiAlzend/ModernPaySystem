import { useState } from 'react';
import { useUsers, useSubSystems, useUserMutations, User } from '../api/usersApi';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Card, CardContent } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import {
    Plus,
    Trash2,
    Edit2,
    Loader2,
    Users,
    Filter,
    Shield
} from 'lucide-react';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
    DialogDescription
} from '@/shared/ui/dialog';
import { Label } from '@/shared/ui/label';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/ui/select';
import { useUIStore } from '@/app/store/uiStore';
import { useAuthStore } from '@/app/store/authStore';
import { cn } from '@/shared/lib/utils';
import { APP_CONFIG } from '@/shared/config/appConfig';

export const UserManagement = () => {
    const { showStatus, showConfirm } = useUIStore();
    const currentUserSubsystem = useAuthStore((state) => state.user?.subsystem);
    const [selectedSubSystem, setSelectedSubSystem] = useState<string>(APP_CONFIG.DEFAULT_SUB_SYSTEM_ID);
    const [isUserDialogOpen, setIsUserDialogOpen] = useState(false);
    const [editingUser, setEditingUser] = useState<User | null>(null);

    // Form State
    const [userName, setUserName] = useState('');
    const [password, setPassword] = useState('');
    const [userSubSystem, setUserSubSystem] = useState<string>(APP_CONFIG.DEFAULT_SUB_SYSTEM_ID);

    const { data: users = [], isLoading: isLoadingUsers } = useUsers(selectedSubSystem);
    const { data: subSystems = [], isLoading: isLoadingSubSystems } = useSubSystems();
    const { createUser, updateUser, deleteUser } = useUserMutations();

    const handleAddUser = () => {
        setEditingUser(null);
        setUserName('');
        setPassword('');
        // إذا كان المستخدم الحالي لديه نظام فرعي محدد، يتم تعيينه تلقائياً وإجباري
        setUserSubSystem(currentUserSubsystem || APP_CONFIG.DEFAULT_SUB_SYSTEM_ID);
        setIsUserDialogOpen(true);
    };

    const handleEditUser = (user: User) => {
        setEditingUser(user);
        setUserName(user.userName);
        setPassword('');
        setUserSubSystem(user.subSystem?.toString() || APP_CONFIG.DEFAULT_SUB_SYSTEM_ID);
        setIsUserDialogOpen(true);
    };

    const handleSaveUser = async () => {
        if (!userName.trim()) {
            showStatus({ type: 'error', title: 'خطأ', message: 'يرجى إدخال اسم المستخدم' });
            return;
        }

        if (!editingUser && !password.trim()) {
            showStatus({ type: 'error', title: 'خطأ', message: 'يرجى إدخال كلمة المرور' });
            return;
        }

        const targetSubSystemValue = subSystems.find(ss =>
            ss.value.toString() == userSubSystem.toString() || ss.name.toString() == userSubSystem.toString()
        )?.value || "0";
        // console.log(subSystems, targetSubSystemValue);
        try {
            if (editingUser) {
                await updateUser.mutateAsync({
                    id: editingUser.id,
                    userName,
                    password: password.trim() || undefined,
                    // subSystem: parseInt(userSubSystem)
                    subSystem: parseInt(targetSubSystemValue)
                });
                showStatus({ type: 'success', title: 'نجاح', message: 'تم تحديث المستخدم بنجاح' });
            } else {
                await createUser.mutateAsync({
                    userName,
                    password,
                    // subSystem: parseInt(userSubSystem)
                    subSystem: parseInt(targetSubSystemValue)
                });
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
                {APP_CONFIG.SHOW_SUB_SYSTEM && (
                    <div className="flex items-center gap-2 bg-muted/30 p-1.5 rounded-xl border border-muted-foreground/10">
                        <Filter className="w-4 h-4 text-muted-foreground mr-2" />
                        <Select value={selectedSubSystem} onValueChange={setSelectedSubSystem}>
                            <SelectTrigger className="w-[180px] h-9 border-none bg-transparent shadow-none focus:ring-0">
                                <SelectValue placeholder={isLoadingSubSystems ? "جاري التحميل..." : "تصفية حسب النظام"} />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="all">الكل</SelectItem>
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
                                    {APP_CONFIG.SHOW_SUB_SYSTEM && <TableHead className="text-right font-bold">النظام الفرعي</TableHead>}
                                    <TableHead className="text-right font-bold">تاريخ الإنشاء</TableHead>
                                    <TableHead className="text-left font-bold w-28">الإجراءات</TableHead>
                                </TableRow>
                            </TableHeader>
                            <TableBody>
                                {isLoadingUsers ? (
                                    <TableRow>
                                        <TableCell colSpan={5} className="h-48 text-center">
                                            <div className="flex flex-col items-center gap-2">
                                                <Loader2 className="w-8 h-8 animate-spin text-primary" />
                                                <span className="text-muted-foreground animate-pulse">جاري تحميل المستخدمين...</span>
                                            </div>
                                        </TableCell>
                                    </TableRow>
                                ) : users.length > 0 ? (
                                    users.map((user, index) => (
                                        <TableRow key={user.id} className="hover:bg-muted/30 transition-colors group">
                                            <TableCell className="text-right font-mono text-muted-foreground">{index + 1}</TableCell>
                                            <TableCell className="text-right">
                                                <div className="flex items-center gap-2">
                                                    <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center text-primary font-bold text-xs">
                                                        {user.userName.charAt(0).toUpperCase()}
                                                    </div>
                                                    <span className="font-bold">{user.userName}</span>
                                                </div>
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
                                                        className="h-8 hidden w-8 rounded-lg hover:bg-primary/10 hover:text-primary"
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
                                        <TableCell colSpan={5} className="text-center h-48">
                                            <div className="flex flex-col items-center justify-center text-muted-foreground gap-2">
                                                <Users className="w-12 h-12 opacity-20" />
                                                <p className="text-lg font-medium">لا يوجد مستخدمين مضافين</p>
                                                <p className="text-sm">أضف أول مستخدم للبدء في إدارة النظام</p>
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

                    <div className="space-y-4 py-4">
                        <div className="space-y-2">
                            <Label htmlFor="userName">اسم المستخدم</Label>
                            <Input
                                id="userName"
                                value={userName}
                                onChange={(e) => setUserName(e.target.value)}
                                placeholder="أدخل اسم المستخدم"
                                className="rounded-xl h-11 border-muted-foreground/20 focus:border-primary"
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="password">كلمة المرور {editingUser && '(اتركه فارغاً إذا كنت لا تريد تغييره)'}</Label>
                            <Input
                                id="password"
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                placeholder="أدخل كلمة المرور"
                                className="rounded-xl h-11 border-muted-foreground/20 focus:border-primary"
                            />
                        </div>
                        {!currentUserSubsystem && APP_CONFIG.SHOW_SUB_SYSTEM &&
                            <div className="space-y-2">
                                <Label htmlFor="subSystem">النظام الفرعي</Label>
                                <Select value={userSubSystem} onValueChange={setUserSubSystem}>
                                    <SelectTrigger
                                        disabled={!!currentUserSubsystem}
                                        className="rounded-xl h-11 border-muted-foreground/20 focus:border-primary disabled:opacity-80 disabled:bg-muted"
                                    >
                                        <SelectValue placeholder="اختر النظام" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {subSystems.map(ss => (
                                            <SelectItem key={ss.value} value={ss.value}>{ss.name}</SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>
                        }
                    </div>

                    <DialogFooter className="gap-2 sm:gap-0 mt-2">
                        <Button
                            onClick={handleSaveUser}
                            className="rounded-xl h-11 px-8 font-bold flex-1 sm:flex-none shadow-lg shadow-primary/20"
                            disabled={createUser.isPending || updateUser.isPending}
                        >
                            {(createUser.isPending || updateUser.isPending) ? (
                                <Loader2 className="w-4 h-4 animate-spin" />
                            ) : (
                                'حفظ البيانات'
                            )}
                        </Button>
                        <Button
                            variant="outline"
                            onClick={() => setIsUserDialogOpen(false)}
                            className="rounded-xl h-11 px-8 border-none bg-muted hover:bg-muted/80 flex-1 sm:flex-none"
                        >
                            إلغاء
                        </Button>
                    </DialogFooter>
                </DialogContent>
            </Dialog>
        </div>
    );
};
