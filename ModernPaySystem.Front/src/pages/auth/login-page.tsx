import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/shared/ui/card';
import { useAppDispatch } from '@/app/store';
import { loginSuccess } from '@/app/store/authSlice';
import authService from '@/shared/api/services/authService';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

// Define login schema with Arabic messages
const loginSchema = z.object({
  userName: z.string().min(1, 'يرجى إدخال اسم المستخدم'),
  password: z.string().min(6, 'كلمة المرور يجب أن تكون 6 أحرف على الأقل'),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export const LoginPage = () => {
  const [searchParams] = useSearchParams();
  const redirectUrl = searchParams.get('redirect') || '/';
  const dispatch = useAppDispatch();
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      userName: '',
      password: '',
    },
  });

  // Use React Query mutation for login
  const loginMutation = useMutation({
    mutationFn: authService.login,
    onSuccess: (data) => {
      dispatch(loginSuccess({ user: data.user, token: data.token }));
      navigate(decodeURIComponent(redirectUrl), { replace: true });
    },
  });

  const onSubmit = (data: LoginFormValues) => {
    loginMutation.mutate(data);
  };

  const errorMessage = (loginMutation.error as any)?.response?.data?.message ||
    'فشل تسجيل الدخول. يرجى التأكد من البيانات والمحاولة مرة أخرى.';

  return (
    <AnimatedContainer className="w-full max-w-md">
      <Card className="border-none shadow-2xl bg-card/80 backdrop-blur-xl" dir="rtl">
        <CardHeader className="space-y-2 text-center pb-8">
          <CardTitle className="text-3xl font-bold tracking-tight text-primary">تسجيل الدخول</CardTitle>
          <CardDescription className="text-muted-foreground text-sm">
            أدخل بيانات الاعتماد الخاصة بك لتسجيل الدخول
          </CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <CardContent className="space-y-5">
            {loginMutation.isError && (
              <div className="p-3 text-sm text-destructive bg-destructive/10 border border-destructive/20 rounded-xl text-center animate-in fade-in zoom-in duration-300">
                {errorMessage}
              </div>
            )}

            <div className="space-y-2 text-right">
              <Label htmlFor="userName" className="text-sm font-semibold mr-1">اسم المستخدم</Label>
              <Input
                id="userName"
                type="text"
                placeholder="اسم المستخدم"
                className="h-12 rounded-xl bg-background/50 focus:ring-primary/20 transition-all border-muted-foreground/20 text-right"
                {...register('userName')}
                disabled={loginMutation.isPending}
              />
              {errors.userName && (
                <p className="text-xs text-destructive mt-1 mr-1">{errors.userName.message}</p>
              )}
            </div>

            <div className="space-y-2 text-right">
              {/* <div className="flex justify-between items-center mb-1">
                <Link to="#" className="text-xs text-primary hover:underline transition-all">نسيت كلمة المرور؟</Link>
              </div> */}
              <Label htmlFor="password" className="text-sm font-semibold mr-1">كلمة المرور</Label>
              <Input
                id="password"
                type="password"
                placeholder="كلمة المرور"
                className="h-12 rounded-xl bg-background/50 focus:ring-primary/20 transition-all border-muted-foreground/20 text-right font-mono"
                {...register('password')}
                disabled={loginMutation.isPending}
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
              disabled={loginMutation.isPending}
            >
              {loginMutation.isPending ? 'جاري الدخول...' : 'دخول'}
            </Button>

            {/* <div className="text-center text-sm text-muted-foreground">
              ليس لديك حساب؟{' '}
              <Link to="/auth/register" className="font-bold text-primary hover:underline transition-all">
                إنشاء حساب جديد
              </Link>
            </div> */}
          </CardFooter>
        </form>
      </Card>
    </AnimatedContainer>
  );
};

export default LoginPage;