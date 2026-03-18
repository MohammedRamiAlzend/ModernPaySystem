import { create } from 'zustand';

export interface User {
  id: string;
  username: string;
  subsystem: string;
  permissions: string[];
  roles: string[];
}

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  loginSuccess: (user: User, token: string) => void;
  logout: () => void;
  updateUserProfile: (user: Partial<User>) => void;
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
    set({ isAuthenticated: true, user, token });
  },

  logout: () => {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
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
}));
