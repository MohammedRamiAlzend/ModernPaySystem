import { useState, useMemo, useRef, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useForms } from '@/features/form-builder/model/useForms';
import { formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { FormRenderer } from '@/widgets/form-renderer/ui/FormRenderer';
import { Card } from '@/shared/ui/card';
import { useMutation } from '@tanstack/react-query';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import type { CreateRequestDto } from '@/entities/form/model/types';
import { useAuthStore } from '@/app/store/authStore';
import { useUIStore } from '@/app/store/uiStore';
import { ImagePlus, X, FileText, Shield } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { UserPicker } from '@/features/users/ui/UserPicker';

export const RequestPage = () => {
    const { showStatus } = useUIStore();
    const [searchParams, setSearchParams] = useSearchParams();
    const { data: templates = [] } = useForms();

    const [formKey, setFormKey] = useState(0);

    // Initialize from URL or empty
    const paramTemplateId = searchParams.get('templateId') || '';
    const [selectedTemplateId, setSelectedTemplateId] = useState<string>(paramTemplateId);
    const [approverId, setApproverId] = useState<string>('');

    // Sync URL when local state changes
    useEffect(() => {
        if (selectedTemplateId) {
            setSearchParams(prev => {
                prev.set('templateId', selectedTemplateId);
                return prev;
            });
        }
    }, [selectedTemplateId, setSearchParams]);

    // Also update local state if URL changes externally (e.g. navigation)
    useEffect(() => {
        if (paramTemplateId && paramTemplateId !== selectedTemplateId) {
            setSelectedTemplateId(paramTemplateId);
        }
    }, [paramTemplateId]);

    const [files, setFiles] = useState<File[]>([]);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const currentUser = useAuthStore((state) => state.user);

    const selectedTemplate = useMemo(() =>
        templates.find(t => t.id === selectedTemplateId),
        [templates, selectedTemplateId]
    );

    const submitMutation = useMutation({
        mutationFn: formEndpoints.createRequest,
        onSuccess: () => {
            showStatus({
                type: 'success',
                title: 'تمت العملية',
                message: 'تم تقديم الطلب بنجاح'
            });
            // لإعادة تعيين الفورم، نقوم بتغيير الـ Key الخاص بالمكون
            setFormKey(prev => prev + 1);
            setFiles([]);
            // setApproverId('');
            // لا نحذف selectedTemplateId كي يبقى الفورم ظاهراً
        },
        onError: () => {
            showStatus({
                type: 'error',
                title: 'خطأ',
                message: 'حدث خطأ أثناء تقديم الطلب'
            });
        }
    });

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files);
            setFiles(prev => [...prev, ...newFiles]);
        }
    };

    const removeFile = (index: number) => {
        setFiles(prev => prev.filter((_, i) => i !== index));
    };

    const handleSubmit = async (formData: any) => {
        if (!selectedTemplate || !currentUser) return;

        if (!approverId) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'يرجى اختيار المرسل اليه أولاً'
            });
            return;
        }

        const payload: CreateRequestDto = {
            TemplateId: selectedTemplate.id,
            RequesterId: currentUser.id,
            ApproverId: approverId,
            Content: JSON.stringify(formData),
            files: files
        };

        submitMutation.mutate(payload);
    };

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6">
            <h1 className="text-3xl font-bold mb-6">تقديم طلب جديد</h1>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
                <div className="lg:col-span-2">
                    {/* Main Content: Form Renderer */}
                    {selectedTemplate ? (
                        <Card className="p-8 shadow-xl border-t-4 border-t-primary animate-in zoom-in-95 duration-300 backdrop-blur-sm">
                            <div className="mb-8 text-center border-b pb-6">
                                <h2 className="text-2xl font-bold text-primary mb-2">{selectedTemplate.title}</h2>
                                <p className="text-muted-foreground">{selectedTemplate.description}</p>
                            </div>

                            <FormRenderer
                                key={`${selectedTemplate.id}-${formKey}`}
                                schema={selectedTemplate}
                                onSubmit={handleSubmit}
                            />

                            {submitMutation.isPending && (
                                <div className="mt-6 flex items-center justify-center gap-2 text-primary font-bold bg-primary/10 p-4 rounded-xl">
                                    <div className="w-5 h-5 border-2 border-current border-t-transparent rounded-full animate-spin" />
                                    جاري إرسال الطلب...
                                </div>
                            )}
                        </Card>
                    ) : (
                        <div className="h-96 flex flex-col items-center justify-center border-2 border-dashed border-muted-foreground/20 rounded-3xl bg-muted/5 text-muted-foreground gap-4">
                            <FileText className="w-16 h-16 opacity-20" />
                            <p className="text-lg font-medium">يرجى اختيار نموذج من القائمة للبدء</p>
                        </div>
                    )}
                </div>

                <div className="space-y-6 sticky top-6">
                    {/* Approver Selection */}
                    <Card className="p-6 shadow-lg bg-card/50 backdrop-blur-sm border-primary/5">
                        <h3 className="font-bold mb-4 flex items-center gap-2">
                            <Shield className="w-4 h-4 text-primary" />
                            إعدادات الطلب
                        </h3>
                        <UserPicker
                            onUserSelect={setApproverId}
                            className="!grid-cols-1"
                            label="مستلم الطلب (المرسل إليه) "
                        />
                    </Card>

                    {/* File Upload */}
                    {selectedTemplate && (
                        <Card className="p-6 shadow-lg bg-card/50 backdrop-blur-sm border-primary/5">
                            <h3 className="font-bold mb-4 flex items-center gap-2">
                                <ImagePlus className="w-4 h-4 text-primary" />
                                المرفقات
                            </h3>
                            <div className="space-y-4">
                                <Button
                                    variant="outline"
                                    className="w-full h-12 gap-2 border-dashed border-primary/20 hover:border-primary/50 hover:bg-primary/5 hover:text-primary transition-all rounded-xl"
                                    onClick={() => fileInputRef.current?.click()}
                                >
                                    <ImagePlus className="w-5 h-5" />
                                    <span>إرفاق ملفات ({files.length})</span>
                                </Button>
                                <input
                                    type="file"
                                    multiple
                                    accept="image/*,.pdf,.doc,.docx"
                                    className="hidden"
                                    ref={fileInputRef}
                                    onChange={handleFileChange}
                                />

                                {files.length > 0 && (
                                    <div className="space-y-2 mt-4">
                                        {files.map((file, idx) => (
                                            <div key={idx} className="flex items-center justify-between p-2 rounded-lg bg-muted/50 border text-xs">
                                                <div className="flex items-center gap-2 truncate">
                                                    {file.type.startsWith('image/') ? <ImagePlus className="w-3 h-3" /> : <FileText className="w-3 h-3" />}
                                                    <span className="truncate">{file.name}</span>
                                                </div>
                                                <button
                                                    onClick={(e) => { e.stopPropagation(); removeFile(idx); }}
                                                    className="text-muted-foreground hover:text-destructive transition-colors"
                                                >
                                                    <X className="w-3 h-3" />
                                                </button>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        </Card>
                    )}
                </div>
            </div>

            {/* Bottom Section: File Previews (Legacy UI, kept if needed but redundant with sidebar list) */}
            {selectedTemplate && files.length > 0 && (
                <Card className="p-6 bg-muted/30 border-dashed animate-in slide-in-from-bottom-6">
                    <h3 className="font-bold mb-4 flex items-center gap-2 text-muted-foreground">
                        <ImagePlus className="w-4 h-4" />
                        معاينة الصور المرفقة
                    </h3>
                    <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
                        {files.filter(f => f.type.startsWith('image/')).map((file, idx) => (
                            <div key={idx} className="relative group bg-background rounded-xl border p-3 flex flex-col gap-2 hover:shadow-md transition-all">
                                <div className="flex items-center justify-center bg-muted/50 rounded-lg h-24 overflow-hidden">
                                    <img
                                        src={URL.createObjectURL(file)}
                                        alt={file.name}
                                        className="w-full h-full object-cover opacity-80 group-hover:opacity-100 transition-opacity"
                                    />
                                </div>
                                <div className="flex items-center justify-between gap-2">
                                    <span className="text-[10px] font-mono truncate flex-1">{file.name}</span>
                                </div>
                            </div>
                        ))}
                    </div>
                </Card>
            )}
        </AnimatedContainer>
    );
};
