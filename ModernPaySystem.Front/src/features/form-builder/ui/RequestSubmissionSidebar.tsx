import { useRef, useState } from 'react';
import { Shield, ImagePlus, FileText, X, Scan } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { UserPicker } from '@/features/users/ui/UserPicker';
import { SidebarSection } from '@/shared/ui/sidebar-section';
import { cn } from '@/shared/lib/utils';
import { ScannerModal } from '@/features/document-scanner';
import type { ImageMeta } from '@/features/document-scanner';

interface RequestSubmissionSidebarProps {
    approverId: string;
    onApproverSelect: (id: string) => void;
    readOnlyUsers: string[];
    onReadOnlyUsersChange: (ids: string[]) => void;
    files: File[];
    onFilesChange: (files: File[]) => void;
    showFiles?: boolean;
    className?: string;
    approverLabel?: string;
}

export const RequestSubmissionSidebar = ({
    approverId,
    onApproverSelect,
    files,
    onFilesChange,
    showFiles = true,
    className,
    approverLabel = "المرسل إليه"
}: RequestSubmissionSidebarProps) => {
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [isScannerOpen, setIsScannerOpen] = useState(false);
    const [scannedFiles, setScannedFiles] = useState<ImageMeta[]>([]);

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files);
            onFilesChange([...files, ...newFiles]);
        }
    };

    const handleScannerApply = (_ocrText: string, images: ImageMeta[]) => {
        const newFiles = images.map(img => img.file);
        onFilesChange([...files, ...newFiles]);
        setIsScannerOpen(false);
        setScannedFiles([]); // Clear local temporary files after applying
    };

    const removeFile = (index: number) => {
        onFilesChange(files.filter((_, i) => i !== index));
    };

    return (
        <div className={cn("space-y-6 sticky top-8", className)}>
            {/* Approver Selection (single) */}
            <SidebarSection title={approverLabel} icon={Shield}>
                <UserPicker
                    onUserSelect={onApproverSelect}
                    className="!grid-cols-1"
                    label={approverLabel}
                    defaultValue={approverId}
                    showCurrentUser={false}
                />
            </SidebarSection>

            {/* ReadOnly (CC) Users Selection (multi) */}
            {/* تم تعليق حاليا حتى يتم اكمال المنطق البرمجي في باك ايند */}
            {/* <SidebarSection title="للاطلاع فقط" icon={Eye}>
                <UserPicker
                    multiple
                    selectedUserIds={readOnlyUsers}
                    onUsersChange={onReadOnlyUsersChange}
                    className="!grid-cols-1"
                    label="المراقبين (CC)"
                    placeholder="اختر للاطلاع..."
                    showCurrentUser={false}
                />
            </SidebarSection> */}

            {/* File Upload & Scanner */}
            {showFiles && (
                <SidebarSection title="المرفقات" icon={ImagePlus}>
                    <div className="space-y-3">
                        <div className="flex gap-2">
                            <Button
                                variant="outline"
                                className="flex-1 h-12 gap-2 border-dashed border-primary/20 hover:border-primary/50 hover:bg-primary/5 hover:text-primary transition-all rounded-xl"
                                onClick={() => fileInputRef.current?.click()}
                                title="إرفاق ملفات من الكمبيوتر"
                            >
                                <ImagePlus className="w-5 h-5" />
                                <span className="text-xs">إرفاق ({files.length})</span>
                            </Button>

                            <Button
                                variant="outline"
                                className="flex-1 h-12 gap-2 border-dashed border-sky-200 hover:border-sky-500 hover:bg-sky-50 hover:text-sky-600 transition-all rounded-xl"
                                onClick={() => setIsScannerOpen(true)}
                                title="مسح ضوئي من الماسح (Scanner)"
                            >
                                <Scan className="w-5 h-5" />
                                <span className="text-xs">مسح ضوئي</span>
                            </Button>
                        </div>

                        <input
                            type="file"
                            multiple
                            accept="image/*,.pdf,.doc,.docx"
                            className="hidden"
                            ref={fileInputRef}
                            onChange={handleFileChange}
                        />

                        {files.length > 0 && (
                            <div className="space-y-2 mt-4 max-h-[150px] overflow-y-auto pr-1 custom-scrollbar">
                                {files.map((file, idx) => (
                                    <div key={idx} className="flex items-center justify-between p-2 rounded-lg bg-muted/50 border text-xs group/item hover:bg-muted/80 transition-colors">
                                        <div className="flex items-center gap-2 truncate">
                                            {file.type.startsWith('image/') ? <ImagePlus className="w-3 h-3 text-primary/70" /> : <FileText className="w-3 h-3 text-primary/70" />}
                                            <span className="truncate max-w-[140px]">{file.name}</span>
                                        </div>
                                        <button
                                            onClick={(e) => { e.stopPropagation(); removeFile(idx); }}
                                            className="text-muted-foreground hover:text-destructive transition-colors p-1"
                                            title="حذف الملف"
                                        >
                                            <X className="w-3.5 h-3.5" />
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </SidebarSection>
            )}

            {/* Document Scanner Modal */}
            <ScannerModal
                isOpen={isScannerOpen}
                onClose={() => setIsScannerOpen(false)}
                onApply={handleScannerApply}
                imageFiles={scannedFiles}
                setImageFiles={setScannedFiles}
                acceptAllFiles={true}
            />
        </div>
    );
};
