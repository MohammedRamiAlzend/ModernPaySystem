import { useState, useMemo } from 'react';
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
import { FileText, Printer, Download } from 'lucide-react';
import { RequestSubmissionSidebar } from '@/features/form-builder/ui/RequestSubmissionSidebar';
import { printImage, downloadImage } from '@/shared/utils/image-actions';

/**
 * RequestPage: Handles the submission of a new form request.
 * Uses templateId from URL to load the correct schema.
 */
export const RequestPage = () => {
    const { showStatus } = useUIStore();
    const [searchParams] = useSearchParams();
    const { data: templates = [] } = useForms();
    const [formKey, setFormKey] = useState(0);

    // Get templateId directly from URL (Single source of truth to avoid infinite loops)
    const selectedTemplateId = searchParams.get('templateId') || '';
    const [approverId, setApproverId] = useState<string>('');
    const [readOnlyUsers, setReadOnlyUsers] = useState<string[]>([]);
    const [files, setFiles] = useState<File[]>([]);
    
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
            // Reset form by changing key
            setFormKey(prev => prev + 1);
            setFiles([]);
            setReadOnlyUsers([]);
        },
        onError: () => {
            showStatus({
                type: 'error',
                title: 'خطأ',
                message: 'حدث خطأ أثناء تقديم الطلب'
            });
        }
    });

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
            ReadOnlyUsers: readOnlyUsers,
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

                <RequestSubmissionSidebar
                    approverId={approverId}
                    onApproverSelect={setApproverId}
                    readOnlyUsers={readOnlyUsers}
                    onReadOnlyUsersChange={setReadOnlyUsers}
                    files={files}
                    onFilesChange={setFiles}
                    showFiles={!!selectedTemplate}
                    approverLabel="مستلم الطلب (المرسل إليه)"
                />
            </div>

            {/* Bottom Section: File Previews */}
            {selectedTemplate && files.length > 0 && (
                <Card className="p-6 bg-muted/30 border-dashed animate-in slide-in-from-bottom-6 mt-8">
                    <h3 className="font-bold mb-4 flex items-center gap-2 text-muted-foreground">
                        <FileText className="w-4 h-4" />
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
                                    <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-2">
                                        <button 
                                            onClick={() => printImage(URL.createObjectURL(file))}
                                            className="p-1.5 bg-white/20 backdrop-blur-md rounded-lg text-white hover:bg-white/40 transition-all hover:scale-110"
                                            title="طباعة"
                                        >
                                            <Printer className="w-4 h-4" />
                                        </button>
                                        <button 
                                            onClick={() => downloadImage(URL.createObjectURL(file), file.name)}
                                            className="p-1.5 bg-white/20 backdrop-blur-md rounded-lg text-white hover:bg-white/40 transition-all hover:scale-110"
                                            title="تنزيل"
                                        >
                                            <Download className="w-4 h-4" />
                                        </button>
                                    </div>
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
