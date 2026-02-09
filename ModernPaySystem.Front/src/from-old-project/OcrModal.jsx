import React, { useEffect, useRef, useState } from 'react';
import Button from '../../../../components/common/Button';
import ocr from '../../../../services/api/ocr';
import { handleScan } from '../../../../utils/scanner';
import Cropper from 'react-cropper';
import 'cropperjs/dist/cropper.css';
import {
    MdRotateLeft,
    MdRotateRight,
    MdZoomIn,
    MdZoomOut,
    MdSwapHoriz,
    MdSwapVert,
    MdCheck,
    MdClose,
    MdEdit,
    MdRefresh,
    MdInsertDriveFile,
    MdVideoLibrary,
    MdPictureAsPdf,
    MdAudioFile
} from 'react-icons/md';

const OcrModal = ({
    isOpen,
    onClose,
    imageFiles = [],
    setImageFiles,
    activeImageIndex: propActiveImageIndex,
    setActiveImageIndex: propSetActiveImageIndex,
    onApplyOcrToSubject,
    acceptAllFiles = false,
    hideOcr = false,
}) => {
    const [ocrLanguage, setOcrLanguage] = useState('ara');
    const [ocrResult, setOcrResult] = useState('');
    const [isOcrLoading, setIsOcrLoading] = useState(false);
    const [ocrError, setOcrError] = useState('');

    const [isScanning, setIsScanning] = useState(false);
    const [scanError, setScanError] = useState('');

    const [activeImageIndex, setActiveImageIndex] = useState(propActiveImageIndex ?? null);

    const ocrTextAreaRef = useRef(null);
    const [ocrSelection, setOcrSelection] = useState({ start: 0, end: 0 });

    const [isEditing, setIsEditing] = useState(false);
    const cropperRef = useRef(null);

    useEffect(() => {
        if (propActiveImageIndex !== undefined) {
            setActiveImageIndex(propActiveImageIndex);
        }
        setIsEditing(false);
    }, [propActiveImageIndex]);

    // allow parent to control active index
    useEffect(() => {
        if (propSetActiveImageIndex) propSetActiveImageIndex(activeImageIndex);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [activeImageIndex]);

    if (!isOpen) return null;

    const readImageMeta = (file) =>
        new Promise((resolve) => {
            const url = URL.createObjectURL(file);
            if (!file.type.startsWith('image/')) {
                resolve({
                    id: `${file.name}-${file.size}-${Date.now()}-${Math.random().toString(36).slice(2)}`,
                    file,
                    url,
                    name: file.name,
                    size: file.size,
                    type: file.type,
                });
                return;
            }
            const img = new Image();
            img.onload = () => {
                resolve({
                    id: `${file.name}-${file.size}-${Date.now()}-${Math.random().toString(36).slice(2)}`,
                    file,
                    url,
                    name: file.name,
                    size: file.size,
                    width: img.width,
                    height: img.height,
                    type: file.type,
                });
            };
            img.onerror = () => {
                resolve({
                    id: `${file.name}-${file.size}-${Date.now()}-${Math.random().toString(36).slice(2)}`,
                    file,
                    url,
                    name: file.name,
                    size: file.size,
                    width: undefined,
                    height: undefined,
                    type: file.type,
                });
            };
            img.src = url;
        });

    const handlePickImages = async (e) => {
        const files = Array.from(e.target.files || []);
        const allowedFiles = acceptAllFiles ? files : files.filter((f) => f.type.startsWith('image/'));
        if (allowedFiles.length === 0) return;

        const metas = await Promise.all(allowedFiles.map(readImageMeta));
        if (setImageFiles) setImageFiles((prev) => [...prev, ...metas]);
        if (activeImageIndex == null && metas.length > 0) setActiveImageIndex(0);
    };

    const handleScanClick = () => {
        if (!window.scanner) {
            setScanError('مكتبة المسح الضوئي غير متوفرة. يرجى التأكد من تحميل السكريبت.');
            return;
        }
        setIsScanning(true);
        setScanError('');

        handleScan(
            async (fileObjects) => {
                try {
                    const metas = await Promise.all(fileObjects.map(readImageMeta));
                    if (setImageFiles) setImageFiles((prev) => [...prev, ...metas]);
                    setActiveImageIndex((currentIndex) => (currentIndex == null && metas.length > 0 ? 0 : currentIndex));
                    setIsScanning(false);
                } catch (error) {
                    console.error('خطأ في معالجة الصور الممسوحة:', error);
                    setScanError('حدث خطأ أثناء معالجة الصور الممسوحة');
                    setIsScanning(false);
                }
            },
            (errorMessage) => {
                setScanError(errorMessage);
                setIsScanning(false);
            }
        );
    };

    const handleSelectImage = (idx) => {
        setActiveImageIndex(idx);
    };

    const handleRemoveImage = (id) => {
        if (!setImageFiles) return;
        setImageFiles((prev) => {
            const next = prev.filter((it) => it.id !== id);
            const removed = prev.find((it) => it.id === id);
            if (removed) {
                try {
                    URL.revokeObjectURL(removed.url);
                } catch (_) { }
            }
            if (next.length === 0) setActiveImageIndex(null);
            else setActiveImageIndex((current) => (current != null ? Math.min(current, next.length - 1) : current));
            return next;
        });
    };

    const extractTextFromActiveImage = async () => {
        if (activeImageIndex == null) {
            setOcrError('يرجى اختيار صورة من المعرض أولاً');
            return null;
        }
        const candidateFile = (imageFiles && imageFiles[activeImageIndex])?.file;
        if (!candidateFile) {
            setOcrError('تعذر العثور على ملف الصورة المحددة');
            return null;
        }
        try {
            setIsOcrLoading(true);
            setOcrError('');
            const data = await ocr.extractText({ language: ocrLanguage, imageFile: candidateFile });
            if (data?.success) return data.extractedText || '';
            setOcrError('تعذر استخراج النص من الصورة المحددة');
            return null;
        } catch (e) {
            setOcrError('حدث خطأ أثناء استخراج النص');
            return null;
        } finally {
            setIsOcrLoading(false);
        }
    };

    const handleOcrExtract = async () => {
        const extracted = await extractTextFromActiveImage();
        if (extracted == null) return;
        setOcrResult(extracted);
    };

    const replaceOcrSelection = async () => {
        const extracted = await extractTextFromActiveImage();
        if (extracted == null) return;
        const { start, end } = ocrSelection;
        const before = ocrResult.slice(0, start);
        const after = ocrResult.slice(end);
        const next = `${before}${extracted}${after}`;
        setOcrResult(next);
        const caret = (before + extracted).length;
        requestAnimationFrame(() => {
            if (ocrTextAreaRef.current) {
                ocrTextAreaRef.current.focus();
                ocrTextAreaRef.current.setSelectionRange(caret, caret);
                setOcrSelection({ start: caret, end: caret });
            }
        });
    };

    const insertUnderOcrText = async () => {
        const extracted = await extractTextFromActiveImage();
        if (extracted == null) return;
        const next = `${ocrResult}\n\n${extracted}`;
        setOcrResult(next);
        requestAnimationFrame(() => {
            if (ocrTextAreaRef.current) {
                ocrTextAreaRef.current.focus();
                const caret = next.length;
                ocrTextAreaRef.current.setSelectionRange(caret, caret);
                setOcrSelection({ start: caret, end: caret });
            }
        });
    };

    const handleOcrTextSelection = () => {
        const ta = ocrTextAreaRef.current;
        if (!ta) return;
        setOcrSelection({ start: ta.selectionStart ?? 0, end: ta.selectionEnd ?? 0 });
    };

    const handleSaveEdit = () => {
        return new Promise((resolve) => {
            const cropper = cropperRef.current?.cropper;
            if (!cropper) {
                resolve(imageFiles);
                return;
            }

            cropper.getCroppedCanvas().toBlob((blob) => {
                if (!blob) {
                    resolve(imageFiles);
                    return;
                }
                const currentImg = imageFiles[activeImageIndex];
                const newUrl = URL.createObjectURL(blob);

                const newFile = new File([blob], currentImg.name, { type: blob.type });

                const updatedMeta = {
                    ...currentImg,
                    file: newFile,
                    url: newUrl,
                    size: newFile.size,
                };

                setImageFiles((prev) => {
                    const next = [...prev];
                    next[activeImageIndex] = updatedMeta;
                    // Resolve with the new list to avoid closure issues
                    resolve(next);
                    return next;
                });

                setIsEditing(false);
            }, (imageFiles[activeImageIndex]?.file?.type) || 'image/jpeg');
        });
    };

    const rotateLeft = () => cropperRef.current?.cropper.rotate(-90);
    const rotateRight = () => cropperRef.current?.cropper.rotate(90);
    const zoomIn = () => cropperRef.current?.cropper.zoom(0.1);
    const zoomOut = () => cropperRef.current?.cropper.zoom(-0.1);

    const flipHorizontal = () => {
        const cropper = cropperRef.current?.cropper;
        if (cropper) cropper.scaleX(-cropper.getData().scaleX);
    };

    const flipVertical = () => {
        const cropper = cropperRef.current?.cropper;
        if (cropper) cropper.scaleY(-cropper.getData().scaleY);
    };

    const resetCrop = () => cropperRef.current?.cropper.reset();

    const applyToSubject = async () => {
        let filesToApply = imageFiles;
        if (isEditing) {
            // Wait for current edit to save before proceeding
            filesToApply = await handleSaveEdit();
        }
        if (onApplyOcrToSubject) onApplyOcrToSubject(ocrResult, filesToApply);
        onClose && onClose();
    };

    return (
        <div className='fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4'>
            <div className='w-full max-w-5xl bg-surface text-text rounded-lg shadow-lg p-4'>
                <div className='flex items-center justify-between mb-3'>
                    <h3 className='text-lg font-semibold'>{hideOcr ? 'إدارة ومعاينة الملفات' : 'استخراج نص بـ OCR'}</h3>
                    <button className='text-sm text-text hover:text-text/70' onClick={onClose}>إغلاق</button>
                </div>
                <div className='grid grid-cols-1 md:grid-cols-3 gap-3'>
                    <div className='md:col-span-1 flex flex-col gap-3'>
                        <div className='flex flex-col gap-2'>
                            <label className='block text-sm mb-1'>اختر صور</label>
                            <div className="flex gap-2">
                                <div className="relative flex-1">
                                    <input
                                        type='file'
                                        accept={acceptAllFiles ? '*' : 'image/*'}
                                        multiple
                                        onChange={handlePickImages}
                                        className='absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10'
                                    />
                                    <button
                                        type='button'
                                        className='border border-linesDefault rounded-md p-2 text-text text-sm bg-background w-full text-center hover:bg-surface transition-colors'
                                    >
                                        {acceptAllFiles ? 'اختر ملفات' : 'اختر الصور'}
                                    </button>
                                </div>
                                <button
                                    type='button'
                                    onClick={handleScanClick}
                                    disabled={isScanning}
                                    className='border border-linesDefault rounded-md p-2 text-text text-sm bg-background hover:bg-surface transition-colors disabled:opacity-50 disabled:cursor-not-allowed whitespace-nowrap'
                                >
                                    {isScanning ? 'جارٍ المسح...' : 'مسح ضوئي'}
                                </button>
                            </div>
                            {scanError && (
                                <div className='text-error text-xs mt-1'>{scanError}</div>
                            )}
                        </div>
                        <div className='border border-linesDefault rounded-md p-2 max-h-72 overflow-y-auto'>
                            {(!imageFiles || imageFiles.length === 0) && (
                                <div className='text-xs text-text/70'>لا توجد صور مضافة بعد.</div>
                            )}
                            <div className='flex flex-col gap-2'>
                                {(imageFiles || []).map((it, idx) => (
                                    <div
                                        key={it.id}
                                        className={`flex items-center gap-2 p-2 rounded cursor-pointer ${idx === activeImageIndex ? 'bg-primary/10 border border-primary' : 'hover:bg-surface/60'}`}
                                        onClick={() => handleSelectImage(idx)}
                                        aria-selected={idx === activeImageIndex}
                                    >
                                        {it.type?.startsWith('image/') ? (
                                            <img src={it.url} alt={it.name} className='w-14 h-14 object-cover rounded border border-linesDefault' />
                                        ) : (
                                            <div className='w-14 h-14 flex items-center justify-center bg-background rounded border border-linesDefault text-primary'>
                                                {it.type?.includes('pdf') ? <MdPictureAsPdf size={32} /> :
                                                    it.type?.includes('video') ? <MdVideoLibrary size={32} /> :
                                                        it.type?.includes('audio') ? <MdAudioFile size={32} /> :
                                                            <MdInsertDriveFile size={32} />}
                                            </div>
                                        )}
                                        <div className='flex-1 min-w-0'>
                                            <div className='text-xs truncate'>{it.name}</div>
                                            <div className='text-[10px] text-text/70'>{it.width ?? '?'}×{it.height ?? '?'} · {(it.size / 1024).toFixed(0)}KB</div>
                                        </div>
                                        <button type='button' className='text-error text-xs' onClick={(ev) => { ev.stopPropagation(); handleRemoveImage(it.id); }}>حذف</button>
                                    </div>
                                ))}
                            </div>
                        </div>
                    </div>

                    <div className='md:col-span-2 flex flex-col gap-3'>
                        {!hideOcr && (
                            <div className='flex flex-col md:flex-row gap-2'>
                                <div className='flex-1'>
                                    <label className='block text-sm mb-1'>اللغة</label>
                                    <select
                                        className='w-full border border-linesDefault rounded-md p-2 text-text text-sm bg-background'
                                        value={ocrLanguage}
                                        onChange={(e) => setOcrLanguage(e.target.value)}
                                    >
                                        <option value='ara'>عربي </option>
                                        <option value='eng'>English</option>
                                    </select>
                                </div>
                            </div>
                        )}

                        <div className={`grid grid-cols-1 ${hideOcr ? '' : 'md:grid-cols-2'} gap-3`}>
                            <div className='border border-linesDefault rounded-md p-2 flex flex-col transition-all duration-300'>
                                <div className='flex justify-between items-center mb-2'>
                                    <div className='text-sm font-semibold'>
                                        {isEditing ? 'تعديل الصورة' : 'معاينة الملف المختار'}
                                    </div>
                                    {activeImageIndex != null && !isEditing && (imageFiles[activeImageIndex]?.type?.startsWith('image/')) && (
                                        <button
                                            type="button"
                                            className='text-primary hover:bg-primary/10 p-1 rounded transition-colors flex items-center gap-1 text-xs'
                                            onClick={() => setIsEditing(true)}
                                        >
                                            <MdEdit size={16} /> تعديل
                                        </button>
                                    )}
                                    {isEditing && (
                                        <div className="flex gap-1">
                                            <button onClick={() => setIsEditing(false)} className="text-text/70 hover:text-text p-1" title="إلغاء">
                                                <MdClose size={20} />
                                            </button>
                                            <button onClick={handleSaveEdit} className="text-secondary hover:text-secondary/80 p-1" title="حفظ">
                                                <MdCheck size={20} />
                                            </button>
                                        </div>
                                    )}
                                </div>

                                {activeImageIndex != null ? (
                                    isEditing ? (
                                        <div className='flex flex-col gap-2'>
                                            <div className='relative w-full h-64 bg-black/5 rounded overflow-hidden'>
                                                <Cropper
                                                    src={(imageFiles && imageFiles[activeImageIndex])?.url}
                                                    style={{ height: '100%', width: '100%' }}
                                                    initialAspectRatio={NaN}
                                                    guides={true}
                                                    ref={cropperRef}
                                                    viewMode={1}
                                                    minCropBoxHeight={10}
                                                    minCropBoxWidth={10}
                                                    background={false}
                                                    responsive={true}
                                                    autoCropArea={1}
                                                    checkOrientation={false}
                                                    className="cropper-container"
                                                />
                                            </div>
                                            <div className="flex items-center justify-center gap-4 p-1 bg-surface border border-linesDefault rounded-md overflow-x-auto">
                                                <button type="button" onClick={rotateRight} className="p-1.5 hover:bg-background rounded text-text" title="تدوير يمين"><MdRotateRight size={20} /></button>
                                                <button type="button" onClick={rotateLeft} className="p-1.5 hover:bg-background rounded text-text" title="تدوير يسار"><MdRotateLeft size={20} /></button>
                                                <div className="w-px h-6 bg-linesDefault mx-1"></div>
                                                <button type="button" onClick={flipHorizontal} className="p-1.5 hover:bg-background rounded text-text" title="عكس أفقي"><MdSwapHoriz size={20} /></button>
                                                <button type="button" onClick={flipVertical} className="p-1.5 hover:bg-background rounded text-text" title="عكس عمودي"><MdSwapVert size={20} /></button>
                                                <div className="w-px h-6 bg-linesDefault mx-1"></div>
                                                <button type="button" onClick={zoomIn} className="p-1.5 hover:bg-background rounded text-text" title="تكبير"><MdZoomIn size={20} /></button>
                                                <button type="button" onClick={zoomOut} className="p-1.5 hover:bg-background rounded text-text" title="تصغير"><MdZoomOut size={20} /></button>
                                                <div className="w-px h-6 bg-linesDefault mx-1"></div>
                                                <button type="button" onClick={resetCrop} className="p-1.5 hover:bg-background rounded text-text" title="إعادة تعيين"><MdRefresh size={20} /></button>
                                            </div>
                                        </div>
                                    ) : (
                                        <>
                                            {imageFiles[activeImageIndex]?.type?.startsWith('image/') ? (
                                                <>
                                                    <img src={(imageFiles && imageFiles[activeImageIndex])?.url} alt={(imageFiles && imageFiles[activeImageIndex])?.name} className='w-full max-h-64 object-contain rounded' />
                                                    <div className='text-[11px] text-text/70 mt-1'>
                                                        {(imageFiles && imageFiles[activeImageIndex])?.name} · {(imageFiles && imageFiles[activeImageIndex])?.width ?? '?'}×{(imageFiles && imageFiles[activeImageIndex])?.height ?? '?'}
                                                    </div>
                                                </>
                                            ) : (
                                                <div className='w-full h-64 flex flex-col items-center justify-center bg-background rounded-lg border border-dashed border-linesDefault gap-4'>
                                                    <div className='text-primary'>
                                                        {imageFiles[activeImageIndex]?.type?.includes('pdf') ? <MdPictureAsPdf size={80} /> :
                                                            imageFiles[activeImageIndex]?.type?.includes('video') ? <MdVideoLibrary size={80} /> :
                                                                imageFiles[activeImageIndex]?.type?.includes('audio') ? <MdAudioFile size={80} /> :
                                                                    <MdInsertDriveFile size={80} />}
                                                    </div>
                                                    <div className='text-center'>
                                                        <div className='font-bold text-sm'>{imageFiles[activeImageIndex]?.name}</div>
                                                        <div className='text-xs text-text/60'>{(imageFiles[activeImageIndex]?.size / 1024 / 1024).toFixed(2)} MB</div>
                                                    </div>
                                                </div>
                                            )}
                                            <div className='flex gap-2 mt-2'>
                                                {imageFiles[activeImageIndex]?.type?.startsWith('image/') && !hideOcr && (
                                                    <>
                                                        <Button type='button' className='bg-primary hover:bg-primary/90' onClick={handleOcrExtract}>
                                                            {isOcrLoading ? 'جاري الاستخراج...' : <span className='text-info'>إستخراج</span>}
                                                        </Button>
                                                        <Button type='button' className='bg-secondary hover:bg-secondary/90' onClick={insertUnderOcrText}>إدراج تحت النص</Button>
                                                        <Button type='button' className='bg-surface hover:bg-surface/80 text-text border border-linesDefault' onClick={replaceOcrSelection}>استبدال النص المحدد</Button>
                                                    </>
                                                )}
                                            </div>
                                        </>
                                    )
                                ) : (
                                    <div className='text-xs text-text/70'>اختر صورة من المعرض لعرضها هنا.</div>
                                )}
                            </div>
                            {!hideOcr && (
                                <div className='flex flex-col'>
                                    {/* <label className='block text-sm mb-1'>النص المستخرج (قابل للتعديل)</label> */}
                                    <textarea
                                        rows={12}
                                        value={ocrResult}
                                        onChange={(e) => setOcrResult(e.target.value)}
                                        ref={ocrTextAreaRef}
                                        onSelect={handleOcrTextSelection}
                                        onKeyUp={handleOcrTextSelection}
                                        onClick={handleOcrTextSelection}
                                        className='w-full border border-linesDefault rounded-md p-3  text-text text-sm bg-background min-h-48 whitespace-pre-wrap'
                                        placeholder='سيظهر النص المستخرج هنا بعد الضغط على استخراج'
                                    />
                                </div>
                            )}
                        </div>
                        {ocrError && <div className='text-error text-xs'>{ocrError}</div>}
                    </div>
                </div>
                <div className='flex justify-end gap-2 mt-4'>
                    <Button type='button' className='bg-surface hover:bg-surface/80 text-text border border-linesDefault' onClick={onClose}>إغلاق</Button>
                    <Button
                        type='button'
                        className='bg-barbackground text-text'
                        onClick={applyToSubject}
                        disabled={(imageFiles || []).length === 0 && !ocrResult}
                    >
                        <span className='text-info font-bold'>
                            {(imageFiles || []).length > 0 ? `تأكيد وإدراج (${imageFiles.length}) ملفات` : 'إدراج النص'}
                        </span>
                    </Button>
                </div>
            </div>
        </div>
    );
};

export default OcrModal;
