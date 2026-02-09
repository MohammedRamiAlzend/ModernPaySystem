import { createSlice } from '@reduxjs/toolkit';
import type { PayloadAction } from '@reduxjs/toolkit';

// تحسين الـ User ليتناسب مع البيانات القادمة من التوكن
export interface User {
  id: string;
  username: string;
  permissions: string[];
  roles: string[];
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
}

// محاولة استعادة البيانات من localStorage عند التشغيل
const savedToken = localStorage.getItem('token');
const savedUser = localStorage.getItem('user');

const initialState: AuthState = {
  user: savedUser ? JSON.parse(savedUser) : null,
  token: savedToken,
  isAuthenticated: !!savedToken,
};

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    loginSuccess: (state, action: PayloadAction<{ user: User; token: string }>) => {
      state.isAuthenticated = true;
      state.user = action.payload.user;
      state.token = action.payload.token;

      localStorage.setItem('token', action.payload.token);
      localStorage.setItem('user', JSON.stringify(action.payload.user));
    },

    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAuthenticated = false;

      localStorage.removeItem('token');
      localStorage.removeItem('user');
    },

    updateUserProfile: (state, action: PayloadAction<Partial<User>>) => {
      if (state.user) {
        state.user = { ...state.user, ...action.payload };
        localStorage.setItem('user', JSON.stringify(state.user));
      }
    },
  },
});

export const {
  loginSuccess,
  logout,
  updateUserProfile,
} = authSlice.actions;

type RootState = {
  auth: AuthState;
};

export const selectCurrentUser = (state: RootState): User | null => state.auth.user;
export const selectIsAuthenticated = (state: RootState): boolean => state.auth.isAuthenticated;
export const selectAuthToken = (state: RootState): string | null => state.auth.token;
export const selectUserPermissions = (state: RootState): string[] => state.auth.user?.permissions || [];
export const selectUserRoles = (state: RootState): string[] => state.auth.user?.roles || [];

export default authSlice.reducer;