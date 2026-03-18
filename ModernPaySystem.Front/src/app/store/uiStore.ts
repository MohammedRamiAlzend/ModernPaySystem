import { create } from 'zustand';

export type StatusType = 'success' | 'error' | 'warning' | 'info';

export interface ConfirmState {
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
    showStatus: (payload: Omit<UIState['status'], 'isOpen'>) => void;
    hideStatus: () => void;
    showConfirm: (payload: Omit<ConfirmState, 'isOpen'>) => void;
    hideConfirm: () => void;
}

const initialStatus = {
    isOpen: false,
    type: 'success' as StatusType,
    title: '',
    message: '',
    confirmLabel: 'حسناً',
};

const initialConfirm: ConfirmState = {
    isOpen: false,
    title: '',
    message: '',
    confirmLabel: 'تأكيد',
    cancelLabel: 'إلغاء',
    variant: 'primary'
};

export const useUIStore = create<UIState>((set) => ({
    status: initialStatus,
    confirm: initialConfirm,

    showStatus: (payload) => set((state) => ({
        status: { ...state.status, ...payload, isOpen: true, confirmLabel: payload.confirmLabel || 'حسناً' }
    })),

    hideStatus: () => set((state) => ({
        status: { ...state.status, isOpen: false }
    })),

    showConfirm: (payload) => set((state) => ({
        confirm: { ...state.confirm, ...payload, isOpen: true, confirmLabel: payload.confirmLabel || 'تأكيد', cancelLabel: payload.cancelLabel || 'إلغاء', variant: payload.variant || 'primary' }
    })),

    hideConfirm: () => set((state) => ({
        confirm: { ...state.confirm, isOpen: false, onConfirm: undefined, onCancel: undefined }
    })),
}));
