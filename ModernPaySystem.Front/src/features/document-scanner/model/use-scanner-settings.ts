import { useState, useCallback } from 'react';

export type ScannerAppType = 'new' | 'old';
export type ColorMode = 'Color' | 'Grayscale' | 'BlackAndWhite';

export interface ScannerSettings {
    appType: ScannerAppType;
    deviceId: string;
    dpi: number;
    colorMode: ColorMode;
    duplex: boolean;
}

const DEFAULT_SETTINGS: ScannerSettings = {
    appType: 'new', // New scanner is default
    deviceId: '',
    dpi: 300,
    colorMode: 'Color',
    duplex: false,
};

const STORAGE_KEY = 'scanner_app_settings';

export const useScannerSettings = () => {
    const [settings, setSettingsState] = useState<ScannerSettings>(() => {
        try {
            const stored = localStorage.getItem(STORAGE_KEY);
            if (stored) {
                return { ...DEFAULT_SETTINGS, ...JSON.parse(stored) };
            }
        } catch {
            // Use defaults if localStorage is disabled or corrupt
        }
        return DEFAULT_SETTINGS;
    });

    const setSettings = useCallback((newSettings: Partial<ScannerSettings>) => {
        setSettingsState(prev => {
            const next = { ...prev, ...newSettings };
            localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
            return next;
        });
    }, []);

    return { settings, setSettings };
};
