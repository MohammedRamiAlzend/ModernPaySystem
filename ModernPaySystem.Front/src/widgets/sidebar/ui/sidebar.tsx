import React, { useState } from 'react';
import { cn } from '@/shared/lib/utils';
import { Button } from '@/shared/ui/button';
import { PrefetchNavLink } from '@/shared/navigation/prefetch-nav-link';
import {
    ChevronRight,
    ChevronLeft,
    LayoutDashboard,
    LogOut,
    Settings,
} from 'lucide-react';
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from '@/shared/ui/tooltip';
import { useAuthStore } from '@/app/store/authStore';

import { useSidebarMode } from '../model/use-sidebar-mode';
import type { SidebarMode } from '../model/sidebar-mode.types';

import { SidebarMainContent } from './sidebar-main-content';
import { SidebarSettingsContent } from './sidebar-settings-content';

interface SidebarProps {
    className?: string;
    onItemClick?: () => void;
}

/**
 * Maps each sidebar mode to its header configuration.
 * Extend here when adding new modes.
 */
const SIDEBAR_HEADERS: Record<SidebarMode, {
    icon: React.ReactNode;
    title: string;
    linkTo: string;
}> = {
    main: {
        icon: <LayoutDashboard className="text-primary-foreground h-5 w-5" />,
        title: 'منصة خدمات ريف دمشق',
        linkTo: '/',
    },
    settings: {
        icon: <Settings className="text-primary-foreground h-5 w-5" />,
        title: 'إعدادات النظام',
        linkTo: '/settings',
    },
};

/**
 * Smart Sidebar Shell
 *
 * Automatically switches between navigation modes based on the current route.
 * Uses Strategy pattern — the content is delegated to mode-specific components.
 *
 * Architecture:
 * - Shell handles: header, collapse toggle, footer (logout)
 * - Content components handle: mode-specific navigation rendering
 */
export const Sidebar: React.FC<SidebarProps> = ({ className, onItemClick }) => {
    const [isCollapsed, setIsCollapsed] = useState(false);
    const logout = useAuthStore((state) => state.logout);
    const handleLogout = () => {
        logout();
        // التوجيه لصفحة تسجيل الدخول بشكل نظيف لمسح أي معاملات redirect في الرابط
        window.location.href = '/auth/login';
    };
    const activeMode = useSidebarMode();

    const headerConfig = SIDEBAR_HEADERS[activeMode];

    /**
     * Renders the content component for the active mode.
     * New modes can be added here by adding a case to the switch.
     */
    const renderContent = () => {
        const contentProps = { isCollapsed, onItemClick };

        switch (activeMode) {
            case 'settings':
                return <SidebarSettingsContent {...contentProps} />;
            case 'main':
            default:
                return <SidebarMainContent {...contentProps} />;
        }
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
                <TooltipProvider delayDuration={0}>
                    <Tooltip>
                        <TooltipTrigger asChild>
                            <PrefetchNavLink to={headerConfig.linkTo} className="flex items-center gap-3 overflow-hidden">
                                <div className="w-8 h-8 min-w-[32px] bg-primary rounded-xl flex items-center justify-center shadow-lg shadow-primary/20">
                                    {headerConfig.icon}
                                </div>
                                {!isCollapsed && (
                                    <span className="text-lg font-black text-foreground drop-shadow-sm whitespace-nowrap animate-in fade-in slide-in-from-right-4">
                                        {headerConfig.title}
                                    </span>
                                )}
                            </PrefetchNavLink>
                        </TooltipTrigger>
                        {isCollapsed && (
                            <TooltipContent side="left" className="font-bold bg-popover text-popover-foreground border shadow-md">
                                {headerConfig.title}
                            </TooltipContent>
                        )}
                    </Tooltip>
                </TooltipProvider>

                {/* Collapse Toggle Button */}
                <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => setIsCollapsed(!isCollapsed)}
                    className={cn(
                        "absolute -left-2.5 top-1/2 -translate-y-1/2 w-6 h-6 rounded-full border bg-background shadow-sm opacity-0 group-hover:opacity-100 transition-opacity z-50",
                        className?.includes("md:hidden") ? "hidden" : ""
                    )}
                >
                    {isCollapsed ? <ChevronLeft className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
                </Button>
            </div>

            {/* Dynamic Content Area */}
            {renderContent()}

            {/* Footer Actions */}
            <div className="p-1 border-t space-y-1">
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
