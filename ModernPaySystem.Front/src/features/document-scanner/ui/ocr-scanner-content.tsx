import React, { useEffect, useRef, useState } from 'react';
import { Button } from '@/shared/ui/button';
import { useOcr } from '../model/use-ocr';
import { useScanner } from '../model/use-scanner';
import { ImageMeta } from '../model/types';
import { cn } from '@/shared/lib/utils';

// Sub-components
import { ImageGallery } from './image-gallery';
import { ImagePreview } from './image-preview';
import { OcrToolbar } from './ocr-toolbar';
import { OcrTextArea, OcrTextAreaRef } from './ocr-text-area';
import { LanguageSelector } from './language-selector';

export interface OcrScannerContentProps {
    imageFiles?: ImageMeta[];
    setImageFiles?: React.Dispatch<React.SetStateAction<ImageMeta[]>>;
    activeImageIndex?: number | null;
    setActiveImageIndex?: React.Dispatch<React.SetStateAction<number | null>>;
    onApply?: (ocrText: string, files: ImageMeta[]) => void;
    acceptAllFiles?: boolean;
    hideOcr?: boolean;
    onClose?: () => void;
}

export const OcrScannerContent: React.FC<OcrScannerContentProps> = ({
    imageFiles = [],
    setImageFiles,
    activeImageIndex: propActiveImageIndex,
    setActiveImageIndex: propSetActiveImageIndex,
    onApply,
    acceptAllFiles = false,
    hideOcr = false,
    onClose,
}) => {
    const [ocrLanguage, setOcrLanguage] = useState('ara');
    const { ocrResult, setOcrResult, extract, isPending: isOcrLoading, error: ocrError } = useOcr();
    const { handleScan, isScanning, scanError } = useScanner();

    const [activeImageIndex, setActiveImageIndex] = useState<number | null>(propActiveImageIndex ?? null);
    const [isEditing, setIsEditing] = useState(false);
    const ocrTextAreaRef = useRef<OcrTextAreaRef>(null);

    useEffect(() => {
        if (propActiveImageIndex !== undefined) {
            setActiveImageIndex(propActiveImageIndex);
        }
        setIsEditing(false);
    }, [propActiveImageIndex]);

    useEffect(() => {
        if (propSetActiveImageIndex) propSetActiveImageIndex(activeImageIndex);
    }, [activeImageIndex, propSetActiveImageIndex]);

    // ─────────────────────────────────────────────────────────
    // Utility: Read image metadata
    // ─────────────────────────────────────────────────────────
    const readImageMeta = (file: File): Promise<ImageMeta> =>
        new Promise((resolve) => {
            const url = URL.createObjectURL(file);
            const id = `${file.name}-${file.size}-${Date.now()}-${Math.random().toString(36).slice(2)}`;

            if (!file.type.startsWith('image/')) {
                resolve({ id, file, url, name: file.name, size: file.size, type: file.type });
                return;
            }

            const img = new Image();
            img.onload = () => {
                resolve({ id, file, url, name: file.name, size: file.size, width: img.width, height: img.height, type: file.type });
            };
            img.onerror = () => {
                resolve({ id, file, url, name: file.name, size: file.size, type: file.type });
            };
            img.src = url;
        });

    // ─────────────────────────────────────────────────────────
    // Handlers
    // ─────────────────────────────────────────────────────────
    const handlePickImages = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const files = Array.from(e.target.files || []);
        const allowedFiles = acceptAllFiles ? files : files.filter((f) => f.type.startsWith('image/'));
        if (allowedFiles.length === 0) return;

        const metas = await Promise.all(allowedFiles.map(readImageMeta));
        if (setImageFiles) setImageFiles((prev) => [...prev, ...metas]);
        if (activeImageIndex == null && metas.length > 0) setActiveImageIndex(0);
    };

    const handleScanClick = () => {
        handleScan(async (fileObjects) => {
            const metas = await Promise.all(fileObjects.map(readImageMeta));
            if (setImageFiles) setImageFiles((prev) => [...prev, ...metas]);
            if (activeImageIndex == null && metas.length > 0) setActiveImageIndex(0);
        });
    };

    const handleRemoveImage = (id: string) => {
        if (!setImageFiles) return;
        setImageFiles((prev) => {
            const next = prev.filter((it) => it.id !== id);
            const removed = prev.find((it) => it.id === id);
            if (removed) URL.revokeObjectURL(removed.url);

            if (next.length === 0) setActiveImageIndex(null);
            else setActiveImageIndex((current) => (current != null ? Math.min(current, next.length - 1) : current));
            return next;
        });
    };

    const handleSaveEdit = (blob: Blob) => {
        if (!setImageFiles || activeImageIndex === null) return;

        const currentImg = imageFiles[activeImageIndex];
        const newUrl = URL.createObjectURL(blob);
        const newFile = new File([blob], currentImg.name, { type: blob.type });

        const updatedMeta: ImageMeta = {
            ...currentImg,
            file: newFile,
            url: newUrl,
            size: newFile.size,
        };

        setImageFiles((prev) => {
            const next = [...prev];
            next[activeImageIndex] = updatedMeta;
            return next;
        });
        setIsEditing(false);
    };

    // ─────────────────────────────────────────────────────────
    // OCR Handlers
    // ─────────────────────────────────────────────────────────
    const handleOcrExtractFull = async () => {
        if (activeImageIndex == null) return;
        const file = imageFiles[activeImageIndex]?.file;
        if (file) {
            await extract({ language: ocrLanguage, imageFile: file });
        }
    };

    const insertUnderOcrText = async () => {
        if (activeImageIndex == null) return;
        const file = imageFiles[activeImageIndex]?.file;
        if (!file) return;

        try {
            const data = await extract({ language: ocrLanguage, imageFile: file });
            if (data?.success) {
                const next = ocrResult ? `${ocrResult}\n\n${data.extractedText}` : data.extractedText;
                setOcrResult(next);
                requestAnimationFrame(() => {
                    ocrTextAreaRef.current?.setCaretPosition(next.length);
                });
            }
        } catch (e) {
            console.error(e);
        }
    };

    const replaceOcrSelection = async () => {
        if (activeImageIndex == null) return;
        const file = imageFiles[activeImageIndex]?.file;
        if (!file) return;

        try {
            const data = await extract({ language: ocrLanguage, imageFile: file });
            if (data?.success) {
                const { start, end } = ocrTextAreaRef.current?.getSelection() ?? { start: 0, end: 0 };
                const before = ocrResult.slice(0, start);
                const after = ocrResult.slice(end);
                const next = `${before}${data.extractedText}${after}`;
                setOcrResult(next);

                const caret = (before + data.extractedText).length;
                requestAnimationFrame(() => {
                    ocrTextAreaRef.current?.setCaretPosition(caret);
                });
            }
        } catch (e) {
            console.error(e);
        }
    };

    // ─────────────────────────────────────────────────────────
    // Derived State
    // ─────────────────────────────────────────────────────────
    const activeImage = activeImageIndex !== null ? imageFiles[activeImageIndex] : null;
    const canShowOcrToolbar = !hideOcr && activeImage && activeImage.type.startsWith('image/');

    return (
        <div className='flex flex-col h-full bg-card rounded-3xl overflow-hidden' dir="rtl">
            {/* Content Body */}
            <div className='flex-1 overflow-y-auto p-2 sm:p-4'>
                <div className='grid grid-cols-1 md:grid-cols-3 gap-6'>

                    {/* Column 1: ImageGallery */}
                    <ImageGallery
                        imageFiles={imageFiles}
                        activeIndex={activeImageIndex}
                        onSelect={setActiveImageIndex}
                        onRemove={handleRemoveImage}
                        onPickFiles={handlePickImages}
                        onScan={handleScanClick}
                        isScanning={isScanning}
                        acceptAllFiles={acceptAllFiles}
                        error={scanError || (ocrError ? 'حدث خطأ في النظام' : null)}
                    />

                    {/* Column 2 & 3: Main Area */}
                    <div className='md:col-span-2 flex flex-col gap-4'>
                        {!hideOcr && (
                            <LanguageSelector
                                value={ocrLanguage}
                                onChange={setOcrLanguage}
                            />
                        )}

                        <div className={cn("grid grid-cols-1 gap-4 flex-1 h-full", !hideOcr && "md:grid-cols-2")}>
                            {/* Preview */}
                            <div className="flex flex-col gap-3">
                                <ImagePreview
                                    activeImage={activeImage}
                                    isEditing={isEditing}
                                    onStartEdit={() => setIsEditing(true)}
                                    onCancelEdit={() => setIsEditing(false)}
                                    onSaveEdit={handleSaveEdit}
                                />

                                {canShowOcrToolbar && (
                                    <OcrToolbar
                                        onExtract={handleOcrExtractFull}
                                        onInsertUnder={insertUnderOcrText}
                                        onReplaceSelection={replaceOcrSelection}
                                        isLoading={isOcrLoading}
                                    />
                                )}
                            </div>

                            {/* Textarea */}
                            {!hideOcr && (
                                <OcrTextArea
                                    ref={ocrTextAreaRef}
                                    value={ocrResult}
                                    onChange={setOcrResult}
                                    isLoading={isOcrLoading}
                                />
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Footer */}
            <div className='p-6 border-t bg-muted/5 flex justify-between items-center'>
                <p className="text-[11px] text-muted-foreground">عدد الملفات: {imageFiles.length}</p>
                <div className="flex gap-3">
                    <Button variant='ghost' onClick={onClose} className="rounded-xl px-6">إلغاء</Button>
                    <Button
                        className="rounded-xl px-10 font-bold shadow-lg shadow-primary/20"
                        onClick={() => onApply?.(ocrResult, imageFiles)}
                        disabled={imageFiles.length === 0 && !ocrResult}
                    >
                        تأكيد وإدراج البيانات
                    </Button>
                </div>
            </div>
        </div>
    );
};
