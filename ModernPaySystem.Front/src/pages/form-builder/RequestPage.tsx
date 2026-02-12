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
            <h1 className="text-3xl font-bold">تقديم طلب جديد</h1>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="md:col-span-1 space-y-4">
                    <Card className="p-4 space-y-4">
                        <div className="space-y-2">
                            <Label>اختر النموذج</Label>
                            <Select value={selectedTemplateId} onValueChange={setSelectedTemplateId}>
                                <SelectTrigger>
                                    <SelectValue placeholder="اختر نوع الطلب" />
                                </SelectTrigger>
                                <SelectContent>
                                    {templates.map(t => (
                                        <SelectItem key={t.id} value={t.id}>{t.title}</SelectItem>
                                    ))}
                                </SelectContent>
                            </Select>
                        </div>

                        {selectedTemplate && (
                            <div className="space-y-4 animate-in fade-in">
                                <div className="p-3 bg-muted rounded text-sm mb-4">
                                    <h4 className="font-bold">{selectedTemplate.title}</h4>
                                    <p className="opacity-70">{selectedTemplate.description}</p>
                                    <div className="mt-2 pt-2 border-t text-[10px] text-muted-foreground">
                                        المقدم: {currentUser?.username}
                                    </div>
                                </div>

                                <div className="space-y-2">
                                    <Label className="flex items-center gap-2">
                                        <ImagePlus className="w-4 h-4 text-primary" />
                                        إرفاق صور/ملفات
                                    </Label>
                                    <input
                                        type="file"
                                        multiple
                                        accept="image/*"
                                        className="hidden"
                                        ref={fileInputRef}
                                        onChange={handleFileChange}
                                    />
                                    <Button
                                        variant="outline"
                                        className="w-full border-dashed"
                                        onClick={() => fileInputRef.current?.click()}
                                    >
                                        اختر الملفات
                                    </Button>

                                    {files.length > 0 && (
                                        <div className="mt-4 space-y-2">
                                            {files.map((file, idx) => (
                                                <div key={idx} className="flex items-center justify-between p-2 bg-accent/30 rounded-lg text-xs group">
                                                    <div className="flex items-center gap-2 truncate">
                                                        <FileText className="w-3 h-3 text-muted-foreground" />
                                                        <span className="truncate max-w-[150px]">{file.name}</span>
                                                    </div>
                                                    <button
                                                        onClick={() => removeFile(idx)}
                                                        className="text-destructive hover:scale-110 transition-transform"
                                                    >
                                                        <X className="w-3 h-3" />
                                                    </button>
                                                </div>
                                            ))}
                                        </div>
                                    )}
                                </div>
                            </div>
                        )}
                    </Card>
                </div>

                <div className="md:col-span-2">
                    {selectedTemplate ? (
                        <Card className="p-6 animate-in slide-in-from-bottom-4">
                            <FormRenderer
                                schema={selectedTemplate}
                                onSubmit={handleSubmit}
                            />
                            {submitMutation.isPending && <p className="mt-2 text-primary">جاري الإرسال...</p>}
                        </Card>
                    ) : (
                        <div className="h-64 flex items-center justify-center border-2 border-dashed rounded-xl text-muted-foreground">
                            اختر نموذجاً للبدء
                        </div>
                    )}
                </div>
            </div>
        </AnimatedContainer>
    );
};
