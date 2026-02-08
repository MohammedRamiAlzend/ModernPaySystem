import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Button } from '@/shared/ui/button';
import { Switch } from '@/shared/ui/switch';
import { Label } from '@/shared/ui/label';
import { useTheme } from '@/app/providers/theme-provider';

export const SettingsPage = () => {
  const { theme, setTheme } = useTheme();

  return (
    <div className="container mx-auto py-6 max-w-3xl">
      <Card>
        <CardHeader>
          <CardTitle>Settings</CardTitle>
          <CardDescription>Manage your account settings and preferences</CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="flex items-center justify-between">
            <div className="space-y-0.5">
              <Label>Dark Mode</Label>
              <p className="text-sm text-muted-foreground">
                Enable dark mode for the application
              </p>
            </div>
            <Switch
              checked={theme === 'dark'}
              onCheckedChange={(checked: boolean) => setTheme(checked ? 'dark' : 'light')}
            />
          </div>

          <div className="flex flex-col sm:flex-row gap-3 pt-4">
            <Button>Save Changes</Button>
            <Button variant="outline">Reset Preferences</Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default SettingsPage;