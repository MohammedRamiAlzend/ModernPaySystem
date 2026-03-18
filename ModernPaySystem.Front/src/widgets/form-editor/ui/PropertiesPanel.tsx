import React from 'react';
import type { FormField } from '@/entities/form/model/types';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Button } from '@/shared/ui/button';

interface PropertiesPanelProps {
    field: FormField;
    allFields: FormField[];
    onChange: (updates: Partial<FormField>) => void;
}

export const PropertiesPanel: React.FC<PropertiesPanelProps> = ({ field, allFields, onChange }) => {
    return (
        <div className="space-y-4 p-4 border rounded-md bg-card">
            <h3 className="font-semibold mb-2">خصائص الحقل</h3>

            {/* <div className="space-y-2">
                <Label>الاسم البرمجي (Name)</Label>
                <Input
                    value={field.name}
                    onChange={e => onChange({ name: e.target.value })}
                />
            </div> */}

            <div className="space-y-2">
                <Label>العنوان (Label)</Label>
                <Input
                    value={field.label}
                    onChange={e => onChange({ label: e.target.value })}
                />
            </div>

            <div className="space-y-2">
                <Label>Placeholder</Label>
                <Input
                    value={field.placeholder || ''}
                    onChange={e => onChange({ placeholder: e.target.value })}
                />
            </div>

            <div className="grid grid-cols-2 gap-4 border-t pt-4">
                <div className="space-y-2">
                    <Label className="text-xs">الرؤية (Visibility)</Label>
                    <select
                        className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring dark:bg-[#1a1a1a] dark:text-white"
                        value={field.initialVisibility || 'visible'}
                        onChange={(e) => onChange({ initialVisibility: e.target.value as 'visible' | 'hidden' })}
                    >
                        <option value="visible">ظاهر (Visible)</option>
                        <option value="hidden">مخفي (Hidden)</option>
                    </select>
                </div>

                <div className="space-y-2">
                    <Label className="text-xs">الحالة (Enabled)</Label>
                    <select
                        className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring dark:bg-[#1a1a1a] dark:text-white"
                        value={field.initialEnabled || 'enabled'}
                        onChange={(e) => onChange({ initialEnabled: e.target.value as 'enabled' | 'disabled' })}
                    >
                        <option value="enabled">مفعل (Enabled)</option>
                        <option value="disabled">معطل (Disabled)</option>
                    </select>
                </div>
            </div>

            <div className="space-y-2 border-t pt-4">
                <Label>العرض (Width)</Label>
                <select
                    className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm focus:ring-2 focus:ring-ring focus:outline-none dark:bg-[#1a1a1a] dark:text-white"
                    value={field.layout?.colSpan || 12}
                    onChange={(e) => onChange({ layout: { ...field.layout, colSpan: Number(e.target.value) } })}
                >
                    <option value={12}>كامل (100%)</option>
                    <option value={6}>نصف (50%)</option>
                    <option value={4}>ثلث (33%)</option>
                    <option value={3}>ربع (25%)</option>
                </select>
            </div>

            {(field.type === 'select' || field.type === 'radio') && (
                <div className="space-y-2 border-t pt-4">
                    <div className="flex flex-col gap-1 mb-2">
                        <Label className="font-semibold text-primary">الخيارات (Options)</Label>
                        {field.dataSource?.type === 'lookup' && (
                            <div className="p-2 bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-900/50 rounded-lg text-[10px] text-amber-700 dark:text-amber-400 font-medium">
                                تنبيه: هذا الحقل مرتبط بالإعدادات . إضافة خيارات يدوية سيقوم بإزالة الارتباط التلقائي.
                            </div>
                        )}
                    </div>
                    <div className="space-y-2">
                        {field.dataSource?.options?.map((opt, idx) => (
                            <div key={idx} className="flex gap-2 items-center group">
                                <Input
                                    placeholder="Label"
                                    value={opt.label}
                                    className="h-8"
                                    onChange={(e) => {
                                        const newOptions = [...(field.dataSource?.options || [])];
                                        newOptions[idx] = { ...opt, label: e.target.value };
                                        onChange({ dataSource: { type: 'static', options: newOptions } });
                                    }}
                                />
                                <Input
                                    placeholder="Value"
                                    value={opt.value}
                                    className="h-8"
                                    onChange={(e) => {
                                        const newOptions = [...(field.dataSource?.options || [])];
                                        newOptions[idx] = { ...opt, value: e.target.value };
                                        onChange({ dataSource: { type: 'static', options: newOptions } });
                                    }}
                                />
                                <button
                                    className="text-gray-400 hover:text-red-500"
                                    onClick={() => {
                                        const newOptions = (field.dataSource?.options || []).filter((_, i) => i !== idx);
                                        onChange({ dataSource: { type: 'static', options: newOptions } });
                                    }}
                                >
                                    ×
                                </button>
                            </div>
                        ))}
                        <Button
                            variant="outline"
                            size="sm"
                            className="w-full mt-1 border-dashed"
                            onClick={() => {
                                const newOptions = [...(field.dataSource?.options || []), { label: 'New Option', value: 'value' }];
                                onChange({ dataSource: { type: 'static', options: newOptions } });
                            }}
                        >
                            + إضافة خيار
                        </Button>
                    </div>
                </div>
            )}

            {/* Validation Rules Section */}
            <div className="space-y-2 border-t pt-4">
                <Label className="font-semibold">قواعد التحقق (Validation)</Label>
                <div className="space-y-3">
                    {field.validation?.map((rule, idx) => (
                        <div key={idx} className="p-2 border rounded bg-muted/20 space-y-2 relative">
                            <button
                                className="absolute top-1 left-1 text-gray-400 hover:text-red-500"
                                onClick={() => {
                                    const newValidation = field.validation?.filter((_, i) => i !== idx);
                                    onChange({ validation: newValidation });
                                }}
                            >
                                ×
                            </button>
                            <div className="text-[10px] font-bold uppercase">{rule.rule}</div>
                            {rule.rule !== 'required' && (
                                <Input
                                    className="h-7 text-xs"
                                    value={rule.value || ''}
                                    onChange={(e) => {
                                        const newValidation = [...(field.validation || [])];
                                        newValidation[idx] = { ...rule, value: e.target.value };
                                        onChange({ validation: newValidation });
                                    }}
                                    placeholder="القيمة..."
                                />
                            )}
                            <Input
                                className="h-7 text-xs"
                                value={rule.message || ''}
                                onChange={(e) => {
                                    const newValidation = [...(field.validation || [])];
                                    newValidation[idx] = { ...rule, message: e.target.value };
                                    onChange({ validation: newValidation });
                                }}
                                placeholder="الرسالة..."
                            />
                        </div>
                    ))}
                    <select
                        className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                        onChange={(e) => {
                            const ruleType = e.target.value as any;
                            if (!ruleType) return;
                            const newValidation = [...(field.validation || [])];
                            if (!newValidation.some(v => v.rule === ruleType)) {
                                newValidation.push({
                                    rule: ruleType,
                                    value: '',
                                    message: ruleType === 'required' ? 'هذا الحقل مطلوب' : 'قيمة غير صحيحة'
                                });
                                onChange({ validation: newValidation });
                            }
                            e.target.value = "";
                        }}
                    >
                        <option value="">+ إضافة قاعدة...</option>
                        <option value="required">مطلوب (Required)</option>
                        <option value="minLength">أصغر طول</option>
                        <option value="maxLength">أكبر طول</option>
                        <option value="minValue">أصغر قيمة</option>
                        <option value="maxValue">أكبر قيمة</option>
                        <option value="pattern">Regex Pattern</option>
                    </select>
                </div>
            </div>

            {/* Advanced: Number Spelling */}
            {(field.type === 'text' || field.type === 'textarea') && (
                <div className="space-y-3 border-t pt-4 bg-primary/5 p-3 rounded-xl border border-primary/10">
                    <div className="flex items-center gap-2 mb-1">
                        <div className="w-2 h-2 rounded-full bg-primary animate-pulse" />
                        <Label className="font-bold text-primary">تحويل الرقم إلى نص (Tafqeet)</Label>
                    </div>
                    <p className="text-[10px] text-muted-foreground leading-relaxed">
                        عند تفعيل هذا الخيار، سيقوم هذا الحقل تلقائياً بكتابة الرقم المحول من الحقل المختار باللغة العربية.
                    </p>
                    <select
                        className="flex h-9 w-full rounded-xl border border-primary/20 bg-background px-3 py-1 text-sm shadow-sm focus:ring-2 focus:ring-primary/20 outline-none transition-all dark:bg-[#1a1a1a]"
                        value={field.numberSpelling?.sourceField || ''}
                        onChange={(e) => {
                            const val = e.target.value;
                            onChange({
                                numberSpelling: val ? { sourceField: val } : undefined,
                                initialEnabled: val ? 'enabled' : field.initialEnabled // Auto-disable if linked
                            });
                        }}
                    >
                        <option value="">-- غير مفعل --</option>
                        {allFields
                            .filter(f => f.id !== field.id && (f.type === 'number' || f.type === 'text'))
                            .map(f => (
                                <option key={f.id} value={f.name}>
                                    {f.label} ({f.name})
                                </option>
                            ))
                        }
                    </select>
                </div>
            )}
        </div>
    );
};
