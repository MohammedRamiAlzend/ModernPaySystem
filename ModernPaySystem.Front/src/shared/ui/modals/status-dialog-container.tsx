import React from 'react';
import { useAppDispatch, useAppSelector } from '@/app/store';
import { hideStatus } from '@/app/store/uiSlice';
import { StatusDialog } from './status-dialog';

export const StatusDialogContainer: React.FC = () => {
    const dispatch = useAppDispatch();
    const { isOpen, type, title, message, confirmLabel } = useAppSelector((state) => state.ui);

    const handleClose = () => {
        dispatch(hideStatus());
    };

    return (
        <StatusDialog
            isOpen={isOpen}
            onClose={handleClose}
            type={type}
            title={title}
            message={message}
            confirmLabel={confirmLabel}
        />
    );
};
