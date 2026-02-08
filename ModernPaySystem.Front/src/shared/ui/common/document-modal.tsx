import React, { useState, useRef } from 'react';
import { CloudUpload, FileText, ImageIcon, FileCode } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { Label } from '@/shared/ui/label';
import { Textarea } from '@/shared/ui/textarea';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from "@/shared/ui/dialog"
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/ui/select"

interface DocumentModalProps {
    open: boolean;
    onClose: () => void;
    type: 'lessor' | 'tenant';
}

export const DocumentModal: React.FC<DocumentModalProps> = ({ open, onClose, type }) => {
    const [documentType, setDocumentType] = useState('');
    const [description, setDescription] = useState('');
    const [file, setFile] = useState<File | null>(null);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            setFile(e.target.files[0]);
        }
    };

    const handleUpload = () => {
        if (!file || !documentType) {
            alert('يرجى اختيار نوع الوثيقة وملف للرفع');
            return;
        }
        console.log('Uploading document:', { type: documentType, file, description });
        onClose();
    };

    const getFileIcon = () => {
        if (!file) return <FileCode className="w-16 h-16 text-muted-foreground/40" />;

        if (file.type === 'application/pdf') {
            return <FileText className="w-16 h-16 text-destructive" />;
        } else if (file.type.startsWith('image/')) {
            return <ImageIcon className="w-16 h-16 text-primary" />;
        } else {
            return <FileCode className="w-16 h-16 text-muted-foreground" />;
        }
    };

    return (
        <Dialog open={open} onOpenChange={onClose}>
            <DialogContent className="max-w-lg rounded-2xl border-none shadow-2xl p-0 overflow-hidden text-right" style={{ direction: 'rtl' }}>
                <DialogHeader className="p-6 border-b bg-muted/20">
                    <DialogTitle className="text-xl font-bold flex items-center gap-3 justify-start">
                        <div className="p-2 bg-primary/10 rounded-lg">
                            <CloudUpload className="w-5 h-5 text-primary" />
                        </div>
                        رفع وثيقة {type === 'lessor' ? 'للمؤجر' : 'للمستأجر'}
                    </DialogTitle>
                </DialogHeader>

                <div className="p-6 space-y-6">
                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground block text-right">نوع الوثيقة *</Label>
                        <Select
                            value={documentType}
                            onValueChange={setDocumentType}
                        >
                            <SelectTrigger className="h-11 rounded-xl text-right flex-row-reverse border-border bg-background">
                                <SelectValue placeholder="اختر النوع من القائمة..." />
                            </SelectTrigger>
                            <SelectContent className="text-right">
                                <SelectItem value="id" className="text-right">بطاقة الهوية / الإقامة</SelectItem>
                                <SelectItem value="contract" className="text-right">عقد سابق / مسودة</SelectItem>
                                <SelectItem value="authorization" className="text-right">قرار تفويض</SelectItem>
                                <SelectItem value="ownership" className="text-right">صك إثبات ملكية</SelectItem>
                                <SelectItem value="other" className="text-right">أخرى</SelectItem>
                            </SelectContent>
                        </Select>
                    </div>

                    <div className="space-y-2">
                        <Label className="text-sm font-semibold text-muted-foreground block text-right">وصف إضافي</Label>
                        <Textarea
                            className="rounded-xl border-border bg-background focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all resize-none h-28 text-right"
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            placeholder="اكتب تفاصيل إضافية عن هذه الوثيقة هنا..."
                        />
                    </div>

                    <div className="flex flex-col items-center justify-center gap-4 py-8 border-2 border-dashed rounded-2xl bg-muted/30 border-muted-foreground/20 group hover:bg-muted/50 transition-colors">
                        {getFileIcon()}
                        <input
                            type="file"
                            ref={fileInputRef}
                            onChange={handleFileSelect}
                            className="hidden"
                        />
                        <div className="text-center space-y-2">
                            <Button
                                variant="outline"
                                type="button"
                                className="gap-2 rounded-xl h-10 px-6 border-primary/20 text-primary hover:bg-primary/5 hover:text-primary transition-all"
                                onClick={() => fileInputRef.current?.click()}
                            >
                                <CloudUpload className="w-4 h-4" /> اختيار الملف
                            </Button>
                            {file ? (
                                <div className="text-sm text-primary font-bold animate-in fade-in slide-in-from-top-1">
                                    تم اختيار: {file.name}
                                </div>
                            ) : (
                                <div className="text-xs text-muted-foreground">
                                    يدعم ملفات PDF، الصور، والمستندات
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                <DialogFooter className="p-6 border-t bg-muted/40 flex flex-row-reverse justify-start gap-4">
                    <Button onClick={handleUpload} className="gap-2 px-10 h-11 rounded-xl shadow-lg shadow-primary/20">
                        حفظ الوثيقة
                    </Button>
                    <Button variant="outline" onClick={onClose} className="h-11 rounded-xl">
                        إلغاء
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};

