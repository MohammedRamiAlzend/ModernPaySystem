import { useMutation } from '@tanstack/react-query';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuthStore } from '@/app/store/authStore';
import { useUIStore } from '@/app/store/uiStore';
import { login } from '../api/login.api';
import { LoginCredentials, DecodedToken } from './types';
import { jwtDecode } from 'jwt-decode';

export const useLogin = () => {
    const navigate = useNavigate();
    const loginSuccessState = useAuthStore((state) => state.loginSuccess);
    const showStatusState = useUIStore((state) => state.showStatus);
    const [searchParams] = useSearchParams();
    const redirectUrl = searchParams.get('redirect') || '/';

    return useMutation({
        mutationFn: (credentials: LoginCredentials) => login(credentials),
        onSuccess: (token: string) => {
            // فك تشفير التوكن لاستخراج بيانات المستخدم
            const decoded = jwtDecode<DecodedToken>(token);

            const roles = Array.isArray(decoded.role) ? decoded.role : [decoded.role];

            const user = {
                id: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
                username: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
                subsystem: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/system"] || '',
                permissions: decoded.permission || [],
                roles: roles,
                isDepartmentHead: decoded.IsDepartmentHead === 'True',
            };

            loginSuccessState(user, token);

            // التوجيه
            navigate(decodeURIComponent(redirectUrl), { replace: true });
        },
        onError: (error: any) => {
            showStatusState({
                type: 'error',
                title: 'فشل تسجيل الدخول',
                message: error.response?.data?.message || 'تأكد من صحة البيانات وحاول مرة أخرى'
            });
            console.error('Login failed:', error);
        },
    });
};
