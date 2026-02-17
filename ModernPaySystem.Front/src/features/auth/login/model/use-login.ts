import { useMutation } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAppDispatch } from '@/app/store';
import { loginSuccess, User } from '@/app/store/authSlice';
import { login } from '../api/login.api';
import { LoginCredentials, DecodedToken } from './types';
import { jwtDecode } from 'jwt-decode';

export const useLogin = () => {
    const navigate = useNavigate();
    const dispatch = useAppDispatch();
    const [searchParams] = useSearchParams();
    const redirectUrl = searchParams.get('redirect') || '/';

    return useMutation({
        mutationFn: (credentials: LoginCredentials) => login(credentials),
        onSuccess: (token: string) => {
            // فك تشفير التوكن لاستخراج بيانات المستخدم
            const decoded = jwtDecode<DecodedToken>(token);

            const roles = Array.isArray(decoded.role) ? decoded.role : [decoded.role];

            const user: User = {
                id: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
                username: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
                subsystem: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system"] || '',
                permissions: decoded.permission || [],
                roles: roles,
            };

            // تحديث حالة Redux
            dispatch(loginSuccess({ user, token }));

            // التوجيه
            navigate(decodeURIComponent(redirectUrl), { replace: true });
        },
        onError: (error: any) => {
            console.error('Login failed:', error);
        },
    });
};
