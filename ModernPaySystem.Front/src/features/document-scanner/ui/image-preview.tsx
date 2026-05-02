import React, { useRef } from 'react';
import { printImage, downloadImage } from '@/shared/utils/image-actions';
import { Button } from '@/shared/ui/button';
import { ImageMeta } from '../model/types';
import Cropper, { ReactCropperElement } from 'react-cropper';
import 'cropperjs/dist/cropper.css';
import {
    ZoomIn,
    ZoomOut,
    Check,
    X,
    Edit2,
    Upload,
    FileText,
    RefreshCcw,
    Printer,
    Download
} from 'lucide-react';

interface ImagePreviewProps {
    activeImage: ImageMeta | null;
    isEditing: boolean;
    onStartEdit: () => void;
    onCancelEdit: () => void;
    onSaveEdit: (blob: Blob) => void;
}

export const ImagePreview: React.FC<ImagePreviewProps> = ({
    activeImage,
    isEditing,
    onStartEdit,
    onCancelEdit,
    onSaveEdit,
}) => {
    const cropperRef = useRef<ReactCropperElement>(null);

    const handleSave = () => {
        const cropper = cropperRef.current?.cropper;
        if (!cropper) return;

        cropper.getCroppedCanvas().toBlob((blob) => {
            if (blob) onSaveEdit(blob);
        }, activeImage?.file.type);
    };

    return (
        <div className="flex flex-col gap-3">
            <div className="flex items-center justify-between px-1">
                <span className="text-xs font-bold text-muted-foreground">
                    {isEditing ? 'وضعية التحرير' : 'معاينة الملف'}
                </span>
                {activeImage && !isEditing && activeImage.type.startsWith('image/') && (
                    <div className="flex gap-1">
                        <Button variant="ghost" size="sm" className="h-7 text-[10px] gap-1 rounded-lg hover:bg-primary/5 px-2" onClick={() => printImage(activeImage.url)}>
                            <Printer className="h-3 w-3" /> طباعة
                        </Button>
                        <Button variant="ghost" size="sm" className="h-7 text-[10px] gap-1 rounded-lg hover:bg-primary/5 px-2" onClick={() => downloadImage(activeImage.url, activeImage.name)}>
                            <Download className="h-3 w-3" /> تنزيل
                        </Button>
                        <Button variant="ghost" size="sm" className="h-7 text-[10px] gap-1 rounded-lg hover:bg-primary/5 px-2" onClick={onStartEdit}>
                            <Edit2 className="h-3 w-3" /> تعديل
                        </Button>
                    </div>
                )}
            </div>

            <div className="flex-1 min-h-[200px] md:min-h-[300px] bg-accent/20 rounded-3xl overflow-hidden relative flex items-center justify-center border-2 border-dashed border-primary/10">
                {activeImage ? (
                    isEditing ? (
                        <div className="w-full h-[35vh] min-h-[250px] relative bg-black/80 rounded-xl overflow-hidden shadow-inner text-left" dir="ltr">
                            <Cropper
                                src={activeImage.url}
                                style={{ height: '100%', maxHeight: '35vh', width: '100%' }}
                                initialAspectRatio={NaN}
                                guides={true}
                                ref={cropperRef}
                                viewMode={1}
                                dragMode="move"
                                minCropBoxHeight={10}
                                minCropBoxWidth={10}
                                background={false}
                                responsive={true}
                                autoCropArea={1}
                                checkOrientation={false}
                                zoomOnWheel={true}
                            />
                            <div className="absolute bottom-4 left-1/2 -translate-x-1/2 bg-background/90 backdrop-blur shadow-2xl rounded-2xl p-2 flex gap-1 border border-primary/20 z-10" dir="rtl">
                                {/* <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.rotate(-90)} title="تدوير يسار"><RotateCcw className="h-4 w-4" /></Button> */}
                                <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.rotate(180)} title="قلب 180 درجة" className="bg-primary/10 hover:bg-primary/20 text-primary rounded-full w-8 h-8"><RefreshCcw className="h-4 w-4" /></Button>
                                {/* <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.rotate(90)} title="تدوير يمين"><RotateCw className="h-4 w-4" /></Button> */}
                                <div className="w-px h-6 bg-border mx-1" />
                                <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.zoom(0.1)} title="تكبير"><ZoomIn className="h-4 w-4" /></Button>
                                <Button variant="ghost" size="icon" onClick={() => cropperRef.current?.cropper.zoom(-0.1)} title="تصغير"><ZoomOut className="h-4 w-4" /></Button>
                                <div className="w-px h-6 bg-border mx-1" />
                                <Button variant="ghost" size="icon" className="text-primary hover:text-primary/80" onClick={handleSave} title="حفظ"><Check className="h-5 w-5" /></Button>
                                <Button variant="ghost" size="icon" className="text-destructive hover:text-destructive/80" onClick={onCancelEdit} title="إلغاء"><X className="h-5 w-5" /></Button>
                            </div>
                        </div>


                    ) : (
                        <div className="w-full h-[35vh] min-h-[250px] p-4 flex flex-col items-center justify-center">
                            {activeImage.type.startsWith('image/') ? (
                                <img src={activeImage.url} alt="" className="max-w-full max-h-full object-contain rounded-xl shadow-lg" />
                            ) : (
                                <div className="flex flex-col items-center gap-4 text-muted-foreground opacity-30">
                                    <FileText className="h-20 w-20" />
                                    <span className="font-bold text-sm">{activeImage.name}</span>
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
        </div>
    );
};
