import { useUser } from '@/features/users/api/usersApi';
import { User as UserIcon, Loader2 } from 'lucide-react';

interface UserDisplayProps {
    userId: string | null | undefined;
    showIcon?: boolean;
    className?: string;
    iconClassName?: string;
}

export const UserDisplay = ({
    userId,
    showIcon = true,
    className = '',
    iconClassName = 'w-3 h-3'
}: UserDisplayProps) => {
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
                <Loader2 className={`${iconClassName} animate-spin text-muted-foreground`} />
                <span className="text-muted-foreground text-xs">جاري التحميل...</span>
            </div>
        );
    }

    if (isError || !user) {
        return (
            <span className={`text-xs font-mono text-muted-foreground ${className}`}>
                {userId.split('-')[0]}...
            </span>
        );
    }

    return (
        <div className={`flex items-center gap-2 ${className}`}>
            {showIcon && <UserIcon className={`${iconClassName} text-muted-foreground`} />}
            <span className="font-medium">{user.userName}</span>
        </div>
    );
};
