# نظام الدفع - Pay System (الواجهة الأمامية)

هذا هو المستودع الخاص بالواجهة الأمامية لنظام الدفع، تم بناؤه باستخدام تقنيات حديثة لضمان أداء عالٍ وتجربة مستخدم متميزة.

## التقنيات المستخدمة (Tech Stack)

تم بناء المشروع باستخدام التقنيات التالية:

*   **الأساس:** [React 19](https://react.dev/)
*   **أداة البناء:** [Vite](https://vitejs.dev/)
*   **اللغة:** [TypeScript](https://www.typescriptlang.org/)
*   **التنسيق (Styling):**
    *   [Tailwind CSS](https://tailwindcss.com/)
    *   [Shadcn UI](https://ui.shadcn.com/) (مجموعة مكونات مبنية على Radix UI)
    *   [Lucide React](https://lucide.dev/) للأيقونات
*   **إدارة النماذج:** [React Hook Form](https://react-hook-form.com/) مع [Zod](https://zod.dev/) للتحقق من البيانات (Validation)
*   **إدارة الحالة (State Management):** [Redux Toolkit](https://redux-toolkit.js.org/)
*   **إدارة البيانات (Data Fetching):** [React Query (TanStack Query)](https://tanstack.com/query/latest)
*   **التعامل مع البيانات:** [Axios](https://axios-http.com/) للطلبات البرمجية (API requests)
*   **التوجيه (Routing):** [React Router 7](https://reactrouter.com/)

## هيكلية المشروع (Project Structure)

تم تنظيم المشروع بشكل يسهل الصيانة والتوسع:

*   `src/api`: يحتوي على إعدادات Axios و Interceptors.
*   `src/hooks`: يحتوي على الـ React Hooks المخصصة والمشتركة.
*   `src/store`: إدارة الحالة العالمية.
*   `src/components`: المكونات المشتركة ومكونات UI.
*   `src/pages`: الصفحات الرئيسية للتطبيق.
*   `src/routes`: إعدادات الروابط والتحميل المتأخر (Lazy Loading).
*   `src/utils`: الوظائف المساعدة العامة (مثل Preloading Utility).
*   `src/layouts`: القوالب الرئيسية للتطبيق (Main & Auth Layouts).

## إدارة البيانات والطلبات (API Management)

يستخدم المشروع مزيجاً قوياً بين **Axios** و **React Query**:

1.  **Axios Interceptors:** يتم إضافة توكن المصادقة تلقائياً لكل طلب، ويتم التعامل مع أخطاء الاستجابة بشكل مركزي.
2.  **React Query Strategies:** تم تطبيق نظام "استراتيجيات التحديث" لضمان أفضل أداء:
    *   `CRITICAL`: للبيانات الحساسة التي تتطلب تحديثاً فورياً.
    *   `BACKGROUND`: للبيانات التي يتم تحديثها بهدوء لتوفير الموارد.
    *   `STATIC`: للبيانات التي نادراً ما تتغير (تُجلب مرة واحدة).
    *   `LIVE`: للبيانات اللحظية التي تتحدث تلقائياً (Polling).

## أفضل ممارسات TypeScript

نحن نتبع أساليب متقدمة في TypeScript لتقليل الأخطاء وتسهيل التطوير:

*   **Utility Types:** نستخدم `Omit` لاستثناء الحقول التي يولدها النظام (مثل `id`) عند إنشاء سجلات جديدة، ونستخدم `Partial` في عمليات التحديث للسماح بإرسال حقول اختيارية فقط.
*   **Centralized Types:** يتم تعريف الأنواع في مكان واحد (`src/types`) لضمان مزامنة البيانات بين الـ API والواجهات.

## أداء فائق (Performance Optimization)

يتميز النظام باستخدام تقنيات متقدمة لتحسين السرعة:

1.  **Lazy Loading:** يتم تحميل كود كل صفحة فقط عند الحاجة إليها لتقليل حجم التحميل الأولي.
2.  **Preloading on Hover:** بمجرد أن يحرك المستخدم الماوس فوق أي رابط، يقوم النظام بتحميل كود تلك الصفحة في الخلفية، مما يجعل الانتقال يبدو لحظياً (Instantaneous Transitions).
3.  **Code Splitting:** تقسيم الكود البرمجي إلى حزم صغيرة لضمان أسرع وقت استجابة ممكن.

## البدء في العمل (Getting Started)

### المتطلبات المسبقة
يجب التأكد من تثبيت [Node.js](https://nodejs.org/) على جهازك.

### التثبيت
```bash
npm install
```

### التشغيل في بيئة التطوير
```bash
npm run dev
```

## تحسين جودة الكود (Code Quality)

يستخدم المشروع **ESLint 9** (Flat Config) مع دعم كامل لـ TypeScript لضمان كتابة كود نظيف وخالٍ من الأخطاء.

> **ملاحظة:** تم ضبط إعدادات ESLint لتعمل بشكل صحيح مع بنية المشروع التي تعتمد على ملفات `tsconfig` متعددة (App و Node).

## المميزات الحالية
*   **نظيرة اللحظية (Instant Navigation):** بفضل ميزة Preloading، التنقل بين الصفحات يتم بدون أي تأخير.
*   **دعم الوضع الليلي (Dark Mode):** يدعم النظام الوضعين الفاتح والداكن مع انتقالات سلسة.
*   **إدارة بيانات متقدمة (Caching):** تقليل الطلبات غير الضرورية للخادم عبر نظام الكاش الاحترافي.
*   **تصميم متميز (Premium UI):** استخدام مكتبة Shadcn UI مع لمسات جمالية عصرية.
*   **أمن البيانات:** استخدام Protected Routes لضمان وصول المستخدمين المصرح لهم فقط.
