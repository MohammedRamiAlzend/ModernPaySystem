import axios from 'axios';
import { useUIStore } from '@/app/store/uiStore';
import { useAuthStore } from '@/app/store/authStore';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5173/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

// إضافة interceptors للتعامل مع التوكن أو الأخطاء بشكل مركزي
api.interceptors.request.use(
    (config) => {
        const token = sessionStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

api.interceptors.response.use(
    (response) => response,
    (error) => {
        const status = error.response?.status;
        const message = error.response?.data?.message || error.message || 'حدث خطأ في الاتصال بالخادم';

        // التعامل مع أخطاء 401 (غير مصرح به) - يعني التوكن انتهى أو غير صالح
        if (status === 401) {
            const { logout } = useAuthStore.getState();
            logout();

            // التحقق مما إذا كنا لسنا في صفحة تسجيل الدخول بالفعل لتجنب التكرار
            if (window.location.pathname !== '/auth/login') {
                // إضافة معامل في الرابط لتمييز حالة انتهاء الجلسة عرض رسالة للمستخدم
                window.location.href = '/auth/login?reason=expired';
            }
        } else {
            // إظهار رسالة خطأ لباقي الأخطاء
            useUIStore.getState().showStatus({
                type: 'error',
                title: 'خطأ',
                message: message
            });
        }

        return Promise.reject(error);
    }
);

export default api;
