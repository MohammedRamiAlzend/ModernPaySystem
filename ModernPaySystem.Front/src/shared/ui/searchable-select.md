# SearchableSelect Component 🔍

مكون اختيار متقدم يدعم البحث الذكي والاختيار المفرد أو المتعدد، مصمم ليوفر تجربة مستخدم سلسة في القوائم الطويلة.

## المميزات الرئيسية
- **بحث ذكي مزدوج**: يبحث بالاسم (نصوص) أو بالرقم الترتيبي (أرقام) بشكل تلقائي.
- **نمطين للاختيار**: يدعم الاختيار المفرد (Single Select) أو المتعدد (Multi Select) عبر خاصية `multiple`.
- **عرض احترافي**: يستخدم Popover و Command لضمان أداء عالٍ حتى مع مئات العناصر.
- **دعم الأيقونات والترتيب**: يتيح عرض أيقونة بجانب كل عنصر ورقم تعريفي (#).

---

## كيف يعمل البحث؟
يحتوي المكون على منطق داخلي يسمى `filterOption`:
1. إذا كان المدخل **رقمياً بالكامل**: يبحث في خاصية `order` الخاصة بكل عنصر.
   - *مثال*: كتابة "1" ستظهر العناصر ذات الترتيب 1، 10، 11...
2. إذا كان المدخل **نصياً**: يبحث في خاصية `label` بدون حساسبة لحالة الأحرف.

---

## كيفية الاستخدام

### 1. الاختيار المفرد (Single Select)
يُستخدم كبديل لـ `Select` التقليدي ولكن مع ميزة البحث.

```tsx
<SearchableSelect
  options={[
    { value: '1', label: 'أحمد محمد', order: 1 },
    { value: '2', label: 'سارة أحمد', order: 2 },
  ]}
  value={selectedValue}
  onValueChange={(val) => setSelectedValue(val)}
  placeholder="اختر موظفاً..."
/>
```

### 2. الاختيار المتعدد (Multi Select)
عند تفعيل خاصية `multiple` وبدلاً من `value` نستخدم `values` كمصفوفة. يتم عرض العناصر المختارة كأسماء (Badges) أسفل القائمة.

```tsx
<SearchableSelect
  multiple
  options={options}
  values={selectedIds}
  onValuesChange={(ids) => setSelectedIds(ids)}
  placeholder="اختر المراقبين..."
/>
```

---

## الخصائص (Props)

| الخاصية | النوع | الوصف |
| :--- | :--- | :--- |
| `options` | `SearchableSelectOption[]` | قائمة الخيارات (تحتوي على `value`, `label`, `order`, `icon`). |
| `multiple` | `boolean` | تفعيل نمط الاختيار المتعدد. |
| `placeholder` | `string` | النص الظاهر في حالة عدم الاختيار. |
| `isLoading` | `boolean` | عرض حالة التحميل (Spinner). |
| `disabled` | `boolean` | تعطيل المكون. |
| `emptyMessage` | `string` | الرسالة الظاهرة عند عدم وجود نتائج للبحث. |

---

## أمثلة متقدمة (UserPicker)
تم دمج هذا المكون داخل `UserPicker` لتوحيد عملية اختيار المستخدمين في النظام:

```tsx
// اختيار مفرد (Approver)
<UserPicker 
  onUserSelect={id => ...} 
  defaultValue={id} 
/>

// اختيار متعدد (ReadOnly/CC)
<UserPicker 
  multiple 
  selectedUserIds={ids} 
  onUsersChange={newIds => ...} 
/>
```
