import React, { useEffect, useRef, useState } from 'react';
import { Button } from '@/shared/ui/button';
import { useOcr } from '../model/use-ocr';
import { useScanner } from '../model/use-scanner';
import { ImageMeta } from '../model/types';
import Cropper, { ReactCropperElement } from 'react-cropper';
import 'cropperjs/dist/cropper.css';
import {
    RotateCcw,
    RotateCw,
    ZoomIn,
    ZoomOut,
    Check,
    X,
    Edit2,
    Trash2,
    Scan,
    Upload,
    Languages,
    FileText,
    Replace,
    ChevronDown,
} from 'lucide-react';
import { cn } from '@/shared/lib/utils';

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
    const ocrTextAreaRef = useRef<HTMLTextAreaElement>(null);
    const [ocrSelection, setOcrSelection] = useState({ start: 0, end: 0 });
    const [isEditing, setIsEditing] = useState(false);
    const cropperRef = useRef<ReactCropperElement>(null);

    useEffect(() => {
        if (propActiveImageIndex !== undefined) {
            setActiveImageIndex(propActiveImageIndex);
        }
        setIsEditing(false);
    }, [propActiveImageIndex]);

    useEffect(() => {
        if (propSetActiveImageIndex) propSetActiveImageIndex(activeImageIndex);
    }, [activeImageIndex, propSetActiveImageIndex]);

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
                    if (ocrTextAreaRef.current) {
                        ocrTextAreaRef.current.focus();
                        const caret = next.length;
                        ocrTextAreaRef.current.setSelectionRange(caret, caret);
                    }
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
                const { start, end } = ocrSelection;
                const before = ocrResult.slice(0, start);
                const after = ocrResult.slice(end);
                const next = `${before}${data.extractedText}${after}`;
                setOcrResult(next);

                const caret = (before + data.extractedText).length;
                requestAnimationFrame(() => {
                    if (ocrTextAreaRef.current) {
                        ocrTextAreaRef.current.focus();
                        ocrTextAreaRef.current.setSelectionRange(caret, caret);
                        setOcrSelection({ start: caret, end: caret });
                    }
                });
            }
        } catch (e) {
            console.error(e);
        }
    };

    const handleOcrTextSelection = () => {
        const ta = ocrTextAreaRef.current;
        if (!ta) return;
        setOcrSelection({ start: ta.selectionStart ?? 0, end: ta.selectionEnd ?? 0 });
    };

    const handleSaveEdit = () => {
        const cropper = cropperRef.current?.cropper;
        if (!cropper || !setImageFiles || activeImageIndex === null) return;

        cropper.getCroppedCanvas().toBlob((blob) => {
            if (!blob) return;

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
        }, imageFiles[activeImageIndex].file.type);
    };

    return (
        <div className='flex flex-col h-full bg-card rounded-3xl overflow-hidden' dir="rtl">
            {/* Content Body */}
            <div className='flex-1 overflow-y-auto p-2 sm:p-4'>
                <div className='grid grid-cols-1 md:grid-cols-3 gap-6'>

                    {/* Column 1: File List & Upload */}
                    <div className='md:col-span-1 flex flex-col gap-4 border-l pl-0 md:pl-4'>
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
                                        onChange={handlePickImages}
                                        className='absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10'
                                    />
                                    <Button variant="outline" className="w-full gap-2 rounded-xl border-2">
                                        {acceptAllFiles ? 'ملفات' : 'صور'}
                                    </Button>
                                </div>
                                <Button
                                    variant="secondary"
                                    onClick={handleScanClick}
                                    disabled={isScanning}
                                    className="gap-2 rounded-xl"
                                >
                                    <Scan className={cn("h-4 w-4", isScanning && "animate-spin")} />
                                    {isScanning ? 'جارٍ...' : 'سكان'}
                                </Button>
                            </div>
                            {(scanError || ocrError) && (
                                <div className='text-destructive text-[11px] p-2 bg-destructive/5 border border-destructive/10 rounded-lg'>
                                    {scanError || 'حدث خطأ في النظام'}
                                </div>
                            )}
                        </div>

                        <div className='flex-1 min-h-[250px] md:min-h-[400px] border-2 border-dashed rounded-2xl p-2 space-y-2 overflow-y-auto bg-muted/5'>
                            {imageFiles.length === 0 && (
                                <div className="h-full flex flex-col items-center justify-center text-muted-foreground opacity-40 gap-2">
                                    <FileText className="h-8 w-8" />
                                    <span className="text-xs">المعرض فارغ</span>
                                </div>
                            )}
                            {imageFiles.map((it, idx) => (
                                <div
                                    key={it.id}
                                    onClick={() => setActiveImageIndex(idx)}
                                    className={cn(
                                        "group flex items-center gap-3 p-2 rounded-xl cursor-pointer transition-all border-2",
                                        idx === activeImageIndex
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
                                        onClick={(e) => { e.stopPropagation(); handleRemoveImage(it.id); }}
                                    >
                                        <Trash2 className="h-4 w-4" />
                                    </Button>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Column 2 & 3: Main Area (Language + Sub-Grid) */}
                    <div className='md:col-span-2 flex flex-col gap-4'>
                        {!hideOcr && (
                            <div className='flex items-center gap-4 bg-muted/20 p-3 rounded-2xl border'>
                                <div className="flex items-center gap-2">
                                    <Languages className="h-4 w-4 text-primary" />
                                    <span className="text-xs font-bold">لغة الاستخراج:</span>
                                </div>
                                <div className="relative group">
                                    <select
                                        className="appearance-none bg-background border-2 rounded-xl px-4 py-1.5 pr-10 text-xs font-bold focus:ring-2 ring-primary transition-all outline-none cursor-pointer"
                                        value={ocrLanguage}
                                        onChange={(e) => setOcrLanguage(e.target.value)}
                                    >
                                        <option value="ara">اللغة العربية</option>
                                        <option value="eng">English Language</option>
                                    </select>
                                    <ChevronDown className="absolute left-3 top-1/2 -translate-y-1/2 h-3 w-3 pointer-events-none opacity-50" />
                                </div>
                            </div>
                        )}

                        <div className={cn("grid grid-cols-1 gap-4 flex-1 h-full", !hideOcr && "md:grid-cols-2")}>
                            {/* Preview */}
                            <div className="flex flex-col gap-3">
                                <div className="flex items-center justify-between px-1">
                                    <span className="text-xs font-bold text-muted-foreground">
                                        {isEditing ? 'وضعية التحرير' : 'معاينة الملف'}
                                    </span>
                                    {activeImageIndex !== null && !isEditing && imageFiles[activeImageIndex].type.startsWith('image/') && (
                                        <Button variant="ghost" size="sm" className="h-7 text-[11px] gap-1.5 rounded-lg" onClick={() => setIsEditing(true)}>
                                            <Edit2 className="h-3 w-3" /> تعديل الصورة
                                        </Button>
                                    )}
                                </div>

                                <div className="flex-1 min-h-[300px] md:min-h-[400px] bg-accent/20 rounded-3xl overflow-hidden relative flex items-center justify-center border-2 border-dashed border-primary/10">
                                    {activeImageIndex !== null ? (
                                        isEditing ? (
                                            <div className="w-full h-full flex flex-col">
                                                <Cropper
                                                    src={imageFiles[activeImageIndex].url}
                                                    style={{ height: '100%', width: '100%' }}
                                                    initialAspectRatio={NaN}
                                                    guides={true}
                                                    ref={cropperRef}
                                                    viewMode={1}
                                                    background={false}
                                                    responsive={true}
                                                    autoCropArea={1}
                                                />
                                                <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-background/90 backdrop-blur shadow-2xl rounded-2xl p-2 flex gap-1 border border-primary/20">
                                                    <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.rotate(-90)}><RotateCcw className="h-4 w-4" /></Button>
                                                    <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.rotate(90)}><RotateCw className="h-4 w-4" /></Button>
                                                    <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.zoom(0.1)}><ZoomIn className="h-4 w-4" /></Button>
                                                    <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.zoom(-0.1)}><ZoomOut className="h-4 w-4" /></Button>
                                                    <div className="w-px bg-border mx-1" />
                                                    <Button variant="ghost" size="icon" className="text-primary hover:text-primary/80" onClick={handleSaveEdit}><Check className="h-5 w-5" /></Button>
                                                    <Button variant="ghost" size="icon" className="text-destructive hover:text-destructive/80" onClick={() => setIsEditing(false)}><X className="h-5 w-5" /></Button>
                                                </div>
                                            </div>
                                        ) : (
                                            <div className="w-full h-full p-4 flex flex-col items-center justify-center">
                                                {imageFiles[activeImageIndex].type.startsWith('image/') ? (
                                                    <img src={imageFiles[activeImageIndex].url} alt="" className="max-w-full max-h-full object-contain rounded-xl shadow-lg" />
                                                ) : (
                                                    <div className="flex flex-col items-center gap-4 text-muted-foreground opacity-30">
                                                        <FileText className="h-20 w-20" />
                                                        <span className="font-bold text-sm">{imageFiles[activeImageIndex].name}</span>
                                                    </div>
                                                )}
                                            </div>
                                        )
                                    ) : (
                                        <div className="text-muted-foreground opacity-20 flex flex-col items-center gap-2">
                                            <Upload className="h-16 w-16" />
                                            <span className="text-sm font-bold">لا يوجد ملف مختار</span>
                                        </div>
                                    )}
                                </div>

                                {!hideOcr && activeImageIndex !== null && imageFiles[activeImageIndex].type.startsWith('image/') && (
                                    <div className="grid grid-cols-2 lg:grid-cols-3 gap-2">
                                        <Button
                                            onClick={handleOcrExtractFull}
                                            disabled={isOcrLoading}
                                            className="rounded-xl text-xs h-10 gap-2"
                                        >
                                            <Scan className={cn("h-4 w-4", isOcrLoading && "animate-spin")} />
                                            {isOcrLoading ? 'جاري الاستخراج...' : 'استخراج جديد'}
                                        </Button>
                                        <Button
                                            variant="secondary"
                                            onClick={insertUnderOcrText}
                                            disabled={isOcrLoading}
                                            className="rounded-xl text-xs h-10"
                                        >
                                            إدراج تحت النص
                                        </Button>
                                        <Button
                                            variant="outline"
                                            onClick={replaceOcrSelection}
                                            disabled={isOcrLoading}
                                            className="rounded-xl text-xs h-10 gap-2 col-span-2 lg:col-span-1 border-2"
                                        >
                                            <Replace className="h-4 w-4" />
                                            استبدال المحدد
                                        </Button>
                                    </div>
                                )}
                            </div>

                            {/* Textarea */}
                            {!hideOcr && (
                                <div className="flex flex-col gap-3">
                                    <span className="text-xs font-bold text-muted-foreground px-1">
                                        النص المستخلص (قابل للتحرير)
                                    </span>
                                    <div className="flex-1 relative flex flex-col">
                                        <textarea
                                            className="flex-1 w-full bg-accent/10 rounded-3xl p-5 text-sm font-medium focus:ring-2 ring-primary/20 transition-all outline-none border-2 border-transparent focus:border-primary/20 resize-none leading-relaxed"
                                            placeholder="سيظهر النص المستخرج هنا..."
                                            value={ocrResult}
                                            ref={ocrTextAreaRef}
                                            onChange={(e) => setOcrResult(e.target.value)}
                                            onSelect={handleOcrTextSelection}
                                            onKeyUp={handleOcrTextSelection}
                                            onClick={handleOcrTextSelection}
                                        />
                                        {isOcrLoading && (
                                            <div className="absolute inset-0 bg-background/50 backdrop-blur-[1px] flex items-center justify-center rounded-3xl">
                                                <div className="flex flex-col items-center gap-2">
                                                    <div className="w-8 h-8 rounded-full border-4 border-primary/20 border-t-primary animate-spin" />
                                                    <span className="text-[10px] font-bold text-primary animate-pulse">جاري المعالجة الرقمية...</span>
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </div>

            {/* Footer Logic (Moved out, will be in the actual modal footer) */}
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
