import { useSearchParams } from 'react-router-dom';
import { SETTINGS_CONFIG } from './config/settings-config';
import { Suspense } from 'react';
import { Loader2 } from 'lucide-react';

export const SettingsPage = () => {
  const [searchParams] = useSearchParams();
  const activeTab = searchParams.get('tab') || SETTINGS_CONFIG[0].id;

  const activeConfig = SETTINGS_CONFIG.find((tab) => tab.id === activeTab);

  if (!activeConfig) {
    return (
      <div className="flex items-center justify-center p-20">
        <p className="text-muted-foreground">القسم المطلوب غير موجود</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 space-y-6" style={{ direction: 'rtl' }}>
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
        {activeConfig.component}
      </Suspense>
    </div>
  );
};

export default SettingsPage;