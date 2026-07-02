import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Textarea } from '@/shared/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/ui/form';
import { DepartmentType } from '@/entities/department/model/types';
import { SearchableSelect, SearchableSelectOption } from '@/shared/ui/searchable-select';
import { UserPicker } from '@/features/users/ui/UserPicker';


const createDepartmentFormSchema = z.object({
    name: z.string().min(2, { message: 'الاسم يجب أن يكون حرفين على الأقل' }),
    code: z.string().optional(),
    description: z.string().optional(),
    parentDepartmentId: z.string().min(1, { message: 'يجب اختيار القسم الأب' }),
    headedUserId: z.string().min(1, { message: 'يجب اختيار رئيس القسم' }),
    type: z.nativeEnum(DepartmentType),
});

const editDepartmentFormSchema = z.object({
    name: z.string().min(2, { message: 'الاسم يجب أن يكون حرفين على الأقل' }),
    code: z.string().optional(),
    description: z.string().optional(),
    parentDepartmentId: z.string().min(1, { message: 'يجب اختيار القسم الأب' }),
    headedUserId: z.string().optional(),
    type: z.nativeEnum(DepartmentType),
});


type DepartmentFormValues = {
    name: string;
    code?: string;
    description?: string;
    parentDepartmentId: string;
    headedUserId?: string;
    type: DepartmentType;
};

interface DepartmentFormProps {
    onSubmit: (data: DepartmentFormValues) => void;
    initialData?: Partial<DepartmentFormValues>;
    parentOptions: SearchableSelectOption[];
    isLoading?: boolean;
    isParentDisabled?: boolean;
    mode?: 'create' | 'edit';
}

export const DepartmentForm: React.FC<DepartmentFormProps> = ({
    onSubmit,
    initialData,
    parentOptions,
    isLoading,
    isParentDisabled = false,
    mode = 'create'
}) => {
    const formSchema = mode === 'edit' ? editDepartmentFormSchema : createDepartmentFormSchema;
    const form = useForm<DepartmentFormValues>({
        resolver: zodResolver(formSchema) as any,
        defaultValues: {
            name: initialData?.name || '',
            code: initialData?.code || '',
            description: initialData?.description || '',
            parentDepartmentId: initialData?.parentDepartmentId || '',
            headedUserId: initialData?.headedUserId || '',
            type: initialData?.type || DepartmentType.Office,
        },
    });

    return (
        <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>اسم القسم</FormLabel>
                            <FormControl>
                                <Input placeholder="مثال: الديوان العام" {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="grid grid-cols-2 gap-4">
                    <FormField
                        control={form.control}
                        name="code"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>كود القسم (اختياري)</FormLabel>
                                <FormControl>
                                    <Input placeholder="D-101" {...field} />
                                </FormControl>
                                <FormMessage />
                            </FormItem>
                        )}
                    />

                    <FormField
                        control={form.control}
                        name="type"
                        render={({ field }) => (
                            <FormItem>
                                <FormLabel>نوع القسم</FormLabel>
                                <Select onValueChange={(val) => field.onChange(Number(val))} defaultValue={String(field.value)}>
                                    <FormControl>
                                        <SelectTrigger>
                                            <SelectValue placeholder="اختر نوع القسم" />
                                        </SelectTrigger>
                                    </FormControl>
                                    <SelectContent>
                                        <SelectItem value={String(DepartmentType.Country)}>دولة</SelectItem>
                                        <SelectItem value={String(DepartmentType.Governorate)}>محافظة</SelectItem>
                                        <SelectItem value={String(DepartmentType.District)}>منطقة</SelectItem>
                                        <SelectItem value={String(DepartmentType.Municipality)}>بلدية</SelectItem>
                                        <SelectItem value={String(DepartmentType.Office)}>مكتب</SelectItem>
                                        <SelectItem value={String(DepartmentType.Unit)}>وحدة إدارية</SelectItem>
                                    </SelectContent>
                                </Select>
                                <FormMessage />
                            </FormItem>
                        )}
                    />
                </div>

                <FormField
                    control={form.control}
                    name="headedUserId"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>رئيس القسم</FormLabel>
                            <FormControl>
                                <UserPicker
                                    onUserSelect={field.onChange}
                                    defaultValue={field.value}
                                    label="اختر رئيس القسم"
                                    className="!grid-cols-1"
                                    showCurrentUser={true}
                                    allowCreateUser={mode === 'edit'}
                                    isCreatingDepartmentHead={mode === 'edit'}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="parentDepartmentId"

                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>القسم الأب</FormLabel>
                            <FormControl>
                                <SearchableSelect
                                    options={parentOptions}
                                    value={field.value}
                                    onValueChange={field.onChange}
                                    placeholder="اختر القسم الأب..."
                                    disabled={isParentDisabled}
                                />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <FormField
                    control={form.control}
                    name="description"
                    render={({ field }) => (
                        <FormItem>
                            <FormLabel>وصف القسم</FormLabel>
                            <FormControl>
                                <Textarea placeholder="وصف موجز لمهام القسم..." {...field} />
                            </FormControl>
                            <FormMessage />
                        </FormItem>
                    )}
                />

                <div className="flex justify-end gap-3 pt-4">
                    <Button type="submit" disabled={isLoading}>
                        {isLoading ? 'جاري الحفظ...' : mode === 'edit' ? 'تحديث القسم' : 'حفظ القسم'}
                    </Button>
                </div>
            </form>
        </Form>
    );
};
