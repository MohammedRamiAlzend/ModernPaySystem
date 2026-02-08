import api from '../baseApi';
import type { User } from '@/app/store/authSlice';

export interface LoginCredentials {
    userName: string;
    password: string;
}

export interface LoginResponse {
    user: User;
    token: string;
}

export const authService = {
    login: async (credentials: LoginCredentials): Promise<LoginResponse> => {
        const response = await api.post<LoginResponse>('/auth/login', credentials);
        return response.data;
    },

    logout: () => {
        localStorage.removeItem('token');
    },

    getCurrentUser: async (): Promise<User> => {
        const response = await api.get<User>('/auth/me');
        return response.data;
    }
};

export default authService;
