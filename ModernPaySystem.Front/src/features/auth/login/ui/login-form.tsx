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
        <AnimatedContainer className="w-full">
            <Card className="border-none shadow-2xl bg-card/60 backdrop-blur-2xl overflow-hidden rounded-3xl" dir="rtl">
                <CardHeader className="space-y-1 text-center pt-8 pb-6">
                    <CardTitle className="text-4xl font-extrabold tracking-tight bg-gradient-to-r from-primary to-primary/60 bg-clip-text text-transparent">
                        تسجيل الدخول
                    </CardTitle>
                    <CardDescription className="text-muted-foreground/80 text-sm font-medium">
                        أدخل بيانات الاعتماد للوصول إلى النظام
                    </CardDescription>
                </CardHeader>

                <form onSubmit={handleSubmit(onSubmit)}>
                    <CardContent className="space-y-6 px-8">
                        {error && (
                            <div className="p-4 text-xs font-bold text-destructive bg-destructive/10 border border-destructive/20 rounded-2xl text-center animate-in fade-in slide-in-from-top-2">
                                {errorMessage}
                            </div>
                        )}

                        <div className="space-y-2 text-right">
                            <Label htmlFor="username" className="text-xs font-bold text-muted-foreground mr-1">اسم المستخدم</Label>
                            <Input
                                id="username"
                                type="text"
                                placeholder="اسم المستخدم"
                                className="h-12 rounded-2xl bg-background/40 focus:ring-primary/30 transition-all border-none text-right px-4 shadow-inner"
                                {...register('username')}
                                disabled={isPending}
                            />
                            {errors.username && (
                                <p className="text-[10px] text-destructive mt-1 mr-1 font-bold">{errors.username.message}</p>
                            )}
                        </div>

                        <div className="space-y-2 text-right">
                            <Label htmlFor="password" className="text-xs font-bold text-muted-foreground mr-1">كلمة المرور</Label>
                            <Input
                                id="password"
                                type="password"
                                placeholder="••••••••"
                                className="h-12 rounded-2xl bg-background/40 focus:ring-primary/30 transition-all border-none text-right px-4 font-mono shadow-inner"
                                {...register('password')}
                                disabled={isPending}
                            />
                            {errors.password && (
                                <p className="text-[10px] text-destructive mt-1 mr-1 font-bold">{errors.password.message}</p>
                            )}
                        </div>
                    </CardContent>

                    <CardFooter className="flex flex-col pt-8 pb-10 px-8 gap-4">
                        <Button
                            type="submit"
                            className="w-full h-14 rounded-2xl text-lg font-black shadow-[0_10px_30px_-10px_rgba(var(--primary),0.5)] hover:scale-[1.03] active:scale-[0.98] transition-all duration-300"
                            disabled={isPending}
                        >
                            {isPending ? (
                                <span className="flex items-center gap-2">
                                    <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                                    جاري الدخول...
                                </span>
                            ) : 'دخول'}
                        </Button>
                    </CardFooter>
                </form>
            </Card>
        </AnimatedContainer>
    );
};
