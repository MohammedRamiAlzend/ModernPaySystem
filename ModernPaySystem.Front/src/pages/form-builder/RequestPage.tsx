import { useState, useMemo, useRef } from 'react';
import { useForms } from '@/features/form-builder/model/useForms';
import { formEndpoints } from '@/features/form-builder/api/formEndpoints';
import { FormRenderer } from '@/widgets/form-renderer/ui/FormRenderer';
import { Card } from '@/shared/ui/card';
import { Select, SelectTrigger, SelectValue, SelectContent, SelectItem } from '@/shared/ui/select';
import { Label } from '@/shared/ui/label';
import { useMutation } from '@tanstack/react-query';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import type { CreateRequestDto } from '@/entities/form/model/types';
import { useAppSelector } from '@/app/store';
import { selectCurrentUser } from '@/app/store/authSlice';
import { ImagePlus, X, FileText } from 'lucide-react';
import { Button } from '@/shared/ui/button';

export const RequestPage = () => {
    const { data: templates = [] } = useForms();
    const [selectedTemplateId, setSelectedTemplateId] = useState<string>('');
    const [files, setFiles] = useState<File[]>([]);
    const fileInputRef = useRef<HTMLInputElement>(null);
    const currentUser = useAppSelector(selectCurrentUser);

    const selectedTemplate = useMemo(() =>
        templates.find(t => t.id === selectedTemplateId),
        [templates, selectedTemplateId]
    );

    const submitMutation = useMutation({
        mutationFn: formEndpoints.createRequest,
        onSuccess: () => {
            alert('تم تقديم الطلب بنجاح');
            setSelectedTemplateId('');
            setFiles([]);
        },
        onError: () => {
            alert('حدث خطأ أثناء تقديم الطلب');
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

        const payload: CreateRequestDto = {
            TemplateId: selectedTemplate.id,
            RequesterId: currentUser.id,
            ApproverId: currentUser.id, // Using current user as approver as a default logic
            Content: JSON.stringify(formData),
            files: files
        };

        submitMutation.mutate(payload);
    };

    return (
        <AnimatedContainer className="container mx-auto p-6 space-y-6">
            <h1 className="text-3xl font-bold mb-6">تقديم طلب جديد</h1>

            {/* Top Bar: Template Selection & File Upload */}
            <Card className="p-4 flex flex-col md:flex-row items-center gap-4 bg-background/50 backdrop-blur-sm sticky top-4 z-10 shadow-md border-primary/10">
                <div className="flex-1 w-full md:w-auto">
                    <Label className="mb-2 block text-xs font-bold text-muted-foreground">اختر النموذج</Label>
                    <Select value={selectedTemplateId} onValueChange={setSelectedTemplateId}>
                        <SelectTrigger className="h-10">
                            <SelectValue placeholder="اختر نوع الطلب..." />
                        </SelectTrigger>
                        <SelectContent>
                            {templates.map(t => (
                                <SelectItem key={t.id} value={t.id}>{t.title}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </div>

                {selectedTemplate && (
                    <div className="flex-1 w-full md:w-auto flex items-end gap-2">
                        <div className="flex-1">
                            <Label className="mb-2 block text-xs font-bold text-muted-foreground">المرفقات والصور</Label>
                            <div className="flex gap-2">
                                <Button
                                    variant="outline"
                                    className="h-10 gap-2 border-dashed flex-1 hover:border-primary/50 hover:bg-primary/5 hover:text-primary transition-all"
                                    onClick={() => fileInputRef.current?.click()}
                                >
                                    <ImagePlus className="w-4 h-4" />
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
                            </div>
                        </div>
                    </div>
                )}
            </Card>

            {/* Main Content: Form Renderer */}
            <div className="flex justify-center">
                <div className="w-full max-w-4xl">
                    {selectedTemplate ? (
                        <Card className="p-8 shadow-xl border-t-4 border-t-primary animate-in zoom-in-95 duration-300  backdrop-blur-sm">
                            <div className="mb-8 text-center border-b pb-6">
                                <h2 className="text-2xl font-bold text-primary mb-2">{selectedTemplate.title}</h2>
                                <p className="text-muted-foreground">{selectedTemplate.description}</p>
                            </div>

                            <FormRenderer
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
                            <p className="text-lg font-medium">يرجى اختيار نموذج من الشريط العلوي للبدء</p>
                        </div>
                    )}
                </div>
            </div>

            {/* Bottom Section: File Previews */}
            {selectedTemplate && files.length > 0 && (
                <Card className="p-6 bg-muted/30 border-dashed animate-in slide-in-from-bottom-6">
                    <h3 className="font-bold mb-4 flex items-center gap-2 text-muted-foreground">
                        <ImagePlus className="w-4 h-4" />
                        الملفات المرفقة ({files.length})
                    </h3>
                    <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-6 gap-4">
                        {files.map((file, idx) => (
                            <div key={idx} className="relative group bg-background rounded-xl border p-3 flex flex-col gap-2 hover:shadow-md transition-all">
                                <div className="flex items-center justify-center bg-muted/50 rounded-lg h-24 overflow-hidden">
                                    {file.type.startsWith('image/') ? (
                                        <img
                                            src={URL.createObjectURL(file)}
                                            alt={file.name}
                                            className="w-full h-full object-cover opacity-80 group-hover:opacity-100 transition-opacity"
                                        />
                                    ) : (
                                        <FileText className="w-10 h-10 text-muted-foreground" />
                                    )}
                                </div>
                                <div className="flex items-center justify-between gap-2">
                                    <span className="text-[10px] font-mono truncate flex-1">{file.name}</span>
                                    <button
                                        onClick={() => removeFile(idx)}
                                        className="text-muted-foreground hover:text-destructive hover:bg-destructive/10 p-1 rounded-full transition-colors"
                                    >
                                        <X className="w-3 h-3" />
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                </Card>
            )}
        </AnimatedContainer>
    );
};
