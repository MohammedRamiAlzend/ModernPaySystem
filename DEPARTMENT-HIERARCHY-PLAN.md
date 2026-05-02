# خطة عمل: تطبيق البنية الشجرية (الهيكلية الإدارية) للنظام

## ملخص تنفيذي

تهدف هذه الخطة إلى تحويل بنية المستخدمين الحالية من بنية مسطحة إلى بنية شجرية هرمية تدعم الهيكل الإداري المتعدد المستويات (مثل: سوريا → ريف دمشق → الغوطة الشرقية → بلدية دوما → مكتب فني ديوان → مستخدمين).

---

## 1. تحليل الوضع الحالي

### الكيانات الحالية:
- **User**: يمثل المستخدم النهائي فقط
- **SubSystemUser**: يربط المستخدم بنظام فرعي
- **Role**: الصلاحيات والأدوار
- **PermissionEntity**: الأذونات التفصيلية

### القيود الحالية:
- لا يوجد كيان يمثل الأقسام/الإدارات
- لا توجد علاقات أبوية بين الكيانات
- لا يمكن تمثيل هيكل تنظيمي هرمي

---

## 2. التصميم المقترح للبنية الشجرية

### 2.1 كيان Department (القسم/الإدارة)

```csharp
public class Department : Entity<Guid>, IAuditableEntity
{
    // المعلومات الأساسية
    public required string Name { get; set; }           // اسم القسم (مثال: "مكتب فني ديوان")
    public string? Code { get; set; }                   // رمز القسم (للتعرف الفريد)
    public string? Description { get; set; }            // وصف القسم
    
    // العلاقات الشجرية
    public Guid? ParentDepartmentId { get; set; }       // القسم الأب
    public Department? ParentDepartment { get; set; }   // مرجع للقسم الأب
    public ICollection<Department> ChildDepartments { get; set; } = new List<Department>(); // الأقسام الفرعية
    
    // المستوى في الهرم (1: سوريا, 2: محافظة, 3: منطقة, 4: بلدية, 5: مكتب, ...)
    public int Level { get; set; }
    
    // مسار المادة الكامل للتسلسل الهرمي (مثال: "1/5/12/45/78")
    public string? MaterializedPath { get; set; }
    
    // نوع القسم (اختياري - للتحقق من الصحة)
    public DepartmentType Type { get; set; }
    
    // المستخدمين التابعين لهذا القسم
    public ICollection<User> Users { get; set; } = new List<User>();
    
    // بيانات التدقيق
    public string? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// تعداد أنواع الأقسام
public enum DepartmentType
{
    Country = 1,        // دولة (سوريا)
    Governorate = 2,    // محافظة (ريف دمشق)
    District = 3,       // منطقة (الغوطة الشرقية)
    Municipality = 4,   // بلدية (دوما)
    Office = 5,         // مكتب (مكتب فني ديوان)
    Unit = 6            // وحدة إدارية أخرى
}
```

### 2.2 تعديل كيان User

```csharp
public class User : Entity<Guid>, IAuditableEntity
{
    // ... الحقول الحالية ...
    
    // إضافة رابط للقسم التابع له المستخدم
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    // هل يمثل هذا المستخدم قسمًا؟ (للمعاملة المزدوجة)
    public bool IsDepartmentHead { get; set; }
    
    // ... بقية الحقول الحالية ...
}
```

### 2.3 DTOs المقترحة

```csharp
// DTO لإنشاء قسم جديد
public class CreateDepartmentDto
{
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public DepartmentType Type { get; set; }
}

// DTO لعرض قسم مع معلوماته الكاملة
public class DepartmentDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public string? ParentDepartmentName { get; set; }
    public int Level { get; set; }
    public string? MaterializedPath { get; set; }
    public DepartmentType Type { get; set; }
    public int ChildrenCount { get; set; }
    public int UsersCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}

// DTO لعرض الشجرة كاملة
public class DepartmentTreeDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Code { get; set; }
    public int Level { get; set; }
    public DepartmentType Type { get; set; }
    public List<DepartmentTreeDto> Children { get; set; } = new();
}
```

---

## 3. مراحل التنفيذ

### المرحلة 1: إعداد قاعدة البيانات والكيانات (الأسبوع 1)

#### المهام:
1. **إنشاء كيان Department** في `ModernPaySystem.Domain/Entities/SharedEntities/`
2. **تعديل كيان User** لإضافة العلاقة مع Department
3. **إعداد Enum DepartmentType** لأنواع الأقسام
4. **تحديث AppDbContext** بإضافة DbSet<Department>
5. **تهيئة علاقات EF Core** بين Department و User

#### الملفات المطلوبة:
- `/workspace/ModernPaySystem.Domain/Entities/SharedEntities/Department.cs` (جديد)
- `/workspace/ModernPaySystem.Domain/Entities/SharedEntities/User.cs` (تعديل)
- `/workspace/ModernPaySystem.Infrastructure.Persistence/AppDbContext.cs` (تعديل)

#### الترحيل (Migration):
```bash
dotnet ef migrations add AddDepartmentHierarchy
dotnet ef database update
```

---

### المرحلة 2: تطوير طبقة الخدمة (الأسبوع 2)

#### المهام:
1. **إنشاء IDepartmentService** في `ModernPaySystem.Application/Interfaces/`
2. **تنفيذ DepartmentService** في `ModernPaySystem.Infrastructure/Services/`
3. **إنشاء DepartmentRepo** في `ModernPaySystem.Application/Repos/`

#### الوظائف الأساسية المطلوبة:

```csharp
public interface IDepartmentService
{
    // CRUD عمليات
    Task<DepartmentDto> CreateAsync(CreateDepartmentDto dto, string userId);
    Task<DepartmentDto?> GetByIdAsync(Guid id);
    Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentDto dto, string userId);
    Task<bool> DeleteAsync(Guid id);
    
    // عمليات الشجرة
    Task<List<DepartmentTreeDto>> GetTreeAsync();
    Task<List<DepartmentTreeDto>> GetSubTreeAsync(Guid departmentId);
    Task<List<DepartmentDto>> GetChildrenAsync(Guid departmentId);
    Task<DepartmentDto?> GetParentAsync(Guid departmentId);
    
    // البحث والاستعلام
    Task<List<DepartmentDto>> SearchAsync(string searchTerm, int level = 0);
    Task<List<DepartmentDto>> GetByLevelAsync(int level);
    Task<List<DepartmentDto>> GetPathToRootAsync(Guid departmentId);
    
    // إدارة المستخدمين
    Task<List<UserDto>> GetUsersInDepartmentAsync(Guid departmentId, bool includeSubDepartments = false);
    Task AssignUserToDepartmentAsync(Guid userId, Guid departmentId);
    Task RemoveUserFromDepartmentAsync(Guid userId);
    
    // التحقق من الصحة
    bool CanAssignParent(Guid departmentId, Guid parentDepartmentId);
}
```

#### منطق Materialized Path:
```csharp
// مثال على توليد المسار
// إذا كان ParentDepartment.MaterializedPath = "1/5/12"
// فإن القسم الجديد سيكون MaterializedPath = "1/5/12/45"
private string GenerateMaterializedPath(Guid? parentId, Guid currentId)
{
    if (!parentId.HasValue)
        return currentId.ToString("N")[..8]; // للجذر
    
    var parent = _departmentRepository.GetById(parentId.Value);
    return $"{parent.MaterializedPath}/{currentId.ToString("N")[..8]}";
}
```

---

### المرحلة 3: تطوير وحدات التحكم API (الأسبوع 3)

#### المهام:
1. **إنشاء DepartmentsController** في `ModernPaySystem/Controllers/`
2. **إضافة الصلاحيات** اللازمة للإدارة

#### النقاط النهائية (Endpoints) المقترحة:

```
GET    /api/departments/tree              # الحصول على الشجرة كاملة
GET    /api/departments/{id}/subtree      # الحصول على شجرة فرعية
GET    /api/departments/{id}              # الحصول على قسم محدد
GET    /api/departments/{id}/children     # الحصول على الأقسام الابنة
GET    /api/departments/{id}/path         # الحصول على المسار للأصل
GET    /api/departments/{id}/users        # الحصول على مستخدمي القسم
POST   /api/departments                   # إنشاء قسم جديد
PUT    /api/departments/{id}              # تحديث قسم
DELETE /api/departments/{id}              # حذف قسم
POST   /api/departments/{id}/assign-user  # تعيين مستخدم للقسم
DELETE /api/departments/{id}/remove-user  # إزالة مستخدم من القسم
GET    /api/departments/search            # البحث في الأقسام
```

#### مثال على Controller:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    
    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }
    
    [HttpGet("tree")]
    [HasPermission("departments.view_tree")]
    public async Task<ActionResult<List<DepartmentTreeDto>>> GetTree()
    {
        var tree = await _departmentService.GetTreeAsync();
        return Ok(tree);
    }
    
    [HttpPost]
    [HasPermission("departments.create")]
    public async Task<ActionResult<DepartmentDto>> Create(CreateDepartmentDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var result = await _departmentService.CreateAsync(dto, userId!);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
    
    // ... بقية العمليات
}
```

---

### المرحلة 4: البذور والبيانات الأولية (الأسبوع 4)

#### المهام:
1. **إنشاء DepartmentSeeder** لتهيئة الهيكل الأساسي
2. **إضافة بيانات تجريبية** تمثل الهيكل المطلوب

#### مثال على Seeder:

```csharp
public class DepartmentSeeder : EntitySeederBase
{
    public override async Task SeedAsync(AppDbContext context)
    {
        if (context.Departments.Any())
            return;
        
        // المستوى 1: سوريا
        var syria = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "الجمهورية العربية السورية",
            Code = "SYR",
            Level = 1,
            Type = DepartmentType.Country,
            MaterializedPath = "00000001"
        };
        
        // المستوى 2: ريف دمشق
        var rifDimashq = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "محافظة ريف دمشق",
            Code = "RD",
            Level = 2,
            Type = DepartmentType.Governorate,
            ParentDepartmentId = syria.Id,
            MaterializedPath = "00000001/00000002"
        };
        
        // المستوى 3: الغوطة الشرقية
        var ghouta = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "منطقة الغوطة الشرقية",
            Code = "GHO",
            Level = 3,
            Type = DepartmentType.District,
            ParentDepartmentId = rifDimashq.Id,
            MaterializedPath = "00000001/00000002/00000003"
        };
        
        // المستوى 4: بلدية دوما
        var doumaMunicipality = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "بلدية دوما",
            Code = "DOU",
            Level = 4,
            Type = DepartmentType.Municipality,
            ParentDepartmentId = ghouta.Id,
            MaterializedPath = "00000001/00000002/00000003/00000004"
        };
        
        // المستوى 5: مكتب فني ديوان
        var technicalOffice = new Department
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Name = "مكتب فني ديوان",
            Code = "TECH-DIWAN",
            Level = 5,
            Type = DepartmentType.Office,
            ParentDepartmentId = doumaMunicipality.Id,
            MaterializedPath = "00000001/00000002/00000003/00000004/00000005"
        };
        
        context.Departments.AddRange(syria, rifDimashq, ghouta, doumaMunicipality, technicalOffice);
        await context.SaveChangesAsync();
    }
}
```

---

### المرحلة 5: التكامل مع نظام الطلبات الحالي (الأسبوع 5)

#### المهام:
1. **ربط Request بـ Department** بدلاً من/بالإضافة إلى User
2. **تعديل منطق سير العمل** ليعتمد على الهيكل الإداري
3. **إضافة صلاحيات مبنية على الأقسام**

#### التعديلات المقترحة على Request:

```csharp
public class Request : Entity<Guid>, IAuditableEntity
{
    // ... الحقول الحالية ...
    
    // القسم الذي صدرت منه الطلب
    public Guid? RequesterDepartmentId { get; set; }
    public Department? RequesterDepartment { get; set; }
    
    // القسم المستهدف (للإحالة)
    public Guid? TargetDepartmentId { get; set; }
    public Department? TargetDepartment { get; set; }
}
```

#### تحسينات الإحالة:
- إحالة طلب إلى قسم (بدلاً من مستخدم محدد)
- أي مستخدم في القسم المستهدف يمكنه معالجة الطلب
- تتبع تاريخ انتقال الطلب بين الأقسام

---

### المرحلة 6: التحسينات المتقدمة (الأسبوع 6-7)

#### 6.1 التخزين المؤقت للشجرة (Caching)
```csharp
// استخدام Redis أو Memory Cache لتخزين الشجرة
// تحديث الكاش عند أي تغيير في الهيكل
```

#### 6.2 التحقق من الصحة المتقدم
```csharp
// منع إنشاء حلقات دائرية (Circular Reference)
// التحقق من عدم تجاوز مستوى معين
// التأكد من تطابق نوع القسم مع المستوى
```

#### 6.3 سجل التدقيق (Audit Trail)
```csharp
// تسجيل جميع التغييرات في الهيكل الإداري
// من قام بنقل قسم من أب لآخر
// متى تم إنشاء/حذف قسم
```

#### 6.4 الاستعلامات المحسّنة
```csharp
// استخدام CTE في SQL للاستعلامات الشجرية
// فهرسة MaterializedPath للبحث السريع
```

---

## 4. اعتبارات الأمان والصلاحيات

### الصلاحيات المقترحة:

| الصلاحية | الوصف | المستوى المطلوب |
|---------|-------|----------------|
| `departments.view` | عرض الأقسام | جميع المستخدمين |
| `departments.view_tree` | عرض الشجرة كاملة | مدراء الأقسام فما فوق |
| `departments.create` | إنشاء أقسام جديدة | مستوى محافظة فما فوق |
| `departments.edit` | تعديل الأقسام | نفس المستوى أو أعلى |
| `departments.delete` | حذف أقسام | مستوى أعلى بفئة واحدة على الأقل |
| `departments.assign_user` | تعيين مستخدمين | مدير القسم مباشرة |
| `departments.move` | نقل قسم بين الآباء | مستوى أعلى بفئتين على الأقل |

### قيود الوصول:
- لا يمكن للمستخدم رؤية أقسام خارج مساره الهرمي إلا بتصريح خاص
- مدير القسم يرى جميع الأقسام التابعة له
- التعديلات تتطلب موافقة من المستوى الأعلى

---

## 5. اختبارات الجودة

###单元测试 المطلوبة:
1. اختبار إنشاء قسم جديد
2. اختبار تحديث قسم وتغيير الأب
3. اختبار حذف قسم مع أبنائه
4. اختبار كشف الحلقات الدائرية
5. اختبار حساب MaterializedPath
6. اختبار GetTreeAsync
7. اختبار GetPathToRootAsync

### اختبارات التكامل:
1. اختبار سلسلة كاملة من الإنشاء حتى الحذف
2. اختبار الأداء مع أشجار كبيرة (1000+ قسم)
3. اختبار التزامن عند التعديل المتزامن

---

## 6. خطة الترحيل من النظام القديم

### إذا كان هناك بيانات حالية:

```sql
-- ترحيل المستخدمين كأقسام فردية (إذا لزم الأمر)
INSERT INTO Departments (Id, Name, Level, Type, MaterializedPath, CreatedAt)
SELECT 
    NEWID(),
    UserName,
    6, -- مستوى وحدة
    6, -- نوع وحدة
    RIGHT(NEWID(), 8),
    GETDATE()
FROM Users
WHERE DepartmentId IS NULL;
```

### استراتيجية الترحيل:
1. حفظ نسخة احتياطية من قاعدة البيانات
2. تشغيل Migration لإضافة الجداول الجديدة
3. تشغيل Seeder للهيكل الأساسي
4. ترحيل المستخدمين الحاليين وربطهم بالأقسام المناسبة
5. اختبار شامل قبل الانتقال للإنتاج

---

## 7. الجدول الزمني المقترح

| الأسبوع | المرحلة | المهام الرئيسية |
|---------|---------|-----------------|
| 1 | إعداد الكيانات | Department Entity, User Modification, DbContext |
| 2 | طبقة الخدمة | Repository, Service Implementation |
| 3 | API | Controllers, Endpoints, Authorization |
| 4 | التهيئة | Seeders, Initial Data |
| 5 | التكامل | Request Integration, Workflow Updates |
| 6-7 | التحسينات | Caching, Performance, Advanced Features |
| 8 | الاختبار والنشر | Testing, Bug Fixes, Production Deployment |

---

## 8. المخاطر والتحديات

### التحديات المتوقعة:

1. **الأداء مع الأشجار الكبيرة**
   - الحل: استخدام Materialized Path + فهرسة + Caching

2. **التعامل مع الحلقات الدائرية**
   - الحل: تحقق صارم قبل تعيين ParentId

3. **تعقيد الاستعلامات الشجرية**
   - الحل: استخدام CTE في SQL أو Stored Procedures

4. **التوافق مع البيانات القديمة**
   - الحل: خطة ترحيل واضحة واختبار مكثف

5. **إدارة الصلاحيات المعقدة**
   - الحل: نظام صلاحيات مرن يعتمد على المستوى الهرمي

---

## 9. الخلاصة

هذه الخطة توفر طريقًا واضحًا لتحويل النظام من بنية مسطحة إلى بنية شجرية هرمية كاملة. التركيز على:

- ✅ **المرونة**: دعم أي عدد من المستويات
- ✅ **الأداء**: استخدام Materialized Path للاستعلامات السريعة
- ✅ **الأمان**: صلاحيات دقيقة تعتمد على الموقع في الشجرة
- ✅ **القابلية للتوسع**: تصميم يدعم النمو المستقبلي
- ✅ **التكامل**: الحفاظ على التوافق مع نظام الطلبات الحالي

---

## المرفقات المقترحة

1. مخطط ERD للعلاقات الجديدة
2. وثيقة API كاملة (Swagger/OpenAPI)
3. دليل الاستخدام للإداريين
4. دليل المطورين للت扩展 المستقبلي

---

**تم إعداد هذه الخطة بواسطة:** مساعد الذكاء الاصطناعي  
**التاريخ:** 2026  
**الإصدار:** 1.0
