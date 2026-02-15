export interface ImageMeta {
    id: string;
    file: File;
    url: string;
    name: string;
    size: number;
    width?: number;
    height?: number;
    type: string;
}

export interface OcrRequest {
    language: string;
    imageFile: File;
}

export interface OcrResponse {
    success: boolean;
    extractedText: string;
}

export interface Language {
    code: string;
    name: string;
}

export interface SupportedLanguagesResponse {
    success: boolean;
    languages: Language[];
    defaultLanguage: string;
}

export interface ScannerResponse {
    src: string; // base64
}

declare global {
    interface Window {
        scanner?: {
            scan: (
                callback: (successful: boolean, mesg: string, response: any) => void,
                options: any
            ) => void;
            getScannedImages: (
                response: any,
                useBase64: boolean,
                asBlob: boolean
            ) => ScannerResponse[];
        };
    }
}
