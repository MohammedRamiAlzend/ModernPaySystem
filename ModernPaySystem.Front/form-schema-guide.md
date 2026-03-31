# 📋 دليل إنشاء النماذج (Form Schema Guide)

## نظرة عامة

يعتمد النظام على **مخطط JSON (JSON Schema)** لتعريف النماذج الإلكترونية. كل نموذج يتم تخزينه في قاعدة البيانات كسلسلة JSON نصية داخل حقل `contentAsJson` في جدول `Templates`.

المفسّر (Form Renderer) يقرأ هذا المخطط ويبني واجهة المستخدم ديناميكياً بناءً على التعريفات الموجودة فيه.

---

## 1. الهيكل الرئيسي للنموذج (`FormSchema`)

```typescript
interface FormSchema {
    id: string;          // معرّف فريد (UUID) — يتم توليده تلقائياً
    title: string;       // عنوان النموذج (يظهر للمستخدم)
    description?: string; // وصف اختياري
    fields: FormField[]; // مصفوفة الحقول
    logic?: LogicRule[]; // قواعد المنطق الشرطي (اختياري)
}
```

### مثال كامل مبسّط:
```json
{
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "title": "طلب إجازة",
    "description": "نموذج تقديم طلب إجازة للموظفين",
    "fields": [ ... ],
    "logic": [ ... ]
}
```

---

## 2. تعريف الحقول (`FormField`)

كل حقل في النموذج يُعرّف بالهيكل التالي:

```typescript
interface FormField {
    id: string;                    // معرّف فريد (UUID)
    name: string;                  // الاسم البرمجي (فريد داخل النموذج)
    type: FieldType;               // نوع الحقل
    label: string;                 // العنوان الظاهر للمستخدم
    placeholder?: string;          // نص توضيحي داخل الحقل
    defaultValue?: string | number | boolean | string[];  // القيمة الافتراضية
    validation?: ValidationRule[]; // قواعد التحقق
    dataSource?: DataSource;       // مصدر البيانات (للقوائم والاختيارات)
    hidden?: boolean;              // مخفي ابتدائياً
    disabled?: boolean;            // معطل ابتدائياً
    readOnly?: boolean;            // للقراءة فقط
    initialVisibility?: 'visible' | 'hidden';   // الحالة الأولية للرؤية
    initialEnabled?: 'enabled' | 'disabled';     // الحالة الأولية للتفعيل
    direction?: 'horizontal' | 'vertical';       // اتجاه عرض الخيارات (radio/checkbox)
    rows?: number;                 // عدد الأسطر (للـ textarea فقط)
    layout?: {
        colSpan?: number;          // عرض الحقل (1-12) نظام Grid
        className?: string;        // CSS classes إضافية
    };
    numberSpelling?: {
        sourceField: string;       // اسم الحقل الرقمي المصدر (Tafqeet)
    };
}
```

---

## 3. أنواع الحقول المدعومة (`FieldType`)

| النوع | الوصف | المكوّن المقابل |
|-------|-------|----------------|
| `text` | حقل نصي عادي | `TextField` → `<input type="text">` |
| `email` | حقل بريد إلكتروني | `TextField` → `<input type="email">` |
| `password` | حقل كلمة مرور | `TextField` → `<input type="password">` |
| `number` | حقل رقمي | `TextField` → `<input type="number">` |
| `textarea` | نص طويل متعدد الأسطر | `TextareaField` → `<textarea>` |
| `select` | قائمة منسدلة | `SelectField` → `<select>` |
| `radio` | أزرار اختيار (واحد فقط) | `RadioField` → `<input type="radio">` |
| `checkbox` | مربعات اختيار (متعدد) | `CheckboxField` → `<input type="checkbox">` |
| `date` | حقل تاريخ | يُعرض كـ `TextField` حالياً |

> **ملاحظة مهمة:** المقارنة في `FieldRenderer` تتم بـ `field.type.toLowerCase()` مما يعني أن `"Text"` و `"text"` و `"TEXT"` كلها مقبولة. كذلك `"dropdown"` يُعامل كـ `"select"`.

---

## 4. أمثلة تفصيلية لكل نوع حقل

### 4.1 حقل نص (`text`)
```json
{
    "id": "f1a2b3c4-...",
    "name": "employee_name",
    "type": "text",
    "label": "اسم الموظف",
    "placeholder": "أدخل الاسم الكامل",
    "validation": [
        { "rule": "required", "message": "اسم الموظف مطلوب" },
        { "rule": "minLength", "value": 3, "message": "يجب أن يكون الاسم 3 حروف على الأقل" }
    ],
    "layout": { "colSpan": 6 }
}
```

### 4.2 حقل رقم (`number`)
```json
{
    "id": "...",
    "name": "salary_amount",
    "type": "number",
    "label": "المبلغ",
    "placeholder": "0.00",
    "validation": [
        { "rule": "required", "message": "المبلغ مطلوب" },
        { "rule": "minValue", "value": 0, "message": "المبلغ لا يمكن أن يكون سالباً" },
        { "rule": "maxValue", "value": 1000000, "message": "المبلغ يتجاوز الحد المسموح" }
    ],
    "layout": { "colSpan": 6 }
}
```

### 4.3 حقل نص طويل (`textarea`)
```json
{
    "id": "...",
    "name": "notes",
    "type": "textarea",
    "label": "ملاحظات",
    "placeholder": "أدخل ملاحظاتك هنا...",
    "rows": 5,
    "layout": { "colSpan": 12 }
}
```

### 4.4 قائمة منسدلة بخيارات ثابتة (`select` - Static)
```json
{
    "id": "...",
    "name": "leave_type",
    "type": "select",
    "label": "نوع الإجازة",
    "validation": [
        { "rule": "required", "message": "يرجى اختيار نوع الإجازة" }
    ],
    "dataSource": {
        "type": "static",
        "options": [
            { "label": "إجازة سنوية", "value": "annual" },
            { "label": "إجازة مرضية", "value": "sick" },
            { "label": "إجازة طارئة", "value": "emergency" }
        ]
    },
    "layout": { "colSpan": 6 }
}
```

### 4.5 قائمة منسدلة من الإعدادات (`select` - LookUp)
```json
{
    "id": "...",
    "name": "department",
    "type": "select",
    "label": "القسم",
    "dataSource": {
        "type": "lookup",
        "lookUpFieldId": "uuid-of-lookup-field"
    }
}
```

> **كيف يعمل LookUp؟**
> - يتم جلب الخيارات ديناميكياً من API عبر `useLookUpFieldValues(lookUpFieldId)`.
> - القيمة المخزّنة في بيانات النموذج هي `desc` (الوصف النصي) وليس المعرّف.
> - ينطبق على `select` و `radio` و `checkbox`.

### 4.6 أزرار اختيار (`radio`)
```json
{
    "id": "...",
    "name": "gender",
    "type": "radio",
    "label": "الجنس",
    "direction": "horizontal",
    "dataSource": {
        "type": "static",
        "options": [
            { "label": "ذكر", "value": "male" },
            { "label": "أنثى", "value": "female" }
        ]
    }
}
```

> **ملاحظة:** `direction` يتحكّم في الاتجاه:
> - `"horizontal"`: عرض أفقي (في صف واحد)
> - `"vertical"` أو غير محدد: عرض عمودي (كل خيار في سطر)

### 4.7 مربعات اختيار (`checkbox`)

#### متعدد الخيارات:
```json
{
    "id": "...",
    "name": "skills",
    "type": "checkbox",
    "label": "المهارات",
    "direction": "horizontal",
    "dataSource": {
        "type": "static",
        "options": [
            { "label": "JavaScript", "value": "js" },
            { "label": "Python", "value": "py" },
            { "label": "C#", "value": "cs" }
        ]
    }
}
```

> **القيمة المُرسلة:** مصفوفة `["js", "py"]` — يتم تخزينها كـ `string[]`.

#### مربع اختيار مفرد (بدون خيارات):
```json
{
    "id": "...",
    "name": "agree_terms",
    "type": "checkbox",
    "label": "أوافق على الشروط والأحكام",
    "validation": [
        { "rule": "required", "message": "يجب الموافقة على الشروط" }
    ]
}
```

> **القيمة المُرسلة:** `true` أو `false` — عندما لا يكون هناك `dataSource.options`.

### 4.8 خاصية تحويل الرقم إلى نص (`numberSpelling` / Tafqeet)
```json
[
    {
        "id": "...",
        "name": "amount",
        "type": "number",
        "label": "المبلغ بالأرقام",
        "layout": { "colSpan": 6 }
    },
    {
        "id": "...",
        "name": "amount_text",
        "type": "text",
        "label": "المبلغ كتابةً",
        "numberSpelling": {
            "sourceField": "amount"
        },
        "layout": { "colSpan": 6 }
    }
]
```

> **كيف يعمل؟**
> - عند تغيير قيمة الحقل `amount`، يتم إرسال طلب API (Debounced بـ 600ms) لتحويل الرقم إلى نص عربي.
> - النتيجة تُدخل تلقائياً في `amount_text`.
> - `sourceField` يُشير إلى الـ `name` (وليس `id`) للحقل الرقمي المصدر.

---

## 5. قواعد التحقق (`ValidationRule`)

```typescript
interface ValidationRule {
    rule: 'required' | 'minLength' | 'maxLength' | 'pattern' | 'minValue' | 'maxValue';
    value?: string | number;  // مطلوب لكل القواعد ما عدا 'required'
    message?: string;         // رسالة الخطأ المخصصة
}
```

| القاعدة | الوصف | مثال على `value` |
|---------|-------|-----------------|
| `required` | الحقل مطلوب | — (لا تحتاج value) |
| `minLength` | أقل عدد حروف | `3` |
| `maxLength` | أكبر عدد حروف | `100` |
| `minValue` | أقل قيمة رقمية | `0` |
| `maxValue` | أكبر قيمة رقمية | `1000000` |
| `pattern` | تعبير نظامي (Regex) | `"^[0-9]+$"` |

> **ملاحظة هامة:** التحقق يتم فقط على الحقول **المرئية**. الحقول المخفية بقواعد المنطق يتم تجاهلها.

---

## 6. قواعد المنطق الشرطي (`LogicRule`)

تتيح هذه القواعد جعل النموذج **ديناميكياً وتفاعلياً** بحيث تظهر أو تختفي حقول بناءً على قيم حقول أخرى.

```typescript
interface LogicRule {
    when: {
        field: string;     // اسم الحقل المراقب (name, NOT id)
        operator: string;  // العملية المنطقية
        value: string | number | boolean;  // القيمة المتوقعة
    };
    actions: LogicAction[];  // الإجراءات المطلوب تنفيذها
}

interface LogicAction {
    targetField: string;  // اسم الحقل المستهدف (name, NOT id)
    effect: 'show' | 'hide' | 'enable' | 'disable' | 'require' | 'unrequire';
}
```

### العمليات المنطقية المتاحة (`operator`)

| العملية | الوصف |
|---------|-------|
| `equals` | يساوي (مقارنة مرنة `==`) |
| `notEquals` | لا يساوي |
| `contains` | يحتوي على (نصي) |
| `greaterThan` | أكبر من (رقمي) |
| `lessThan` | أصغر من (رقمي) |
| `greaterThanOrEqual` | أكبر من أو يساوي |
| `lessThanOrEqual` | أصغر من أو يساوي |
| `startsWith` | يبدأ بـ (نصي) |
| `endsWith` | ينتهي بـ (نصي) |

### التأثيرات المتاحة (`effect`)

| التأثير | الوصف |
|---------|-------|
| `show` | إظهار الحقل المستهدف |
| `hide` | إخفاء الحقل المستهدف (+ إعادة قيمته للافتراضية) |
| `enable` | تفعيل الحقل المستهدف |
| `disable` | تعطيل الحقل المستهدف |
| `require` | جعل الحقل مطلوباً |
| `unrequire` | جعل الحقل اختيارياً |

### مثال عملي:
```json
{
    "logic": [
        {
            "when": {
                "field": "leave_type",
                "operator": "equals",
                "value": "sick"
            },
            "actions": [
                { "targetField": "medical_report", "effect": "show" },
                { "targetField": "medical_report", "effect": "require" }
            ]
        },
        {
            "when": {
                "field": "salary_amount",
                "operator": "greaterThan",
                "value": 50000
            },
            "actions": [
                { "targetField": "manager_approval", "effect": "show" },
                { "targetField": "manager_approval", "effect": "require" }
            ]
        }
    ]
}
```

> **سلوك مهم:**
> - القواعد تُقيّم كلما تغيرت أي قيمة في النموذج.
> - عند إخفاء حقل (`hide`)، تُعاد قيمته تلقائياً إلى `defaultValue` أو `''`.
> - القواعد **تراكمية**: يمكن لعدة قواعد أن تؤثر على نفس الحقل.
> - القاعدة تُنفّذ **فقط إذا تحقّق الشرط**. إذا لم يتحقق، يعود الحقل لحالته الأولية.

---

## 7. نظام التخطيط (`layout`)

يستخدم النظام شبكة 12 عمود (CSS Grid) لتنظيم الحقول:

| `colSpan` | العرض | الوصف |
|-----------|-------|-------|
| `12` | 100% | عرض كامل (الافتراضي) |
| `6` | 50% | نصف العرض |
| `4` | 33% | ثلث العرض |
| `3` | 25% | ربع العرض |

```json
{
    "layout": {
        "colSpan": 6,
        "className": "custom-class"
    }
}
```

> يتم تطبيق هذا فقط على الشاشات المتوسطة والأكبر (`md:`). على الشاشات الصغيرة، كل حقل يأخذ العرض الكامل.

---

## 8. مثال شامل لنموذج كامل

```json
{
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "title": "طلب سلفة مالية",
    "description": "نموذج تقديم طلب سلفة على الراتب",
    "fields": [
        {
            "id": "f001",
            "name": "employee_name",
            "type": "text",
            "label": "اسم الموظف",
            "placeholder": "الاسم الثلاثي",
            "validation": [
                { "rule": "required", "message": "هذا الحقل مطلوب" }
            ],
            "layout": { "colSpan": 6 }
        },
        {
            "id": "f002",
            "name": "employee_id",
            "type": "text",
            "label": "الرقم الوظيفي",
            "layout": { "colSpan": 6 }
        },
        {
            "id": "f003",
            "name": "department",
            "type": "select",
            "label": "القسم",
            "dataSource": {
                "type": "lookup",
                "lookUpFieldId": "departments-lookup-uuid"
            },
            "validation": [
                { "rule": "required", "message": "يرجى اختيار القسم" }
            ],
            "layout": { "colSpan": 6 }
        },
        {
            "id": "f004",
            "name": "amount",
            "type": "number",
            "label": "مبلغ السلفة",
            "placeholder": "0",
            "validation": [
                { "rule": "required", "message": "المبلغ مطلوب" },
                { "rule": "minValue", "value": 100, "message": "الحد الأدنى 100" },
                { "rule": "maxValue", "value": 50000, "message": "الحد الأقصى 50,000" }
            ],
            "layout": { "colSpan": 6 }
        },
        {
            "id": "f005",
            "name": "amount_text",
            "type": "text",
            "label": "المبلغ كتابة (تلقائي)",
            "readOnly": true,
            "numberSpelling": {
                "sourceField": "amount"
            },
            "layout": { "colSpan": 6 }
        },
        {
            "id": "f006",
            "name": "reason",
            "type": "textarea",
            "label": "سبب السلفة",
            "placeholder": "اذكر سبب طلب السلفة...",
            "rows": 4,
            "validation": [
                { "rule": "required", "message": "يرجى ذكر سبب الطلب" }
            ]
        },
        {
            "id": "f007",
            "name": "urgency",
            "type": "radio",
            "label": "درجة الاستعجال",
            "direction": "horizontal",
            "dataSource": {
                "type": "static",
                "options": [
                    { "label": "عادي", "value": "normal" },
                    { "label": "مستعجل", "value": "urgent" }
                ]
            },
            "layout": { "colSpan": 6 }
        },
        {
            "id": "f008",
            "name": "urgent_reason",
            "type": "textarea",
            "label": "سبب الاستعجال",
            "initialVisibility": "hidden",
            "rows": 3
        },
        {
            "id": "f009",
            "name": "agree_terms",
            "type": "checkbox",
            "label": "أقر بصحة المعلومات المقدمة أعلاه",
            "validation": [
                { "rule": "required", "message": "يجب الإقرار بصحة المعلومات" }
            ]
        }
    ],
    "logic": [
        {
            "when": {
                "field": "urgency",
                "operator": "equals",
                "value": "urgent"
            },
            "actions": [
                { "targetField": "urgent_reason", "effect": "show" },
                { "targetField": "urgent_reason", "effect": "require" }
            ]
        }
    ]
}
```

---

## 9. كيفية التخزين في قاعدة البيانات

عند حفظ النموذج عبر API:

```typescript
// DTO المُرسل
interface CreateTemplateDto {
    contentAsJson: string;           // JSON.stringify(FormSchema)
    templateName: string;            // عنوان النموذج
    templateDescription?: string;    // وصف اختياري
}
```

```
POST /api/Templates
Content-Type: application/json

{
    "contentAsJson": "{\"id\":\"...\",\"title\":\"طلب سلفة\",\"fields\":[...],\"logic\":[...]}",
    "templateName": "طلب سلفة مالية",
    "templateDescription": "نموذج طلب سلفة على الراتب"
}
```

> **`contentAsJson`** يجب أن يكون **سلسلة JSON مُرمّزة** (escaped JSON string) وليس كائن JSON مباشر.

---

## 10. كيفية إرسال بيانات النموذج (Request)

عند تعبئة المستخدم للنموذج وإرساله:

```
POST /api/Requests  (multipart/form-data)

FormData:
  - TemplateId: "uuid-of-template"
  - ApproverId: "uuid-of-approver"
  - Content: "{\"employee_name\":\"أحمد\",\"amount\":5000,...}"
  - files[0]: (Binary File)
  - files[1]: (Binary File)
```

> **`Content`** هو JSON string يحتوي على أزواج `{ fieldName: value }` لكل حقل معبّأ.

---

## 11. قواعد أساسية يجب الالتزام بها

> [!IMPORTANT]
> 1. **`name`** يجب أن يكون **فريداً** داخل النموذج الواحد.
> 2. **قواعد المنطق** تستخدم **`name`** وليس **`id`** للإشارة إلى الحقول.
> 3. **`id`** يجب أن يكون **UUID** صالحاً.
> 4. **الحقول من نوع `select`/`radio`/`checkbox`** يجب أن تحتوي على `dataSource` إما `static` مع خيارات أو `lookup` مع `lookUpFieldId`.
> 5. **`numberSpelling.sourceField`** يُشير إلى `name` حقل آخر (يُفضل أن يكون من نوع `number`).
> 6. عند تخزين `contentAsJson` في قاعدة البيانات، يتم حفظ الـ Schema **كاملاً** بما في ذلك الحقول والمنطق، حتى يمكن إعادة عرض النموذج بنفس الشكل لاحقاً حتى لو تم تعديل القالب.
