import React from 'react';
import { Image as ImageIcon, Loader2, Maximize2 } from 'lucide-react';
import type { ZipImage } from '@/shared/utils/zip-handler';

interface ResponseDetailsAttachmentsProps {
    attachmentsCount: number;
    isLoadingImages: boolean;
    zipImages: ZipImage[];
    setSelectedImage: (img: ZipImage) => void;
}

export const ResponseDetailsAttachments: React.FC<ResponseDetailsAttachmentsProps> = ({
    attachmentsCount,
    isLoadingImages,
    zipImages,
    setSelectedImage
}) => {
    if (attachmentsCount === 0) return null;

    return (
        <div className="mt-8 pt-8 border-t-2 border-gray-100">
            <div className="flex items-center justify-between mb-6">
                <h3 className="text-xl font-bold flex items-center gap-2">
                    <ImageIcon className="w-6 h-6 text-primary" />
                    المرفقات والصور
                </h3>
                <span className="px-3 py-1 bg-primary/10 text-primary text-xs font-bold rounded-full">
                    {attachmentsCount} ملف
                </span>
            </div>

            {isLoadingImages ? (
                <div className="flex flex-col items-center justify-center py-12 bg-muted/20 rounded-3xl border-2 border-dashed border-primary/20">
                    <Loader2 className="w-10 h-10 animate-spin text-primary mb-4" />
                    <p className="text-muted-foreground font-medium">جاري استخراج الصور من الملف المضغوط...</p>
                </div>
            ) : zipImages.length > 0 ? (
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                    {zipImages.map((img, idx) => (
                        <div
                            key={idx}
                            className="group relative aspect-square rounded-2xl overflow-hidden border-2 border-transparent hover:border-primary transition-all cursor-pointer shadow-md hover:shadow-xl"
                            onClick={() => setSelectedImage(img)}
                        >
                            <img
                                src={img.url}
                                alt={img.name}
                                className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                            />
                            <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                <Maximize2 className="text-white w-8 h-8" />
                            </div>
                            <div className="absolute bottom-0 inset-x-0 p-2 bg-black/60 backdrop-blur-md opacity-0 group-hover:opacity-100 transition-opacity">
                                <p className="text-[10px] text-white truncate text-center font-mono">{img.name}</p>
                            </div>
                        </div>
                    ))}
                </div>
            ) : (
                <div className="p-8 text-center bg-muted/30 rounded-2xl text-muted-foreground">
                    <p>يحتوي الملف على مرفقات غير مرئية (مثل الملفات النصية)، يمكنك تحميلها بالكامل باستخدام الزر في الأسفل.</p>
                </div>
            )}
        </div>
    );
};
