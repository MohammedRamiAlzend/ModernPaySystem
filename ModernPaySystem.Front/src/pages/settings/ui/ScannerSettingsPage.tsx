import React, { useEffect } from 'react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { Switch } from '@/shared/ui/switch';
import { useScannerSettings, ScannerAppType, ColorMode } from '@/features/document-scanner/model/use-scanner-settings';
import { useScannerDevices, useTestScannerConnection } from '@/features/document-scanner/model/use-scanner-devices';
import { Label } from '@/shared/ui/label';
import { Button } from '@/shared/ui/button';
import { useUIStore } from '@/app/store/uiStore';

export const ScannerSettingsPage: React.FC = () => {
    const { settings, setSettings } = useScannerSettings();
    const { data: devices = [], isLoading } = useScannerDevices(settings.appType === 'new');
    const { testConnection, isTesting } = useTestScannerConnection();
    const showStatus = useUIStore(state => state.showStatus);

    useEffect(() => {
        if (settings.appType === 'new' && devices.length > 0 && !settings.deviceId) {
            setSettings({ deviceId: devices[0].id });
        }
    }, [settings.appType, settings.deviceId, devices, setSettings]);

    const handleTestConnection = async () => {
        const result = await testConnection(settings.deviceId);
        showStatus({
            type: result.success ? 'success' : 'error',
            title: result.success ? 'حالة الاتصال' : 'فشل الاتصال',
            message: result.message
        });
    };

    return (
        <div className="max-w-2xl bg-card border rounded-3xl p-6 md:p-8 space-y-8 shadow-sm">
            <div className="space-y-6">
                <div className="space-y-2">
                    <h3 className="text-xl font-bold">تفضيلات المسح الضوئي</h3>
                    <p className="text-sm text-muted-foreground">اختر النظام الذي تفضل استخدامه وإعدادات الجودة والألوان الافتراضية.</p>
                </div>

                <div className="grid gap-6">
                    <div className="space-y-3 bg-muted/20 p-4 rounded-2xl border">
                        <Label className="text-base font-bold">نوع النظام الأساسي</Label>
                        <Select 
                            value={settings.appType} 
                            onValueChange={(v) => setSettings({ appType: v as ScannerAppType })}
                        >
                            <SelectTrigger className="bg-background">
                                <SelectValue placeholder="اختر النظام" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value="new">النظام الحديث (محلي - موصى به)</SelectItem>
                                <SelectItem value="old">النظام القديم (Asprise)</SelectItem>
                            </SelectContent>
                        </Select>
                        <p className="text-[11px] text-muted-foreground">
                            النظام الحديث يعمل من خلال الخادم المحلي ويوفر أداء واستقرار أعلى.
                        </p>
                    </div>

                    {settings.appType === 'new' && (
                        <div className="space-y-5 bg-primary/5 p-4 rounded-2xl border border-primary/10">
                            <h4 className="font-bold text-primary mb-2">إعدادات النظام الحديث</h4>
                            
                            <div className="space-y-2">
                                <Label>الجهاز الافتراضي</Label>
                                <div className="flex gap-2">
                                    <div className="flex-1">
                                        <Select 
                                            value={settings.deviceId} 
                                            onValueChange={(v) => setSettings({ deviceId: v })}
                                        >
                                            <SelectTrigger disabled={isLoading || devices.length === 0} className="bg-background">
                                                <SelectValue placeholder={isLoading ? 'جاري التحميل...' : (devices.length === 0 ? 'لا يوجد أجهزة متصلة' : 'اختر الجهاز')} />
                                            </SelectTrigger>
                                            <SelectContent>
                                                {devices.map(d => (
                                                    <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>
                                                ))}
                                            </SelectContent>
                                        </Select>
                                    </div>
                                    <Button 
                                        variant="outline" 
                                        disabled={isTesting || isLoading || !settings.deviceId}
                                        onClick={handleTestConnection}
                                    >
                                        {isTesting ? 'جاري الفحص...' : 'اختبار الاتصال'}
                                    </Button>
                                </div>
                            </div>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="space-y-2">
                                    <Label>الدقة (DPI)</Label>
                                    <Select 
                                        value={settings.dpi.toString()} 
                                        onValueChange={(v) => setSettings({ dpi: parseInt(v) })}
                                    >
                                        <SelectTrigger className="bg-background">
                                            <SelectValue placeholder="اختر الدقة" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="150">150 DPI (سريع وحجم أقل)</SelectItem>
                                            <SelectItem value="300">300 DPI (عادي - موصى به)</SelectItem>
                                            <SelectItem value="600">600 DPI (عالي الجودة)</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                <div className="space-y-2">
                                    <Label>نظام الألوان</Label>
                                    <Select 
                                        value={settings.colorMode} 
                                        onValueChange={(v) => setSettings({ colorMode: v as ColorMode })}
                                    >
                                        <SelectTrigger className="bg-background">
                                            <SelectValue placeholder="اختر اللون" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Color">ملون</SelectItem>
                                            <SelectItem value="Grayscale">تدرج رمادي</SelectItem>
                                            <SelectItem value="BlackAndWhite">أبيض وأسود</SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>
                            </div>

                            <div className="flex items-center justify-between pt-2 bg-background p-3 rounded-xl border">
                                <Label className="cursor-pointer flex-1" htmlFor="duplex-mode">
                                    <div className="font-bold">مسح على الوجهين (Duplex)</div>
                                    <div className="text-xs text-muted-foreground font-normal">تفعيل مسح الوجهين إذا كان الجهاز يدعم ذلك</div>
                                </Label>
                                <Switch 
                                    id="duplex-mode"
                                    checked={settings.duplex} 
                                    onCheckedChange={(v) => setSettings({ duplex: v })} 
                                />
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};
