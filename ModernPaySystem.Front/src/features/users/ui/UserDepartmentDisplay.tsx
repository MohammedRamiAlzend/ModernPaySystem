import { useUser } from '@/features/users/api/usersApi';
import { DepartmentDisplay } from '@/entities/department/ui/DepartmentDisplay';
import { Loader2 } from 'lucide-react';

interface UserDepartmentDisplayProps {
    userId: string | null | undefined;
    className?: string;
}

export const UserDepartmentDisplay = ({ userId, className = '' }: UserDepartmentDisplayProps) => {
    const { data: user, isLoading, isError } = useUser(userId);

    if (!userId) {
        return (
            <span className={`text-muted-foreground italic ${className}`}>
                غير محدد
            </span>
        );
    }

    if (isLoading) {
        return (
            <div className={`flex items-center gap-2 ${className}`}>
                <Loader2 className="w-3 h-3 animate-spin text-muted-foreground" />
                <span className="text-muted-foreground text-[10px]">جاري التحميل...</span>
            </div>
        );
    }

    if (isError || !user) {
        return (
            <span className={`text-xs font-mono text-muted-foreground ${className}`}>
                غير محدد
            </span>
        );
    }

    return (
        <DepartmentDisplay
            departmentId={user.departmentId}
            className={className}
        />
    );
};
