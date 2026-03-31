import React, { useState, useLayoutEffect, useRef } from 'react';
import { useFormEditor } from '@/features/form-builder/model/use-form-editor';
import type { FormSchema } from '@/entities/form/model/types';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Card } from '@/shared/ui/card';
import { ChevronDown, Settings2 } from 'lucide-react';
import { PropertiesPanel } from './PropertiesPanel';
import { LogicEditor } from './LogicEditor';
import { FormRenderer } from '@/widgets/form-renderer/ui/FormRenderer';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import gsap from 'gsap';
import { useLookUpFields } from '@/features/lookup-management/api/lookupApi';
import { cn } from '@/shared/lib/utils';

interface FormEditorProps {
    initialForm?: FormSchema;
    onSave?: () => void;
    onCancel?: () => void;
}

export const FormEditor: React.FC<FormEditorProps> = ({ initialForm, onSave, onCancel }) => {
    const {
        form,
        updateFormTitle,
        addField,
        updateField,
        deleteField,
        moveField,
        addLogicRule,
        updateLogicRule,
        deleteLogicRule,
        saveForm,
        isLoading
    } = useFormEditor(initialForm);

    const { data: lookupFields = [], isLoading: isLoadingLookups } = useLookUpFields();

    const [selectedFieldId, setSelectedFieldId] = useState<string | null>(null);
    const [mode, setMode] = useState<'edit' | 'preview'>('edit');
    const [activeTab, setActiveTab] = useState<'fields' | 'logic'>('fields');
    const [isLookupMenuOpen, setIsLookupMenuOpen] = useState(false);
    const [isRadioLookupOpen, setIsRadioLookupOpen] = useState(false);
    const [isCheckLookupOpen, setIsCheckLookupOpen] = useState(false);

    // Advanced GSAP FLIP Animation
    const listRef = useRef<HTMLDivElement>(null);
    const positionsRef = useRef<Record<string, number>>({});

    // Capture positions of all elements BEFORE they move or after mount
    const capturePositions = () => {
        if (!listRef.current) return;
        const children = Array.from(listRef.current.children) as HTMLElement[];
        children.forEach((child) => {
            const id = child.getAttribute('data-id');
            if (id) {
                // Use offsetTop for stability against scrolling
                positionsRef.current[id] = child.offsetTop;
            }
        });
    };

    // Initial capture on mount and whenever we switch tabs back to fields
    useLayoutEffect(() => {
        if (activeTab === 'fields' && mode === 'edit') {
            // Short timeout to ensure DOM is ready
            const timer = setTimeout(capturePositions, 50);
            return () => clearTimeout(timer);
        }
    }, [activeTab, mode]);

    useLayoutEffect(() => {
        if (!listRef.current || mode !== 'edit' || activeTab !== 'fields') return;

        const children = Array.from(listRef.current.children) as HTMLElement[];

        children.forEach((child) => {
            const id = child.getAttribute('data-id');
            if (!id) return;

            const first = positionsRef.current[id];
            const last = child.offsetTop;

            // Only animate if the position actually changed more than a pixel
            if (first !== undefined && Math.abs(first - last) > 1) {
                const invert = first - last;

                // Create a clean animation for each affected element
                gsap.fromTo(child,
                    {
                        y: invert,
                        zIndex: Math.abs(invert) > 50 ? 20 : 1 // High z-index for the main moving element
                    },
                    {
                        y: 0,
                        duration: 0.5,
                        ease: "power3.out",
                        clearProps: "y,zIndex"
                    }
                );
            }
            // Update reference for next move
            positionsRef.current[id] = last;
        });
    }, [form.fields, activeTab, mode]); // Triggered when fields order/content changes or tab/mode switches

    const handleSave = async () => {
        const success = await saveForm();
        if (success && onSave) {
            onSave();
        }
    };

    if (mode === 'preview') {
        return (
            <AnimatedContainer className="space-y-4">
                <div className="flex justify-between items-center  p-4 rounded border">
                    <h2 className="text-2xl font-bold">معاينة: {form.title}</h2>
                    <Button variant="outline" onClick={() => setMode('edit')}>رجوع للتحرير</Button>
                </div>
                <Card className="p-6">
                    <FormRenderer schema={form} onSubmit={(data) => console.log('Preview Submit:', data)} />
                </Card>
            </AnimatedContainer>
        );
    }

    const selectedField = form.fields.find(f => f.id === selectedFieldId);

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center border-b p-4 rounded shadow-sm">
                <div className="flex items-center space-x-4 space-x-reverse">
                    <Input
                        value={form.title}
                        onChange={e => updateFormTitle(e.target.value)}
                        className="text-xl font-bold w-64"
                    />
                    <span className="text-sm ">({form.fields.length} حقول)</span>
                </div>
                <div className="space-x-2 space-x-reverse">
                    <Button variant="secondary" onClick={() => setMode('preview')}>معاينة</Button>
                    <Button onClick={handleSave} disabled={isLoading}>
                        {isLoading ? 'جاري الحفظ...' : 'حفظ النموذج'}
                    </Button>
                    <Button variant="ghost" onClick={onCancel}>إلغاء</Button>
                </div>
            </div>

            {/* Navigation Tabs */}
            <div className="flex space-x-4 space-x-reverse border-b">
                <button
                    className={`pb-2 px-4 font-medium transition-colors border-b-2 ${activeTab === 'fields' ? 'border-primary text-primary' : 'border-transparent text-gray-500 hover:text-gray-700'}`}
                    onClick={() => setActiveTab('fields')}
                >
                    محرر الحقول
                </button>
                <button
                    className={`pb-2 px-4 font-medium transition-colors border-b-2 ${activeTab === 'logic' ? 'border-primary text-primary' : 'border-transparent text-gray-500 hover:text-gray-700'}`}
                    onClick={() => setActiveTab('logic')}
                >
                    قواعد المنطق
                </button>
            </div>

            {activeTab === 'fields' ? (
                <div className="grid grid-cols-12 gap-6 animate-in fade-in duration-300">
                    {/* Tools Panel */}
                    <div className="col-span-12 md:col-span-3 space-y-2">
                        <h3 className="font-semibold mb-2">الأدوات</h3>
                        <div className="grid grid-cols-1 gap-2">
                            <Button variant="outline" className="justify-start h-9" onClick={() => addField('text')}>نص (Text)</Button>
                            <Button variant="outline" className="justify-start h-9" onClick={() => addField('number')}>رقم (Number)</Button>
                            <Button variant="outline" className="justify-start h-9" onClick={() => addField('email')}>بريد إلكتروني</Button>
                            <Button variant="outline" className="justify-start h-9" onClick={() => addField('textarea')}>نص طويل</Button>
                            <Button variant="outline" className="justify-start h-9" onClick={() => addField('date')}>تاريخ (Date)</Button>
                            <Button variant="outline" className="justify-start h-9 text-blue-600 border-blue-200 bg-blue-50/10" onClick={() => addField('label')}>نص توضيحي فقط (Label Only)</Button>

                            <div className="space-y-1">
                                <Button variant="outline" className="justify-start h-9 w-full" onClick={() => addField('select')}>قائمة (Select) - عادي</Button>
                                <Button
                                    variant="outline"
                                    className={cn("justify-between h-9 w-full  border-primary/20 ", isLookupMenuOpen ? "bg-primary/5" : "")}
                                    onClick={() => setIsLookupMenuOpen(!isLookupMenuOpen)}
                                >
                                    <span className="flex items-center gap-2">
                                        <Settings2 className="w-3 h-3 text-primary" />
                                        قائمة من الإعدادات
                                    </span>
                                    <ChevronDown className={`w-3 h-3 transition-transform duration-200 ${isLookupMenuOpen ? 'rotate-180' : ''}`} />
                                </Button>

                                {isLookupMenuOpen && (
                                    <div className="animate-in slide-in-from-top-2 duration-200">
                                        {isLoadingLookups ? (
                                            <div className="text-[10px] text-center p-2 italic text-muted-foreground animate-pulse">جاري جلب الحقول...</div>
                                        ) : lookupFields.length > 0 ? (
                                            <div className="pr-4 space-y-1 border-r-2 border-primary/20 mr-2 py-1">
                                                {lookupFields.map(f => (
                                                    <button
                                                        key={f.id}
                                                        onClick={() => addField('select', f.id, f.filedName)}
                                                        className="text-[10px] block w-full text-right py-1.5 px-3 hover:bg-primary/10 rounded-lg transition-colors font-medium border border-transparent hover:border-primary/20"
                                                    >
                                                        + {f.filedName}
                                                    </button>
                                                ))}
                                            </div>
                                        ) : (
                                            <div className="text-[10px] text-center p-2 text-muted-foreground italic">لا توجد حقول معرفة</div>
                                        )}
                                    </div>
                                )}
                            </div>

                            {/* Radio with LookUp */}
                            <div className="space-y-1">
                                <Button variant="outline" className="justify-start h-9 w-full" onClick={() => addField('radio')}>أزرار اختيار - عادي</Button>
                                <Button
                                    variant="outline"
                                    className={cn("justify-between h-9 w-full border-primary/20 ", isRadioLookupOpen ? "bg-primary/5" : "")}
                                    onClick={() => setIsRadioLookupOpen(!isRadioLookupOpen)}
                                >
                                    <span className="flex items-center gap-2">
                                        <Settings2 className="w-3 h-3 text-primary" />
                                        أزرار من الإعدادات
                                    </span>
                                    <ChevronDown className={`w-3 h-3 transition-transform duration-200 ${isRadioLookupOpen ? 'rotate-180' : ''}`} />
                                </Button>

                                {isRadioLookupOpen && (
                                    <div className="animate-in slide-in-from-top-2 duration-200">
                                        {isLoadingLookups ? (
                                            <div className="text-[10px] text-center p-2 italic text-muted-foreground animate-pulse">جاري جلب الحقول...</div>
                                        ) : lookupFields.length > 0 ? (
                                            <div className="pr-4 space-y-1 border-r-2 border-primary/20 mr-2 py-1">
                                                {lookupFields.map(f => (
                                                    <button
                                                        key={f.id}
                                                        onClick={() => addField('radio', f.id, f.filedName)}
                                                        className="text-[10px] block w-full text-right py-1.5 px-3 hover:bg-primary/10 rounded-lg transition-colors font-medium border border-transparent hover:border-primary/20"
                                                    >
                                                        + {f.filedName}
                                                    </button>
                                                ))}
                                            </div>
                                        ) : (
                                            <div className="text-[10px] text-center p-2 text-muted-foreground italic">لا توجد حقول معرفة</div>
                                        )}
                                    </div>
                                )}
                            </div>

                            {/* Checkbox with LookUp */}
                            <div className="space-y-1">
                                <Button variant="outline" className="justify-start h-9 w-full" onClick={() => addField('checkbox')}>مربع اختيار - عادي</Button>
                                <Button
                                    variant="outline"
                                    className={cn("justify-between h-9 w-full border-primary/20 ", isCheckLookupOpen ? "bg-primary/5" : "")}
                                    onClick={() => setIsCheckLookupOpen(!isCheckLookupOpen)}
                                >
                                    <span className="flex items-center gap-2">
                                        <Settings2 className="w-3 h-3 text-primary" />
                                        مربعات من الإعدادات
                                    </span>
                                    <ChevronDown className={`w-3 h-3 transition-transform duration-200 ${isCheckLookupOpen ? 'rotate-180' : ''}`} />
                                </Button>

                                {isCheckLookupOpen && (
                                    <div className="animate-in slide-in-from-top-2 duration-200">
                                        {isLoadingLookups ? (
                                            <div className="text-[10px] text-center p-2 italic text-muted-foreground animate-pulse">جاري جلب الحقول...</div>
                                        ) : lookupFields.length > 0 ? (
                                            <div className="pr-4 space-y-1 border-r-2 border-primary/20 mr-2 py-1">
                                                {lookupFields.map(f => (
                                                    <button
                                                        key={f.id}
                                                        onClick={() => addField('checkbox', f.id, f.filedName)}
                                                        className="text-[10px] block w-full text-right py-1.5 px-3 hover:bg-primary/10 rounded-lg transition-colors font-medium border border-transparent hover:border-primary/20"
                                                    >
                                                        + {f.filedName}
                                                    </button>
                                                ))}
                                            </div>
                                        ) : (
                                            <div className="text-[10px] text-center p-2 text-muted-foreground italic">لا توجد حقول معرفة</div>
                                        )}
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>

                    {/* Canvas (Fields List) */}
                    <div
                        ref={listRef}
                        className="col-span-12 md:col-span-6 space-y-4 min-h-[400px] border px-4 py-4 rounded  shadow-inner overflow-hidden"
                    >
                        {form.fields.length === 0 && <div className="text-center  mt-10">لا توجد حقول حالياً.</div>}

                        {form.fields.map((field) => (
                            <Card
                                key={field.id}
                                data-id={field.id}
                                className={`group p-4 cursor-pointer relative border-2 transition-colors duration-200
                                    ${selectedFieldId === field.id ? 'border-primary bg-primary/5' : 'border-border hover:border-primary/50'} 
                                    ${field.initialVisibility === 'hidden' ? 'opacity-60 ' : ''} 
                                    ${field.initialEnabled === 'disabled' ? 'grayscale-[0.5] bg-gray-100/10' : ''}
                                `}
                                onClick={() => setSelectedFieldId(field.id)}
                            >
                                <div className="flex justify-between items-start">
                                    <div className="space-y-1">
                                        <div className="flex items-center gap-2 font-bold text-sm">
                                            {field.label}
                                            {field.validation?.some(v => v.rule === 'required') && <span className="text-red-500">*</span>}
                                            <div className="flex gap-1">
                                                {field.initialVisibility === 'hidden' && (
                                                    <span className="text-[9px] bg-amber-100 text-amber-700 px-1.5 py-0.5 rounded-full font-bold">مخفي</span>
                                                )}
                                                {field.initialEnabled === 'disabled' && (
                                                    <span className="text-[9px] bg-gray-200 text-gray-600 px-1.5 py-0.5 rounded-full font-bold">معطل</span>
                                                )}
                                            </div>
                                        </div>
                                        <div className="text-[10px] text-gray-500  px-1 py-0.5 rounded w-fit">
                                            {field.type.toUpperCase()} | {field.name}
                                        </div>
                                    </div>

                                    <div className="flex gap-2 items-center opacity-0 group-hover:opacity-100 transition-opacity">
                                        <div className="flex flex-col gap-0.5">
                                            <Button variant="ghost" className="h-5 w-5 p-0" onClick={(e) => { e.stopPropagation(); moveField(field.id, 'up'); }}>↑</Button>
                                            <Button variant="ghost" className="h-5 w-5 p-0" onClick={(e) => { e.stopPropagation(); moveField(field.id, 'down'); }}>↓</Button>
                                        </div>
                                        <Button
                                            variant="destructive"
                                            className="h-7 w-7 p-0"
                                            onClick={(e) => { e.stopPropagation(); deleteField(field.id); }}
                                        >
                                            ×
                                        </Button>
                                    </div>
                                </div>
                            </Card>
                        ))}
                    </div>

                    {/* Properties Panel */}
                    <div className="col-span-12 md:col-span-3">
                        {selectedField ? (
                            <PropertiesPanel
                                field={selectedField}
                                allFields={form.fields}
                                onChange={(updates) => updateField(selectedField.id, updates)}
                            />
                        ) : (
                            <div className="text-gray-400 text-center mt-10 p-4 border rounded border-dashed">اختر حقلاً لتعديل خصائصه</div>
                        )}
                    </div>
                </div>
            ) : (
                <LogicEditor
                    form={form}
                    onAddRule={addLogicRule}
                    onUpdateRule={updateLogicRule}
                    onDeleteRule={deleteLogicRule}
                />
            )}
        </div>
    );
};
