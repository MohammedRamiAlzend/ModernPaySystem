import React, { useState } from 'react';
import { Button } from '@/shared/ui/button';
import { ImageMeta } from '../model/types';
import { FileText, Trash2, Upload, Scan, Settings } from 'lucide-react';
import { cn } from '@/shared/lib/utils';
import { useScannerSettings, ScannerAppType, ColorMode } from '../model/use-scanner-settings';
import { useScannerDevices } from '../model/use-scanner-devices';
import { Switch } from '@/shared/ui/switch';
import { Label } from '@/shared/ui/label';

interface ImageGalleryProps {
    imageFiles: ImageMeta[];
    activeIndex: number | null;
    onSelect: (index: number) => void;
    onRemove: (id: string) => void;
    onPickFiles: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onScan: () => void;
    isScanning: boolean;
    acceptAllFiles?: boolean;
    error?: string | null;
}

export const ImageGallery: React.FC<ImageGalleryProps> = ({
    imageFiles,
    activeIndex,
    onSelect,
    onRemove,
    onPickFiles,
    onScan,
    isScanning,
    acceptAllFiles = false,
    error,
}) => {
    const { settings, setSettings } = useScannerSettings();
    const { data: devices = [], isLoading: isLoadingDevices } = useScannerDevices(settings.appType === 'new');
    const [showSettings, setShowSettings] = useState(false);

    React.useEffect(() => {
        if (settings.appType === 'new' && devices.length > 0 && !settings.deviceId) {
            setSettings({ deviceId: devices[0].id });
        }
    }, [settings.appType, settings.deviceId, devices, setSettings]);

    return (
        <div className='md:col-span-1 flex flex-col gap-4 border-l pl-0 md:pl-4' dir="rtl">
            {/* Upload Controls */}
            <div className="space-y-3">
                <label className="text-sm font-bold flex items-center gap-2 px-1">
                    <Upload className="h-4 w-4" /> اختيار الملفات
                </label>
                <div className="flex gap-2">
                    <div className="relative flex-1">
                        <input
                            type='file'
                            accept={acceptAllFiles ? '*' : 'image/*'}
                            multiple
                            onChange={onPickFiles}
                            className='absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10'
                        />
                        <Button variant="outline" className="w-full gap-2 rounded-xl border-2">
                            {acceptAllFiles ? 'ملفات' : 'صور'}
                        </Button>
                    </div>
                    <Button
                        variant="secondary"
                        onClick={onScan}
                        disabled={isScanning}
                        className="gap-2 rounded-xl"
                    >
                        <Scan className={cn("h-4 w-4", isScanning && "animate-spin")} />
                        {isScanning ? 'جارٍ...' : 'سكان'}
                    </Button>
                    <Button
                        variant="outline"
                        size="icon"
                        onClick={() => setShowSettings(!showSettings)}
                        className={cn("rounded-xl border-2 shrink-0 transition-colors", showSettings && "bg-muted border-primary")}
                        title="إعدادات الماسح الضوئي"
                    >
                        <Settings className="h-4 w-4" />
                    </Button>
                </div>

                {showSettings && (
                    <div className="p-4 bg-muted/40 border rounded-2xl space-y-4 animate-in fade-in-50 duration-200">
                        <div className="flex items-center justify-between border-b pb-2">
                            <span className="font-bold text-xs text-primary">إعدادات الماسح الضوئي</span>
                        </div>

                        <div className="space-y-3">
                            <div className="space-y-1">
                                <Label className="text-[11px] font-bold">نوع النظام الأساسي</Label>
                                <select 
                                    value={settings.appType} 
                                    onChange={(e) => setSettings({ appType: e.target.value as ScannerAppType })}
                                    className="w-full h-8 text-xs bg-background border rounded-xl px-2 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer"
                                >
                                    <option value="new">النظام الحديث (محلي)</option>
                                    <option value="old">النظام القديم (Asprise)</option>
                                </select>
                            </div>

                            {settings.appType === 'new' && (
                                <div className="space-y-3 pt-2 border-t border-muted-foreground/10">
                                    <div className="space-y-1">
                                        <Label className="text-[11px] font-bold">الجهاز الافتراضي</Label>
                                        <select 
                                            value={settings.deviceId} 
                                            onChange={(e) => setSettings({ deviceId: e.target.value })}
                                            disabled={isLoadingDevices || devices.length === 0}
                                            className="w-full h-8 text-xs bg-background border rounded-xl px-2 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer disabled:opacity-50"
                                        >
                                            {isLoadingDevices ? (
                                                <option>جاري التحميل...</option>
                                            ) : devices.length === 0 ? (
                                                <option>لا يوجد أجهزة متصلة</option>
                                            ) : (
                                                <>
                                                    <option value="" disabled>اختر الجهاز</option>
                                                    {devices.map(d => (
                                                        <option key={d.id} value={d.id}>{d.name}</option>
                                                    ))}
                                                </>
                                            )}
                                        </select>
                                    </div>

                                    <div className="grid grid-cols-2 gap-2">
                                        <div className="space-y-1">
                                            <Label className="text-[11px] font-bold">الدقة (DPI)</Label>
                                            <select 
                                                value={settings.dpi.toString()} 
                                                onChange={(e) => setSettings({ dpi: parseInt(e.target.value) })}
                                                className="w-full h-8 text-xs bg-background border rounded-xl px-2 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer"
                                            >
                                                <option value="150">150 DPI</option>
                                                <option value="300">300 DPI</option>
                                                <option value="600">600 DPI</option>
                                            </select>
                                        </div>

                                        <div className="space-y-1">
                                            <Label className="text-[11px] font-bold">نظام الألوان</Label>
                                            <select 
                                                value={settings.colorMode} 
                                                onChange={(e) => setSettings({ colorMode: e.target.value as ColorMode })}
                                                className="w-full h-8 text-xs bg-background border rounded-xl px-2 focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring cursor-pointer"
                                            >
                                                <option value="Color">ملون</option>
                                                <option value="Grayscale">تدرج رمادي</option>
                                                <option value="BlackAndWhite">أبيض وأسود</option>
                                            </select>
                                        </div>
                                    </div>

                                    <div className="flex items-center justify-between pt-1">
                                        <Label className="cursor-pointer text-[11px] font-bold" htmlFor="modal-duplex-mode">
                                            مسح على الوجهين (Duplex)
                                        </Label>
                                        <Switch 
                                            id="modal-duplex-mode"
                                            checked={settings.duplex} 
                                            onCheckedChange={(v) => setSettings({ duplex: v })} 
                                        />
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                )}
                {error && (
                    <div className='text-destructive text-[11px] p-2 bg-destructive/5 border border-destructive/10 rounded-lg'>
                        {error}
                    </div>
                )}
            </div>

            {/* File List */}
            <div className='flex-1 min-h-[200px] md:min-h-[300px] border-2 border-dashed rounded-2xl p-2 space-y-2 overflow-y-auto bg-muted/5'>
                {imageFiles.length === 0 && (
                    <div className="h-full flex flex-col items-center justify-center text-muted-foreground opacity-40 gap-2">
                        <FileText className="h-8 w-8" />
                        <span className="text-xs">المعرض فارغ</span>
                    </div>
                )}
                {imageFiles.map((it, idx) => (
                    <div
                        key={it.id}
                        onClick={() => onSelect(idx)}
                        className={cn(
                            "group flex items-center gap-3 p-2 rounded-xl cursor-pointer transition-all border-2",
                            idx === activeIndex
                                ? "bg-primary/5 border-primary"
                                : "bg-background border-transparent hover:border-primary/20"
                        )}
                    >
                        <div className="w-14 h-14 rounded-lg overflow-hidden bg-muted flex items-center justify-center border shrink-0">
                            {it.type.startsWith('image/') ? (
                                <img src={it.url} alt="" className="w-full h-full object-cover" />
                            ) : (
                                <FileText className="h-6 w-6 text-primary" />
                            )}
                        </div>
                        <div className='flex-1 min-w-0'>
                            <div className='text-xs font-bold truncate'>{it.name}</div>
                            <div className='text-[10px] text-muted-foreground'>
                                {it.width ? `${it.width}x${it.height}` : it.type.split('/')[1].toUpperCase()} · {(it.size / 1024).toFixed(0)}KB
                            </div>
                        </div>
                        <Button
                            variant="ghost"
                            size="icon"
                            className="opacity-0 group-hover:opacity-100 h-8 w-8 text-destructive hover:bg-destructive/10"
                            onClick={(e) => { e.stopPropagation(); onRemove(it.id); }}
                        >
                            <Trash2 className="h-4 w-4" />
                        </Button>
                    </div>
                ))}
            </div>
        </div>
    );
};
