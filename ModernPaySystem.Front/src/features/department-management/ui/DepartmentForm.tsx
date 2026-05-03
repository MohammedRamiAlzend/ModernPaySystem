import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Textarea } from '@/shared/ui/textarea';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from '@/shared/ui/form';
import { DepartmentType, CreateDepartmentDto } from '@/entities/department/model/types';
import { SearchableSelect, SearchableSelectOption } from '@/shared/ui/searchable-select';

const departmentFormSchema = z.object({
    name: z.string().min(2, { message: 'الاسم يجب أن يكون حرفين على الأقل' }),
    code: z.string().optional(),
    description: z.string().optional(),
    parentDepartmentId: z.string().min(1, { message: 'يجب اختيار القسم الأب' }),
    type: z.nativeEnum(DepartmentType),
});

type DepartmentFormValues = z.infer<typeof departmentFormSchema>;

interface DepartmentFormProps {
    onSubmit: (data: CreateDepartmentDto) => void;
    initialData?: Partial<DepartmentFormValues>;
    parentOptions: SearchableSelectOption[];
    isLoading?: boolean;
}

export const DepartmentForm: React.FC<DepartmentFormProps> = ({
    onSubmit,
    initialData,
    parentOptions,
    isLoading
}) => {
    const form = useForm<DepartmentFormValues>({
        resolver: zodResolver(departmentFormSchema),
        defaultValues: {
            name: initialData?.name || '',
            code: initialData?.code || '',
            description: initialData?.description || '',
            parentDepartmentId: initialData?.parentDepartmentId || '',
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
                        {isLoading ? 'جاري الحفظ...' : 'حفظ القسم'}
                    </Button>
                </div>
            </form>
        </Form>
    );
};
