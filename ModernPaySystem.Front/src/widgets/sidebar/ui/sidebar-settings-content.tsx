import React from 'react';
import { cn } from '@/shared/lib/utils';
import { useNavigate, useLocation } from 'react-router-dom';
import { SETTINGS_CONFIG } from '@/pages/settings/config/settings-config';
import { ArrowLeft, Settings } from 'lucide-react';
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from '@/shared/ui/tooltip';
import type { SidebarContentProps } from '../model/sidebar-mode.types';

/**
 * Settings sidebar content — replaces navigation tabs with a smart sidebar.
 */
export const SidebarSettingsContent: React.FC<SidebarContentProps> = ({
    isCollapsed,
    onItemClick,
}) => {
    const navigate = useNavigate();
    const location = useLocation();

    const searchParams = new URLSearchParams(location.search);
    const activeSection = searchParams.get('tab') || SETTINGS_CONFIG[0].id;

    const handleSectionClick = (sectionId: string) => {
        navigate(`/settings?tab=${sectionId}`);
        onItemClick?.();
    };

    const handleBackClick = () => {
        navigate('/');
        onItemClick?.();
    };

    return (
        <TooltipProvider delayDuration={0}>
            <div className="flex-1 overflow-y-auto py-4 px-3 space-y-4">
                {/* Back Navigation */}
                <Tooltip>
                    <TooltipTrigger asChild>
                        <button
                            onClick={handleBackClick}
                            className={cn(
                                "flex items-center gap-3 w-full px-3 py-3 rounded-2xl transition-all duration-300",
                                "text-muted-foreground hover:bg-accent hover:text-foreground group/back",
                                isCollapsed && "justify-center"
                            )}
                        >
                            <ArrowLeft className="h-4 w-4 transition-transform group-hover/back:-translate-x-1" />
                            {!isCollapsed && (
                                <span className="font-bold text-sm animate-in fade-in slide-in-from-right-3">
                                    العودة للرئيسية
                                </span>
                            )}
                        </button>
                    </TooltipTrigger>
                    {isCollapsed && (
                        <TooltipContent side="left" className="bg-popover text-popover-foreground border font-bold">
                            العودة للرئيسية
                        </TooltipContent>
                    )}
                </Tooltip>

                <div className="h-px bg-border/50 mx-2" />

                {/* Settings Header */}
                {!isCollapsed && (
                    <div className="px-3 py-1 space-y-1 animate-in fade-in slide-in-from-right-3 text-right">
                        <div className="flex items-center gap-2 justify-start">
                            <Settings className="h-4 w-4 text-primary" />
                            <h3 className="text-sm font-black text-primary">الإعدادات</h3>
                        </div>
                        <p className="text-[11px] text-muted-foreground leading-relaxed">
                            تخصيص الخيارات والحقول
                        </p>
                    </div>
                )}

                {/* Sections */}
                <div className="space-y-1.5">
                    {SETTINGS_CONFIG.map((section) => {
                        const isActive = activeSection === section.id;
                        const Icon = section.icon;

                        return (
                            <Tooltip key={section.id}>
                                <TooltipTrigger asChild>
                                    <button
                                        onClick={() => handleSectionClick(section.id)}
                                        onMouseEnter={() => section.preload?.()}
                                        className={cn(
                                            "w-full flex items-center gap-3 px-3 py-3 rounded-2xl transition-all duration-300 group/item relative",
                                            isActive
                                                ? "bg-primary text-primary-foreground shadow-lg shadow-primary/25"
                                                : "text-muted-foreground hover:bg-accent hover:text-foreground",
                                            isCollapsed && "justify-center px-2"
                                        )}
                                    >
                                        <Icon className="h-[18px] w-[18px] transition-transform group-hover/item:scale-110" />
                                        
                                        {!isCollapsed && (
                                            <div className="flex flex-col items-start min-w-0 flex-1 text-right">
                                                <span className="font-bold text-sm truncate w-full animate-in fade-in slide-in-from-right-2">
                                                    {section.label}
                                                </span>
                                                <span className={cn(
                                                    "text-[10px] truncate w-full opacity-70",
                                                    isActive ? "text-primary-foreground" : "text-muted-foreground"
                                                )}>
                                                    {section.description}
                                                </span>
                                            </div>
                                        )}
                                    </button>
                                </TooltipTrigger>
                                {isCollapsed && (
                                    <TooltipContent side="left" align="center" className="bg-popover text-popover-foreground border shadow-xl">
                                        <div className="flex flex-col gap-0.5 p-1 min-w-[120px] text-right" style={{ direction: 'rtl' }}>
                                            <p className="font-bold text-xs">{section.label}</p>
                                            <p className="text-[10px] text-muted-foreground">{section.description}</p>
                                        </div>
                                    </TooltipContent>
                                )}
                            </Tooltip>
                        );
                    })}
                </div>
            </div>
        </TooltipProvider>
    );
};
