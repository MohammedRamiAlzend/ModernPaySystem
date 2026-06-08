import React from 'react';
import { Folder } from '@/features/archiving/model/types';
import { Input } from '@/shared/ui/input';
import { Search, X, LayoutGrid, List, ChevronLeft, ChevronRight } from 'lucide-react';

interface ExplorerToolbarProps {
    searchTerm: string;
    onSearchTermChange: (val: string) => void;
    viewMode: 'explorer' | 'list';
    onViewModeChange: (mode: 'explorer' | 'list') => void;
    breadcrumbs: Folder[];
    onNavigateToFolder: (folder: Folder | null) => void;
    canGoBack?: boolean;
    canGoForward?: boolean;
    onGoBack?: () => void;
    onGoForward?: () => void;
}

export function ExplorerToolbar({
    searchTerm,
    onSearchTermChange,
    viewMode,
    onViewModeChange,
    breadcrumbs,
    onNavigateToFolder,
    canGoBack = false,
    canGoForward = false,
    onGoBack,
    onGoForward
}: ExplorerToolbarProps) {
    return (
        <div className="flex flex-col gap-4 w-full min-w-0">
            {/* Toolbar & Filter Bar */}
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-muted/20 border border-border/80 p-4 rounded-3xl w-full min-w-0">
                {/* Search Input */}
                <div className="relative flex-1 max-w-md">
                    <Search className="absolute right-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        value={searchTerm}
                        onChange={(e) => onSearchTermChange(e.target.value)}
                        placeholder="ابحث عن مجلد أو رقم أرشيف..."
                        className="pr-10 rounded-2xl h-11 bg-background border-border"
                    />
                    {searchTerm && (
                        <button
                            onClick={() => onSearchTermChange('')}
                            className="absolute left-3.5 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground"
                        >
                            <X className="h-4 w-4" />
                        </button>
                    )}
                </div>

                {/* View Mode Toggle */}
                <div className="flex items-center gap-2 justify-end">
                    <div className="flex bg-muted/80 p-1.5 rounded-2xl border border-border/50">
                        <button
                            onClick={() => onViewModeChange('explorer')}
                            className={`flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold transition-all ${viewMode === 'explorer'
                                ? 'bg-card text-primary shadow-sm'
                                : 'text-muted-foreground hover:text-foreground'
                                }`}
                            title="عرض شبكي مستكشف"
                        >
                            <LayoutGrid className="h-4 w-4" />
                            <span>شبكة</span>
                        </button>
                        <button
                            onClick={() => onViewModeChange('list')}
                            className={`flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold transition-all ${viewMode === 'list'
                                ? 'bg-card text-primary shadow-sm'
                                : 'text-muted-foreground hover:text-foreground'
                                }`}
                            title="عرض كقائمة جدولية"
                        >
                            <List className="h-4 w-4" />
                            <span>قائمة</span>
                        </button>
                    </div>
                </div>
            </div>

            {/* Breadcrumbs & Navigation History */}
            <div className="flex items-center gap-2 text-sm bg-muted/50 p-2.5 rounded-2xl border border-border select-none w-full min-w-0" style={{ direction: 'rtl' }}>
                {/* History Navigation Buttons */}
                <div className="flex items-center gap-1 pl-2 border-l border-border/80 shrink-0">
                    <button
                        onClick={onGoBack}
                        disabled={!canGoBack}
                        className={`p-1.5 rounded-lg transition-colors shrink-0 ${
                            canGoBack
                                ? 'text-foreground hover:bg-muted/80 cursor-pointer'
                                : 'text-muted-foreground/30 cursor-not-allowed'
                        }`}
                        title="السابق"
                    >
                        <ChevronRight className="h-4 w-4" />
                    </button>
                    <button
                        onClick={onGoForward}
                        disabled={!canGoForward}
                        className={`p-1.5 rounded-lg transition-colors shrink-0 ${
                            canGoForward
                                ? 'text-foreground hover:bg-muted/80 cursor-pointer'
                                : 'text-muted-foreground/30 cursor-not-allowed'
                        }`}
                        title="التالي"
                    >
                        <ChevronLeft className="h-4 w-4" />
                    </button>
                </div>

                {/* Scrollable Breadcrumbs Container */}
                <div className="flex items-center gap-2 overflow-x-auto flex-1 min-w-0 max-w-full whitespace-nowrap py-0.5 custom-scrollbar">
                    <button
                        onClick={() => onNavigateToFolder(null)}
                        className="text-muted-foreground hover:text-primary transition-colors font-bold shrink-0"
                    >
                        الأرشيف الرئيسي
                    </button>
                    {breadcrumbs.map((crumb, idx) => (
                        <React.Fragment key={crumb.id}>
                            <ChevronLeft className="h-4 w-4 text-muted-foreground/60 shrink-0" />
                            <button
                                onClick={() => onNavigateToFolder(crumb)}
                                className={`font-semibold transition-colors shrink-0 ${idx === breadcrumbs.length - 1 ? 'text-primary font-bold' : 'text-muted-foreground hover:text-primary'
                                    }`}
                            >
                                {crumb.name}
                            </button>
                        </React.Fragment>
                    ))}
                </div>
            </div>
        </div>
    );
}
