import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/ui/form';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { User, SubSystem } from '../api/usersApi';
import { APP_CONFIG } from '@/shared/config/appConfig';
import { SearchableSelect } from '@/shared/ui/searchable-select';
import { Switch } from '@/shared/ui/switch';

const userFormSchema = z.object({
    userName: z.string().min(APP_CONFIG.IS_DEV ? 1 : 3, {
        message: `اسم المستخدم يجب أن يكون ${APP_CONFIG.IS_DEV ? 1 : 3} أحرف على الأقل`
    }),
    password: z.string().min(APP_CONFIG.IS_DEV ? 1 : 6, {
        message: `كلمة المرور يجب أن تكون ${APP_CONFIG.IS_DEV ? 1 : 6} أحرف على الأقل`
    }).optional().or(z.literal('')),
    subSystem: z.string().min(1, { message: 'يرجى اختيار النظام الفرعي' }),
    departmentId: z.string().optional().nullable(),
    isArchiveLeader: z.boolean().optional(),
});

export type UserFormValues = z.infer<typeof userFormSchema>;

interface UserFormProps {
    onSubmit: (data: UserFormValues) => void;
    initialData?: User | null;
    subSystems: SubSystem[];
    currentUserSubsystem?: number | null;
    isLoading?: boolean;
    departmentOptions: { value: string; label: string }[];
}

export const UserForm: React.FC<UserFormProps> = ({
    onSubmit,
    initialData,
    subSystems,
    currentUserSubsystem,
    isLoading,
    departmentOptions
}) => {
    const form = useForm<UserFormValues>({
        resolver: zodResolver(userFormSchema),
        defaultValues: {
            userName: '',
            password: '',
            subSystem: currentUserSubsystem?.toString() || APP_CONFIG.DEFAULT_SUB_SYSTEM_ID,
            departmentId: '',
            isArchiveLeader: false,
        },
    });

    // تحديث قيم النموذج عند تغيير البيانات الأولية (عند الضغط على تعديل مستخدم مختلف)
    React.useEffect(() => {
        if (initialData) {
            form.reset({
                userName: initialData.userName,
                password: '',
                subSystem: initialData.subSystem?.toString() || currentUserSubsystem?.toString() || APP_CONFIG.DEFAULT_SUB_SYSTEM_ID,
                departmentId: initialData.departmentId || '',
                isArchiveLeader: initialData.isArchiveLeader || false,
            });
        } else {
            form.reset({
                userName: '',
                password: '',
                subSystem: currentUserSubsystem?.toString() || APP_CONFIG.DEFAULT_SUB_SYSTEM_ID,
                departmentId: '',
                isArchiveLeader: false,
            });
        }
    }, [initialData, form, currentUserSubsystem]);

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="userName"
                    render={({ field }) => (
                        <FormItem className="text-right">
                            <FormLabel>اسم المستخدم</FormLabel>
                            <FormControl>
                                <Input placeholder="أدخل اسم المستخدم" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="password"
                    render={({ field }) => (
                        <FormItem className="text-right">
                            <FormLabel>كلمة المرور {initialData && '(اتركه فارغاً إذا كنت لا تريد تغييره)'}</FormLabel>
                            <FormControl>
                                <Input type="password" placeholder="أدخل كلمة المرور" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                {!currentUserSubsystem && APP_CONFIG.SHOW_SUB_SYSTEM && (
                    <FormField
                        control={form.control}
                        name="subSystem"
                        render={({ field }) => (
                            <FormItem className="text-right">
                                <FormLabel>النظام الفرعي</FormLabel>
                                <Select onValueChange={field.onChange} value={field.value}>
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue placeholder="اختر النظام" />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        {subSystems.map(ss => (
                                            <SelectItem key={ss.value} value={ss.value}>{ss.name}</SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                )}

                <FormField
                    control={form.control}
                    name="departmentId"
                    render={({ field }) => (
                        <FormItem className="flex flex-col gap-1 text-right">
                            <FormLabel>القسم</FormLabel>
                            <FormControl>
                                <SearchableSelect
                                    options={departmentOptions}
                                    value={field.value || ''}
                                    onValueChange={field.onChange}
                                    placeholder="اختر القسم"
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="isArchiveLeader"
                    render={({ field }) => (
                        <FormItem className="flex items-center justify-between p-3 rounded-xl border border-border bg-muted/10 text-right">
                            <div className="flex flex-col gap-0.5">
                                <FormLabel className="font-bold">مدير الأرشيف</FormLabel>
                                <span className="text-[10px] text-muted-foreground font-semibold">
                                    تحديد ما إذا كان هذا المستخدم مدير أرشيف للقسم المختار.
                                </span>
                            </div>
                            <FormControl>
                                <Switch
                                    checked={field.value}
                                    onCheckedChange={field.onChange}
                                />
                            </FormControl>
                        </FormItem>
                    )}
                />

                <div className="flex justify-end gap-3 pt-4">
                    <Button type="submit" disabled={isLoading} className="rounded-xl px-8">
                        {isLoading ? 'جاري الحفظ...' : 'حفظ البيانات'}
                    </Button>
                </div>
            </form>
        </Form>
    );
};
