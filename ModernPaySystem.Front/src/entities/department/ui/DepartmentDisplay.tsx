import { useDepartment } from '../model/useDepartments';
import { Building2, Loader2 } from 'lucide-react';

interface DepartmentDisplayProps {
    departmentId: string | null | undefined;
    showIcon?: boolean;
    className?: string;
    iconClassName?: string;
}

export const DepartmentDisplay = ({
    departmentId,
    showIcon = false,
    className = '',
    iconClassName = 'w-3 h-3'
}: DepartmentDisplayProps) => {
    const { data: department, isLoading, isError } = useDepartment(departmentId);

    if (!departmentId) {
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
                <span className="text-muted-foreground text-[10px]">جاري التحميل...</span>
            </div>
        );
    }

    if (isError || !department) {
        return (
            <span className={`text-xs font-mono text-muted-foreground ${className}`}>
                {departmentId.split('-')[0]}...
            </span>
        );
    }

    return (
        <div className={`flex items-center gap-1.5 ${className}`}>
            {showIcon && <Building2 className={`${iconClassName} text-muted-foreground`} />}
            <span className="font-semibold text-foreground">{department.name}</span>
        </div>
    );
};
