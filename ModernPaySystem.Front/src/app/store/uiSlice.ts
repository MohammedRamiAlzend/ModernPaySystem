import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export type StatusType = 'success' | 'error' | 'warning' | 'info';

interface ConfirmState {
    isOpen: boolean;
    title: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    onConfirm?: () => void;
    onCancel?: () => void;
    variant?: 'primary' | 'destructive' | 'warning' | 'success';
}

interface UIState {
    status: {
        isOpen: boolean;
        type: StatusType;
        title: string;
        message: string;
        confirmLabel?: string;
    };
    confirm: ConfirmState;
}

const initialState: UIState = {
    status: {
        isOpen: false,
        type: 'success',
        title: '',
        message: '',
        confirmLabel: 'حسناً',
    },
    confirm: {
        isOpen: false,
        title: '',
        message: '',
        confirmLabel: 'تأكيد',
        cancelLabel: 'إلغاء',
        variant: 'primary'
    }
};

const uiSlice = createSlice({
    name: 'ui',
    initialState,
    reducers: {
        showStatus: (state, action: PayloadAction<Omit<UIState['status'], 'isOpen'>>) => {
            state.status.isOpen = true;
            state.status.type = action.payload.type;
            state.status.title = action.payload.title;
            state.status.message = action.payload.message;
            state.status.confirmLabel = action.payload.confirmLabel || 'حسناً';
        },
        hideStatus: (state) => {
            state.status.isOpen = false;
        },
        showConfirm: (state, action: PayloadAction<Omit<ConfirmState, 'isOpen'>>) => {
            state.confirm.isOpen = true;
            state.confirm.title = action.payload.title;
            state.confirm.message = action.payload.message;
            state.confirm.confirmLabel = action.payload.confirmLabel || 'تأكيد';
            state.confirm.cancelLabel = action.payload.cancelLabel || 'إلغاء';
            state.confirm.onConfirm = action.payload.onConfirm;
            state.confirm.onCancel = action.payload.onCancel;
            state.confirm.variant = action.payload.variant || 'primary';
        },
        hideConfirm: (state) => {
            state.confirm.isOpen = false;
            state.confirm.onConfirm = undefined;
            state.confirm.onCancel = undefined;
        },
    },
});

export const { showStatus, hideStatus, showConfirm, hideConfirm } = uiSlice.actions;
export default uiSlice.reducer;
