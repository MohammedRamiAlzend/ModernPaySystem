import { UserPicker } from './UserPicker';
import { UserDisplay } from './UserDisplay';
import { Badge } from '@/shared/ui/badge';
import { X, Users } from 'lucide-react';

interface MultiUserPickerProps {
    onUsersChange: (usersIds: string[]) => void;
    selectedUserIds: string[];
    label?: string;
    placeholder?: string;
    className?: string;
    departmentOnly?: boolean;
}

export const MultiUserPicker = ({
    onUsersChange,
    selectedUserIds = [],
    label = "للاطلاع فقط (ReadOnly)",
    placeholder = "اضف مستخدم للاطلاع...",
    className,
    departmentOnly
}: MultiUserPickerProps) => {
    const handleAddUser = (userId: string) => {
        if (!userId || selectedUserIds.includes(userId)) return;
        
        const newIds = [...selectedUserIds, userId];
        onUsersChange(newIds);
    };

    const removeUser = (userId: string) => {
        const newIds = selectedUserIds.filter(id => id !== userId);
        onUsersChange(newIds);
    };

    return (
        <div className={className}>
            <UserPicker 
                onUserSelect={handleAddUser}
                label={label}
                placeholder={placeholder}
                className="!grid-cols-1"
                showCurrentUser={false}
                departmentOnly={departmentOnly}
            />
            
            {selectedUserIds.length > 0 && (
                <div className="mt-3 flex flex-wrap gap-2 p-2 rounded-xl bg-muted/30 border border-dashed border-primary/10">
                    {selectedUserIds.map(userId => (
                        <Badge 
                            key={userId} 
                            variant="secondary" 
                            className="pl-1 pr-2 py-1 gap-1 flex items-center bg-background/80 backdrop-blur-sm border-primary/10 hover:bg-background transition-colors"
                        >
                            <Users className="w-3 h-3 text-primary/70 shrink-0" />
                            <UserDisplay userId={userId} showIcon={false} className="text-[10px] font-medium" />
                            <button 
                                onClick={() => removeUser(userId)}
                                className="hover:text-destructive transition-colors p-0.5 rounded-full hover:bg-destructive/10"
                            >
                                <X className="w-3 h-3" />
                            </button>
                        </Badge>
                    ))}
                </div>
            )}
        </div>
    );
};
