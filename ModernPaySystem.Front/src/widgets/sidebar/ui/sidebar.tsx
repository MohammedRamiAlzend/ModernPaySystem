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
import { useAuthStore } from '@/app/store/authStore';
import { useNavigate, useLocation } from 'react-router-dom';
import { useEffect, useMemo } from 'react';
import { useResponsesByRequester, useRequests, useTemplates } from '@/features/form-builder/api/formEndpoints';
import { useBadgeCount } from '@/shared/hooks/use-badge-count';
import { FileText } from 'lucide-react';

const LAST_SEEN_RESPONSES_KEY = 'last_seen_total_responses';
const LAST_SEEN_PENDING_KEY = 'last_seen_total_pending';

interface SidebarProps {
    className?: string;
    onItemClick?: () => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ className, onItemClick }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);
    const user = useAuthStore((state) => state.user);
    const logoutAction = useAuthStore((state) => state.logout);
    const navigate = useNavigate();
    const location = useLocation();

    // 1. Incoming Responses (For Requester)
    const { data: pagedResponses } = useResponsesByRequester(user?.id || null, 1, 1);
    const totalResponses = (pagedResponses as any)?.totalItems || 0;
    const incomingResponsesBadge = useBadgeCount(totalResponses, LAST_SEEN_RESPONSES_KEY, '/form-builder/my-responses');

    // 2. Pending Requests (For Approver)
    const { data: pagedPending } = useRequests(false, 1, 1);
    const totalPending = pagedPending?.totalItems || 0;
    const pendingRequestsBadge = useBadgeCount(totalPending, LAST_SEEN_PENDING_KEY, '/form-builder/responses');

    const badges = useMemo(() => ({
        '/form-builder/my-responses': incomingResponsesBadge,
        '/form-builder/responses': pendingRequestsBadge,
    }), [incomingResponsesBadge, pendingRequestsBadge]);

    const [expandedItems, setExpandedItems] = useState<Record<string, boolean>>(() => {
        const initialStates: Record<string, boolean> = {};

        const setInitialStates = (items: typeof NAVIGATION_ITEMS) => {
            items.forEach(item => {
                if (item.isOpen) {
                    initialStates[item.path] = true;
                }
                if (item.children) {
                    setInitialStates(item.children);
                }
            });
        };

        setInitialStates(NAVIGATION_ITEMS);
        return initialStates;
    });

    // Fetch Templates
    const { data: templates } = useTemplates();

    // Dynamically build navigation items
    const navItems = useMemo(() => {
        return NAVIGATION_ITEMS.map(item => {
            // Target the specific section "منصة خدمات ريف دمشق"
            // Identifying by title or path. Path "/form-builder/actioned" seems to be reused, but let's check title or unique properties.
            // The item has path "/form-builder/actioned".
            if (item.title === "منصة خدمات ريف دمشق") {
                const templateChildren = templates
                    ?.filter(t => !t.isExternal && !t.templateName.toLocaleLowerCase().includes("delphi"))
                    ?.map(t => ({
                        title: t.templateName,
                        path: `/form-builder/requests/new?templateId=${t.id}`,
                        icon: <FileText className="h-3 w-3" />,
                    })) || [];

                const templatesSection = {
                    title: "الخدمات ",
                    path: "templates-section", // Virtual path for toggling
                    icon: <FileText className="h-4 w-4" />,
                    children: templateChildren
                };

                return {
                    ...item,
                    children: [templatesSection, ...(item.children || [])]
                };
            }
            return item;
        });
    }, [templates]);

    // Auto-expand parent if child is active
    useEffect(() => {
        navItems.forEach(item => {
            if (item.children?.some(child => {
                // Check direct children
                if (location.pathname.startsWith(child.path)) return true;
                // Check grandchildren
                return child.children?.some(grandChild => location.pathname + location.search === grandChild.path || location.pathname.startsWith(grandChild.path.split('?')[0]));
            })) {
                setExpandedItems(prev => ({ ...prev, [item.path]: true }));
            }

            // Also expand the "Templates" section if a template is active
            item.children?.forEach(child => {
                if (child.children?.some(grandChild => location.search.includes(grandChild.path.split('?')[1] || 'impossible'))) {
                    setExpandedItems(prev => ({ ...prev, [child.path]: true }));
                }
            });
        });
    }, [location.pathname, location.search, navItems]);

    const toggleExpand = (path: string, e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        setExpandedItems(prev => ({
            ...prev,
            [path]: !prev[path]
        }));
    };

    const handleLogout = () => {
        logoutAction();
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
                        <span className="text-lg font-bold text-primary whitespace-nowrap animate-in fade-in slide-in-from-right-4">
                            منصة خدمات ريف دمشق
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
                {navItems.map((item) => {
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
                                    {item.children!.map((child) => {
                                        const hasGrandChildren = child.children && child.children.length > 0;
                                        const isChildExpanded = expandedItems[child.path];

                                        if (hasGrandChildren) {
                                            return (
                                                <div key={child.path} className="space-y-1">
                                                    <button
                                                        onClick={(e) => toggleExpand(child.path, e)}
                                                        className={cn(
                                                            "w-full flex items-center gap-3 px-3 py-2 rounded-xl transition-all duration-200 text-sm",
                                                            "text-muted-foreground hover:bg-accent/50 hover:text-foreground"
                                                        )}
                                                    >
                                                        <div className="opacity-70">{child.icon}</div>
                                                        <span className='font-bold flex-1 text-right'>{child.title}</span>
                                                        <div className={cn(
                                                            "transition-transform duration-200",
                                                            isChildExpanded ? "-rotate-90" : "rotate-0"
                                                        )}>
                                                            <ChevronLeft className="h-3 w-3 opacity-50" />
                                                        </div>
                                                    </button>

                                                    {isChildExpanded && (
                                                        <div className="mr-6 space-y-1 border-r pr-2 border-border/50 animate-in fade-in slide-in-from-top-1 duration-200">
                                                            {child.children!.map(grandChild => (
                                                                <PrefetchNavLink
                                                                    key={grandChild.path}
                                                                    to={grandChild.path}
                                                                    onClick={onItemClick}
                                                                    className={() => {
                                                                        const isCurrent = (location.pathname + location.search) === grandChild.path;
                                                                        return cn(
                                                                            "flex items-center gap-3 px-3 py-2 rounded-xl transition-all duration-200 text-xs",
                                                                            isCurrent
                                                                                ? "bg-primary/10 text-primary font-bold shadow-sm"
                                                                                : "text-muted-foreground hover:bg-accent/50 hover:text-foreground"
                                                                        );
                                                                    }}
                                                                >
                                                                    <div className="opacity-70">{grandChild.icon}</div>
                                                                    <span className='truncate'>{grandChild.title}</span>
                                                                </PrefetchNavLink>
                                                            ))}
                                                        </div>
                                                    )}
                                                </div>
                                            );
                                        }

                                        return (
                                            <PrefetchNavLink
                                                key={child.path}
                                                to={child.path}
                                                onClick={onItemClick}
                                                className={() => {
                                                    const isStrictlyActive = (location.pathname + location.search) === child.path;
                                                    return cn(
                                                        "flex items-center gap-3 px-3 py-2 rounded-xl transition-all duration-200 text-sm",
                                                        isStrictlyActive
                                                            ? "bg-primary text-primary-foreground font-medium shadow-md shadow-primary/20"
                                                            : "text-muted-foreground hover:bg-accent/50 hover:text-foreground"
                                                    );
                                                }}
                                            >
                                                <div className="opacity-70">{child.icon}</div>
                                                <span className='font-bold flex-1'>{child.title} </span>
                                                {(badges as any)[child.path] > 0 && (
                                                    <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1.5 text-[10px] font-bold text-primary-foreground animate-in zoom-in-50 duration-300">
                                                        {(badges as any)[child.path]}
                                                    </span>
                                                )}
                                            </PrefetchNavLink>
                                        );
                                    })}
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
