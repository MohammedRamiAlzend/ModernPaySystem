import { useState } from 'react';
import { Card } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { FormRenderer } from '@/widgets/form-renderer/ui/FormRenderer';
import { useDelphiTransaction } from '@/features/delphi-integration/model/useDelphiTransaction';
import { useCreateRequest } from '@/features/form-builder/api/formEndpoints';
import { useUIStore } from '@/app/store/uiStore';
import { Loader2, Monitor, AlertCircle, RefreshCcw } from 'lucide-react';
import { useAuthStore } from '@/app/store/authStore';
import { RequestSubmissionSidebar } from '@/features/form-builder/ui/RequestSubmissionSidebar';
import type { CreateRequestDto } from '@/entities/form/model/types';

/**
 * DelphiTransactionPage: Handles incoming technical data from Delphi desktop app.
 * Normalizes it, merges services, and maps it to a fixed server template.
 */
export const DelphiTransactionPage = () => {
    const showStatus = useUIStore(state => state.showStatus);
    const { user } = useAuthStore();
    const [rawInput, setRawInput] = useState("");
    const [receiverDepartmentId, setReceiverDepartmentId] = useState<string>("");
    const [readOnlyUsers, setReadOnlyUsers] = useState<string[]>([]);
    const [files, setFiles] = useState<File[]>([]);

    const { mutateAsync: createRequest, isPending: isSubmitting } = useCreateRequest();

    const { template, processed, isLoading, error } = useDelphiTransaction(rawInput);

    // Mock data function for testing
    const loadMockData = () => {
        const mock = {
            "id": "م/777",
            "client": "  مازن العلي  ",
            "clientas": "مالك العقار",
            "action_date": "2024/05/15",
            "id_reality": 778,
            "region": "المنطقة الأولى - دمشق",
            "sum_amount": 1000,
            "list_services": [
                { "id_service": 101, "num": 5 },
                { "id_service": 101, "num": 2 },
                { "id_service": 105, "num": 1 }
            ]
        };
        setRawInput(JSON.stringify(mock, null, 2));
    };

    // File handlers removed, now in RequestSubmissionSidebar component.

    const handleSubmit = async (formData: Record<string, any>) => {
        if (!template?.id || !user?.id) return;

        if (!receiverDepartmentId) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: 'يرجى اختيار القسم المستلم أولاً'
            });
            return;
        }

        try {
            const payload: CreateRequestDto = {
                TemplateId: template.id,
                DepartmentId: receiverDepartmentId,
                ReadOnlyUsers: readOnlyUsers,
                Content: Object.entries(formData).map(([key, value]) => ({
                    key,
                    value: String(value)
                })),
                files: files
            };

            await createRequest(payload);

            showStatus({
                type: 'success',
                title: 'تمت العملية',
                message: 'تم استلام ومعالجة بيانات.'
            });

            setRawInput(""); // Clear for next one
            setFiles([]);    // Clear files
            setReadOnlyUsers([]); // Clear read only users
        } catch {
            showStatus({
                type: 'error',
                title: 'فشل الإرسال',
                message: 'حدث خطأ أثناء محاولة حفظ المعاملة في النظام.'
            });
        }
    };

    return (
        <div className="container py-8 max-w-[1600px] mx-auto space-y-8 animate-in fade-in duration-500">
            {/* Header Section */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 p-8 bg-gradient-to-r from-primary/10 via-primary/5 to-transparent rounded-3xl border border-primary/10 shadow-sm relative overflow-hidden">
                <div className="flex items-center gap-5 z-10">
                    <div className="p-4 bg-primary text-white rounded-2xl shadow-xl shadow-primary/20">
                        <Monitor className="w-8 h-8" />
                    </div>
                    <div>
                        <h1 className="text-3xl font-extrabold tracking-tight">نظام الربط مع المصالح العقارية</h1>
                        <p className="text-muted-foreground mt-2 max-w-lg">
                            نظام تكامل البيانات مع تطبيق سطح المكتب. يقوم تلقائياً بتنظيف البيانات ودمج مكررات الخدمات.
                        </p>
                    </div>
                </div>
                <div className="flex gap-2">
                    <Button variant="outline" size="sm" onClick={loadMockData} className="gap-2 bg-background/50 backdrop-blur-sm">
                        <RefreshCcw className="w-4 h-4" /> تحميل بيانات اختبار
                    </Button>
                </div>
                {/* Background Decor */}
                <div className="absolute top-0 right-0 w-64 h-64 bg-primary/5 rounded-full blur-3xl -mr-32 -mt-32" />
            </div>

            {/* Main Application Area */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
                <SourcePanel
                    rawInput={rawInput}
                    setRawInput={setRawInput}
                    error={error}
                />

                <ResultPanel
                    isLoading={isLoading}
                    isSubmitting={isSubmitting}
                    template={template}
                    processed={processed}
                    rawInput={rawInput}
                    onSubmit={handleSubmit}
                />

                <RequestSubmissionSidebar
                    departmentId={receiverDepartmentId}
                    onDepartmentSelect={setReceiverDepartmentId}
                    readOnlyUsers={readOnlyUsers}
                    onReadOnlyUsersChange={setReadOnlyUsers}
                    files={files}
                    onFilesChange={setFiles}
                    className="lg:col-span-3 h-full"
                />
            </div>
        </div>
    );
};

// --- Internal Helper Components ---

const SourcePanel = ({ rawInput, setRawInput, error }: { rawInput: string, setRawInput: (v: string) => void, error: string | null | undefined }) => (
    <Card className="lg:col-span-3 p-6 space-y-4 border-primary/10 bg-muted/20 shadow-lg">
        <div className="flex items-center justify-between border-b pb-4">
            <h2 className="font-bold text-lg flex items-center gap-2">
                <RefreshCcw className="w-5 h-5 text-primary" /> مصدر البيانات
            </h2>
        </div>
        <textarea
            value={rawInput}
            onChange={(e) => setRawInput(e.target.value)}
            placeholder="الصق بيانات JSON هنا..."
            className="w-full h-[400px] p-4 text-xs font-mono bg-background border-2 rounded-2xl focus:ring-4 focus:ring-primary/10 focus:border-primary transition-all outline-none resize-none custom-scrollbar shadow-inner"
        />
        {error && (
            <div className="flex items-start gap-3 p-4 bg-red-50 text-red-700 rounded-xl border border-red-200 text-sm animate-in slide-in-from-top-2">
                <AlertCircle className="w-5 h-5 mt-0.5 shrink-0" />
                <p className="line-clamp-3">{error}</p>
            </div>
        )}
    </Card>
);

const ResultPanel = ({ isLoading, isSubmitting, template, processed, rawInput, onSubmit }: {
    isLoading: boolean,
    isSubmitting: boolean,
    template: any,
    processed: any,
    rawInput: string,
    onSubmit: (data: any) => void
}) => (
    <Card className="lg:col-span-6 p-6 border-primary/20 shadow-2xl relative min-h-[600px] overflow-hidden">
        {isLoading ? (
            <LoadingState />
        ) : template && processed?.status === 'success' ? (
            <div className="space-y-6 animate-in slide-in-from-left-4 duration-500">
                <div className="flex items-center gap-2 pb-2 mb-4 border-b">
                    <div className="w-2 h-8 bg-primary rounded-full" />
                    <h2 className="text-xl font-bold">بيانات المعاملة</h2>
                </div>
                <FormRenderer
                    key={`${template.id}-${processed.data?.id ?? ''}`}
                    schema={template}
                    onSubmit={onSubmit}
                    initialData={processed.data}
                />
                {isSubmitting && <SubmittingOverlay />}
            </div>
        ) : (rawInput.trim() !== "" && processed?.status === 'error') ? (
            <ErrorState />
        ) : (
            <EmptyState />
        )}
    </Card>
);

const LoadingState = () => (
    <div className="absolute inset-0 bg-background/60 backdrop-blur-md flex flex-col items-center justify-center z-50">
        <div className="flex flex-col items-center gap-4">
            <Loader2 className="w-12 h-12 animate-spin text-primary" />
            <div className="text-center">
                <p className="font-bold text-xl">جاري مزامنة النموذج...</p>
                <p className="text-muted-foreground text-sm mt-1 italic">قم بتحديث الصفحة للمزامنة مع السيرفر</p>
            </div>
        </div>
    </div>
);

const SubmittingOverlay = () => (
    <div className="mt-6 flex items-center justify-center gap-2 text-primary font-bold bg-primary/10 p-4 rounded-xl">
        <Loader2 className="w-5 h-5 animate-spin" />
        جاري تقديم المعاملة...
    </div>
);

const ErrorState = () => (
    <div className="flex flex-col items-center justify-center h-[500px] text-muted-foreground opacity-50 space-y-4">
        <AlertCircle className="w-20 h-20" />
        <div className="text-center">
            <p className="font-bold text-lg">خطأ في التحقق من البيانات</p>
            <p className="text-sm">يرجى مراجعة هيكلية JSON المرفقة.</p>
        </div>
    </div>
);

const EmptyState = () => (
    <div className="flex flex-col items-center justify-center h-[500px] text-muted-foreground opacity-30 space-y-4">
        <Monitor className="w-20 h-20" />
        <div className="text-center">
            <p className="font-bold text-lg">بانتظار استلام البيانات</p>
            <p className="text-sm">الصق البيانات لبدء المعالجة.</p>
        </div>
    </div>
);
