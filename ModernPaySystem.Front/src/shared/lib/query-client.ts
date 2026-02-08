import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            staleTime: 1000 * 60 * 5, // البيانات تعتبر "طازجة" لمدة 5 دقائق
            gcTime: 1000 * 60 * 30,    // الاحتفاظ بالبيانات في الكاش لمدة 30 دقيقة
            retry: 0,                 //  عدم إعادة المحاولة مرة  عند الفشل 
            refetchOnWindowFocus: false, // عدم إعادة التحميل عند العودة للنافذة
        },
        mutations: {
            retry: 0, // عدم إعادة المحاولة في عمليات الإرسال (Post/Put/Delete)
        },
    },
});
