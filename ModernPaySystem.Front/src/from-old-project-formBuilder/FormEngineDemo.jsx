import React from 'react';
import FormRenderer from './FormRenderer';
import Card from './components/common/Card';

// Example schema for demonstration
const userRegistrationSchema = {
  id: 'user-registration',
  title: 'نموذج تسجيل مستخدم',
  fields: [
    {
      id: 'name-field',
      name: 'fullName',
      type: 'text',
      label: 'الاسم الكامل',
      placeholder: 'أدخل اسمك الكامل',
      defaultValue: '',
      validation: [
        { rule: 'required', message: 'الاسم الكامل مطلوب' },
        { rule: 'minLength', value: 2, message: 'يجب أن يكون الاسم حرفين على الأقل' },
        { rule: 'maxLength', value: 50, message: 'يجب ألا يتجاوز الاسم 50 حرفاً' }
      ]
    },
    {
      id: 'email-field',
      name: 'email',
      type: 'email',
      label: 'البريد الإلكتروني',
      placeholder: 'أدخل بريدك الإلكتروني',
      defaultValue: '',
      validation: [
        { rule: 'required', message: 'البريد الإلكتروني مطلوب' },
        {
          rule: 'pattern',
          value: '^[\\w.-]+@([\\w-]+\\.)+[\\w-]{2,}$',
          message: 'يرجى إدخال بريد إلكتروني صحيح'
        }
      ]
    },
    {
      id: 'role-field',
      name: 'role',
      type: 'select',
      label: 'الدور',
      placeholder: 'اختر دورك',
      defaultValue: '',
      dataSource: {
        type: 'static',
        options: [
          { label: 'مستخدم', value: 'user' },
          { label: 'مسؤول', value: 'admin' },
          { label: 'مدير', value: 'manager' }
        ]
      },
      validation: [
        { rule: 'required', message: 'يجب اختيار الدور' }
      ]
    },
    {
      id: 'age-field',
      name: 'age',
      type: 'number',
      label: 'العمر',
      placeholder: 'أدخل عمرك',
      defaultValue: '',
      validation: [
        { rule: 'required', message: 'العمر مطلوب' },
        { rule: 'minValue', value: 18, message: 'يجب أن يكون العمر 18 عاماً على الأقل' },
        { rule: 'maxValue', value: 100, message: 'يجب ألا يتجاوز العمر 100 عام' }
      ]
    },
    {
      id: 'gender-field',
      name: 'gender',
      type: 'radio',
      label: 'الجنس',
      defaultValue: 'male',
      dataSource: {
        type: 'static',
        options: [
          { label: 'ذكر', value: 'male' },
          { label: 'أنثى', value: 'female' }
        ]
      },
      validation: [
        { rule: 'required', message: 'يرجى تحديد الجنس' }
      ]
    },
    {
      id: 'notes-field',
      name: 'notes',
      type: 'textarea',
      label: 'ملاحظات إضافية',
      placeholder: 'اكتب أي ملاحظات هنا...',
      rows: 3,
      defaultValue: ''
    },
    {
      id: 'newsletter-field',
      name: 'newsletter',
      type: 'checkbox',
      label: 'الاشتراك في النشرة الإخبارية',
      defaultValue: false
    },
    {
      id: 'admin-code-field',
      name: 'adminCode',
      type: 'text',
      label: 'رمز المسؤول',
      placeholder: 'أدخل رمز المسؤول',
      defaultValue: '',
      hidden: true, // Initially hidden
      validation: [
        { rule: 'required', message: 'رمز المسؤول مطلوب للمسؤولين' },
        { rule: 'minLength', value: 5, message: 'يجب أن يكون الرمز 5 أحرف على الأقل' }
      ]
    }
  ],
  logic: [
    {
      when: { field: 'role', operator: 'equals', value: 'admin' },
      actions: [
        { targetField: 'adminCode', effect: 'show' },
        { targetField: 'adminCode', effect: 'require' }
      ]
    },
    {
      when: { field: 'role', operator: 'notEquals', value: 'admin' },
      actions: [
        { targetField: 'adminCode', effect: 'hide' },
        { targetField: 'adminCode', effect: 'unrequire' }
      ]
    }
  ]
};

const FormEngineDemo = () => {
  const handleSubmit = (formData) => {
    console.log('Form submitted with data:', formData);
    alert('تم إرسال النموذج بنجاح! تحقق من القنصل لمزيد من التفاصيل.');
  };

  return (
    <div className="max-w-3xl mx-auto p-4 md:p-8">
      <Card className="animate-fade-in shadow-xl border-t-4 border-t-primary">
        <h1 className="text-2xl font-bold mb-8 text-primary border-b pb-4">{userRegistrationSchema.title}</h1>
        <FormRenderer schema={userRegistrationSchema} onSubmit={handleSubmit} />
      </Card>
    </div>
  );
};

export default FormEngineDemo;