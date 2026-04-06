import { useState, useMemo } from 'react';
import { useAuthStore } from '@/app/store/authStore';
import { useUsers, useSubSystems } from '../api/usersApi';
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
import { Shield, User as UserIcon } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

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
}

export type UserPickerProps = UserPickerSharedProps & (UserPickerSingleProps | UserPickerMultiProps);

// ─── Component ───────────────────────────────────────────────────────────────

export const UserPicker = (props: UserPickerProps) => {
    const {
        label = 'الموافق (Approver)',
        placeholder = 'اختر المستخدم...',
        subSystemPlaceholder = 'اختر النظام...',
        defaultSubSystemId = '1',
        showCurrentUser = false,
        className,
    } = props;

    const isMulti = props.multiple === true;

    const { user: currentUserData } = useAuthStore();
    const [selectedSubSystem, setSelectedSubSystem] = useState<string>(defaultSubSystemId);

    const { data: subSystems = [], isLoading: isLoadingSubSystems } = useSubSystems();
    const { data: rawUsers = [], isLoading: isLoadingUsers } = useUsers(selectedSubSystem);

    // Filter out current user if needed
    const users = useMemo(() => {
        if (showCurrentUser || !currentUserData) return rawUsers;
        return rawUsers.filter(u => u.id !== currentUserData.id);
    }, [rawUsers, showCurrentUser, currentUserData]);

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

            {/* User selector – single or multi via SearchableSelect */}
            <div className="space-y-2">
                <Label className="text-xs font-bold text-muted-foreground flex items-center gap-2">
                    <UserIcon className="w-3 h-3" />
                    {label}
                </Label>

                {isMulti ? (
                    <SearchableSelect
                        multiple
                        options={userOptions}
                        values={props.selectedUserIds}
                        onValuesChange={props.onUsersChange}
                        placeholder={placeholder}
                        searchPlaceholder="ابحث بالاسم أو الترتيب..."
                        emptyMessage="لا يوجد مستخدمين لهذا النظام"
                        isLoading={isLoadingUsers}
                    />
                ) : (
                    <SearchableSelect
                        options={userOptions}
                        value={props.defaultValue || ''}
                        onValueChange={props.onUserSelect}
                        placeholder={placeholder}
                        searchPlaceholder="ابحث بالاسم أو الترتيب..."
                        emptyMessage="لا يوجد مستخدمين لهذا النظام"
                        isLoading={isLoadingUsers}
                    />
                )}
            </div>
        </div>
    );
};
