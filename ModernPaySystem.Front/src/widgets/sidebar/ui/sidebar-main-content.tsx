import React, { useState, useMemo } from 'react';
import { cn } from '@/shared/lib/utils';
import { PrefetchNavLink } from '@/shared/navigation/prefetch-nav-link';
import { NAVIGATION_ITEMS } from '@/shared/config/navigation';
import { FileText, ChevronDown } from 'lucide-react';
import { useLocation, Link } from 'react-router-dom';
import { useResponsesByRequester, useRequests, useTemplates, useRequestTransactions } from '@/features/form-builder/api/formEndpoints';
import { useBadgeCount } from '@/shared/hooks/use-badge-count';
import { useAuthStore } from '@/app/store/authStore';
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from '@/shared/ui/tooltip';
import type { SidebarContentProps } from '../model/sidebar-mode.types';

const LAST_SEEN_RESPONSES_KEY = 'last_seen_total_responses';
const LAST_SEEN_PENDING_KEY = 'last_seen_total_pending';

/**
 * Hook to manage sidebar badges count
 */
const useSidebarBadges = (userId: string | null) => {
    const { data: pagedResponses } = useResponsesByRequester(userId, { page: 1, pageSize: 1 });
    const { data: pagedPending } = useRequests(false, { page: 1, pageSize: 1 });
    const { data: pagedPendingReferrals } = useRequestTransactions(0, { page: 1, pageSize: 1 });
    const { data: pagedSentReferrals } = useRequestTransactions(1, { page: 1, pageSize: 1 });

    const incomingResponsesBadge = useBadgeCount((pagedResponses as any)?.totalItems || 0, LAST_SEEN_RESPONSES_KEY, '/form-builder/my-responses');
    const pendingRequestsBadge = useBadgeCount(pagedPending?.totalItems || 0, LAST_SEEN_PENDING_KEY, '/form-builder/responses');
    const pendingReferralsBadge = useBadgeCount(pagedPendingReferrals?.totalItems || 0, 'last_seen_pending_referrals', '/form-builder/referrals/pending');
    const sentReferralsBadge = useBadgeCount(pagedSentReferrals?.totalItems || 0, 'last_seen_sent_referrals', '/form-builder/referrals/sent');

    return useMemo(() => ({
        '/form-builder/my-responses': incomingResponsesBadge,
        '/form-builder/responses': pendingRequestsBadge,
        '/form-builder/referrals/pending': pendingReferralsBadge,
        '/form-builder/referrals/sent': sentReferralsBadge,
    }), [incomingResponsesBadge, pendingRequestsBadge, pendingReferralsBadge, sentReferralsBadge]);
};

export const SidebarMainContent: React.FC<SidebarContentProps> = ({
    isCollapsed,
    onItemClick,
}) => {
    const user = useAuthStore((state) => state.user);
    const location = useLocation();

    // Badges fetching
    const badges = useSidebarBadges(user?.id || null);

    const [expandedItems, setExpandedItems] = useState<Record<string, boolean>>({});

    // Fetch Templates and inject into navigation
    const { data: templates } = useTemplates();
    const navItems = useMemo(() => {
        return NAVIGATION_ITEMS.map(item => {
            let children = item.children || [];
            // تعطيل مؤقت لشرط
            // تصفية العناصر بناءً على صلاحية رئيس القسم
            console.log('user', user);
            if (!user?.isDepartmentHead) {
                children = children.filter(child =>
                    child.title !== "الرد على الطلبات" &&
                    child.title !== "الردود الصادرة"
                    // child.title !== "الطلبات التي تم الرد عليها" &&
                );
            }

            if (item.title === "منصة خدمات ريف دمشق") {
                const templateChildren = templates
                    ?.filter(t => !t.isExternal && !t.templateName.toLowerCase().includes("delphi"))
                    ?.map(t => ({
                        title: t.templateName,
                        path: `/form-builder/requests/new?templateId=${t.id}`,
                        icon: <FileText className="h-3.5 w-3.5" />,
                    })) || [];

                return {
                    ...item,
                    children: [
                        {
                            title: "الخدمات المتاحة",
                            path: "templates-section",
                            icon: <FileText className="h-4 w-4 text-primary" />,
                            children: templateChildren
                        },
                        ...children
                    ]
                };
            }

            return {
                ...item,
                children
            };
        });
    }, [templates, user]);

    // AUTO-EXPAND Logic: Synchronize state with URL during render
    const fullPath = location.pathname + location.search;
    const [prevPath, setPrevPath] = useState(fullPath);

    if (prevPath !== fullPath) {
        setPrevPath(fullPath);

        let hasChanges = false;
        const newExpanded = { ...expandedItems };

        navItems.forEach(item => {
            if (!item.children) return;

            const isItemParentOfActive = item.children.some(child => {
                if (child.path === fullPath || (child.path !== "#" && fullPath.startsWith(child.path))) return true;
                if (child.children?.some(grand => grand.path === fullPath)) {
                    if (!newExpanded[child.path]) {
                        newExpanded[child.path] = true;
                        hasChanges = true;
                    }
                    return true;
                }
                return false;
            });

            if (isItemParentOfActive && !newExpanded[item.path]) {
                newExpanded[item.path] = true;
                hasChanges = true;
            }
        });

        if (hasChanges) {
            setExpandedItems(newExpanded);
        }
    }

    const toggleExpand = (path: string, e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();
        setExpandedItems(prev => ({ ...prev, [path]: !prev[path] }));
    };

    return (
        <TooltipProvider delayDuration={0}>
            <div className="flex-1 overflow-y-auto py-4 px-3 space-y-1.5 custom-scrollbar">
                {navItems.map((item) => {
                    const isExpanded = expandedItems[item.path];
                    const hasChildren = item.children && item.children.length > 0;
                    const isActive = location.pathname === item.path || (hasChildren && item.children?.some(c => location.pathname.startsWith(c.path)));

                    return (
                        <div key={item.path} className="space-y-1">
                            <Tooltip>
                                <TooltipTrigger asChild>
                                    <PrefetchNavLink
                                        to={hasChildren ? "#" : item.path}
                                        onClick={(e) => {
                                            if (hasChildren) {
                                                e.preventDefault();
                                                if (!isCollapsed) toggleExpand(item.path, e);
                                            } else {
                                                onItemClick?.();
                                            }
                                        }}
                                        className={() => cn(
                                            "group/item flex items-center gap-3 px-3 py-3 rounded-2xl transition-all duration-300 relative",
                                            isActive && (!hasChildren || isCollapsed)
                                                ? "bg-primary text-primary-foreground shadow-lg shadow-primary/25"
                                                : "text-foreground hover:bg-accent",
                                            isCollapsed && "justify-center"
                                        )}
                                    >
                                        <div className={cn(
                                            "transition-transform duration-300 group-hover/item:scale-110",
                                            hasChildren ? "text-primary" : "text-current"
                                        )}>
                                            {item.icon}
                                        </div>
                                        {!isCollapsed && (
                                            <>
                                                <span className={cn(
                                                    "text-sm tracking-tight flex-1 animate-in fade-in slide-in-from-right-3",
                                                    hasChildren ? "font-black text-primary" : "font-bold"
                                                )}>
                                                    {item.title}
                                                </span>
                                                {hasChildren && (
                                                    <ChevronDown className={cn(
                                                        "h-4 w-4 opacity-40 transition-transform duration-300",
                                                        isExpanded ? "rotate-0" : "-rotate-90"
                                                    )} />
                                                )}
                                            </>
                                        )}
                                    </PrefetchNavLink>
                                </TooltipTrigger>
                                {isCollapsed && (
                                    <TooltipContent side="left" align="start" sideOffset={10} className="bg-popover text-popover-foreground border shadow-xl animate-in zoom-in-95 duration-200 p-0">
                                        <div className="flex flex-col min-w-[190px] font-bold overflow-hidden rounded-lg">
                                            <div className="px-3 py-2 bg-accent/30 border-b border-border/50 text-[12px] text-primary flex items-center justify-between">
                                                <span>{item.title}</span>
                                            </div>
                                            {hasChildren && (
                                                <div className="p-1 flex flex-col gap-0.5 max-h-[70vh] overflow-y-auto custom-scrollbar">
                                                    {item.children?.map(child => {
                                                        const isChildExpanded = expandedItems[child.path];
                                                        const hasGrandChildren = child.children && child.children.length > 0;

                                                        return (
                                                            <div key={child.path} className="flex flex-col">
                                                                {hasGrandChildren ? (
                                                                    <>
                                                                        <button
                                                                            onClick={(e) => {
                                                                                e.preventDefault();
                                                                                e.stopPropagation();
                                                                                toggleExpand(child.path, e);
                                                                            }}
                                                                            className={cn(
                                                                                "flex items-center gap-2 px-3 py-2 rounded-md text-[11px] transition-all text-muted-foreground hover:bg-accent hover:text-foreground",
                                                                                isChildExpanded && "text-primary bg-primary/5"
                                                                            )}
                                                                        >
                                                                            <div className="w-1 h-1 rounded-full bg-primary/40 shrink-0" />
                                                                            <span className="flex-1 text-right">{child.title}</span>
                                                                            <ChevronDown className={cn("h-3 w-3 opacity-50 transition-transform", isChildExpanded ? "rotate-0" : "-rotate-90")} />
                                                                        </button>

                                                                        {isChildExpanded && (
                                                                            <div className="flex flex-col mt-0.5 mr-3 border-r border-border/30 pr-1 animate-in slide-in-from-top-1 duration-200">
                                                                                {child.children!.map(grand => (
                                                                                    <Link
                                                                                        key={grand.path}
                                                                                        to={grand.path}
                                                                                        className={cn(
                                                                                            "text-[10px] font-bold px-3 py-1.5 rounded hover:bg-primary/5 transition-all flex items-center gap-2",
                                                                                            (location.pathname + location.search) === grand.path
                                                                                                ? "text-primary bg-primary/5"
                                                                                                : "text-muted-foreground hover:text-primary"
                                                                                        )}
                                                                                    >
                                                                                        <div className="w-0.5 h-0.5 rounded-full bg-primary/30 shrink-0" />
                                                                                        {grand.title}
                                                                                    </Link>
                                                                                ))}
                                                                            </div>
                                                                        )}
                                                                    </>
                                                                ) : (
                                                                    <Link
                                                                        to={child.path}
                                                                        className={cn(
                                                                            "flex items-center gap-2 px-3 py-2 rounded-md text-[11px] transition-all",
                                                                            location.pathname === child.path
                                                                                ? "text-primary bg-primary/5"
                                                                                : "text-muted-foreground hover:text-primary hover:bg-accent"
                                                                        )}
                                                                    >
                                                                        <div className="w-1 h-1 rounded-full bg-primary/40 shrink-0" />
                                                                        {child.title}
                                                                    </Link>
                                                                )}
                                                            </div>
                                                        );
                                                    })}
                                                </div>
                                            )}
                                        </div>
                                    </TooltipContent>
                                )}
                            </Tooltip>

                            {/* Submenu rendering */}
                            {!isCollapsed && hasChildren && isExpanded && (
                                <div className="mr-6 pr-2 border-r border-border/50 space-y-1 animate-in fade-in slide-in-from-top-2 duration-300">
                                    {item.children!.map((child) => (
                                        <div key={child.path} className="space-y-1">
                                            {child.children ? (
                                                <>
                                                    <button
                                                        onClick={(e) => toggleExpand(child.path, e)}
                                                        className="w-full flex items-center gap-2 px-3 py-2 rounded-xl text-xs font-bold text-muted-foreground hover:bg-accent hover:text-foreground transition-all"
                                                    >
                                                        {child.icon}
                                                        <span className="flex-1 text-right">{child.title}</span>
                                                        <ChevronDown className={cn("h-3 w-3 opacity-40 transition-transform", expandedItems[child.path] ? "rotate-0" : "-rotate-90")} />
                                                    </button>
                                                    {expandedItems[child.path] && (
                                                        <div className="mr-4 pr-2 border-r border-border/30 space-y-1 animate-in slide-in-from-top-1">
                                                            {child.children.map(grandChild => (
                                                                <PrefetchNavLink
                                                                    key={grandChild.path}
                                                                    to={grandChild.path}
                                                                    className={() => {
                                                                        const isCurrent = (location.pathname + location.search) === grandChild.path;
                                                                        return cn(
                                                                            "flex items-center gap-2 px-3 py-2 rounded-lg text-[11px] transition-all",
                                                                            isCurrent
                                                                                ? "bg-primary/10 text-primary font-black shadow-sm"
                                                                                : "text-muted-foreground/80 hover:text-foreground hover:bg-accent/50"
                                                                        );
                                                                    }}
                                                                >
                                                                    {grandChild.icon}
                                                                    <span>{grandChild.title}</span>
                                                                </PrefetchNavLink>
                                                            ))}
                                                        </div>
                                                    )}
                                                </>
                                            ) : (
                                                <PrefetchNavLink
                                                    to={child.path}
                                                    className={() => {
                                                        const isStrictlyActive = (location.pathname + location.search) === child.path;
                                                        return cn(
                                                            "flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm transition-all relative overflow-hidden",
                                                            isStrictlyActive
                                                                ? "bg-primary text-primary-foreground font-bold shadow-md shadow-primary/10"
                                                                : "text-muted-foreground hover:bg-accent hover:text-foreground"
                                                        );
                                                    }}
                                                >
                                                    <div className="opacity-70">{child.icon}</div>
                                                    <span className="flex-1">{child.title}</span>
                                                    {(badges as any)[child.path] > 0 && (
                                                        <span className="flex h-5 min-w-5 px-1.5 items-center justify-center rounded-full bg-primary text-primary-foreground text-[10px] font-black animate-in zoom-in-50">
                                                            {(badges as any)[child.path]}
                                                        </span>
                                                    )}
                                                </PrefetchNavLink>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        </TooltipProvider>
    );
};
