import { useSearchParams } from 'react-router-dom';
import { SETTINGS_CONFIG } from './config/settings-config';
import { Suspense, useState } from 'react';
import { Loader2, LockKeyhole } from 'lucide-react';
import { Input } from '@/shared/ui/input';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';

export const SettingsPage = () => {
  const [searchParams] = useSearchParams();
  const activeTab = searchParams.get('tab') || SETTINGS_CONFIG[0].id;
  const activeConfig = SETTINGS_CONFIG.find((tab) => tab.id === activeTab);

  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isUnlocked, setIsUnlocked] = useState(() => {
    return sessionStorage.getItem('settings_unlocked') === 'true';
  });

  const handleUnlock = (e: React.FormEvent) => {
    e.preventDefault();
    if (password === '135997') {
      sessionStorage.setItem('settings_unlocked', 'true');
      setIsUnlocked(true);
      setError('');
    } else {
      setError('كلمة المرور غير صحيحة!');
    }
  };

  if (!isUnlocked) {
    return (
      <div className="flex items-center justify-center min-h-[60vh] p-4" style={{ direction: 'rtl' }}>
        <Card className="w-full max-w-md border shadow-lg bg-card/50 backdrop-blur-md">
          <CardHeader className="text-center space-y-2">
            <div className="mx-auto bg-primary/10 text-primary w-12 h-12 rounded-full flex items-center justify-center mb-2">
              <LockKeyhole className="w-6 h-6 animate-pulse" />
            </div>
            <CardTitle className="text-xl font-bold">حماية الإعدادات</CardTitle>
            <CardDescription>
              يرجى إدخال كلمة المرور للوصول إلى إعدادات النظام.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleUnlock} className="space-y-4">
              <div className="space-y-2">
                <Input
                  type="password"
                  placeholder="كلمة المرور"
                  value={password}
                  onChange={(e) => {
                    setPassword(e.target.value);
                    if (error) setError('');
                  }}
                  className="text-center tracking-widest text-lg"
                  autoFocus
                />
                {error && (
                  <p className="text-sm font-semibold text-destructive text-center">
                    {error}
                  </p>
                )}
              </div>
              <Button type="submit" className="w-full font-bold">
                دخول
              </Button>
            </form>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!activeConfig) {
    return (
      <div className="flex items-center justify-center p-20">
        <p className="text-muted-foreground">القسم المطلوب غير موجود</p>
      </div>
    );
  }

  const ActiveComponent = activeConfig.component;

  return (
    <AnimatedContainer className="container mx-auto py-8 space-y-6" style={{ direction: 'rtl' }}>
      {/* Section Header */}
      {(activeConfig.showDescription !== false) && (
        <div className="flex flex-col gap-1">
          <h1 className="text-2xl font-black text-foreground tracking-tight">
            {activeConfig.label}
          </h1>
          <p className="text-sm text-muted-foreground">
            {activeConfig.description}
          </p>
        </div>
      )}

      {/* Section Content */}
      <Suspense fallback={
        <div className="flex items-center justify-center p-20">
          <Loader2 className="w-8 h-8 animate-spin text-primary opacity-50" />
        </div>
      }>
        <ActiveComponent />
      </Suspense>
    </AnimatedContainer>
  );
};

export default SettingsPage;