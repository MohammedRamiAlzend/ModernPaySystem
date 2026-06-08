import React from 'react';
import { createPortal } from 'react-dom';
import { XCircle, Printer, Download, FileIcon } from 'lucide-react';
import type { ZipFile } from '@/shared/utils/zip-handler';
import { printImage, downloadImage } from '@/shared/utils/image-actions';
import { DocxPreview, ExcelPreview } from '@/features/archiving/ui/DocumentPreviewRenderer';

interface ImagePreviewProps {
    image: ZipFile | null;
    onClose: () => void;
}

export const ImagePreview: React.FC<ImagePreviewProps> = ({ image, onClose }) => {
    if (!image) return null;

    return createPortal(
        <div
            className="fixed inset-0 z-[200] bg-black/95 flex flex-col items-center justify-center p-4 md:p-12 transition-all animate-in fade-in duration-300"
            style={{ pointerEvents: 'auto' }}
            onClick={onClose}
        >
            <div className="absolute top-6 right-6 flex items-center gap-2 md:gap-4">
                {image.type === 'image' && (
                    <button
                        className="text-white/50 hover:text-white transition-all p-2 hover:bg-white/10 rounded-xl flex flex-col items-center gap-1 group"
                        onClick={(e) => {
                            e.stopPropagation();
                            printImage(image.url, `صورة - ${image.name}`);
                        }}
                        title="طباعة"
                    >
                        <Printer className="w-6 h-6 md:w-8 md:h-8 group-hover:scale-110 transition-transform" />
                        <span className="text-[10px] font-bold opacity-0 group-hover:opacity-100 transition-opacity">طباعة</span>
                    </button>
                )}
                <button
                    className="text-white/50 hover:text-white transition-all p-2 hover:bg-white/10 rounded-xl flex flex-col items-center gap-1 group"
                    onClick={(e) => {
                        e.stopPropagation();
                        downloadImage(image.url, image.name);
                    }}
                    title="تنزيل"
                >
                    <Download className="w-6 h-6 md:w-8 md:h-8 group-hover:scale-110 transition-transform" />
                    <span className="text-[10px] font-bold opacity-0 group-hover:opacity-100 transition-opacity">تنزيل</span>
                </button>
                <div className="w-px h-8 bg-white/10 mx-2 hidden md:block" />
                <button
                    className="text-white/50 hover:text-white transition-all p-2 hover:bg-white/20 rounded-full group"
                    onClick={onClose}
                    title="إغلاق"
                >

                    <XCircle className="w-8 h-8 md:w-10 md:h-10 group-hover:rotate-90 transition-transform duration-300" />
                </button>
            </div>

            <div className="relative w-full h-full flex flex-col items-center justify-center pointer-events-none gap-4">
                {image.type === 'image' && (
                    <img
                        src={image.url}
                        alt={image.name}
                        className="max-w-full max-h-[75vh] object-contain rounded-lg shadow-2xl animate-in zoom-in-95 duration-300 pointer-events-auto cursor-default"
                        onClick={(e) => e.stopPropagation()}
                    />
                )}
                {image.type === 'pdf' && (
                    <iframe
                        src={image.url}
                        className="w-full max-w-5xl h-[75vh] border-none rounded-xl bg-white animate-in zoom-in-95 duration-300 pointer-events-auto"
                        title={image.name}
                        onClick={(e) => e.stopPropagation()}
                    />
                )}
                {image.type === 'docx' && (
                    <div
                        className="w-full max-w-5xl h-[75vh] overflow-y-auto rounded-xl bg-background border border-border p-4 pointer-events-auto animate-in zoom-in-95 duration-300 text-right"
                        onClick={(e) => e.stopPropagation()}
                        style={{ direction: 'rtl' }}
                    >
                        <DocxPreview blobUrl={image.url} />
                    </div>
                )}
                {image.type === 'xlsx' && (
                    <div
                        className="w-full max-w-5xl h-[75vh] overflow-y-auto rounded-xl bg-background border border-border p-4 pointer-events-auto animate-in zoom-in-95 duration-300 text-right"
                        onClick={(e) => e.stopPropagation()}
                        style={{ direction: 'rtl' }}
                    >
                        <ExcelPreview blobUrl={image.url} />
                    </div>
                )}
                {image.type === 'other' && (
                    <div className="flex flex-col items-center justify-center p-12 bg-white/5 border border-white/10 rounded-3xl pointer-events-auto gap-4">
                        <FileIcon className="w-16 h-16 text-white/40" />
                        <span className="text-white font-bold">{image.name}</span>
                        <span className="text-white/60 text-xs">معاينة هذا النوع من الملفات غير مدعومة. يرجى تحميله لعرضه.</span>
                    </div>
                )}
                <p className="text-white/90 font-bold text-sm md:text-base bg-white/5 px-6 py-2 rounded-2xl backdrop-blur-md border border-white/10 shadow-2xl animate-in slide-in-from-bottom-4 duration-500 pointer-events-auto">
                    {image.name}
                </p>
            </div>
        </div>,
        document.body
    );
};
