import { useState, useEffect } from 'react';
import { useUsers, useSubSystems } from '../api/usersApi';
import { Label } from '@/shared/ui/label';
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from '@/shared/ui/select';
import { Loader2, User as UserIcon, Shield } from 'lucide-react';
import { cn } from '@/shared/lib/utils';

interface UserPickerProps {
    onUserSelect: (userId: string) => void;
    label?: string;
    placeholder?: string;
    subSystemPlaceholder?: string;
    defaultValue?: string;
    defaultSubSystemId?: string;
    className?: string;
}

export const UserPicker = ({
    onUserSelect,
    label = "الموافق (Approver)",
    placeholder = "اختر المستخدم...",
    subSystemPlaceholder = "اختر النظام...",
    defaultValue,
    defaultSubSystemId = "1",
    className
}: UserPickerProps) => {
    const [selectedSubSystem, setSelectedSubSystem] = useState<string>(defaultSubSystemId);
    const [selectedUser, setSelectedUser] = useState<string>(defaultValue || '');

    const { data: subSystems = [], isLoading: isLoadingSubSystems } = useSubSystems();
    const { data: users = [], isLoading: isLoadingUsers } = useUsers(selectedSubSystem);

    // Update internal state if defaultValue changes
    useEffect(() => {
        if (defaultValue) {
            setSelectedUser(defaultValue);
        }
    }, [defaultValue]);

    const handleSubSystemChange = (value: string) => {
        setSelectedSubSystem(value);
        setSelectedUser(''); // Reset user when subsystem changes
    };

    const handleUserChange = (value: string) => {
        setSelectedUser(value);
        onUserSelect(value);
    };

    return (
        <div className={cn("grid grid-cols-1 md:grid-cols-2 gap-4", className)}>
            <div className="space-y-2">
                <Label className="text-xs font-bold text-muted-foreground flex items-center gap-2">
                    <Shield className="w-3 h-3" />
                    النظام الفرعي
                </Label>
                <Select value={selectedSubSystem} onValueChange={handleSubSystemChange}>
                    <SelectTrigger className="h-10 rounded-xl bg-background/50 backdrop-blur-sm border-primary/10">
                        <SelectValue placeholder={isLoadingSubSystems ? "جاري التحميل..." : subSystemPlaceholder} />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl border-primary/10">
                        {subSystems.map(ss => (
                            <SelectItem key={ss.value} value={ss.value}>{ss.name}</SelectItem>
                        ))}
                    </SelectContent>
                </Select>
            </div>

            <div className="space-y-2">
                <Label className="text-xs font-bold text-muted-foreground flex items-center gap-2">
                    <UserIcon className="w-3 h-3" />
                    {label}
                </Label>
                <Select value={selectedUser} onValueChange={handleUserChange}>
                    <SelectTrigger className="h-10 rounded-xl bg-background/50 backdrop-blur-sm border-primary/10">
                        <SelectValue placeholder={isLoadingUsers ? "جاري التحميل..." : placeholder} />
                    </SelectTrigger>
                    <SelectContent className="rounded-xl border-primary/10">
                        {users.length === 0 && !isLoadingUsers ? (
                            <div className="p-4 text-center text-sm text-muted-foreground italic">
                                لا يوجد مستخدمين لهذا النظام
                            </div>
                        ) : (
                            users.map(user => (
                                <SelectItem key={user.id} value={user.id}>
                                    {user.userName}
                                </SelectItem>
                            ))
                        )}
                        {isLoadingUsers && (
                            <div className="p-4 flex items-center justify-center gap-2 text-sm text-muted-foreground">
                                <Loader2 className="w-4 h-4 animate-spin" />
                                جاري تحميل المستخدمين...
                            </div>
                        )}
                    </SelectContent>
                </Select>
            </div>
        </div>
    );
};
