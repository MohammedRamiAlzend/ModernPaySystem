# محرك النماذج الديناميكي (Form Builder)

هذا المحرك عبارة عن أداة مبنية باستخدام React لإنشاء ونمذجة الاستمارات الديناميكية بناءً على ملفات JSON (Schemas). يدعم المحرك أنواعاً متعددة من الحقول، قواعد التحقق من البيانات، والمنطق الشرطي المتقدم.

## المميزات الرئيسية 

- **قائم على المخطط (Schema-Driven)**: تعاريف النماذج تتم عبر ملفات JSON بسيطة.
- **دعم متكامل للعربية (RTL)**: واجهات متوافقة تماماً مع اتجاه النصوص في المشروع.
- **أنواع حقول متعددة**: (نص، بريد إلكتروني، كلمة مرور، رقم، قائمة منسدلة، اختيار، أزرار اختيار، نصوص طويلة).
- **نظام التحقق (Validation)**: قواعد مدمجة (مطلوب، الحد الأدنى/الأقصى للطول، الأنماط، إلخ).
- **المنطق الشرطي (Conditional Logic)**: إظهار/إخفاء أو تفعيل/تعطيل الحقول بناءً على قيم حقول أخرى.
- **سهل التوسيع**: هيكلية مرنة لإضافة أنواع حقول وقواعد تحقق جديدة.

## هيكلية الملفات 

تم تنظيم الكود داخل مجلد `src/formBuilder` لضمان الترتيب وسهولة الوصول:

```text
src/formBuilder/
├── common/             # المكونات المشتركة الصرفة (مثل TextArea)
├── components/         # مغلفات الحقول الخاصة بمحرك النماذج
├── utils/              # الأدوات المساعدة (محرك المنطق، نظام التحقق)
├── pages/              # صفحات العرض والتجربة (Demo)
├── FieldRenderer.jsx   # المكون المسؤول عن اختيار نوع الحقل
└── FormRenderer.jsx    # المكون الأساسي لبناء النموذج
```

### هيكلية المخطط (Schema Structure) 

يستخدم المحرك الهيكلية التالية لتعريف النماذج، مع أمثلة لكل نوع حقل:

```js
{
  id: "form-id",
  title: "عنوان النموذج",
  fields: [
    // 1. حقل نصي عادي (Text)
    {
      id: "f1",
      name: "username",
      type: "text",
      label: "اسم المستخدم",
      placeholder: "أدخل الاسم...",
      validation: [{ rule: "required", message: "هذا الحقل مطلوب" }]
    },
    
    // 2. حقل بريد إلكتروني (Email)
    {
      id: "f2",
      name: "user_email",
      type: "email",
      label: "البريد الإلكتروني",
      validation: [{ rule: "pattern", value: "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$", message: "ايميل غير صحيح" }]
    },

    // 3. حقل نصوص طويلة (TextArea)
    {
      id: "f3",
      name: "description",
      type: "textarea",
      label: "الوصف أو السيرة الذاتية",
      rows: 5,
      placeholder: "اكتب هنا..."
    },

    // 4. قائمة منسدلة (Select)
    {
      id: "f4",
      name: "country",
      type: "select",
      label: "الدولة",
      dataSource: {
        type: "static",
        options: [
          { label: "العراق", value: "IQ" },
          { label: "مصر", value: "EG" }
        ]
      }
    },

    // 5. أزرار اختيار (Radio Buttons)
    {
      id: "f5",
      name: "gender",
      type: "radio",
      label: "الجنس",
      defaultValue: "male",
      dataSource: {
        type: "static",
        options: [
          { label: "ذكر", value: "male" },
          { label: "أنثى", value: "female" }
        ]
      }
    },

    // 6. صندوق اختيار (Checkbox)
    {
      id: "f6",
      name: "agree_terms",
      type: "checkbox",
      label: "أوافق على الشروط والأحكام",
      defaultValue: false
    }
  ],
  logic: [
    {
      when: { field: "country", operator: "equals", value: "IQ" },
      actions: [{ targetField: "description", effect: "show" }]
    }
  ]
}
```

## مثال على الاستخدام 

```jsx
import FormRenderer from './formBuilder/FormRenderer';

const mySchema = {
  id: 'register',
  title: 'نموذج التسجيل',
  fields: [
    { id: '1', name: 'user', type: 'text', label: 'اسم المستخدم', validation: [{rule: 'required'}] },
    { id: '2', name: 'bio', type: 'textarea', label: 'نبذة شخصية' }
  ]
};

const MyPage = () => {
  const handleSave = (data) => console.log(data);
  return <FormRenderer schema={mySchema} onSubmit={handleSave} />;
};
```

## أنواع الحقول المتاحة 

- `text`: إدخال نصي عادي.
- `textarea`: إدخال نصوص طويلة متعددة الأسطر.
- `email`: حقل بريد إلكتروني مع تحقق مدمج.
- `number`: حقل إدخال أرقام.
- `select`: قائمة منسدلة.
- `checkbox`: صندوق اختيار (نعم/لا).
- `radio`: أزرار اختيار وحيد من متعدد.

## قواعد التحقق المدعومة 

- `required`: الحقل مطلوب إجباري.
- `minLength` / `maxLength`: عدد الأحرف الأدنى والأقصى.
- `minValue` / `maxValue`: القيمة الرقمية الدنيا والقصوى.
- `pattern`: التحقق عبر التعبيرات النمطية (Regex).

## تأثيرات المنطق الشرطي 

- `show` / `hide`: إظهار أو إخفاء الحقل.
- `enable` / `disable`: تفعيل أو تعطيل الحقل.
- `require` / `unrequire`: جعل الحقل مطلوباً أو اختيارياً ديناميكياً.

---
تم تحديث هذا الدليل ليتناسب مع الإصدار المنظم للمشروع في مجلد `formBuilder`.