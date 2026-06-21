import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { queryKeys } from '@/shared/constants/query-keys';
import { localScannerService } from '../api/localScannerService';

export const useScannerDevices = (enabled = true) => {
    return useQuery({
        queryKey: queryKeys.scanner.devices(),
        queryFn: () => localScannerService.getDevices(),
        enabled,
        staleTime: 30000, // 30 seconds stale time for local devices listing
    });
};

export const useTestScannerConnection = () => {
    const [isTesting, setIsTesting] = useState(false);

    const testConnection = async (deviceId: string) => {
        if (!deviceId) {
            return {
                success: false,
                message: 'يرجى اختيار جهاز أولاً للاختبار.'
            };
        }

        setIsTesting(true);
        try {
            const devices = await localScannerService.getDevices();
            const device = devices.find(d => d.id === deviceId);
            if (device) {
                return {
                    success: true,
                    message: `الماسح الضوئي "${device.name}" متصل وجاهز للعمل. الحالة: ${device.status || 'نشط'}`
                };
            } else {
                return {
                    success: false,
                    message: 'لم يتم العثور على الجهاز المحدد في قائمة الأجهزة المتصلة. يرجى التأكد من تشغيله وتوصيله بالكمبيوتر.'
                };
            }
        } catch (error) {
            return {
                success: false,
                message: 'فشل الاتصال بالخدمة المحلية للماسح الضوئي. يرجى التأكد من تشغيل تطبيق الخدمة المحلية (Scanner Service).'
            };
        } finally {
            setIsTesting(false);
        }
    };

    return { testConnection, isTesting };
};
