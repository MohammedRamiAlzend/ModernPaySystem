import React from 'react';
import { ActionModal } from './base-modal';
import { HelpCircle, Trash2, AlertTriangle, CheckCircle2 } from 'lucide-react';

interface ConfirmDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    title: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    variant?: 'primary' | 'destructive' | 'warning' | 'success';
}

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
    isOpen,
    onClose,
    onConfirm,
    title,
    message,
    confirmLabel = 'تأكيد',
    cancelLabel = 'إلغاء',
    variant = 'primary',
}) => {
    const icons = {
        primary: <HelpCircle className="w-6 h-6" />,
        destructive: <Trash2 className="w-6 h-6" />,
        warning: <AlertTriangle className="w-6 h-6" />,
        success: <CheckCircle2 className="w-6 h-6" />,
    };

    return (
        <ActionModal
            isOpen={isOpen}
            onClose={onClose}
            onConfirm={onConfirm}
            title={title}
            description={message}
            confirmLabel={confirmLabel}
            cancelLabel={cancelLabel}
            variant={variant}
            icon={icons[variant]}
            maxWidth="sm"
        />
    );
};
