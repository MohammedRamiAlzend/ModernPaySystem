import React from 'react';
import { XCircle, Printer, Download } from 'lucide-react';
import type { ZipImage } from '@/shared/utils/zip-handler';
import { printImage, downloadImage } from '@/shared/utils/image-actions';

interface ImagePreviewProps {
    image: ZipImage | null;
    onClose: () => void;
}

export const ImagePreview: React.FC<ImagePreviewProps> = ({ image, onClose }) => {
    if (!image) return null;

    return (
        <div
            className="fixed inset-0 z-[200] bg-black/95 flex flex-col items-center justify-center p-4 md:p-12 transition-all animate-in fade-in duration-300"
            onClick={onClose}
        >
            <div className="absolute top-6 right-6 flex items-center gap-2 md:gap-4">
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
            
            <div className="relative w-full h-full flex flex-col items-center justify-center pointer-events-none">
                <img
                    src={image.url}
                    alt={image.name}
                    className="max-w-full max-h-[85vh] object-contain rounded-lg shadow-2xl animate-in zoom-in-95 duration-300 pointer-events-auto cursor-default"
                    onClick={(e) => e.stopPropagation()}
                />
                <p className="mt-8 text-white/90 font-bold text-lg md:text-xl bg-white/5 px-6 py-2.5 rounded-2xl backdrop-blur-md border border-white/10 shadow-2xl animate-in slide-in-from-bottom-4 duration-500 pointer-events-auto">
                    {image.name}
                </p>
            </div>
        </div>
    );
};
