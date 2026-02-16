import React from 'react';
import type { FormSchema, LogicRule } from '@/entities/form/model/types';
import { Button } from '@/shared/ui/button';
import { Card } from '@/shared/ui/card';
import { Label } from '@/shared/ui/label';
import { Input } from '@/shared/ui/input';

interface LogicEditorProps {
    form: FormSchema;
    onUpdateRule: (index: number, updates: Partial<LogicRule>) => void;
    onDeleteRule: (index: number) => void;
    onAddRule: () => void;
}

export const LogicEditor: React.FC<LogicEditorProps> = ({ form, onUpdateRule, onDeleteRule, onAddRule }) => {
    return (
        <div className="space-y-6 max-w-4xl mx-auto">
            <div className="flex justify-between items-center  p-4 rounded-lg border">
                <h3 className="text-lg font-bold">قواعد المنطق الشرطي</h3>
                <Button onClick={onAddRule}>+ إضافة قاعدة جديدة</Button>
            </div>

            <div className="space-y-4">
                {(!form.logic || form.logic.length === 0) && (
                    <Card className="p-8 text-center text-gray-500 border-dashed border-2">
                        لا توجد قواعد منطقية حالياً. أضف قواعد لجعل النموذج تفاعلياً.
                    </Card>
                )}

                {form.logic?.map((rule, ruleIdx) => (
                    <Card key={ruleIdx} className="p-4 relative  border-r-4 border-r-primary">
                        <button
                            className="absolute top-2 left-2 text-red-500 hover:text-red-700 text-xl font-bold"
                            onClick={() => onDeleteRule(ruleIdx)}
                        >
                            ×
                        </button>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-6 pt-4">
                            <div className="space-y-3">
                                <Label className="font-bold text-primary italic">إذا تحقق الشرط التالي (When):</Label>
                                <div className="space-y-2">
                                    <Label>عندما يكون الحقل</Label>
                                    <select
                                        className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm"
                                        value={rule.when.field}
                                        onChange={(e) => onUpdateRule(ruleIdx, { when: { ...rule.when, field: e.target.value } })}
                                    >
                                        <option value="">اختر حقل...</option>
                                        {form.fields.map(f => (
                                            <option key={f.id} value={f.name}>{f.label} ({f.name})</option>
                                        ))}
                                    </select>
                                </div>

                                <div className="grid grid-cols-2 gap-2">
                                    <div className="space-y-2">
                                        <Label>العملية</Label>
                                        <select
                                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-1 text-sm shadow-sm"
                                            value={rule.when.operator}
                                            onChange={(e) => onUpdateRule(ruleIdx, { when: { ...rule.when, operator: e.target.value as any } })}
                                        >
                                            <option value="equals">يساوي</option>
                                            <option value="notEquals">لا يساوي</option>
                                            <option value="contains">يحتوي</option>
                                            <option value="greaterThan">أكبر من</option>
                                            <option value="lessThan">أصغر من</option>
                                        </select>
                                    </div>
                                    <div className="space-y-2">
                                        <Label>القيمة</Label>
                                        <Input
                                            value={typeof rule.when.value === 'boolean' ? String(rule.when.value) : rule.when.value}
                                            onChange={(e) => onUpdateRule(ruleIdx, { when: { ...rule.when, value: e.target.value } })}
                                            placeholder="القيمة..."
                                        />
                                    </div>
                                </div>
                            </div>

                            <div className="space-y-3 border-r pr-6">
                                <Label className="font-bold text-primary italic ">نفذ الإجراءات التالية (Then):</Label>
                                {rule.actions.map((action, actionIdx) => (
                                    <div key={actionIdx} className="space-y-2 p-3 border rounded-lg  mb-2 relative group">
                                        {rule.actions.length > 1 && (
                                            <button
                                                className="absolute -top-2 -left-2  border rounded-full w-5 h-5 flex items-center justify-center text-[20px] text-red-500 font-bold  opacity-0 group-hover:opacity-100 transition-opacity shadow-sm"
                                                onClick={() => {
                                                    const newActions = rule.actions.filter((_, i) => i !== actionIdx);
                                                    onUpdateRule(ruleIdx, { actions: newActions });
                                                }}
                                            >
                                                ×
                                            </button>
                                        )}
                                        <div className="space-y-2">
                                            <Label className="text-xs">تغيير حقل</Label>
                                            <select
                                                className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                                                value={action.targetField}
                                                onChange={(e) => {
                                                    const newActions = [...rule.actions];
                                                    newActions[actionIdx] = { ...action, targetField: e.target.value };
                                                    onUpdateRule(ruleIdx, { actions: newActions });
                                                }}
                                            >
                                                <option value="">اختر حقل...</option>
                                                {form.fields.map(f => (
                                                    <option key={f.id} value={f.name}>{f.label} ({f.name})</option>
                                                ))}
                                            </select>
                                        </div>

                                        <div className="space-y-2">
                                            <Label className="text-xs">التأثير</Label>
                                            <select
                                                className="flex h-9 w-full rounded-md border border-input bg-background px-3 py-1 text-sm"
                                                value={action.effect}
                                                onChange={(e) => {
                                                    const newActions = [...rule.actions];
                                                    newActions[actionIdx] = { ...action, effect: e.target.value as any };
                                                    onUpdateRule(ruleIdx, { actions: newActions });
                                                }}
                                            >
                                                <option value="show">إظهار (Show)</option>
                                                <option value="hide">إخفاء (Hide)</option>
                                                <option value="enable">تفعيل (Enable)</option>
                                                <option value="disable">تعطيل (Disable)</option>
                                                <option value="require">جعله مطلوباً</option>
                                                <option value="unrequire">جعله اختيارياً</option>
                                            </select>
                                        </div>
                                    </div>
                                ))}
                                <Button
                                    variant="outline"
                                    size="sm"
                                    className="w-full border-dashed text-[10px]"
                                    onClick={() => {
                                        const newActions = [...rule.actions, { targetField: '', effect: 'show' as const }];
                                        onUpdateRule(ruleIdx, { actions: newActions });
                                    }}
                                >
                                    + إضافة إجراء إضافي لنفس الشرط
                                </Button>
                            </div>
                        </div>
                    </Card>
                ))}

                {form.logic && form.logic.length > 0 && (
                    <div className="flex justify-center py-4">
                        <Button
                            variant="secondary"
                            className="px-12 border-dashed border-2 shadow-sm"
                            onClick={onAddRule}
                        >
                            + إضافة قاعدة منطقية كاملة (شرط جديد)
                        </Button>
                    </div>
                )}
            </div>
        </div>
    );
};
