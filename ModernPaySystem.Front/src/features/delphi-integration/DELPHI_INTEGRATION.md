# 🖥️ دليل التكامل مع تطبيق سطح المكتب (Delphi)

> **الموقع:** `src/features/delphi-integration/`  
> **الغرض:** استقبال، التحقق، تطبيع، وعرض بيانات المعاملات القادمة من تطبيق دلفي للمصالح العقارية.

---

## 📁 هيكل الملفات

```
src/
├── features/
│   └── delphi-integration/
│       └── model/
│           ├── delphi-schema.ts          ← تعريف مخطط النموذج الثابت
│           ├── delphi-data-processor.ts  ← منطق التحقق والتطبيع
│           └── useDelphiTransaction.ts   ← Hook رئيسي للمزامنة والمعالجة
└── pages/
    └── delphi-transaction/
        └── delphi-transaction-page.tsx   ← صفحة العرض النهائية
```

---

## 🔄 تدفق البيانات

```
تطبيق دلفي (JSON)
        ↓
  [delphi-transaction-page.tsx]
  يستقبل النص الخام من textarea
        ↓
  [useDelphiTransaction]
  ├── مزامنة النموذج مع السيرفر (إنشاء/تحديث)
  └── معالجة بيانات JSON الخام
        ↓
  [delphi-data-processor.ts]
  ├── 1. التحقق (Validation)
  ├── 2. التطبيع (Normalization)
  └── 3. التحويل (Transformation)
        ↓
  [FormRenderer] يعرض النتائج
        ↓
  [createRequest] يحفظ الطلب في قاعدة البيانات
```

---

## 📄 شرح كل ملف

### 1. `delphi-schema.ts` — مخطط النموذج الثابت

يحتوي على:
- `DELPHI_TEMPLATE_NAME` — اسم النموذج في قاعدة البيانات
- `DELPHI_FIXED_SCHEMA` — تعريف حقول النموذج كـ `FormSchema`

**مبدأ مهم:** المخطط المحلي هو **المرجع الأساسي** دائماً.  
قاعدة البيانات تحتفظ فقط بالـ `id` الفريد للنموذج.

```ts
export const DELPHI_TEMPLATE_NAME = "نموذج معاملات سطح المكتب (Delphi)";

export const DELPHI_FIXED_SCHEMA: FormSchema = {
    id: "delphi-fixed-template-id", // ID مؤقت — يُستبدل بـ UUID من السيرفر
    title: DELPHI_TEMPLATE_NAME,
    fields: [ ... ]
};
```

---

### 2. `delphi-data-processor.ts` — معالج البيانات

**المدخلات:** JSON خام من تطبيق دلفي  
**المخرجات:** `DelphiValidationResult` يحتوي على:
- `status: 'success' | 'error'`
- `data` — البيانات المطبّعة جاهزة للعرض
- `ui.ready_to_submit` — هل البيانات صالحة للحفظ?

**مراحل المعالجة:**

| المرحلة | الوصف |
|---------|-------|
| **التحقق** | التأكد من وجود الحقول الإلزامية (`id`, `client`, `list_services`) |
| **التطبيع** | تنظيف النصوص، توحيد تنسيق التواريخ (`/` → `-`), تحويل الأنواع |
| **التحويل** | دمج الخدمات المكررة، حساب المجاميع، توليد ملخص نصي |

---

### 3. `useDelphiTransaction.ts` — الـ Hook الرئيسي

**مسؤوليتان:**

**أ) مزامنة النموذج مع السيرفر:**
```
تحميل قائمة النماذج (مع تفعيل showExternal=true لرؤية النماذج المخفية)
    ↓
هل يوجد نموذج باسم DELPHI_TEMPLATE_NAME؟
    ├── نعم → هل المخطط محدّث (عدد الحقول مطابق)?
    │           ├── نعم → استخدمه مباشرة
    │           └── لا  → حدّثه تلقائياً في قاعدة البيانات
    └── لا  → أنشئه مع وسم isExternal: true (مرة واحدة فقط بفضل isSyncingRef)
```

**ب) معالجة بيانات JSON الخام:**  
يستدعي `processDelphiData()` عند تغيير النص المُدخَل.

---

### 4. `delphi-transaction-page.tsx` — صفحة العرض

**نقاط مهمة:**
- `key={${template.id}-${processed.data?.id ?? ''}}` على `<FormRenderer>` — يُعيد بناء المكوّن عند وصول البيانات لضمان ظهور القيم.
- `readOnly={true}` — الحقول للاستعراض فقط قبل الحفظ.

---

## ✅ كيفية إضافة نموذج دلفي جديد مستقبلاً

### الخطوة 1: إنشاء ملف المخطط

أنشئ ملفاً جديداً في `src/features/delphi-integration/model/`:

```ts
// delphi-property-evaluation-schema.ts
import type { FormSchema } from '@/entities/form/model/types';

export const DELPHI_EVAL_TEMPLATE_NAME = "نموذج تقييم العقارات (Delphi)";

export const DELPHI_EVAL_SCHEMA: FormSchema = {
    id: "delphi-eval-template-id",
    title: DELPHI_EVAL_TEMPLATE_NAME,
    description: "نموذج تقييم العقارات القادم من نظام دلفي",
    fields: [
        {
            id: "e1",
            name: "property_id",
            type: "text",
            label: "رقم العقار",
            readOnly: true,
            layout: { colSpan: 6 }
        },
        // ... باقي الحقول
    ],
    logic: []
};
```

> **قواعد تسمية IDs الحقول:** استخدم بادئة مختلفة عن `d` (مثل `e1`, `e2`) لتجنب التعارض.

---

### الخطوة 2: إنشاء معالج البيانات

أنشئ ملفاً جديداً:

```ts
// delphi-property-evaluation-processor.ts

export interface DelphiEvalInput {
    property_id?: any;
    evaluation_date?: any;
    // ... الحقول الأخرى
}

export const processDelphiEvalData = (raw: DelphiEvalInput): DelphiValidationResult => {
    const errors: string[] = [];

    // 1. التحقق من الحقول الإلزامية
    if (!raw.property_id) {
        errors.push("رقم العقار مطلوب");
    }

    if (errors.length > 0) {
        return { status: 'error', errors, ui: { highlight_missing: [], warnings: [], ready_to_submit: false } };
    }

    // 2. التطبيع
    const normalized = {
        property_id: String(raw.property_id).trim(),
        evaluation_date: raw.evaluation_date
            ? String(raw.evaluation_date).replace(/\//g, '-')
            : new Date().toISOString().split('T')[0],
    };

    return {
        status: 'success',
        data: { ...normalized },
        ui: { highlight_missing: [], warnings: [], ready_to_submit: true }
    };
};
```

---

### الخطوة 3: إنشاء Hook المزامنة

```ts
// useDelphiEvalTransaction.ts
import { useState, useEffect, useRef } from 'react';
import { useTemplates, useCreateTemplate, useUpdateTemplate } from '@/features/form-builder/api/formEndpoints';
import { DELPHI_EVAL_TEMPLATE_NAME, DELPHI_EVAL_SCHEMA } from './delphi-property-evaluation-schema';
import { processDelphiEvalData, type DelphiEvalInput } from './delphi-property-evaluation-processor';
import type { FormSchema } from '@/entities/form/model/types';

export const useDelphiEvalTransaction = (rawInput?: string) => {
    // 1. استدعاء القوالب مع تفعيل showExternal لرؤية القوالب التقنية
    const { data: templates = [] } = useTemplates(true);
    
    // ... باقي منطق useDelphiTransaction تماماً
    // مع تعيين isExternal: true عند الإنشاء/التعديل
}
```

---

### الخطوة 4: إنشاء الصفحة

```ts
// src/pages/delphi-eval/delphi-eval-page.tsx
import { useDelphiEvalTransaction } from '@/features/delphi-integration/model/useDelphiEvalTransaction';

export const DelphiEvalPage = () => {
    const [rawInput, setRawInput] = useState("");
    const { template, processed, isLoading, error } = useDelphiEvalTransaction(rawInput);

    // نفس JSX الموجود في delphi-transaction-page.tsx
    // مع تغيير العنوان والوصف المناسبين
};
```

---

### الخطوة 5: إضافة المسار في الـ Router

في `src/app/router/index.tsx` أضف السطرين التاليين:

**أولاً** — استيراد الصفحة (مع باقي imports في أعلى الملف):
```ts
const DelphiEvalPage = lazyWithPreload(() =>
    import('@/pages/delphi-eval/delphi-eval-page').then(module => ({ default: module.DelphiEvalPage }))
);
```

**ثانياً** — إضافة المسار داخل `form-builder` children (بعد `delphi-transaction`):
```ts
{
    path: 'delphi-eval',
    element: (
        <Suspense fallback={<LoadingSpinner />}>
            <DelphiEvalPage />
        </Suspense>
    ),
    handle: {
        crumb: () => 'تقييم العقارات',
        permission: RoutePermissions.AUTHENTICATED,
        preload: () => DelphiEvalPage.preload(),
    },
},
```

> **ملاحظة:** المسار الكامل سيكون: `/form-builder/delphi-eval`  
> (نفس النمط الموجود: `/form-builder/delphi-transaction`)

---

## 🐛 استكشاف الأخطاء الشائعة

| المشكلة | السبب المحتمل | الحل |
|---------|--------------|-------|
| النموذج يُنشأ مرتين في قاعدة البيانات | Race condition في `useEffect` | يتم التعامل معها بواسطة `isSyncingRef` + `hasCreatedRef` |
| الحقول تظهر فارغة بعد لصق JSON | `FormRenderer` يُهيأ مرة واحدة | تم حله بإضافة `key` prop مركّب |
| حقول جديدة لا تظهر عند إضافتها | النموذج القديم محفوظ في DB | يتم تحديثه تلقائياً عبر مقارنة عدد الحقول |
| خطأ في تحليل JSON | الصيغة غير صحيحة | يظهر رسالة خطأ في واجهة المستخدم تلقائياً |
| `sum_amount` يظهر `null` | الحقل غير موجود في JSON المستلم | طبيعي — الحقل اختياري |

---

## 📌 قواعد مهمة لا تنسَها

> [!IMPORTANT]
> **المخطط المحلي هو المرجع الأساسي:** لا تعتمد على `contentAsJson` المخزون في قاعدة البيانات للعرض.  
> قاعدة البيانات تحتفظ فقط بالـ `id` الفريد للنموذج.

> [!WARNING]
> **لا تعدّل `ids` الحقول الموجودة** (`d1`, `d2`... إلخ) لأن ذلك قد يُسبب تشابكاً مع بيانات محفوظة مسبقاً.  
> عند إضافة حقل جديد، استخدم ID جديداً لم يُستخدم من قبل.

> [!TIP]
> **اختبار سريع:** استخدم زر "تحميل بيانات اختبار" في الصفحة لتحميل JSON تجريبي جاهز واختبار دورة المعالجة الكاملة.

> [!NOTE]
> **تحديث الـ schema في DB:** يتم تلقائياً عند كل تشغيل للصفحة إذا اختلف عدد الحقول.  
> يتم إرسال وسم `isExternal: true` لضمان بقاء النموذج مخفياً عن القوائم العامة والـ Sidebar.
