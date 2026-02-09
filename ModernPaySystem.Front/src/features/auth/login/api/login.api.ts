import api from '@/shared/api/baseApi';
import { LoginCredentials, LoginResponse } from '../model/types';

export const login = async (credentials: LoginCredentials): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('Auth/login', credentials);
    return response.data;
};
