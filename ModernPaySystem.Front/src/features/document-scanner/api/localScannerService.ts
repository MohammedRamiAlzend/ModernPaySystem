export interface DeviceDto {
    id: string;
    name: string;
    description: string;
    status: string;
}

export interface ScanPageDto {
    id: string;
    sessionId: string;
    pageNumber: number;
    fileSize: number;
    width: number;
    height: number;
    format: string;
}

export interface ScanSessionDto {
    id: string;
    name: string;
}

const SCANNER_API_URL = import.meta.env.VITE_SCANNER_API_URL || 'http://localhost:3124/api';

export const localScannerService = {
    async getDevices(): Promise<DeviceDto[]> {
        const response = await fetch(`${SCANNER_API_URL}/devices`);
        if (!response.ok) throw new Error('فشل جلب قائمة الماسحات الضوئية');
        return response.json();
    },

    async createSession(name: string = 'Web Scan Session'): Promise<ScanSessionDto> {
        const response = await fetch(`${SCANNER_API_URL}/sessions`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name })
        });
        if (!response.ok) throw new Error('فشل إنشاء جلسة المسح الضوئي');
        return response.json();
    },

    async scanToSession(
        sessionId: string,
        request: { deviceId: string; dpi: number; colorMode?: string; duplex: boolean; paperSource?: string }
    ): Promise<ScanPageDto[]> {
        const response = await fetch(`${SCANNER_API_URL}/sessions/${sessionId}/scan`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });
        if (!response.ok) {
            const err = await response.json().catch(() => ({}));
            throw new Error(err.message || err.detail || 'فشل عملية المسح الضوئي');
        }
        return response.json();
    },

    async getPageImageBlob(sessionId: string, pageId: string): Promise<Blob> {
        const response = await fetch(`${SCANNER_API_URL}/sessions/${sessionId}/pages/${pageId}/image`);
        if (!response.ok) throw new Error('فشل جلب صورة الصفحة الممسوحة');
        return response.blob();
    }
};
