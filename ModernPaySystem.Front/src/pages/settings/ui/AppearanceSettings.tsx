import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Switch } from '@/shared/ui/switch';
import { Label } from '@/shared/ui/label';
import { useTheme } from '@/app/providers/theme-provider';
import { Palette } from 'lucide-react';

export const AppearanceSettings = () => {
    const { theme, setTheme } = useTheme();

    return (
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
            </CardContent>
        </Card>
    );
};
