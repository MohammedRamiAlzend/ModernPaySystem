import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useArchiveConfig, useLedDepartments } from '@/features/archiving/model/queries';
import { useUpdateArchiveConfig } from '@/features/archiving/model/mutations';
import { Form, FormField, FormItem, FormLabel, FormControl, FormMessage, FormDescription } from '@/shared/ui/form';
import { Input } from '@/shared/ui/input';
import { Textarea } from '@/shared/ui/textarea';
// import { Switch } from '@/shared/ui/switch';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { LoadingSpinner } from '@/shared/ui/common/loading-spinner';
import { Alert, AlertDescription, AlertTitle } from '@/shared/ui/alert';
import { AlertCircle, Loader2, Save, Settings2 } from 'lucide-react';
// import { AlertCircle, Loader2, Save, Settings2, FolderOpen } from 'lucide-react';
import { FolderPickerModal } from '@/features/archiving/ui/FolderPickerModal';
import { TagInput } from '@/shared/ui/tag-input';


const formSchema = z.object({
    defaultPath: z.string().min(1, 'حقل المسار الافتراضي مطلوب'),
    description: z.string().optional(),
    isActive: z.boolean(),
    allowedFileExtensions: z.array(z.string()).optional(),
});

type FormValues = z.infer<typeof formSchema>;

export const ArchiveConfigSettings = () => {
    const { data: config, isLoading: configLoading, isError } = useArchiveConfig();
    const { data: ledDepartments = [] } = useLedDepartments();
    const updateMutation = useUpdateArchiveConfig();

    const isArchiveLeader = ledDepartments.length > 0;
    const [isFolderPickerOpen, setIsFolderPickerOpen] = useState(false);

    const form = useForm<FormValues>({
        resolver: zodResolver(formSchema),
        values: {
            defaultPath: config?.defaultPath ?? '',
            description: config?.description ?? '',
            isActive: config?.isActive ?? true,
            allowedFileExtensions: config?.allowedFileExtensions
                ? config.allowedFileExtensions.split(',').map(s => s.trim()).filter(Boolean)
                : [],
        },
    });

    const onSubmit = (values: FormValues) => {
        updateMutation.mutate({
            defaultPath: values.defaultPath,
            description: values.description || null,
            isActive: values.isActive,
            allowedFileExtensions: values.allowedFileExtensions?.join(',') || null,
        });
    };

    if (configLoading) {
        return (
            <div className="flex items-center justify-center p-20">
                <LoadingSpinner />
            </div>
        );
    }

    if (isError) {
        return (
            <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertTitle>خطأ</AlertTitle>
                <AlertDescription>فشل تحميل إعدادات نظام الأرشفة</AlertDescription>
            </Alert>
        );
    }

    return (
        <Card className="max-w-3xl border-none shadow-lg bg-card/50 backdrop-blur-sm">
            <CardHeader>
                <div className="flex items-center gap-2 mb-1">
                    <Settings2 className="w-5 h-5 text-primary" />
                    <CardTitle>إعدادات نظام الأرشفة</CardTitle>
                </div>
                <CardDescription>
                    إعدادات التخزين والمسار الافتراضي لنظام الأرشفة
                    {!isArchiveLeader && (
                        <span className="block mt-1 text-xs text-amber-500">
                            فقط قادة الأرشيف يمكنهم تعديل هذه الإعدادات
                        </span>
                    )}
                </CardDescription>
            </CardHeader>
            <CardContent>
                <Form {...form}>
                    <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                        <FormField
                            control={form.control}
                            name="defaultPath"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>المسار الافتراضي للتخزين</FormLabel>
                                    <div className="flex gap-2">
                                        <FormControl>
                                            <Input
                                                {...field}
                                                placeholder="Uploads"
                                                disabled={!isArchiveLeader}
                                                className="font-mono text-xs"
                                            // dir="ltr"
                                            />
                                        </FormControl>
                                        {/* {isArchiveLeader && (
                                            <Button
                                                type="button"
                                                variant="outline"
                                                onClick={() => setIsFolderPickerOpen(true)}
                                                className="shrink-0 flex items-center gap-1.5 rounded-xl"
                                            >
                                                <FolderOpen className="w-4 h-4" />
                                                <span>استعراض...</span>
                                            </Button>
                                        )} */}
                                    </div>
                                    <FormMessage />
                                    <FolderPickerModal
                                        isOpen={isFolderPickerOpen}
                                        onClose={() => setIsFolderPickerOpen(false)}
                                        onSelect={(selectedPath) => form.setValue('defaultPath', selectedPath)}
                                        initialPath={field.value}
                                    />
                                </FormItem>
                            )}
                        />

                        <FormField
                            control={form.control}
                            name="description"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>الوصف</FormLabel>
                                    <FormControl>
                                        <Textarea
                                            {...field}
                                            placeholder="وصف إعدادات الأرشفة"
                                            disabled={!isArchiveLeader}
                                        />
                                    </FormControl>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        <FormField
                            control={form.control}
                            name="allowedFileExtensions"
                            render={({ field }) => (
                                <FormItem>
                                    <FormLabel>اللواحق المسموح بها للملفات</FormLabel>
                                    <FormControl>
                                        <TagInput
                                            value={field.value ?? []}
                                            onChange={field.onChange}
                                            disabled={!isArchiveLeader}
                                            placeholder="مثال: pdf, docx, jpg"
                                        />
                                    </FormControl>
                                    <FormDescription className="text-xs">
                                        حدد أنواع الملفات المسموح برفعها (مثال: .pdf, .docx, .jpg)
                                    </FormDescription>
                                    <FormMessage />
                                </FormItem>
                            )}
                        />

                        {/* <FormField
                            control={form.control}
                            name="isActive"
                            render={({ field }) => (
                                <FormItem>
                                    <div className="flex items-center justify-between p-4 bg-muted/20 rounded-2xl">
                                        <div className="space-y-1">
                                            <FormLabel className="text-base font-bold">تفعيل النظام</FormLabel>
                                            <p className="text-sm text-muted-foreground">
                                                تفعيل أو إيقاف نظام الأرشفة
                                            </p>
                                        </div>
                                        <FormControl>
                                            <Switch
                                                checked={field.value}
                                                onCheckedChange={field.onChange}
                                                disabled={!isArchiveLeader}
                                            />
                                        </FormControl>
                                    </div>
                                    <FormMessage />
                                </FormItem>
                            )}
                        /> */}

                        {isArchiveLeader && (
                            <Button type="submit" disabled={updateMutation.isPending}>
                                {updateMutation.isPending ? (
                                    <>
                                        <Loader2 className="ml-2 h-4 w-4 animate-spin" />
                                        جاري الحفظ...
                                    </>
                                ) : (
                                    <>
                                        <Save className="ml-2 h-4 w-4" />
                                        حفظ الإعدادات
                                    </>
                                )}
                            </Button>
                        )}
                    </form>
                </Form>
            </CardContent>
        </Card>
    );
};

export default ArchiveConfigSettings;
