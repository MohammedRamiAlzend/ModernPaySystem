import { createSlice, PayloadAction } from '@reduxjs/toolkit';

export type StatusType = 'success' | 'error' | 'warning' | 'info';

interface StatusState {
    isOpen: boolean;
    type: StatusType;
    title: string;
    message: string;
    confirmLabel?: string;
}

const initialState: StatusState = {
    isOpen: false,
    type: 'success',
    title: '',
    message: '',
    confirmLabel: 'حسناً',
};

const uiSlice = createSlice({
    name: 'ui',
    initialState,
    reducers: {
        showStatus: (state, action: PayloadAction<Omit<StatusState, 'isOpen'>>) => {
            state.isOpen = true;
            state.type = action.payload.type;
            state.title = action.payload.title;
            state.message = action.payload.message;
            state.confirmLabel = action.payload.confirmLabel || 'حسناً';
        },
        hideStatus: (state) => {
            state.isOpen = false;
        },
    },
});

export const { showStatus, hideStatus } = uiSlice.actions;
export default uiSlice.reducer;
