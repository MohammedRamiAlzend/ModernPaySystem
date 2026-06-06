import React, { useRef, useEffect } from 'react';
import { useForm, useFieldArray, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Switch } from '@/shared/ui/switch';
import { Plus, Trash2, ArrowUp, ArrowDown, X, Save } from 'lucide-react';
import { DynamicFormTemplate, CreateDynamicFormTemplateDto } from '../model/types';
import { useUIStore } from '@/app/store/uiStore';
import { gsap } from 'gsap';

interface ArchivingTemplateEditorProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: (data: CreateDynamicFormTemplateDto) => Promise<void>;
    template: DynamicFormTemplate | null;
    isSaving: boolean;
}

const fieldSchema = z.object({
    label: z.string().min(1, 'يجب إدخال تسمية للحقل'),
    type: z.enum(['text', 'textarea', 'number', 'date']),
    required: z.boolean(),
});

const formSchema = z.object({
    templateFormName: z.string().min(2, 'يجب أن يكون اسم النموذج حقلين على الأقل'),
    fields: z.array(fieldSchema).min(1, 'يجب إضافة حقل واحد على الأقل للنموذج'),
});

type FormValues = z.infer<typeof formSchema>;

export const ArchivingTemplateEditor: React.FC<ArchivingTemplateEditorProps> = ({
    isOpen,
    onClose,
    onSave,
    template,
    isSaving
}) => {
    const { showStatus } = useUIStore();
    const containerRef = useRef<HTMLDivElement>(null);
    const listRef = useRef<HTMLDivElement>(null);

    const { 
        register, 
        control, 
        handleSubmit, 
        reset,
        formState: { errors } 
    } = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        defaultValues: {
            templateFormName: '',
            fields: [{ label: 'رقم المستند', type: 'text', required: true }]
        }
    });

    const { fields, append, remove, move } = useFieldArray({
        control,
        name: 'fields'
    });

    // مزامنة النموذج عند الفتح أو تغيير القالب المحدد
    useEffect(() => {
        if (isOpen) {
            if (template) {
                try {
                    const parsed = JSON.parse(template.contentAsJson);
                    reset({
                        templateFormName: template.templateFormName,
                        fields: Array.isArray(parsed) ? parsed : []
                    });
                } catch (e) {
                    console.error('Failed to parse contentAsJson', e);
                    reset({
                        templateFormName: template.templateFormName,
                        fields: []
                    });
                }
            } else {
                reset({
                    templateFormName: '',
                    fields: [
                        { label: 'رقم المستند', type: 'text', required: true }
                    ]
                });
            }

            // حركة فتح الـ Modal بالكامل باستخدام GSAP
            if (containerRef.current) {
                gsap.fromTo(containerRef.current, 
                    { opacity: 0, scale: 0.95 },
                    { opacity: 1, scale: 1, duration: 0.3, ease: 'back.out(1.2)' }
                );
            }
        }
    }, [template, isOpen, reset]);

    // حركة إضافة حقل جديد
    const handleAddField = () => {
        append({ label: '', type: 'text', required: false });
        
        // تأخير بسيط للتأكد من رندر العنصر الجديد قبل تحريكه
        setTimeout(() => {
            if (listRef.current) {
                const items = listRef.current.querySelectorAll('.field-item-row');
                if (items.length > 0) {
                    const lastItem = items[items.length - 1];
                    gsap.fromTo(lastItem,
                        { opacity: 0, y: 15, scale: 0.98 },
                        { opacity: 1, y: 0, scale: 1, duration: 0.35, ease: 'power2.out' }
                    );
                    
                    // عمل Scroll تلقائي لأسفل القائمة
                    lastItem.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }
            }
        }, 50);
    };

    const handleRemoveField = (index: number) => {
        if (listRef.current) {
            const items = listRef.current.querySelectorAll('.field-item-row');
            if (items[index]) {
                gsap.to(items[index], {
                    opacity: 0,
                    x: -30,
                    height: 0,
                    padding: 0,
                    marginBottom: 0,
                    duration: 0.25,
                    ease: 'power2.in',
                    onComplete: () => {
                        remove(index);
                    }
                });
            } else {
                remove(index);
            }
        } else {
            remove(index);
        }
    };

    const handleMoveField = (index: number, direction: 'up' | 'down') => {
        if (direction === 'up' && index === 0) return;
        if (direction === 'down' && index === fields.length - 1) return;

        const targetIndex = direction === 'up' ? index - 1 : index + 1;
        
        if (listRef.current) {
            const items = listRef.current.querySelectorAll('.field-item-row');
            const itemA = items[index];
            const itemB = items[targetIndex];

            if (itemA && itemB) {
                const yOffset = direction === 'up' ? -50 : 50;

                gsap.to(itemA, { y: yOffset, duration: 0.2, ease: 'power2.inOut' });
                gsap.to(itemB, { y: -yOffset, duration: 0.2, ease: 'power2.inOut', onComplete: () => {
                    move(index, targetIndex);
                    // إعادة تعيين الموضع بعد التحريك الفعلي للـ DOM
                    gsap.set([itemA, itemB], { y: 0 });
                }});
            } else {
                move(index, targetIndex);
            }
        } else {
            move(index, targetIndex);
        }
    };

    const onSubmit = async (values: FormValues) => {
        await onSave({
            templateFormName: values.templateFormName.trim(),
            contentAsJson: JSON.stringify(values.fields)
        });
    };

    const handleFormSubmitError = () => {
        // إظهار تنبيه في حال وجود أخطاء في تعبئة النموذج
        showStatus({
            type: 'error',
            title: 'خطأ في المدخلات',
            message: 'يرجى التحقق من تسمية كافة الحقول وتصحيح الأخطاء الموضحة.'
        });
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 overflow-y-auto" dir="rtl">
            <div 
                ref={containerRef}
                className="bg-card border border-border rounded-3xl p-6 max-w-2xl w-full shadow-2xl flex flex-col gap-6 text-right my-8 max-h-[90vh]"
            >
                {/* الرأس */}
                <div className="flex items-center justify-between border-b border-border pb-4">
                    <div className="flex flex-col gap-1">
                        <h2 className="text-lg font-bold text-foreground">
                            {template ? 'تعديل نموذج الأرشفة' : 'إنشاء نموذج أرشفة جديد'}
                        </h2>
                        <p className="text-xs text-muted-foreground">
                            قم ببناء حقول مخصصة للأرشفة الإلكترونية بشكل ديناميكي.
                        </p>
                    </div>
                    <button 
                        onClick={onClose}
                        className="p-1.5 rounded-xl hover:bg-muted text-muted-foreground transition-colors"
                    >
                        <X className="h-5 w-5" />
                    </button>
                </div>

                {/* النموذج */}
                <form 
                    onSubmit={handleSubmit(onSubmit, handleFormSubmitError)} 
                    className="flex-1 overflow-y-auto flex flex-col gap-5 pr-1"
                >
                    {/* اسم النموذج */}
                    <div className="flex flex-col gap-1.5">
                        <Label htmlFor="template-name" className="text-xs font-bold text-muted-foreground">اسم النموذج الأرشيفي</Label>
                        <Input 
                            id="template-name"
                            {...register('templateFormName')}
                            placeholder="مثال: نموذج المشتريات، نموذج العقود..."
                            className={`rounded-xl h-11 ${errors.templateFormName ? 'border-destructive focus-visible:ring-destructive' : ''}`}
                            disabled={isSaving}
                        />
                        {errors.templateFormName && (
                            <span className="text-xs text-destructive font-semibold mt-0.5">
                                {errors.templateFormName.message}
                            </span>
                        )}
                    </div>

                    {/* قائمة الحقول */}
                    <div className="flex flex-col gap-3">
                        <div className="flex items-center justify-between pb-2 border-b border-border">
                            <span className="text-xs font-bold text-muted-foreground">الحقول الديناميكية ({fields.length})</span>
                            <Button
                                type="button"
                                onClick={handleAddField}
                                variant="outline"
                                size="sm"
                                className="rounded-xl flex items-center gap-1 border-border text-foreground hover:bg-muted font-bold transition-all hover:scale-[1.02]"
                                disabled={isSaving}
                            >
                                <Plus className="h-4 w-4" />
                                <span>إضافة حقل</span>
                            </Button>
                        </div>

                        {errors.fields?.root && (
                            <span className="text-xs text-destructive font-semibold">
                                {errors.fields.root.message}
                            </span>
                        )}

                        {fields.length === 0 ? (
                            <div className="py-12 border-2 border-dashed border-border rounded-2xl flex flex-col items-center justify-center text-muted-foreground gap-2">
                                <span className="text-xs">لم يتم إضافة أي حقول بعد. أضف حقلاً لبدء البناء.</span>
                            </div>
                        ) : (
                            <div 
                                ref={listRef}
                                className="flex flex-col gap-3 max-h-[40vh] overflow-y-auto pl-1"
                            >
                                {fields.map((field, idx) => (
                                    <div 
                                        key={field.id} 
                                        className="field-item-row flex flex-col sm:flex-row items-start sm:items-center gap-3 p-4 bg-muted/20 border border-border rounded-2xl relative group transition-colors hover:bg-muted/40"
                                    >
                                        {/* اسم الحقل */}
                                        <div className="flex-1 w-full flex flex-col gap-1">
                                            <Label className="text-[10px] font-bold text-muted-foreground sm:hidden">اسم الحقل</Label>
                                            <Input 
                                                {...register(`fields.${idx}.label`)}
                                                placeholder="مثال: تاريخ انتهاء العقد..."
                                                className={`rounded-lg h-9 bg-background ${errors.fields?.[idx]?.label ? 'border-destructive' : ''}`}
                                                disabled={isSaving}
                                            />
                                            {errors.fields?.[idx]?.label && (
                                                <span className="text-[10px] text-destructive font-semibold">
                                                    {errors.fields[idx].label.message}
                                                </span>
                                            )}
                                        </div>

                                        {/* نوع الحقل */}
                                        <div className="w-full sm:w-36 flex flex-col gap-1">
                                            <Label className="text-[10px] font-bold text-muted-foreground sm:hidden">نوع الحقل</Label>
                                            <select
                                                {...register(`fields.${idx}.type`)}
                                                className="w-full h-9 rounded-lg border border-input bg-background px-3 text-xs focus-visible:outline-none focus:ring-1 focus:ring-ring"
                                                disabled={isSaving}
                                            >
                                                <option value="text">نص قصير</option>
                                                <option value="textarea">نص طويل</option>
                                                <option value="number">رقم</option>
                                                <option value="date">تاريخ</option>
                                            </select>
                                        </div>

                                        {/* مطلوب */}
                                        <div className="flex items-center gap-2 self-center sm:self-auto shrink-0 mt-2 sm:mt-0">
                                            <span className="text-xs text-muted-foreground font-medium">مطلوب</span>
                                            <Controller
                                                control={control}
                                                name={`fields.${idx}.required`}
                                                render={({ field: { value, onChange } }) => (
                                                    <Switch 
                                                        checked={value}
                                                        onCheckedChange={onChange}
                                                        disabled={isSaving}
                                                    />
                                                )}
                                            />
                                        </div>

                                        {/* أدوات التحكم الإضافية */}
                                        <div className="flex items-center gap-1 sm:border-r border-border sm:pr-2 w-full sm:w-auto justify-end sm:justify-start mt-2 sm:mt-0">
                                            <button
                                                type="button"
                                                onClick={() => handleMoveField(idx, 'up')}
                                                disabled={idx === 0 || isSaving}
                                                className="p-1.5 rounded-lg hover:bg-muted text-muted-foreground disabled:opacity-30 transition-colors hover:text-foreground"
                                                title="نقل للأعلى"
                                            >
                                                <ArrowUp className="h-4 w-4" />
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => handleMoveField(idx, 'down')}
                                                disabled={idx === fields.length - 1 || isSaving}
                                                className="p-1.5 rounded-lg hover:bg-muted text-muted-foreground disabled:opacity-30 transition-colors hover:text-foreground"
                                                title="نقل للأسفل"
                                            >
                                                <ArrowDown className="h-4 w-4" />
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => handleRemoveField(idx)}
                                                disabled={isSaving}
                                                className="p-1.5 rounded-lg hover:bg-destructive/10 text-destructive transition-colors"
                                                title="حذف الحقل"
                                            >
                                                <Trash2 className="h-4 w-4" />
                                            </button>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* أزرار الإجراءات داخل الفورم لتفعيل submit */}
                    <div className="border-t border-border pt-4 flex justify-end gap-3 mt-4">
                        <Button
                            type="button"
                            variant="ghost"
                            onClick={onClose}
                            className="rounded-xl px-5 font-bold"
                            disabled={isSaving}
                        >
                            إلغاء
                        </Button>
                        <Button
                            type="submit"
                            className="rounded-xl px-8 font-bold shadow-lg shadow-primary/20 flex items-center gap-2"
                            disabled={isSaving}
                        >
                            <Save className="h-4 w-4" />
                            <span>{isSaving ? 'جاري الحفظ...' : 'حفظ النموذج'}</span>
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
};
