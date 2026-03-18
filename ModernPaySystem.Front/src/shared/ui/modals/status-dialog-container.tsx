import React from 'react';
import { useUIStore } from '@/app/store/uiStore';
import { StatusDialog } from './status-dialog';
import { ConfirmDialog } from './confirm-dialog';

export const GlobalDialogContainer: React.FC = () => {
    const { status, confirm, hideStatus, hideConfirm } = useUIStore();

    const handleStatusClose = () => {
        hideStatus();
    };

    const handleConfirmClose = () => {
        if (confirm.onCancel) confirm.onCancel();
        hideConfirm();
    };

    const handleConfirmAction = () => {
        if (confirm.onConfirm) confirm.onConfirm();
        hideConfirm();
    };

    return (
        <>
            <StatusDialog
                isOpen={status.isOpen}
                onClose={handleStatusClose}
                type={status.type}
                title={status.title}
                message={status.message}
                confirmLabel={status.confirmLabel}
            />
            <ConfirmDialog
                isOpen={confirm.isOpen}
                onClose={handleConfirmClose}
                onConfirm={handleConfirmAction}
                title={confirm.title}
                message={confirm.message}
                confirmLabel={confirm.confirmLabel}
                cancelLabel={confirm.cancelLabel}
                variant={confirm.variant}
            />
        </>
    );
};
