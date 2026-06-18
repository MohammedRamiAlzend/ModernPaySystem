import React, { useEffect, useState } from 'react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import { Switch } from '@/shared/ui/switch';
import { useScannerSettings, ScannerAppType, ColorMode } from '@/features/document-scanner/model/use-scanner-settings';
import { localScannerService, DeviceDto } from '@/features/document-scanner/api/localScannerService';
import { Label } from '@/shared/ui/label';

export const ScannerSettingsPage: React.FC = () => {
    const { settings, setSettings } = useScannerSettings();
    const [devices, setDevices] = useState<DeviceDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    useEffect(() => {
        if (settings.appType === 'new') {
            Promise.resolve().then(() => setIsLoading(true));
            localScannerService.getDevices()
                .then(data => {
                    setDevices(data);
                    if (data.length > 0 && !settings.deviceId) {
                        setSettings({ deviceId: data[0].id });
                    }
                })
                .catch(err => console.error('Failed to load devices', err))
                .finally(() => setIsLoading(false));
        }
    }, [settings.appType, settings.deviceId, setSettings]);

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
