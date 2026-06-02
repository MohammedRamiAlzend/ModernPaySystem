# خطة عمل محدثة: نظام الأرشفة الاحترافي (Modern Archiving System)

بناءً على تعليقاتك وتوجيهاتك الأخيرة، تم تحديث خطة العمل وإعادة صياغة المتطلبات كالتالي:

---

## 🎯 الأهداف الأساسية المحدثة

1. **عرض المجلدات والملفات (Windows Explorer & List Views)**:
   - توفير نمط عرض شبيه بمتصفح ويندوز (أيقونات مجلدات وملفات كبيرة وقابلة للنقر المزدوج).
   - توفير نمط عرض القائمة (جدول تفصيلي بالاسم، الحجم، الرقم الأرشيفي، تاريخ الإنشاء).
   - توفير زر للتبديل بين النمطين.

2. **التمرير اللانهائي (Paginated Scroll / Infinite Scroll)**:
   - جلب وتصفح قوائم الملفات وسجلات الأرشيف باستخدام ميزة التمرير اللانهائي لضمان الكفاءة وسرعة التحميل.

3. **معرض الوثائق والمستندات (Media & Document Gallery)**:
   - دعم المعاينة المباشرة داخل النظام للصور والفيديوهات وملفات PDF والنصوص (`.txt`, `.md`).
   - بالنسبة لملفات Word و Excel: سنقوم بتصميم بطاقة معلومات مخصصة وأنيقة تعرض حجم وتفاصيل الملف مع زر تحميل مباشر وجلي (بدون معاينة سحابية).

4. **توليد ورقة غلاف اختيارية مع رمز QR (QR Cover Page)**:
   - توفير خيار للمستخدم (Checkbox) باسم "توليد ورقة غلاف مع رمز QR".
   - إذا تم تفعيله، سيقوم النظام تلقائياً بتوليد صورة (Cover Page Image) كأول مستند في الأرشيف.
   - تحتوي الصورة على:
     - رمز QR مشتق من معرّف السجل (GUID).
     - قيم الحقول الديناميكية المدخلة (Dynamic Form Content).
     - تاريخ وقت الأرشفة.
   - لتنفيذ هذا بشكل متزامن واحترافي، سنسمح للواجهة الأمامية بتوليد المعرّف (GUID) مسبقاً وتوليد الصورة محلياً باستخدام `html2canvas` ثم رفعها كجزء من قائمة الملفات في طلب الإنشاء.

5. **أشرطة تقدم الرفع والتحميل (Upload & Download Progress)**:
   - إظهار شريط تقدم حركي (Progress Indicator) عند رفع الوثائق والملفات الكبيرة.
   - إظهار مؤشر تقدم عند تحميل حزم الـ ZIP أو الملفات الفردية.

6. **تكامل الـ OCR**:
   - ربط وتفعيل الماسح الضوئي (OCR) الموجود مسبقاً في مجلد `features/document-scanner` للسماح بقراءة المستندات وملء حقول النماذج ديناميكياً.

7. **الأرقام الأرشيفية الفريدة**:
   - الحفاظ على منطق التحقق الحالي المتوفر في قاعدة البيانات والباك إند دون تعديل أو تعقيد إضافي.

8. **الصلاحيات**:
   - إرجاء/تجاوز موضوع صلاحيات المجلدات بالكامل في هذه المرحلة بناءً على طلبك.

---

## 🛠️ التغييرات البرمجية المقترحة

### 🖥️ البنية الخلفية (C# Backend)

#### [MODIFY] [ArchiveRecord.cs](file:///c:/Users/DELL/Desktop/ModernPaySystem/ModernPaySystem.Domain/Entities/Archiving/ArchiveRecord.cs)
* تعديل كلاس `CreateArchiveRecordDto` لإضافة حقل معرّف اختياري:
  ```csharp
  public Guid? Id { get; set; }
  ```

#### [MODIFY] [ArchiveRecordService.cs](file:///c:/Users/DELL/Desktop/ModernPaySystem/ModernPaySystem.Infrastructure/Services/ArchiveRecordService.cs)
* تعديل منطق إنشاء السجل في دالة `CreateAsync` لاستخدام المعرّف الممرر من الواجهة الأمامية إن وجد، بدلاً من توليد واحد جديد دائماً:
  ```csharp
  var recordId = dto.Id ?? Guid.NewGuid();
  var record = new ArchiveRecord
  {
      Id = recordId,
      ...
  };
  ```

---

### 🎨 الواجهة الأمامية (React Frontend)

#### [NEW] [archiving feature components](file:///c:/Users/DELL/Desktop/ModernPaySystem/ModernPaySystem.Front/src/features/archiving)
* **`ui/ExplorerView.tsx`**: عرض شبكي بأسلوب المجلدات والملفات الكبيرة.
* **`ui/ListView.tsx`**: عرض قائمة جدولية مع الترتيب والفلترة.
* **`ui/DocumentGallery.tsx`**: المعرض التفاعلي للمستندات والوسائط مع كرت تحميل ملفات Word/Excel.
* **`ui/QRPreviewTemplate.tsx`**: كامبوننت مخفي (Hidden DOM Template) يُرسم فيه كرت الغلاف الأنيق (معلومات النموذج الديناميكي، التاريخ، ورمز الـ QR)، ويتم تحويله لصورة عبر `html2canvas`.

#### [MODIFY] [navigation.tsx](file:///c:/Users/DELL/Desktop/ModernPaySystem/ModernPaySystem.Front/src/shared/config/navigation.tsx)
* إضافة تبويب "نظام الأرشفة" كعنوان رئيسي في القائمة الجانبية (Navigation sidebar) لربطه بصفحة الأرشيف الجديدة.

#### [NEW] [archiving-routes.tsx](file:///c:/Users/DELL/Desktop/ModernPaySystem/ModernPaySystem.Front/src/app/router/routes/archiving-routes.tsx) & [explorer-page.tsx](file:///c:/Users/DELL/Desktop/ModernPaySystem/ModernPaySystem.Front/src/pages/archiving/explorer-page.tsx)
* إنشاء مسارات التوجيه وصفحة مستكشف الأرشيف التي تضم المجلدات والملفات وتكامل OCR.

---

## 🧪 خطة التحقق والاختبار

1. **اختبار ورقة غلاف الـ QR**:
   - التحقق من تفعيل خيار ورقة الغلاف وظهور صورة الغلاف كملف أول في المستند المؤرشف.
   - التحقق من قراءة الـ QR والتأكد من أنه يشير إلى GUID الخاص بالمستند بنجاح.
2. **اختبار معاينات الملفات المختلفة**:
   - فتح ملفات الصور والفيديو والـ PDF والـ Markdown للتأكد من أنها تظهر بشكل صحيح في المعرض.
   - فتح ملفات Word/Excel والتأكد من ظهور كرت المعلومات الأنيق وزر التحميل المباشر.
3. **اختبار التحميل والتنزيل مع الـ Progress**:
   - محاكاة سرعات إنترنت بطيئة للتأكد من ظهور تقدم الرفع والتحميل (Upload/Download progress bars) بشكل دقيق.
