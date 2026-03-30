import React from 'react';
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "@/shared/ui/alert-dialog";
import { cn } from '@/shared/lib/utils';
import { Loader2 } from 'lucide-react';

export interface BaseModalProps {
    isOpen: boolean;
    onClose: () => void;
    title: React.ReactNode;
    description?: React.ReactNode;
    children?: React.ReactNode;
    footer?: React.ReactNode;
    maxWidth?: 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl';
    maxHeight?: 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl' | '4xl';
    showCloseButton?: boolean;
    hideHeaderVisually?: boolean;
}

/**
 * BaseModal: The foundational structure for all modals in the system.
 * It provides a consistent look, animations, and accessibility.
 */
export const BaseModal: React.FC<BaseModalProps> = ({
    isOpen,
    onClose,
    title,
    description,
    children,
    footer,
    maxWidth = 'md',
    maxHeight = 'lg',
    hideHeaderVisually = false
}) => {
    const maxWidthClasses = {
        'sm': 'max-w-[380px]',
        'md': 'max-w-[450px]',
        'lg': 'max-w-[600px]',
        'xl': 'max-w-[800px]',
        '2xl': 'max-w-[1000px]',
        '3xl': 'max-w-[1200px]',
        '4xl': 'max-w-[1400px]',
    };
    const maxHeightClasses = {
        'sm': 'h-auto max-h-[30vh]',
        'md': 'h-auto max-h-[50vh]',
        'lg': 'h-auto max-h-[70vh]',
        'xl': 'h-auto max-h-[85vh]',
        '2xl': 'h-auto max-h-[90vh]',
        '3xl': 'h-auto max-h-[92vh]',
        '4xl': 'h-auto max-h-[95vh]',
    };

    return (
        <AlertDialog open={isOpen} onOpenChange={onClose}>
            <AlertDialogContent
                className={cn(
                    "border-none shadow-2xl rounded-3xl p-0 overflow-hidden bg-background flex flex-col",
                    maxWidthClasses[maxWidth],
                    maxHeightClasses[maxHeight]
                )}
            >
                <div className="p-4 md:p-6 flex flex-col h-full overflow-hidden">
                    <AlertDialogHeader className={cn("text-right space-y-2 shrink-0", (hideHeaderVisually || (!title && !description)) && "sr-only")}>
                        <AlertDialogTitle asChild className={cn("text-2xl font-black leading-tight", !title && !hideHeaderVisually && "sr-only")}>
                            <div>{title || 'تنبيه'}</div>
                        </AlertDialogTitle>
                        <AlertDialogDescription asChild className={cn("font-medium text-base leading-relaxed", !description && !hideHeaderVisually && "sr-only")}>
                            <div>{description || title || 'تنبيه'}</div>
                        </AlertDialogDescription>
                    </AlertDialogHeader>

                    <div className="my-4 overflow-y-auto flex-1 pr-2 custom-scrollbar min-h-0">
                        {children}
                    </div>

                    {footer && (
                        <AlertDialogFooter className="mt-auto gap-3 flex-col-reverse sm:flex-row shrink-0">
                            {footer}
                        </AlertDialogFooter>
                    )}
                </div>
            </AlertDialogContent>
        </AlertDialog>
    );
};

interface ActionModalProps extends Omit<BaseModalProps, 'footer' | 'children' | 'title'> {
    title?: string;
    confirmLabel?: string;
    cancelLabel?: string;
    onConfirm: () => void;
    isLoading?: boolean;
    variant?: 'primary' | 'destructive' | 'warning' | 'success';
    icon?: React.ReactNode;
}

/**
 * ActionModal: A prepared modal for standard actions (Confirm, Success, Warning).
 */
export const ActionModal: React.FC<ActionModalProps> = ({
    title = 'تأكيد العملية',
    confirmLabel = 'تأكيد',
    cancelLabel = 'إلغاء',
    onConfirm,
    isLoading = false,
    variant = 'primary',
    icon,
    ...baseProps
}) => {
    const variantStyles = {
        primary: "bg-primary hover:bg-primary/90 shadow-primary/20",
        destructive: "bg-red-500 hover:bg-red-600 shadow-red-200",
        warning: "bg-amber-500 hover:bg-amber-600 shadow-amber-200",
        success: "bg-emerald-500 hover:bg-emerald-600 shadow-emerald-200",
    };

    const iconBgStyles = {
        primary: "bg-blue-50 text-blue-600",
        destructive: "bg-red-50 text-red-600",
        warning: "bg-amber-50 text-amber-600",
        success: "bg-emerald-50 text-emerald-600",
    };

    return (
        <BaseModal
            {...baseProps}
            title={
                <div className="flex items-center gap-4">
                    {icon && (
                        <div className={cn("p-3 rounded-2xl", iconBgStyles[variant])}>
                            {icon}
                        </div>
                    )}
                    <span>{title}</span>
                </div>
            }
            footer={
                <>
                    <AlertDialogCancel
                        className="flex-1 h-12 rounded-xl font-bold border-slate-200 hover:bg-slate-50/20 transition-all"
                    >
                        {cancelLabel}
                    </AlertDialogCancel>
                    <AlertDialogAction
                        className={cn(
                            "flex-1 h-12 rounded-xl font-bold text-white shadow-lg transition-all active:scale-95 flex items-center justify-center gap-2",
                            variantStyles[variant]
                        )}
                        onClick={(e) => {
                            e.preventDefault();
                            onConfirm();
                        }}
                        disabled={isLoading}
                    >
                        {isLoading && <Loader2 className="w-4 h-4 animate-spin" />}
                        {isLoading ? 'جاري التنفيذ...' : confirmLabel}
                    </AlertDialogAction>
                </>
            }
        />
    );
};
