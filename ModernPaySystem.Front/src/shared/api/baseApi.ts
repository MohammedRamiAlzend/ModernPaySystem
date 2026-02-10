import axios from 'axios';

const api = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
    headers: {
        'Content-Type': 'application/json',
    },
});

// إضافة interceptors للتعامل مع التوكن أو الأخطاء بشكل مركزي
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
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
        console.log(error);
        const status = error.response?.status;
        // التعامل مع أخطاء 401 (غير مصرح به)
        if (status === 401) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');

            // Redirect to login if not already there
            if (window.location.pathname !== '/auth/login') {
                window.location.href = '/auth/login';
            }
        }

        return Promise.reject(error);
    }
);

export default api;
