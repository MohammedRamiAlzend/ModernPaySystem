import { create } from 'zustand';
import { jwtDecode } from 'jwt-decode';
import { queryClient } from '@/shared/lib/query-client';

export interface User {
  id: string;
  username: string;
  subsystem: string;
  permissions: string[];
  roles: string[];
  isDepartmentHead: boolean;
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  loginSuccess: (user: User, token: string) => void;
  logout: () => void;
  updateUserProfile: (user: Partial<User>) => void;
  checkTokenExpiration: () => void;
}

const savedToken = sessionStorage.getItem('token');
const savedUser = sessionStorage.getItem('user');

export const useAuthStore = create<AuthState>((set) => ({
  user: savedUser ? JSON.parse(savedUser) : null,
  token: savedToken,
  isAuthenticated: !!savedToken,

  loginSuccess: (user, token) => {
    sessionStorage.setItem('token', token);
    sessionStorage.setItem('user', JSON.stringify(user));
    queryClient.clear(); // مسح الكاش عند تسجيل دخول جديد
    set({ isAuthenticated: true, user, token });
  },

  logout: () => {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    sessionStorage.removeItem('redirectAfterLogin');

    queryClient.clear(); // مسح الكاش تماماً عند تسجيل الخروج
    set({ user: null, token: null, isAuthenticated: false });
  },

  updateUserProfile: (updates) => {
    set((state) => {
      if (state.user) {
        const updatedUser = { ...state.user, ...updates };
        sessionStorage.setItem('user', JSON.stringify(updatedUser));
        return { user: updatedUser };
      }
      return state;
    });
  },

  checkTokenExpiration: () => {
    const { token, logout } = useAuthStore.getState();
    if (!token) return;

    try {
      const decoded = jwtDecode<{ exp: number }>(token);
      const currentTime = Date.now() / 1000;

      if (decoded.exp < currentTime) {
        console.warn('Session expired based on token payload');
        logout();
        if (window.location.pathname !== '/auth/login') {
          window.location.href = '/auth/login?reason=expired';
        }
      }
    } catch (error) {
      console.error('Error decoding token:', error);
      logout();
    }
  },
}));
