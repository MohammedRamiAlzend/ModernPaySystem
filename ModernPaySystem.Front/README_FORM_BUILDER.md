# توثيق نظام بناء النماذج المطور (Advanced Form Builder)

## 📌 نبذة عامة
تم تحديث نظام بناء النماذج (Form Builder) ليصبح نظاماً متكاملاً يعتمد على أحدث المبادئ البرمجية. يتبع النظام هيكلية **Feature-Sliced Design (FSD)** بشكل صارم، مع استخدام **React Query** لإدارة الحالة السحابية (Server State) وفصل كامل للطلبات البرمجية (API) لضمان أقصى درجات القابلية للتوسع والأداء.

---

## 🏗️ الهيكلية المعمارية المطورة (FSD Architecture)

تم توزيع الكود وفقاً لطبقات النظام لضمان استقلالية المكونات:

### 1. الكيانات (Entities) - `src/entities/form`
تمثل "ما هو" النموذج، وهي المسؤولة عن تعريف البيانات وجلبها.
- **api/formApi.ts**: مسؤول **فقط** عن عمليات القراءة (Read-Only) مثل `getForms` و `getFormById`.
- **model/types.ts**: واجهات الـ TypeScript الأساسية (`FormSchema`, `FormField`).

### 2. الميزات (Features) - `src/features/form-builder`
تمثل "الأفعال" أو العمليات التي يقوم بها المستخدم على النماذج.
- **api/formService.ts**: مسؤول **فقط** عن عمليات التعديل (Actions/Writes) مثل `saveForm` و `deleteForm`.
- **model/useForms.ts**: يحتوي على Hooks الـ **React Query** (مثل `useForms`, `useSaveForm`, `useDeleteForm`).
- **model/useFormEditor.ts**: يدير الحالة المعقدة للمحرر أثناء عملية البناء.

### 3. العناصر الرسومية (Widgets) - `src/widgets`
هي المكونات الضخمة التي تجمع الميزات والكيانات:
- **widgets/forms-list**: يعرض قائمة النماذج باستخدام ميزات الحذف والتحرير.
- **widgets/form-editor**: المحرر الرئيسي الذي يجمع لوحة الخصائص ومحرر المنطق.
- **widgets/form-renderer**: المحرك المسؤول عن تحويل الـ JSON إلى واجهة تفاعلية.

### 4. الصفحات (Pages) - `src/pages/form-builder`
- **FormBuilderPage.tsx**: منظم (Orchestrator) بسيط يتنقل بين قائمة النماذج والمحرر، دون احتواء أي منطق عمل بداخله.

---

## 🧠 إدارة الحالة والبيانات (State Management)

1. **React Query (Server State)**:
   - يتم استخدام `QueryClient` لإدارة الكاش وتحديث البيانات تلقائياً.
   - عند حفظ نموذج أو حذفه، يتم عمل `Invalidation` للكاش لضمان تحديث القائمة فوراً.

2. **Local Management**:
   - يتم استخدام `useState` داخل `useFormEditor` لإدارة التغييرات اللحظية أثناء التصميم لسرعة الاستجابة، ثم إرسال النسخة النهائية للسيرفر عند الحفظ.

---

## 📜 القاعدة الذهبية للتعامل مع الـ API
يتبع المشروع قاعدة صارمة في مكان استدعاء الـ API:
- **Entities/api**: توضع فيها الـ Endpoints التي تقوم بـ **جلب البيانات فقط** (Data Retrieval).
- **Features/api**: توضع فيها الـ Endpoints التي تقوم بـ **تعديل شيء ما** (Actions/Mutations مثل POST, PUT, DELETE).

---

## 🌟 المميزات التقنية (Technical Features)

1. **نظام تحقق متطور (Advanced Validation)**: دعم كامل للـ Regex والقيود الرقمية والطولية مع رسائل خطأ مخصصة لكل حقل.
2. **محرك منطق شرطي (Logic Engine)**: إمكانية بناء سيناريوهات معقدة (إظهار/إخفاء، تفعيل/تعطيل) بناءً على قيم الحقول الأخرى.
3. **تصميم متجاوب (Responsive Layout)**: نظام شبكي (Grid System) يسمح بتوزيع الحقول بنسب مختلفة (100%, 50%, 33%, 25%).
4. **تزامن البيانات**: ضمان عدم فقدان البيانات واستقرار الواجهة أثناء عمليات الحفظ والحذف بفضل الـ Mutations و الـ Optimistic updates (عند الحاجة).

---

## ⚙️ التكنولوجيا المستخدمة
- **State**: @tanstack/react-query
- **Architecture**: Feature-Sliced Design (FSD)
- **Icons**: Lucide React
- **Animations**: Framer Motion (via AnimatedContainer)
- **Forms Logic**: Custom Engine (validation + logicEngine)
