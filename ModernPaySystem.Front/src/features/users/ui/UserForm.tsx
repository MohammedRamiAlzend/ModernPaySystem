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

const userFormSchema = z.object({
    userName: z.string().min(3, { message: 'اسم المستخدم يجب أن يكون 3 أحرف على الأقل' }),
    password: z.string().min(6, { message: 'كلمة المرور يجب أن تكون 6 أحرف على الأقل' }).optional().or(z.literal('')),
    subSystem: z.string().min(1, { message: 'يرجى اختيار النظام الفرعي' }),
});

export type UserFormValues = z.infer<typeof userFormSchema>;

interface UserFormProps {
    onSubmit: (data: UserFormValues) => void;
    initialData?: User | null;
    subSystems: SubSystem[];
    currentUserSubsystem?: number | null;
    isLoading?: boolean;
}

export const UserForm: React.FC<UserFormProps> = ({
    onSubmit,
    initialData,
    subSystems,
    currentUserSubsystem,
    isLoading
}) => {
    const form = useForm<UserFormValues>({
        resolver: zodResolver(userFormSchema),
        defaultValues: {
            userName: initialData?.userName || '',
            password: '',
            subSystem: initialData?.subSystem?.toString() || currentUserSubsystem?.toString() || APP_CONFIG.DEFAULT_SUB_SYSTEM_ID,
        },
    });

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="userName"
                    render={({ field }) => (
                        <FormItem>
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
                        <FormItem>
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
                            <FormItem>
                                <FormLabel>النظام الفرعي</FormLabel>
                                <Select onValueChange={field.onChange} defaultValue={field.value}>
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

                <div className="flex justify-end gap-3 pt-4">
                    <Button type="submit" disabled={isLoading} className="rounded-xl px-8">
                        {isLoading ? 'جاري الحفظ...' : 'حفظ البيانات'}
                    </Button>
                </div>
            </form>
        </Form>
    );
};
