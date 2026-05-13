/**
 * CurrentLocationBadge.tsx
 * @description بانتظار تجهيز end point من اجل التفعيل.
 * @param {string | null} name - The name of the current user.
 * @param {string | null} department - The department of the current user.
 * @param {string} className - Additional CSS classes to apply to the badge.
 * @returns {React.FC<CurrentLocationBadgeProps>} The CurrentLocationBadge component.
 */

import React from 'react';

interface CurrentLocationBadgeProps {
    name?: string | null;
    department?: string | null;
    className?: string;
}

export const CurrentLocationBadge: React.FC<CurrentLocationBadgeProps> = ({ name, department, className = "" }) => {
    if (!name || name === "غير محدد") return null;
    console.log(name, department);
    return (
        <div className={`flex items-center gap-2 px-3 py-1 bg-primary/5 rounded-full border border-primary/10 ${className}`}>
            <span className="text-muted-foreground text-xs md:text-sm">الموقع الحالي:</span>
            <span className="font-bold text-primary text-xs md:text-sm">
                {name}
                {department && (
                    <span className="text-muted-foreground font-normal"> - {department}</span>
                )}
            </span>
        </div>
    );
};
