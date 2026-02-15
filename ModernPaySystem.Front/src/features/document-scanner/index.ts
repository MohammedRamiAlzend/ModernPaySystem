// Feature: Document Scanner
// Public API exports following FSD conventions

// Types
export * from './model/types';

// Hooks
export * from './model/use-scanner';
export * from './model/use-ocr';

// UI Components
export { OcrScannerContent } from './ui/ocr-scanner-content';
export type { OcrScannerContentProps } from './ui/ocr-scanner-content';

export { ImageGallery } from './ui/image-gallery';
export { ImagePreview } from './ui/image-preview';
export { OcrToolbar } from './ui/ocr-toolbar';
export { OcrTextArea } from './ui/ocr-text-area';
export type { OcrTextAreaRef } from './ui/ocr-text-area';
export { LanguageSelector } from './ui/language-selector';
export { ScannerModal } from './ui/scanner-modal';
