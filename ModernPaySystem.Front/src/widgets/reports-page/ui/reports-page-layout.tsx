import type { ReactNode } from 'react';
import { Card, CardContent } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Tabs, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { AnimatedContainer } from '@/shared/ui/common/animated-container';
import { RefreshCw, Ban } from 'lucide-react';
import type { ReportTabId, ReportTabConfig } from '@/shared/types/reports';

interface ReportsPageLayoutProps {
  title: string;
  description: string;
  tabs: ReportTabConfig[];
  activeTab: ReportTabId;
  onTabChange: (tab: ReportTabId) => void;
  onPrefetch: (tab: ReportTabId) => void;
  allRefetch: () => void;
  children: ReactNode;
  isAuthorized?: boolean;
  unauthorizedMessage?: string;
}

export const ReportsPageLayout = ({
  title,
  description,
  tabs,
  activeTab,
  onTabChange,
  onPrefetch,
  allRefetch,
  children,
  isAuthorized = true,
  unauthorizedMessage = 'هذه الصفحة متاحة فقط لمديري الأقسام. يرجى التواصل مع المشرف إذا كنت بحاجة إلى صلاحية الوصول.',
}: ReportsPageLayoutProps) => {
  if (!isAuthorized) {
    return (
      <div className="flex h-[60vh] items-center justify-center" dir="rtl">
        <Card className="max-w-md w-full border-destructive/20">
          <CardContent className="pt-12 pb-12 text-center space-y-4">
            <Ban className="w-16 h-16 mx-auto text-destructive/60" />
            <h2 className="text-2xl font-bold text-foreground">غير مصرح بالوصول</h2>
            <p className="text-muted-foreground">{unauthorizedMessage}</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <AnimatedContainer className="space-y-6 max-w-7xl mx-auto px-4 py-6" dir="rtl">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-foreground">{title}</h1>
          <p className="text-sm text-muted-foreground mt-1">{description}</p>
        </div>
        <Button variant="outline" size="sm" onClick={allRefetch}>
          <RefreshCw className="w-4 h-4 ml-2" />
          <span>تحديث</span>
        </Button>
      </div>

      <Tabs defaultValue="dashboard" value={activeTab} onValueChange={(v) => onTabChange(v as ReportTabId)}>
        <div className="overflow-x-auto pb-2">
          <TabsList className="w-full justify-start gap-1 bg-muted/50 p-1 rounded-lg">
            {tabs.map((tab) => (
              <TabsTrigger
                key={tab.id}
                value={tab.id}
                className="px-4 py-2 text-sm whitespace-nowrap"
                onMouseEnter={() => onPrefetch(tab.id)}
                onFocus={() => onPrefetch(tab.id)}
              >
                {tab.label}
              </TabsTrigger>
            ))}
          </TabsList>
        </div>
        {children}
      </Tabs>
    </AnimatedContainer>
  );
};
