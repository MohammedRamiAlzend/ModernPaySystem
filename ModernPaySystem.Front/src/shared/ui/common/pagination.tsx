import React from 'react';
import { ChevronRight, ChevronLeft, MoreHorizontal } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { cn } from '@/shared/lib/utils';

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
    className?: string;
}

export const Pagination: React.FC<PaginationProps> = ({
    currentPage,
    totalPages,
    onPageChange,
    className
}) => {
    if (totalPages <= 1) return null;

    const getPageNumbers = () => {
        const pages: (number | string)[] = [];
        const showMax = 5;

        if (totalPages <= showMax) {
            for (let i = 1; i <= totalPages; i++) pages.push(i);
        } else {
            // Always show first page
            pages.push(1);

            if (currentPage > 3) {
                pages.push('ellipsis-start');
            }

            // Show pages around current
            const start = Math.max(2, currentPage - 1);
            const end = Math.min(totalPages - 1, currentPage + 1);

            for (let i = start; i <= end; i++) {
                if (!pages.includes(i)) pages.push(i);
            }

            if (currentPage < totalPages - 2) {
                pages.push('ellipsis-end');
            }

            // Always show last page
            if (!pages.includes(totalPages)) pages.push(totalPages);
        }
        return pages;
    };

    return (
        <nav
            role="navigation"
            aria-label="pagination"
            className={cn("flex justify-center items-center gap-2", className)}
            dir="rtl"
        >
            <Button
                variant="outline"
                size="icon"
                className="h-9 w-9 rounded-xl border-primary/20 hover:bg-primary/10 hover:text-primary transition-all"
                disabled={currentPage <= 1}
                onClick={() => onPageChange(currentPage - 1)}
            >
                <ChevronRight className="w-4 h-4" />
            </Button>

            <div className="flex items-center gap-1.5">
                {getPageNumbers().map((page, idx) => {
                    if (typeof page === 'string') {
                        return (
                            <div key={`ellipsis-${idx}`} className="w-9 h-9 flex items-center justify-center text-muted-foreground">
                                <MoreHorizontal className="w-4 h-4" />
                            </div>
                        );
                    }

                    const isCurrent = page === currentPage;
                    return (
                        <Button
                            key={page}
                            variant={isCurrent ? "default" : "outline"}
                            size="icon"
                            className={cn(
                                "h-9 w-9 rounded-xl transition-all duration-300 font-bold text-sm",
                                isCurrent
                                    ? "shadow-lg shadow-primary/30 scale-110 z-10"
                                    : "border-primary/10 hover:border-primary/30 hover:bg-primary/5 hover:text-primary"
                            )}
                            onClick={() => onPageChange(page)}
                        >
                            {page}
                        </Button>
                    );
                })}
            </div>

            <Button
                variant="outline"
                size="icon"
                className="h-9 w-9 rounded-xl border-primary/20 hover:bg-primary/10 hover:text-primary transition-all"
                disabled={currentPage >= totalPages}
                onClick={() => onPageChange(currentPage + 1)}
            >
                <ChevronLeft className="w-4 h-4" />
            </Button>
        </nav>
    );
};
