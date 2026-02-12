import React from 'react';
import { XCircle } from 'lucide-react';
import type { ZipImage } from '@/shared/utils/zip-handler';

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
            <button
                className="absolute top-8 right-8 text-white/50 hover:text-white transition-colors"
                onClick={onClose}
            >
                <XCircle className="w-10 h-10" />
            </button>
            <img
                src={image.url}
                alt={image.name}
                className="max-w-full max-h-full object-contain rounded-lg shadow-2xl animate-in zoom-in-95 duration-300"
            />
            <p className="mt-4 text-white font-bold text-lg">{image.name}</p>
        </div>
    );
};
