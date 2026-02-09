import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useLogin } from '../model/use-login';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/shared/ui/card';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

const loginSchema = z.object({
    username: z.string().min(1, 'يرجى إدخال اسم المستخدم'),
    password: z.string().min(1, 'كلمة المرور يجب أن تكون1 أحرف على الأقل'),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export const LoginForm: React.FC = () => {
    const { mutate: login, isPending, error } = useLogin();

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<LoginFormValues>({
        resolver: zodResolver(loginSchema),
        defaultValues: {
            username: '',
            password: '',
        },
    });

    const onSubmit = (data: LoginFormValues) => {
        login(data);
    };

    const errorMessage = (error as any)?.response?.data?.message ||
        'فشل تسجيل الدخول. يرجى التأكد من البيانات والمحاولة مرة أخرى.';

    return (
        <AnimatedContainer className="w-full max-w-md">
            <Card className="border-none shadow-2xl bg-card/80 backdrop-blur-xl" dir="rtl">
                <CardHeader className="space-y-2 text-center pb-8">
                    <CardTitle className="text-3xl font-bold tracking-tight text-primary">تسجيل الدخول</CardTitle>
                    <CardDescription className="text-muted-foreground text-sm">
                        أدخل بيانات الاعتماد الخاصة بك للوصول إلى النظام
                    </CardDescription>
                </CardHeader>
                <form onSubmit={handleSubmit(onSubmit)}>
                    <CardContent className="space-y-5">
                        {error && (
                            <div className="p-3 text-sm text-destructive bg-destructive/10 border border-destructive/20 rounded-xl text-center animate-in fade-in zoom-in duration-300">
                                {errorMessage}
                            </div>
                        )}

                        <div className="space-y-2 text-right">
                            <Label htmlFor="username" className="text-sm font-semibold mr-1">اسم المستخدم</Label>
                            <Input
                                id="username"
                                type="text"
                                placeholder="اسم المستخدم"
                                className="h-12 rounded-xl bg-background/50 focus:ring-primary/20 transition-all border-muted-foreground/20 text-right"
                                {...register('username')}
                                disabled={isPending}
                            />
                            {errors.username && (
                                <p className="text-xs text-destructive mt-1 mr-1">{errors.username.message}</p>
                            )}
                        </div>

                        <div className="space-y-2 text-right">
                            <Label htmlFor="password" className="text-sm font-semibold mr-1">كلمة المرور</Label>
                            <Input
                                id="password"
                                type="password"
                                placeholder="كلمة المرور"
                                className="h-12 rounded-xl bg-background/50 focus:ring-primary/20 transition-all border-muted-foreground/20 text-right font-mono"
                                {...register('password')}
                                disabled={isPending}
                            />
                            {errors.password && (
                                <p className="text-xs text-destructive mt-1 mr-1">{errors.password.message}</p>
                            )}
                        </div>
                    </CardContent>
                    <CardFooter className="flex flex-col pt-6 gap-4">
                        <Button
                            type="submit"
                            className="w-full h-12 rounded-xl text-md font-bold shadow-lg shadow-primary/20 hover:scale-[1.02] transition-all duration-300"
                            disabled={isPending}
                        >
                            {isPending ? 'جاري الدخول...' : 'دخول'}
                        </Button>
                    </CardFooter>
                </form>
            </Card>
        </AnimatedContainer>
    );
};
