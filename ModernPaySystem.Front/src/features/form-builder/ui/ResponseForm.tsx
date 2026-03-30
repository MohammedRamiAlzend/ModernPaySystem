import { useState } from 'react';
import { Card } from '@/shared/ui/card';
import { Reply, Paperclip, FileText, X, Scan } from 'lucide-react';
import { Label } from '@/shared/ui/label';
import { Textarea } from '@/shared/ui/textarea';
import { Button } from '@/shared/ui/button';
import { ScannerModal } from '@/features/document-scanner';
import type { ImageMeta } from '@/features/document-scanner';

interface ResponseFormProps {
    requestId: string;
    comment: string;
    files: File[];
    isPending: boolean;
    onCommentChange: (value: string) => void;
    onFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onFilesAdd?: (files: File[]) => void;
    onRemoveFile: (index: number) => void;
    onSubmit: () => void;
}

export const ResponseForm = ({
    requestId,
    comment,
    files,
    isPending,
    onCommentChange,
    onFileChange,
    onFilesAdd,
    onRemoveFile,
    onSubmit
}: ResponseFormProps) => {
    const [isScannerOpen, setIsScannerOpen] = useState(false);
    const [scannedImages, setScannedImages] = useState<ImageMeta[]>([]);

    const handleScannerApply = (ocrText: string, images: ImageMeta[]) => {
        // 1. Add images as files
        const newFilesList = images.map(img => img.file);
        if (onFilesAdd) {
            onFilesAdd(newFilesList);
        } else {
            // Fallback: manually trigger file change if possible (but better to have onFilesAdd)
            console.warn("onFilesAdd not provided to ResponseForm");
        }

        // 2. Add OCR text to comment if available
        if (ocrText) {
            const separator = comment ? '\n\n' : '';
            onCommentChange(comment + separator + ocrText);
        }

        setIsScannerOpen(false);
        setScannedImages([]);
    };

    return (
        <div className="space-y-6">
            <Card className="p-6 space-y-4 shadow-xl border-primary/10">
                <h2 className="text-xl font-semibold flex items-center gap-2 text-primary">
                    <Reply className="w-5 h-5" />
                    إرسال رد
                </h2>
                <div className="space-y-4">
                    <div className="space-y-2">
                        <div className="flex items-center justify-between">
                            <Label className="text-sm font-bold">نص الرد / القرار</Label>
                            <Button
                                variant="ghost"
                                size="sm"
                                className="h-7 text-[10px] gap-1.5 text-primary hover:bg-primary/5 rounded-lg"
                                onClick={() => setIsScannerOpen(true)}
                            >
                                <Scan className="w-3 h-3" />
                                استخراج نص من وثيقة
                            </Button>
                        </div>
                        <Textarea
                            placeholder="اكتب تعليقك هنا..."
                            value={comment}
                            onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) => onCommentChange(e.target.value)}
                            rows={5}
                            className="resize-none rounded-xl"
                        />
                    </div>

                    <div className="space-y-3">
                        <Label className="text-sm font-bold">المرفقات (صور، مستندات)</Label>
                        <div className="flex flex-col gap-3">
                            <div className="flex gap-2">
                                <Label
                                    htmlFor="file-upload"
                                    className="flex-1 flex flex-col items-center justify-center h-20 border-2 border-dashed border-muted-foreground/20 rounded-xl cursor-pointer hover:bg-muted/30 hover:border-primary/50 transition-all group"
                                >
                                    <div className="flex flex-col items-center justify-center">
                                        <Paperclip className="w-5 h-5 mb-1 text-muted-foreground group-hover:text-primary transition-colors" />
                                        <p className="text-[10px] text-muted-foreground">رفع ملفات</p>
                                    </div>
                                    <input
                                        id="file-upload"
                                        type="file"
                                        className="hidden"
                                        multiple
                                        onChange={onFileChange}
                                    />
                                </Label>

                                <button
                                    type="button"
                                    onClick={() => setIsScannerOpen(true)}
                                    className="flex-1 flex flex-col items-center justify-center h-20 border-2 border-dashed border-sky-200 rounded-xl cursor-pointer hover:bg-sky-50 hover:border-sky-400 transition-all group"
                                >
                                    <Scan className="w-5 h-5 mb-1 text-sky-400 group-hover:text-sky-600 transition-colors" />
                                    <p className="text-[10px] text-sky-500 font-bold">مسح ضوئي</p>
                                </button>
                            </div>

                            {files.length > 0 && (
                                <div className="space-y-1.5 max-h-[120px] overflow-y-auto pr-1 custom-scrollbar">
                                    {files.map((file, index) => (
                                        <div key={index} className="flex items-center justify-between p-1.5 bg-muted/50 rounded-lg text-[10px] border border-border">
                                            <div className="flex items-center gap-2 truncate">
                                                <FileText className="w-3 h-3 text-primary" />
                                                <span className="truncate max-w-[120px]">{file.name}</span>
                                            </div>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-5 w-5 rounded-full hover:bg-destructive/10 hover:text-destructive"
                                                onClick={() => onRemoveFile(index)}
                                            >
                                                <X className="w-2.5 h-2.5" />
                                            </Button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>

                    <Button
                        onClick={onSubmit}
                        disabled={!requestId || isPending}
                        className="w-full h-12 text-md font-bold shadow-lg shadow-primary/20 rounded-xl"
                    >
                        {isPending ? 'جاري الإرسال...' : 'إرسال الرد النهائي'}
                    </Button>
                </div>
            </Card>

            {/* <Card className="p-4 bg-muted/30 border-dashed text-[10px] text-muted-foreground rounded-xl">
                <p>• سيتم ربط هذا الرد بطلبك الملحق أعلاه.</p>
                <p>• تأكد من صحة البيانات قبل الضغط على إرسال.</p>
                <p>• المنفذ الحالي: {currentUser?.username}</p>
            </Card> */}

            {/* Document Scanner & OCR Modal */}
            <ScannerModal
                isOpen={isScannerOpen}
                onClose={() => setIsScannerOpen(false)}
                onApply={handleScannerApply}
                imageFiles={scannedImages}
                setImageFiles={setScannedImages}
                acceptAllFiles={true}
            />
        </div>
    );
};
