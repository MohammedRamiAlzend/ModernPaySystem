import { cn } from '@/shared/lib/utils';

interface AuthLayoutProps {
  children: React.ReactNode;
  className?: string;
}

export const AuthLayout: React.FC<AuthLayoutProps> = ({
  children,
  className
}) => {
  return (
    <div className={cn(
      "min-h-screen flex items-center justify-center bg-gradient-to-br from-primary/5 to-secondary/5 p-4",
      className
    )}>
      <div className="w-full max-w-md -translate-y-12">
        {children}
      </div>
    </div>
  );
};