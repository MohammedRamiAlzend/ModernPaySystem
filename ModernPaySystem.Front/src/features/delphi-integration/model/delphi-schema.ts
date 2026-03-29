import type { FormSchema } from '@/entities/form/model/types';

/**
 * The fixed FormSchema used for processing transactions from Delphi.
 * This is "Hardcoded" in the frontend to ensure consistency with the desktop app's data structure.
 */
export const DELPHI_TEMPLATE_NAME = "نموذج معاملات سطح المكتب (Delphi)";

export const DELPHI_FIXED_SCHEMA: FormSchema = {
    id: "delphi-fixed-template-id", // Note: This is our internal UI ID, the backend will assign a UUID
    title: DELPHI_TEMPLATE_NAME,
    description: "نموذج معالجة البيانات القادمة من تطبيق المواطن (نظام دلفي)",
    fields: [
        {
            id: "d1",
            name: "id",
            type: "text",
            label: "رقم المعاملة",
            readOnly: true,
            layout: { colSpan: 6 }
        },
        {
            id: "d2",
            name: "action_date",
            type: "text",
            label: "تاريخ العملية",
            readOnly: true,
            layout: { colSpan: 6 }
        },
        {
            id: "d3",
            name: "client",
            type: "text",
            label: "اسم المواطن",
            layout: { colSpan: 6 },
            validation: [{ rule: 'required', message: 'اسم المواطن مطلوب' }]
        },
        {
            id: "d4",
            name: "clientas",
            type: "text",
            label: "صفة المواطن",
            layout: { colSpan: 6 }
        },
        {
            id: "d5",
            name: "id_reality",
            type: "number",
            label: "رقم العقار",
            layout: { colSpan: 6 }
        },
        {
            id: "d6",
            name: "region",
            type: "text",
            label: "المنطقة العقارية",
            layout: { colSpan: 6 }
        },
        {
            id: "d10",
            name: "sum_amount",
            type: "number",
            label: "المبلغ الإجمالي",
            readOnly: true,
            layout: { colSpan: 6 }
        },
        {
            id: "d7",
            name: "services_summary",
            type: "textarea",
            label: "ملخص الخدمات",
            rows: 4,
            layout: { colSpan: 12 },
            readOnly: true
        },
        {
            id: "d8",
            name: "total_services_count",
            type: "number",
            label: "إجمالي عدد الخدمات",
            readOnly: true,
            layout: { colSpan: 6 }
        },
        {
            id: "d9",
            name: "services_types_count",
            type: "number",
            label: "عدد أنواع الخدمات",
            readOnly: true,
            layout: { colSpan: 6 }
        }
    ],
    logic: []
};
