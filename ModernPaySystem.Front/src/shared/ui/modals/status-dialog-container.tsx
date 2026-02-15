import React from 'react';
import { useAppDispatch, useAppSelector } from '@/app/store';
import { hideStatus, hideConfirm } from '@/app/store/uiSlice';
import { StatusDialog } from './status-dialog';
import { ConfirmDialog } from './confirm-dialog';

export const GlobalDialogContainer: React.FC = () => {
    const dispatch = useAppDispatch();
    const { status, confirm } = useAppSelector((state) => state.ui);

    const handleStatusClose = () => {
        dispatch(hideStatus());
    };

    const handleConfirmClose = () => {
        if (confirm.onCancel) confirm.onCancel();
        dispatch(hideConfirm());
    };

    const handleConfirmAction = () => {
        if (confirm.onConfirm) confirm.onConfirm();
        dispatch(hideConfirm());
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
