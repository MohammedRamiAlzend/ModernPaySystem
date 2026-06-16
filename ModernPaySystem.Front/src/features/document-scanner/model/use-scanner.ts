import { useState } from 'react';
import { useScannerSettings } from './use-scanner-settings';
import { localScannerService } from '../api/localScannerService';

/**
 * تحويل base64 string إلى File object
 */
export const convertBase64ToFile = (base64String: string, fileName: string): File => {
    const base64Data = base64String.includes(',')
        ? base64String.split(',')[1]
        : base64String;

    const byteCharacters = atob(base64Data);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: 'image/jpeg' });

    return new File([blob], fileName, { type: 'image/jpeg' });
};

export const useScanner = () => {
    const [isScanning, setIsScanning] = useState(false);
    const [scanError, setScanError] = useState<string | null>(null);
    const { settings } = useScannerSettings();

    const handleScanNew = async (onSuccess: (fileObjects: File[]) => void) => {
        setIsScanning(true);
        setScanError(null);

        try {
            let activeDeviceId = settings.deviceId;
            
            // Auto-select first device if none is selected
            if (!activeDeviceId) {
                const devices = await localScannerService.getDevices();
                if (devices && devices.length > 0) {
                    activeDeviceId = devices[0].id;
                } else {
                    setScanError('لا يوجد أجهزة ماسح ضوئي متصلة.');
                    setIsScanning(false);
                    return;
                }
            }

            // 1. Create a session
            const session = await localScannerService.createSession('Web Session');

            // 2. Start scanning
            const pages = await localScannerService.scanToSession(session.id, {
                deviceId: activeDeviceId,
                dpi: settings.dpi,
                colorMode: settings.colorMode,
                duplex: settings.duplex
            });

            if (!pages || pages.length === 0) {
                setScanError('لم يتم العثور على صور ممسوحة');
                setIsScanning(false);
                return;
            }

            // 3. Fetch blobs and convert to Files
            const now = new Date();
            const dateStr = now.toISOString().slice(0, 10);
            const timeStr = now.toTimeString().slice(0, 8).replace(/:/g, '');

            const fileObjects: File[] = [];
            for (let i = 0; i < pages.length; i++) {
                const page = pages[i];
                const blob = await localScannerService.getPageImageBlob(session.id, page.id);
                const extension = page.format?.toLowerCase() === 'png' ? 'png' : 'jpg';
                const mimeType = extension === 'png' ? 'image/png' : 'image/jpeg';
                const fileName = `scan-${dateStr}-${timeStr}-${i + 1}.${extension}`;
                
                const file = new File([blob], fileName, { type: mimeType });
                fileObjects.push(file);
            }

            onSuccess(fileObjects);
        } catch (error: any) {
            console.error('Error in new scanner:', error);
            setScanError(error.message || 'حدث خطأ أثناء الاتصال بالماسح الضوئي المحلي');
        } finally {
            setIsScanning(false);
        }
    };

    const handleScanOld = (onSuccess: (fileObjects: File[]) => void) => {
        if (!window.scanner) {
            setScanError('مكتبة المسح الضوئي غير متوفرة. يرجى التأكد من تحميل السكريبت.');
            return;
        }

        setIsScanning(true);
        setScanError(null);

        window.scanner.scan(
            (successful: boolean, mesg: string, response: any) => {
                if (!successful) {
                    setScanError('فشل المسح الضوئي: ' + mesg);
                    setIsScanning(false);
                    return;
                }

                if (mesg != null && mesg.toLowerCase().indexOf('user cancel') >= 0) {
                    setScanError('تم إلغاء المسح الضوئي');
                    setIsScanning(false);
                    return;
                }

                try {
                    const scannedImages = window.scanner!.getScannedImages(response, true, false);

                    if (!scannedImages || !Array.isArray(scannedImages) || scannedImages.length === 0) {
                        setScanError('لم يتم العثور على صور ممسوحة');
                        setIsScanning(false);
                        return;
                    }

                    const now = new Date();
                    const dateStr = now.toISOString().slice(0, 10);
                    const timeStr = now.toTimeString().slice(0, 8).replace(/:/g, '');

                    const fileObjects = scannedImages.map((scannedImage: any, index: number) => {
                        const fileName = `scan-${dateStr}-${timeStr}-${index + 1}.jpg`;
                        return convertBase64ToFile(scannedImage.src, fileName);
                    });

                    onSuccess(fileObjects);
                    setIsScanning(false);
                } catch (error: any) {
                    console.error('Error processing scanned images:', error);
                    setScanError('حدث خطأ أثناء معالجة الصور الممسوحة: ' + error.message);
                    setIsScanning(false);
                }
            },
            {
                use_asprise_dialog: true,
                show_scanner_ui: true,
                twain_cap_setting: {
                    ICAP_PIXELTYPE: "TWPT_RGB"
                },
                output_settings: [{
                    type: "return-base64",
                    format: "jpg"
                }]
            }
        );
    };

    const handleScan = (onSuccess: (fileObjects: File[]) => void) => {
        if (settings.appType === 'new') {
            handleScanNew(onSuccess);
        } else {
            handleScanOld(onSuccess);
        }
    };

    return { handleScan, isScanning, scanError, setScanError };
};
