import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Switch } from '@/shared/ui/switch';
import { Label } from '@/shared/ui/label';
import { useTheme } from '@/app/providers/theme-provider';
import { LookUpManagement } from '@/features/lookup-management/ui/LookUpManagement';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { Palette } from 'lucide-react';

export const SettingsPage = () => {
  const { theme, setTheme } = useTheme();

  return (
    <div className="container mx-auto py-8 space-y-8" style={{ direction: 'rtl' }}>
      <div className="flex flex-col gap-2">
        <h1 className="text-3xl font-black text-primary tracking-tight">إعدادات النظام</h1>
        <p className="text-muted-foreground">قم بتخصيص خيارات النظام والحقول المساعدة</p>
      </div>

      <Tabs defaultValue="lookup" className="space-y-6">
        <TabsList className="bg-muted/50 p-1 rounded-2xl h-12 inline-flex items-center gap-1">
          <TabsTrigger value="lookup" className="rounded-xl px-6 data-[state=active]:bg-background data-[state=active]:shadow-sm">
            إدارة الحقول (LookUp)
          </TabsTrigger>
          <TabsTrigger value="appearance" className="rounded-xl px-6 data-[state=active]:bg-background data-[state=active]:shadow-sm">
            المظهر والتفضيلات
          </TabsTrigger>
        </TabsList>

        <TabsContent value="lookup" className="mt-0">
          <LookUpManagement />
        </TabsContent>

        <TabsContent value="appearance" className="mt-0">
          <Card className="max-w-3xl border-none shadow-lg bg-card/50 backdrop-blur-sm">
            <CardHeader>
              <div className="flex items-center gap-2 mb-1">
                <Palette className="w-5 h-5 text-primary" />
                <CardTitle>المظهر</CardTitle>
              </div>
              <CardDescription>تحكم في كيفية ظهور التطبيق لك</CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="flex items-center justify-between p-4 bg-muted/20 rounded-2xl">
                <div className="space-y-1">
                  <Label className="text-base font-bold">الوضع الليلي</Label>
                  <p className="text-sm text-muted-foreground">
                    تفعيل المظهر الداكن للتطبيق لراحة العين
                  </p>
                </div>
                <Switch
                  checked={theme === 'dark'}
                  onCheckedChange={(checked: boolean) => setTheme(checked ? 'dark' : 'light')}
                />
              </div>

              {/* <div className="flex flex-col sm:flex-row gap-3 pt-4">
                <Button className="rounded-xl px-10 h-11 font-bold">حفظ التغييرات</Button>
                <Button variant="outline" className="rounded-xl px-10 h-11 border-none bg-muted hover:bg-muted/80">إعادة تعيين</Button>
              </div> */}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
};

export default SettingsPage;