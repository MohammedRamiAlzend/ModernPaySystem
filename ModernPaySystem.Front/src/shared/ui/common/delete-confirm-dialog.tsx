import React from 'react';
import { Trash2, AlertTriangle } from 'lucide-react';
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

interface DeleteConfirmDialogProps {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    onConfirm: () => void;
    title?: string;
    description?: string;
    confirmText?: string;
    cancelText?: string;
}

export const DeleteConfirmDialog: React.FC<DeleteConfirmDialogProps> = ({
    open,
    onOpenChange,
    onConfirm,
    title = "هل أنت متأكد من الحذف؟",
    description = "هذا الإجراء نهائي ولا يمكن التراجع عنه. سيتم حذف كافة البيانات المرتبطة بشكل دائم.",
    confirmText = "تأكيد الحذف",
    cancelText = "إلغاء",
}) => {
    return (
        <AlertDialog open={open} onOpenChange={onOpenChange}>
            <AlertDialogContent className="rounded-2xl border-none shadow-2xl max-w-md text-right overflow-hidden p-0" style={{ direction: 'rtl' }}>
                <div className="bg-destructive/5 pt-8 pb-6 flex flex-col items-center gap-4">
                    <div className="w-16 h-16 rounded-full bg-destructive/10 flex items-center justify-center text-destructive animate-in zoom-in-50 duration-300">
                        <Trash2 className="w-8 h-8" />
                    </div>
                </div>

                <div className="p-6 pt-2">
                    <AlertDialogHeader className="text-right space-y-3">
                        <AlertDialogTitle className="text-2xl font-bold text-slate-800 dark:text-slate-100">
                            {title}
                        </AlertDialogTitle>
                        <AlertDialogDescription className="text-base text-muted-foreground leading-relaxed">
                            {description}
                        </AlertDialogDescription>
                    </AlertDialogHeader>

                    <div className="bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-900/50 rounded-xl p-3 mt-4 flex items-center gap-3 text-amber-700 dark:text-amber-400">
                        <AlertTriangle className="w-5 h-5 flex-shrink-0" />
                        <span className="text-xs font-medium">تحذير: لا يمكن استعادة البيانات بعد الحذف.</span>
                    </div>

                    <AlertDialogFooter className="flex-row-reverse gap-3 pt-8">
                        <AlertDialogAction
                            onClick={onConfirm}
                            className="bg-destructive hover:bg-destructive/90 text-destructive-foreground rounded-xl px-10 h-11 shadow-lg shadow-destructive/20 font-bold flex-1"
                        >
                            {confirmText}
                        </AlertDialogAction>
                        <AlertDialogCancel className="rounded-xl px-8 h-11 border-slate-200 dark:border-slate-800 hover:bg-slate-50 dark:hover:bg-slate-900 transition-all font-medium flex-1 mt-0">
                            {cancelText}
                        </AlertDialogCancel>
                    </AlertDialogFooter>
                </div>
            </AlertDialogContent>
        </AlertDialog>
    );
};

