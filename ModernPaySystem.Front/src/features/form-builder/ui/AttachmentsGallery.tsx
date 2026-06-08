import { useState } from 'react';
import { 
    Image as ImageIcon, 
    Printer, 
    Download, 
    Loader2, 
    FileText, 
    ChevronDown, 
    ChevronUp,
    FileDown,
    FileSpreadsheet,
    FileIcon,
    Maximize2
} from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { printImage, downloadImage } from '@/shared/utils/image-actions';
import { imagesToPdf } from '@/shared/utils/zip-handler';
import type { ZipFile } from '@/shared/utils/zip-handler';
import { cn } from '@/shared/lib/utils';
import { ImagePreview } from '@/widgets/form-editor/ui/response-details/ImagePreview';

interface AttachmentsGalleryProps {
    files?: ZipFile[];
    images?: ZipFile[]; // fallback for backward compatibility
    isLoading: boolean;
    isAllImages: boolean;
    totalFiles: number;
    title?: string;
    requestId?: string;
    onDownloadAll?: () => void;
    className?: string;
    initialExpanded?: boolean;
}

export const AttachmentsGallery = ({
    files,
    images: propImages,
    isLoading,
    isAllImages,
    totalFiles,
    title = 'المرفقات',
    requestId,
    onDownloadAll,
    className,
    initialExpanded = false
}: AttachmentsGalleryProps) => {
    const [isExpanded, setIsExpanded] = useState(initialExpanded);
    const [isGeneratingPDF, setIsGeneratingPDF] = useState(false);
    const [selectedPreviewFile, setSelectedPreviewFile] = useState<ZipFile | null>(null);

    const displayFiles = files || propImages || [];
    const imageFiles = displayFiles.filter(f => f.type === 'image');

    const handleDownloadImagesPDF = async () => {
        if (imageFiles.length === 0) return;
        setIsGeneratingPDF(true);
        try {
            await imagesToPdf(imageFiles, `Attachments_${requestId?.split('-')[0] || 'files'}`);
        } catch (error) {
            console.error('Error generating images PDF:', error);
        } finally {
            setIsGeneratingPDF(false);
        }
    };

    if (totalFiles === 0 && !isLoading) return null;

    return (
        <div className={cn("mt-6 pt-6 border-t animate-in fade-in slide-in-from-top-4 duration-500", className)}>
            <div className="flex items-center justify-between mb-4">
                <button 
                    onClick={() => setIsExpanded(!isExpanded)}
                    className="flex items-center gap-2 text-sm font-bold text-primary hover:opacity-80 transition-opacity"
                >
                    <ImageIcon className="w-4 h-4" />
                    {title} ({totalFiles})
                    {isExpanded ? <ChevronUp className="w-4 h-4" /> : <ChevronDown className="w-4 h-4" />}
                </button>

                <div className="flex items-center gap-2">
                    {isAllImages && imageFiles.length > 0 && isExpanded && (
                        <Button
                            variant="ghost"
                            size="sm"
                            disabled={isGeneratingPDF}
                            onClick={handleDownloadImagesPDF}
                            className="h-8 text-[10px] font-bold bg-emerald-50 text-emerald-600 hover:text-emerald-700 hover:bg-emerald-100 rounded-xl"
                        >
                            {isGeneratingPDF ? <Loader2 className="w-3 h-3 animate-spin ml-1" /> : <FileDown className="w-3 h-3 ml-1" />}
                            تحميل الصور كـ PDF
                        </Button>
                    )}
                    
                    {onDownloadAll && isExpanded && (
                        <Button
                            variant="ghost"
                            size="sm"
                            onClick={onDownloadAll}
                            className="h-8 text-[10px] font-bold bg-primary/10 text-primary hover:bg-primary/20 rounded-xl"
                        >
                            <Download className="w-3 h-3 ml-1" />
                            تحميل الكل (ZIP)
                        </Button>
                    )}
                </div>
            </div>

            {isExpanded && (
                <div className="space-y-6 animate-in zoom-in-95 duration-300">
                    {isLoading ? (
                        <div className="flex flex-col items-center justify-center p-12 text-muted-foreground gap-3 border-2 border-dashed rounded-3xl bg-muted/5">
                            <Loader2 className="w-8 h-8 animate-spin text-primary" />
                            <span className="text-sm font-bold">جاري استخراج المرفقات...</span>
                        </div>
                    ) : (
                        <>
                            {displayFiles.length > 0 ? (
                                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                                    {displayFiles.map((file, idx) => (
                                        <div 
                                            key={idx} 
                                            className="group relative aspect-video bg-muted rounded-2xl overflow-hidden border-2 border-transparent hover:border-primary/20 transition-all duration-300 shadow-sm hover:shadow-xl cursor-pointer"
                                            onClick={() => setSelectedPreviewFile(file)}
                                        >
                                            {file.type === 'image' ? (
                                                <img 
                                                    src={file.url} 
                                                    alt={file.name} 
                                                    className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110"
                                                />
                                            ) : (
                                                <div className="flex flex-col items-center justify-center h-full p-4 bg-muted/40 text-center gap-2 group-hover:bg-muted/60 transition-all select-none">
                                                    {file.type === 'pdf' && <FileText className="w-10 h-10 text-red-500 stroke-[1.5]" />}
                                                    {file.type === 'docx' && <FileText className="w-10 h-10 text-blue-500 stroke-[1.5]" />}
                                                    {file.type === 'xlsx' && <FileSpreadsheet className="w-10 h-10 text-emerald-600 stroke-[1.5]" />}
                                                    {file.type === 'other' && <FileIcon className="w-10 h-10 text-muted-foreground stroke-[1.5]" />}
                                                    <span className="text-xs font-bold text-foreground truncate max-w-full px-2 mt-1" title={file.name}>
                                                        {file.name}
                                                    </span>
                                                </div>
                                            )}
                                            <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity duration-300 flex items-center justify-center gap-3 backdrop-blur-[2px]">
                                                 {file.type === 'image' && (
                                                     <button 
                                                         onClick={(e) => {
                                                             e.stopPropagation();
                                                             printImage(file.url);
                                                         }}
                                                         className="p-3 bg-white/20 backdrop-blur-md rounded-2xl text-white hover:bg-white/40 transition-all hover:scale-110 shadow-lg"
                                                         title="طباعة"
                                                     >
                                                         <Printer className="w-5 h-5" />
                                                     </button>
                                                 )}
                                                 <button 
                                                     onClick={(e) => {
                                                         e.stopPropagation();
                                                         downloadImage(file.url, file.name);
                                                     }}
                                                     className="p-3 bg-white/20 backdrop-blur-md rounded-2xl text-white hover:bg-white/40 transition-all hover:scale-110 shadow-lg"
                                                     title="تنزيل"
                                                 >
                                                     <Download className="w-5 h-5" />
                                                 </button>
                                                 <button 
                                                     onClick={(e) => {
                                                         e.stopPropagation();
                                                         setSelectedPreviewFile(file);
                                                     }}
                                                     className="p-3 bg-white/20 backdrop-blur-md rounded-2xl text-white hover:bg-white/40 transition-all hover:scale-110 shadow-lg"
                                                     title="معاينة"
                                                 >
                                                     <Maximize2 className="w-5 h-5" />
                                                 </button>
                                            </div>
                                            <div className="absolute bottom-0 left-0 right-0 p-3 bg-gradient-to-t from-black/80 via-black/40 to-transparent">
                                                 <p className="text-[10px] text-white font-medium truncate drop-shadow-sm">{file.name}</p>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : totalFiles > 0 && (
                                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                                    <div className="col-span-full p-8 text-center border-2 border-dashed rounded-3xl bg-muted/5">
                                        <FileText className="w-10 h-10 mx-auto mb-3 text-muted-foreground opacity-20" />
                                        <p className="text-sm font-bold text-muted-foreground">توجد ملفات مرفقة غير صور</p>
                                        <p className="text-xs text-muted-foreground mt-1">يرجى استخدام خيار "تحميل الكل" لمشاهدة كافة المرفقات</p>
                                    </div>
                                </div>
                            )}
                        </>
                    )}
                </div>
            )}
            {selectedPreviewFile && (
                <ImagePreview 
                    image={selectedPreviewFile} 
                    onClose={() => setSelectedPreviewFile(null)} 
                />
            )}
        </div>
    );
};
