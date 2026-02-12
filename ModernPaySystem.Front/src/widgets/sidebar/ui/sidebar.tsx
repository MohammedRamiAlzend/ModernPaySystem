import React, { useState } from 'react';
import { cn } from '@/shared/lib/utils';
import { Button } from '@/shared/ui/button';
import { PrefetchNavLink } from '@/shared/navigation/prefetch-nav-link';
import { NAVIGATION_ITEMS } from '@/shared/config/navigation';
import {
    ChevronRight,
    ChevronLeft,
    LayoutDashboard,
    LogOut
} from 'lucide-react';
import { useAppDispatch } from '@/app/store';
import { logout } from '@/app/store/authSlice';
import { useNavigate, useLocation } from 'react-router-dom';
import { useEffect } from 'react';

interface SidebarProps {
    className?: string;
    onItemClick?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ className, onItemClick }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);
    const dispatch = useAppDispatch();
    const navigate = useNavigate();
    const location = useLocation();

    const [expandedItems, setExpandedItems] = useState<Record<string, boolean>>({});

    // Auto-expand parent if child is active
    useEffect(() => {
        NAVIGATION_ITEMS.forEach(item => {
            if (item.children?.some(child => location.pathname.startsWith(child.path))) {
                setExpandedItems(prev => ({ ...prev, [item.path]: true }));
            }
        });
    }, [location.pathname]);

    const toggleExpand = (path: string, e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        setExpandedItems(prev => ({
            ...prev,
            [path]: !prev[path]
        }));
    };

    const handleLogout = () => {
        dispatch(logout());
        navigate('/auth/login');
        if (onItemClick) onItemClick();
    };

    return (
        <aside
            className={cn(
                "flex flex-col h-screen border-l bg-card transition-all duration-300 ease-in-out group relative",
                isCollapsed ? "w-20" : "w-72",
                className
            )}
        >
            {/* Header / Logo */}
            <div className="h-16 flex items-center px-6 border-b relative">
                <PrefetchNavLink to="/" className="flex items-center gap-3 overflow-hidden">
                    <div className="w-8 h-8 min-w-[32px] bg-primary rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
                        <LayoutDashboard className="text-primary-foreground h-5 w-5" />
                    </div>
                    {!isCollapsed && (
                        <span className="text-xl font-bold text-primary whitespace-nowrap animate-in fade-in slide-in-from-right-4">
                            PaySystem
                        </span>
                    )}
                </PrefetchNavLink>

                {/* Collapse Toggle Button - Only visible on desktop (hidden via className when in mobile sheet) */}
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setIsCollapsed(!isCollapsed)}
                    className={cn(
                        "absolute -left-2.5 top-1/2 -translate-y-1/2 w-6 h-6 rounded-full border bg-background shadow-sm opacity-0 group-hover:opacity-100 transition-opacity z-50",
                        className?.includes("md:hidden") ? "hidden" : "" // Hide if we are forcing mobile view
                    )}
                >
                    {isCollapsed ? <ChevronLeft className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
                </Button>
            </div>

            {/* Navigation Items */}
            <div className="flex-1 overflow-y-auto py-6 px-3 space-y-2">
                {NAVIGATION_ITEMS.map((item) => {
                    const isExpanded = expandedItems[item.path];
                    const hasChildren = item.children && item.children.length > 0;

                    return (
                        <div key={item.path} className="space-y-1">
                            <PrefetchNavLink
                                to={item.path}
                                onClick={(e) => {
                                    if (hasChildren && !isCollapsed) {
                                        toggleExpand(item.path, e);
                                    } else if (onItemClick) {
                                        onItemClick();
                                    }
                                }}
                                className={({ isActive }) => cn(
                                    "flex items-center gap-4 px-3 py-3 rounded-2xl transition-all duration-200 group/item relative",
                                    isActive && (!hasChildren || isCollapsed)
                                        ? "bg-primary text-primary-foreground shadow-lg shadow-primary/20"
                                        : "text-muted-foreground hover:bg-accent hover:text-foreground"
                                )}
                            >
                                <div className={cn(
                                    "transition-transform duration-200",
                                    "group-hover/item:scale-110"
                                )}>
                                    {item.icon}
                                </div>
                                {!isCollapsed && (
                                    <>
                                        <span className="font-bold whitespace-nowrap animate-in fade-in slide-in-from-right-2 flex-1">
                                            {item.title}
                                        </span>
                                        {hasChildren && (
                                            <div className={cn(
                                                "transition-transform duration-200",
                                                isExpanded ? "-rotate-90" : "rotate-0"
                                            )}>
                                                <ChevronLeft className="h-4 w-4 opacity-50" />
                                            </div>
                                        )}
                                    </>
                                )}
                                {isCollapsed && (
                                    <div className="absolute left-full ml-4 px-2 py-1 bg-popover text-popover-foreground text-xs rounded border opacity-0 pointer-events-none group-hover/item:opacity-100 transition-opacity whitespace-nowrap z-[100] shadow-md">
                                        {item.title}
                                    </div>
                                )}
                            </PrefetchNavLink>

                            {/* Render Children */}
                            {!isCollapsed && hasChildren && isExpanded && (
                                <div className="mr-8 space-y-1 border-r pr-2 border-border/50 animate-in fade-in slide-in-from-top-2 duration-200">
                                    {item.children!.map((child) => (
                                        <PrefetchNavLink
                                            key={child.path}
                                            to={child.path}
                                            onClick={onItemClick}
                                            className={({ isActive }) => cn(
                                                "flex items-center gap-3 px-3 py-2 rounded-xl transition-all duration-200 text-sm",
                                                isActive
                                                    ? "bg-primary text-primary-foreground font-medium"
                                                    : "text-muted-foreground hover:bg-accent/50 hover:text-foreground"
                                            )}
                                        >
                                            <div className="opacity-70">{child.icon}</div>
                                            <span className='font-bold'>{child.title} </span>
                                        </PrefetchNavLink>
                                    ))}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>

            {/* Footer Actions */}
            <div className="p-1 border-t space-y-1">
                {/* <div className={cn(
                    "flex items-center p-2 rounded-2xl hover:bg-accent transition-colors cursor-pointer",
                    isCollapsed ? "justify-center" : "gap-3"
                )}>
                    <div className="w-9 h-9 rounded-xl bg-slate-100 dark:bg-slate-800 flex items-center justify-center">
                        <User className="h-5 w-5 text-slate-500" />
                    </div>
                    {!isCollapsed && (
                        <div className="flex flex-col min-w-0">
                            <span className="text-sm font-bold truncate">أحمد محمد</span>
                            <span className="text-[10px] text-muted-foreground truncate">مدير النظام</span>
                        </div>
                    )}
                </div> */}

                <Button
                    variant="ghost"
                    onClick={handleLogout}
                    className={cn(
                        "w-full rounded-2xl text-red-500 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-950/30",
                        isCollapsed ? "px-0 justify-center" : "justify-start px-3 gap-4"
                    )}
                >
                    <LogOut className="h-5 w-5" />
                    {!isCollapsed && <span className="font-bold">تسجيل الخروج</span>}
                </Button>
            </div>
        </aside>
    );
};
