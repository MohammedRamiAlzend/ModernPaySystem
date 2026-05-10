import { useState } from 'react';
import { Card } from '@/shared/ui/card';
import { Reply, Paperclip, FileText, X, Scan } from 'lucide-react';
import { Label } from '@/shared/ui/label';
import { Textarea } from '@/shared/ui/textarea';
import { Button } from '@/shared/ui/button';
import { ScannerModal } from '@/features/document-scanner';
import type { ImageMeta } from '@/features/document-scanner';
import { Tabs, TabsList, TabsTrigger } from '@/shared/ui/tabs';
import { UserPicker } from '@/features/users/ui/UserPicker';
import { User } from 'lucide-react';

interface ResponseFormProps {
    requestId: string;
    comment: string;
    files: File[];
    isPending: boolean;
    onCommentChange: (value: string) => void;
    onFileChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
    onFilesAdd?: (files: File[]) => void;
    onRemoveFile: (index: number) => void;
    submissionMode: 'submit' | 'referral';
    onSubmissionModeChange: (mode: 'submit' | 'referral') => void;
    targetUserId: string;
    onTargetUserChange: (userId: string) => void;
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
    submissionMode,
    onSubmissionModeChange,
    targetUserId,
    onTargetUserChange,
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
        <>
            <Card className="flex flex-col h-full w-full shadow-2xl border-primary/10 overflow-hidden">
                {/* Fixed Header */}
                <div className="p-5 border-b bg-muted/20 shrink-0">
                    <h2 className="text-xl font-bold flex items-center gap-2 text-primary">
                        <Reply className="w-5 h-5 transition-transform group-hover:-translate-x-1" />
                        {submissionMode === 'submit' ? 'إرسال رد نهائي' : 'إحالة الطلب لجهة أخرى'}
                    </h2>
                </div>

                {/* Mode Selector */}
                <div className="px-5 py-2 border-b bg-muted/5">
                    <Tabs 
                        defaultValue="submit" 
                        value={submissionMode} 
                        onValueChange={(v) => onSubmissionModeChange(v as 'submit' | 'referral')}
                        className="w-full"
                    >
                        <TabsList className="grid w-full grid-cols-2 rounded-xl">
                            <TabsTrigger value="submit" className="gap-2 rounded-lg font-bold">
                                <Reply className="w-4 h-4" />
                                رد نهائي
                            </TabsTrigger>
                            <TabsTrigger value="referral" className="gap-2 rounded-lg font-bold">
                                <User className="w-4 h-4" />
                                إحالة الطلب
                            </TabsTrigger>
                        </TabsList>
                    </Tabs>
                </div>

                {/* Scrollable Body */}
                <div className="flex-1 overflow-y-auto p-5 custom-scrollbar space-y-5">
                    {submissionMode === 'referral' && (
                        <div className="space-y-2 animate-in fade-in slide-in-from-top-2">
                             <Label className="text-sm font-bold border-r-4 border-amber-400 pr-2 block mb-2">جهة الإحالة</Label>
                             <UserPicker
                                onUserSelect={onTargetUserChange}
                                defaultValue={targetUserId}
                                label="الموظف المستلم"
                                placeholder="اختر الموظف لإحالة الطلب إليه..."
                                className="!grid-cols-1 md:!grid-cols-1 space-y-4"
                                showCurrentUser={false}
                            />
                        </div>
                    )}

                    <div className="space-y-2">
                        <div className="flex items-center justify-between sticky top-0 bg-background/95 backdrop-blur z-10 py-1">
                            <Label className="text-sm font-bold border-r-4 border-primary pr-2">نص الرد / القرار <span className="text-destructive">*</span></Label>

                            <Button
                                variant="ghost"
                                size="sm"
                                className="h-7 text-[10px] gap-1.5 text-primary hover:bg-primary/5 rounded-lg border border-primary/20"
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
                            rows={4}
                            className="resize-none rounded-xl focus-visible:ring-primary/20 border-muted-foreground/20"
                        />
                    </div>

                    <div className="space-y-3">
                        <Label className="text-sm font-bold border-r-4 border-sky-400 pr-2">المرفقات (صور، مستندات)</Label>
                        <div className="flex flex-col gap-3">
                            <div className="flex gap-2">
                                <Label
                                    htmlFor="file-upload"
                                    className="flex-1 flex flex-col items-center justify-center h-24 border-2 border-dashed border-muted-foreground/20 rounded-xl cursor-pointer hover:bg-muted/30 hover:border-primary/50 transition-all group"
                                >
                                    <div className="flex flex-col items-center justify-center">
                                        <Paperclip className="w-6 h-6 mb-1.5 text-muted-foreground group-hover:text-primary transition-colors" />
                                        <p className="text-[10px] text-muted-foreground font-bold">رفع ملفات</p>
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
                                    className="flex-1 flex flex-col items-center justify-center h-24 border-2 border-dashed border-sky-200 rounded-xl cursor-pointer hover:bg-sky-50 hover:border-sky-400 transition-all group"
                                >
                                    <Scan className="w-6 h-6 mb-1.5 text-sky-400 group-hover:text-sky-600 transition-colors" />
                                    <p className="text-[10px] text-sky-500 font-bold">مسح ضوئي</p>
                                </button>
                            </div>

                            {files.length > 0 && (
                                <div className="grid grid-cols-1 gap-2 mt-2 animate-in fade-in slide-in-from-top-2">
                                    {files.map((file, index) => (
                                        <div key={index} className="flex items-center justify-between p-2 bg-muted/40 rounded-xl text-[10px] border border-border group hover:border-primary/30 transition-colors">
                                            <div className="flex items-center gap-2 truncate">
                                                <div className="p-1 bg-primary/10 rounded-md">
                                                    <FileText className="w-3.5 h-3.5 text-primary" />
                                                </div>
                                                <span className="truncate max-w-[180px] font-medium">{file.name}</span>
                                            </div>
                                            <Button
                                                variant="ghost"
                                                size="icon"
                                                className="h-6 w-6 rounded-full hover:bg-destructive/10 hover:text-destructive shrink-0"
                                                onClick={() => onRemoveFile(index)}
                                            >
                                                <X className="w-3 h-3" />
                                            </Button>
                                        </div>
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* Fixed Footer */}
                <div className="p-5 border-t bg-muted/20 shrink-0">
                    <Button
                        onClick={onSubmit}
                        disabled={!requestId || isPending || !comment.trim() || (submissionMode === 'referral' && !targetUserId)}

                        className="w-full h-12 text-md font-black shadow-xl shadow-primary/10 rounded-xl transition-all hover:scale-[1.01] active:scale-95"
                    >
                        {isPending ? 'جاري الإرسال...' : submissionMode === 'submit' ? 'إرسال الرد النهائي' : 'تأكيد الإحالة'}
                    </Button>
                </div>
            </Card>

            {/* Document Scanner & OCR Modal */}
            <ScannerModal
                isOpen={isScannerOpen}
                onClose={() => setIsScannerOpen(false)}
                onApply={handleScannerApply}
                imageFiles={scannedImages}
                setImageFiles={setScannedImages}
                acceptAllFiles={true}
            />
        </>
    );
};
