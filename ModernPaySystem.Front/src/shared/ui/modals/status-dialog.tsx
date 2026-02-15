import React from 'react';
import { BaseModal } from './base-modal';
import { CheckCircle2, XCircle, AlertTriangle, Info } from 'lucide-react';
import { Button } from '../button';
import { cn } from '@/shared/lib/utils';

export type StatusType = 'success' | 'error' | 'warning' | 'info';

interface StatusDialogProps {
    isOpen: boolean;
    onClose: () => void;
    type: StatusType;
    title: string;
    message: string;
    confirmLabel?: string;
}

export const StatusDialog: React.FC<StatusDialogProps> = ({
    isOpen,
    onClose,
    type,
    title,
    message,
    confirmLabel = 'حسناً',
}) => {
    const icons = {
        success: <CheckCircle2 className="w-12 h-12 text-emerald-500" />,
        error: <XCircle className="w-12 h-12 text-red-500" />,
        warning: <AlertTriangle className="w-12 h-12 text-amber-500" />,
        info: <Info className="w-12 h-12 text-blue-500" />,
    };

    const colors = {
        success: "text-emerald-500 bg-emerald-50",
        error: "text-red-500 bg-red-50",
        warning: "text-amber-500 bg-amber-50",
        info: "text-blue-50 bg-blue-50",
    };

    return (
        <BaseModal
            isOpen={isOpen}
            onClose={onClose}
            maxWidth="sm"
            title={null} // We render custom title in children for better design
        >
            <div className="flex flex-col items-center text-center p-4 gap-4">
                <div className={cn("p-4 rounded-full", colors[type])}>
                    {icons[type]}
                </div>

                <div className="space-y-2">
                    <h3 className="text-xl font-black text-slate-800">{title}</h3>
                    <p className="text-slate-500 font-medium leading-relaxed">
                        {message}
                    </p>
                </div>

                <Button
                    onClick={onClose}
                    className={cn(
                        "w-full h-12 rounded-xl font-bold mt-2",
                        type === 'success' && "bg-emerald-500 hover:bg-emerald-600",
                        type === 'error' && "bg-red-500 hover:bg-red-600",
                        type === 'warning' && "bg-amber-500 hover:bg-amber-600",
                        type === 'info' && "bg-blue-500 hover:bg-blue-600"
                    )}
                >
                    {confirmLabel}
                </Button>
            </div>
        </BaseModal>
    );
};
