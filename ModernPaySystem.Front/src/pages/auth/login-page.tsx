import { LoginForm } from '@/features/auth/login/ui/login-form';

export const LoginPage = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-muted/30 p-4">
      <LoginForm />
    </div>
  );
};

export default LoginPage;