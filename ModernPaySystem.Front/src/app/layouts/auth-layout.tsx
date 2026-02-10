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
      "min-h-[100dvh] w-full flex items-center justify-center bg-background relative overflow-hidden p-6",
      className
    )}>
      {/* Background decoration */}
      <div className="absolute inset-0 bg-gradient-to-br from-primary/10 via-background to-secondary/10 opacity-50" />

      <div className="w-full max-w-md z-10">
        {children}
      </div>
    </div>
  );
};