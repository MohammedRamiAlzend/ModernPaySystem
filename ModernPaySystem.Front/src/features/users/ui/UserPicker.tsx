import { useState, useMemo } from 'react';
import { useAuthStore } from '@/app/store/authStore';
import { useUsers, useSubSystems, fetchUsersByCurrentDepartment, useUserMutations } from '../api/usersApi';
import { useQuery } from '@tanstack/react-query';
import { Label } from '@/shared/ui/label';
import { SearchableSelect } from '@/shared/ui/searchable-select';
import type { SearchableSelectOption } from '@/shared/ui/searchable-select';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/ui/select';
import { Shield, User as UserIcon, UserPlus } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { APP_CONFIG } from '@/shared/config/appConfig';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/shared/ui/dialog';
import { Input } from '@/shared/ui/input';
import { Button } from '@/shared/ui/button';
import { useUIStore } from '@/app/store/uiStore';

// ─── Single-select mode props ────────────────────────────────────────────────

interface UserPickerSingleProps {
    multiple?: false;
    /** Callback when a single user is selected */
    onUserSelect: (userId: string) => void;
    /** Currently selected user id */
    defaultValue?: string;
    /** Not used in single mode */
    selectedUserIds?: never;
    /** Not used in single mode */
    onUsersChange?: never;
}

// ─── Multi-select mode props ─────────────────────────────────────────────────

interface UserPickerMultiProps {
    multiple: true;
    /** Selected user IDs */
    selectedUserIds: string[];
    /** Callback when the set of selected users changes */
    onUsersChange: (userIds: string[]) => void;
    /** Not used in multi mode */
    onUserSelect?: never;
    /** Not used in multi mode */
    defaultValue?: never;
}

// ─── Shared props ────────────────────────────────────────────────────────────

interface UserPickerSharedProps {
    label?: string;
    placeholder?: string;
    subSystemPlaceholder?: string;
    defaultSubSystemId?: string;
    showCurrentUser?: boolean;
    className?: string;
    departmentOnly?: boolean;
    allowCreateUser?: boolean;
    isCreatingDepartmentHead?: boolean;
}

export type UserPickerProps = UserPickerSharedProps & (UserPickerSingleProps | UserPickerMultiProps);

// ─── Component ───────────────────────────────────────────────────────────────

export const UserPicker = (props: UserPickerProps) => {
    const {
        label = 'الموافق (Approver)',
        placeholder = 'اختر المستخدم...',
        subSystemPlaceholder = 'اختر النظام...',
        defaultSubSystemId = APP_CONFIG.DEFAULT_SUB_SYSTEM_ID,
        showCurrentUser = false,
        className,
        departmentOnly = false,
        allowCreateUser = false,
        isCreatingDepartmentHead = false,
    } = props;

    const isMulti = props.multiple === true;

    const { user: currentUserData } = useAuthStore();
    const [selectedSubSystem, setSelectedSubSystem] = useState<string>(defaultSubSystemId);

    // New state for creating user inline
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [newUserName, setNewUserName] = useState('');
    const [newUserPassword, setNewUserPassword] = useState('');

    const { createUser } = useUserMutations();
    const { showStatus } = useUIStore();

    const handleCreateUser = async (e: React.FormEvent) => {
        e.preventDefault();
        if (!newUserName.trim() || !newUserPassword.trim()) {
            showStatus({
                type: 'error',
                title: 'خطأ في المدخلات',
                message: 'يرجى إدخال اسم المستخدم وكلمة المرور'
            });
            return;
        }

        try {
            const systemId = parseInt(selectedSubSystem);
            const newUser = await createUser.mutateAsync({
                userName: newUserName.trim(),
                password: newUserPassword.trim(),
                subSystem: isNaN(systemId) ? 1 : systemId,
                isDepartmentHead: isCreatingDepartmentHead,
            });

            showStatus({
                type: 'success',
                title: 'تم إنشاء المستخدم',
                message: `تم إنشاء المستخدم "${newUser.userName}" بنجاح`
            });

            setIsCreateOpen(false);
            setNewUserName('');
            setNewUserPassword('');

            if (!isMulti && props.onUserSelect) {
                props.onUserSelect(newUser.id);
            }
        } catch (error: any) {
            const backendErrors = error.response?.data?.errors;
            let errorMessage = error.response?.data?.message || 'حدث خطأ أثناء إنشاء المستخدم';
            if (Array.isArray(backendErrors) && backendErrors.length > 0) {
                errorMessage = backendErrors[0].arabicDescription || backendErrors[0].description || errorMessage;
            }
            showStatus({
                type: 'error',
                title: 'خطأ في إنشاء المستخدم',
                message: errorMessage
            });
        }
    };

    const { data: subSystems = [], isLoading: isLoadingSubSystems } = useSubSystems();

    const { data: departmentUsers = [], isLoading: isLoadingDepartmentUsers } = useQuery({
        queryKey: ['users', 'current-department'],
        queryFn: fetchUsersByCurrentDepartment,
        enabled: departmentOnly,
    });

    const { data: rawUsers = [], isLoading: isLoadingUsers } = useUsers(
        departmentOnly ? undefined : selectedSubSystem
    );

    const sourceUsers = departmentOnly ? departmentUsers : rawUsers;
    const isLoading = departmentOnly ? isLoadingDepartmentUsers : isLoadingUsers;

    // Filter out current user if needed
    const users = useMemo(() => {
        if (showCurrentUser || !currentUserData) return sourceUsers;
        return sourceUsers.filter(u => u.id !== currentUserData.id);
    }, [sourceUsers, showCurrentUser, currentUserData]);

    // Convert users to SearchableSelect options
    const userOptions: SearchableSelectOption[] = useMemo(() => {
        return users.map((user, index) => ({
            value: user.id,
            label: user.userName,
            order: index + 1,
            icon: <UserIcon className="w-3.5 h-3.5 text-primary/60" />,
        }));
    }, [users]);

    const handleSubSystemChange = (value: string) => {
        setSelectedSubSystem(value);
        // Reset selection when subsystem changes
        if (isMulti) {
            props.onUsersChange([]);
        } else {
            props.onUserSelect('');
        }
    };

    return (
        <div className={cn('grid grid-cols-1 md:grid-cols-2 gap-4', className)}>
            {/* SubSystem selector */}
            {!departmentOnly && APP_CONFIG.SHOW_SUB_SYSTEM && (
                <div className="space-y-2">
                    <Label className="text-xs font-bold text-muted-foreground flex items-center gap-2">
                        <Shield className="w-3 h-3" />
                        النظام الفرعي
                    </Label>
                    <Select value={selectedSubSystem} onValueChange={handleSubSystemChange}>
                        <SelectTrigger className="h-10 rounded-xl bg-background/50 backdrop-blur-sm border-primary/10">
                            <SelectValue placeholder={isLoadingSubSystems ? 'جاري التحميل...' : subSystemPlaceholder} />
                        </SelectTrigger>
                        <SelectContent className="rounded-xl border-primary/10">
                            {subSystems.map(ss => (
                                <SelectItem key={ss.value} value={ss.value}>{ss.name}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>
            )}

            {/* User selector – single or multi via SearchableSelect */}
            <div className="space-y-2">
                <div className="flex justify-between items-center">
                    <Label className="text-xs font-bold text-muted-foreground flex items-center gap-2">
                        <UserIcon className="w-3 h-3" />
                        {label}
                    </Label>
                    {!isMulti && allowCreateUser && (
                        <button
                            type="button"
                            onClick={() => setIsCreateOpen(true)}
                            className="text-xs text-primary hover:underline font-bold transition-all"
                        >
                            + إنشاء مستخدم جديد
                        </button>
                    )}
                </div>

                {isMulti ? (
                    <SearchableSelect
                        multiple
                        options={userOptions}
                        values={props.selectedUserIds}
                        onValuesChange={props.onUsersChange}
                        placeholder={placeholder}
                        searchPlaceholder="ابحث بالاسم أو الترتيب..."
                        emptyMessage="لا يوجد مستخدمين لهذا النظام"
                        isLoading={isLoading}
                    />
                ) : (
                    <SearchableSelect
                        options={userOptions}
                        value={props.defaultValue || ''}
                        onValueChange={props.onUserSelect}
                        placeholder={placeholder}
                        searchPlaceholder="ابحث بالاسم أو الترتيب..."
                        emptyMessage="لا يوجد مستخدمين لهذا النظام"
                        isLoading={isLoading}
                    />
                )}
            </div>

            {/* Inline User Creation Dialog */}
            <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
                <DialogContent className="rounded-2xl max-w-sm" style={{ direction: 'rtl' }}>
                    <DialogHeader>
                        <DialogTitle className="flex items-center gap-2 text-right">
                            <UserPlus className="w-5 h-5 text-primary" />
                            إنشاء مستخدم جديد
                        </DialogTitle>
                        <DialogDescription className="text-right">
                            سيتم إنشاء المستخدم تلقائياً وربطه بالنظام الفرعي المختار
                        </DialogDescription>
                    </DialogHeader>

                    <form onSubmit={handleCreateUser} className="space-y-4 pt-2">
                        <div className="space-y-2">
                            <Label className="text-right block">اسم المستخدم</Label>
                            <Input
                                placeholder="مثال: mohammed_ali"
                                value={newUserName}
                                onChange={(e) => setNewUserName(e.target.value)}
                                className="text-right rounded-xl"
                            />
                        </div>

                        <div className="space-y-2">
                            <Label className="text-right block">كلمة المرور</Label>
                            <Input
                                type="password"
                                placeholder="كلمة المرور..."
                                value={newUserPassword}
                                onChange={(e) => setNewUserPassword(e.target.value)}
                                className="text-right rounded-xl"
                            />
                        </div>

                        <div className="flex justify-end gap-2 pt-2">
                            <Button
                                type="button"
                                variant="outline"
                                onClick={() => setIsCreateOpen(false)}
                                className="rounded-xl"
                            >
                                إلغاء
                            </Button>
                            <Button
                                type="submit"
                                disabled={createUser.isPending}
                                className="rounded-xl bg-primary hover:bg-primary/90"
                            >
                                {createUser.isPending ? 'جاري الإنشاء...' : 'إنشاء وتعيين'}
                            </Button>
                        </div>
                    </form>
                </DialogContent>
            </Dialog>
        </div>
    );
};
