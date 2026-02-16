import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { useSearchParams } from 'react-router-dom';
import { SETTINGS_CONFIG } from './config/settings-config';
import { Suspense } from 'react';
import { Loader2 } from 'lucide-react';

export const SettingsPage = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const activeTab = searchParams.get('tab') || SETTINGS_CONFIG[0].id;

  const handleTabChange = (value: string) => {
    setSearchParams({ tab: value });
  };

  return (
    <div className="container mx-auto py-8 space-y-8" style={{ direction: 'rtl' }}>
      <div className="flex flex-col gap-2">
        <h1 className="text-3xl font-black text-primary tracking-tight">إعدادات النظام</h1>
        <p className="text-muted-foreground">قم بتخصيص خيارات النظام والحقول المساعدة</p>
      </div>

      <Tabs value={activeTab} onValueChange={handleTabChange} defaultValue={SETTINGS_CONFIG[0].id} className="space-y-6">
        <TabsList className="bg-muted/50 p-1 rounded-2xl h-12 inline-flex items-center gap-1 overflow-x-auto overflow-y-hidden no-scrollbar max-w-full">
          {SETTINGS_CONFIG.map((tab) => (
            <TabsTrigger
              key={tab.id}
              value={tab.id}
              className="rounded-xl px-6 data-[state=active]:bg-background data-[state=active]:shadow-sm whitespace-nowrap"
              onMouseEnter={() => tab.preload?.()}
            >
              {tab.label}
            </TabsTrigger>
          ))}
        </TabsList>

        {SETTINGS_CONFIG.map((tab) => (
          <TabsContent key={tab.id} value={tab.id} className="mt-0">
            <Suspense fallback={
              <div className="flex items-center justify-center p-20">
                <Loader2 className="w-8 h-8 animate-spin text-primary opacity-50" />
              </div>
            }>
              {tab.component}
            </Suspense>
          </TabsContent>
        ))}
      </Tabs>
    </div>
  );
};

export default SettingsPage;