/**
 * استراتيجيات تحديث البيانات (Update Strategies)
 * تُستخدم لتحديد سلوك React Query بناءً على نوع البيانات المجلوبة وأهميتها.
 */
export const UpdateStrategy = {
    /** بيانات بالغة الأهمية: تتطلب تحديثاً فورياً عند كل طلب ولا تقبل كاش قديم (مثل الرصيد، حالة الطلب) */
    CRITICAL: 'CRITICAL',
    /** بيانات الخلفية: بيانات يتم تحديثها بشكل غير مستعجل ولا تحتاج لإعادة تحميل فورية (مثل الإعدادات الشخصية) */
    BACKGROUND: 'BACKGROUND',
    /** بيانات ثابتة: نادراً ما تتغير، تُجلب مرة واحدة ويتم الحفاظ عليها لفترة طويلة (مثل قائمة الدول، الثوابت) */
    STATIC: 'STATIC',
    /** بيانات مباشرة: بيانات تتغير بسرعة وتحتاج إلى مراقبة مستمرة (مثل الإشعارات، لوحة البيانات اللحظية) */
    LIVE: 'LIVE',
} as const;

export type UpdateStrategy = (typeof UpdateStrategy)[keyof typeof UpdateStrategy];

export interface QueryConfigOptions {
    /** المدة التي تعتبر فيها البيانات "طازجة" ولا تحتاج لجلب جديد (بالملي ثانية) */
    staleTime: number;
    /** المدة التي تبقى فيها البيانات في الذاكرة (الكاش) بعد توقف استخدامها (بالملي ثانية) */
    gcTime: number;
    /** عدد مرات إعادة المحاولة عند فشل الطلب */
    retry: number | boolean;
    /** هل يتم إعادة جلب البيانات تلقائياً عند عودة التركيز للنافذة (Window Focus) */
    refetchOnWindowFocus: boolean;
    /** فاصل زمني للتحديث التلقائي الدوري (اختياري) */
    refetchInterval?: number;
}

/**
 * التوصيف التفصيلي لكل استراتيجية تحديث.
 */
export const QUERY_STRATEGIES: Record<UpdateStrategy, QueryConfigOptions> = {
    [UpdateStrategy.CRITICAL]: {
        staleTime: 0,            // جلب البيانات دائماً لضمان أحدث نسخة
        gcTime: 1000 * 60 * 60,  // الاحتفاظ في الكاش لمدة ساعة
        retry: 3,                // إعادة المحاولة 3 مرات للأهمية
        refetchOnWindowFocus: true, // تحديث بمجرد فتح المستخدم للمتصفح
    },
    [UpdateStrategy.BACKGROUND]: {
        staleTime: 1000 * 60 * 10, // البيانات تعتبر طازجة لمدة 10 دقائق
        gcTime: 1000 * 60 * 30,    // كاش لمدة 30 دقيقة
        retry: 1,                  // إعادة محاولة واحدة فقط
        refetchOnWindowFocus: false, // لا داعي للإزعاج عند التنقل بين النوافذ
    },
    [UpdateStrategy.STATIC]: {
        staleTime: 1000 * 60 * 60 * 24, // طازجة لمدة يوم كامل
        gcTime: Infinity,               // لا يتم حذفه من الكاش أبداً أثناء الجلسة
        retry: 0,                       // لا تعيد المحاولة إذا فشلت (بيانات ثابتة)
        refetchOnWindowFocus: false,
    },
    [UpdateStrategy.LIVE]: {
        staleTime: 0,
        gcTime: 1000 * 60 * 5,
        retry: 1,
        refetchOnWindowFocus: true,     // تحديث عند العودة
        refetchInterval: 5000,          // جلب تلقائي كل 5 ثوانٍ
    },
};