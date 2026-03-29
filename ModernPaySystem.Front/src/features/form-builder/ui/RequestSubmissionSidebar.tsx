import { useRef } from 'react';
import { Shield, ImagePlus, FileText, X } from 'lucide-react';
import { Button } from '@/shared/ui/button';
import { UserPicker } from '@/features/users/ui/UserPicker';
import { SidebarSection } from '@/shared/ui/sidebar-section';
import { cn } from '@/shared/lib/utils';

interface RequestSubmissionSidebarProps {
    approverId: string;
    onApproverSelect: (id: string) => void;
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

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files);
            onFilesChange([...files, ...newFiles]);
        }
    };

    const removeFile = (index: number) => {
        onFilesChange(files.filter((_, i) => i !== index));
    };

    return (
        <div className={cn("space-y-6 sticky top-8", className)}>
            {/* Approver Selection */}
            <SidebarSection title={approverLabel} icon={Shield}>
                <UserPicker
                    onUserSelect={onApproverSelect}
                    className="!grid-cols-1"
                    label={approverLabel}
                    defaultValue={approverId}
                />
            </SidebarSection>

            {/* File Upload */}
            {showFiles && (
                <SidebarSection title="المرفقات" icon={ImagePlus}>
                    <div className="space-y-4">
                        <Button
                            variant="outline"
                            className="w-full h-12 gap-2 border-dashed border-primary/20 hover:border-primary/50 hover:bg-primary/5 hover:text-primary transition-all rounded-xl"
                            onClick={() => fileInputRef.current?.click()}
                        >
                            <ImagePlus className="w-5 h-5" />
                            <span>إرفاق ملفات ({files.length})</span>
                        </Button>
                        <input
                            type="file"
                            multiple
                            accept="image/*,.pdf,.doc,.docx"
                            className="hidden"
                            ref={fileInputRef}
                            onChange={handleFileChange}
                        />

                        {files.length > 0 && (
                            <div className="space-y-2 mt-4">
                                {files.map((file, idx) => (
                                    <div key={idx} className="flex items-center justify-between p-2 rounded-lg bg-muted/50 border text-xs group/item hover:bg-muted/80 transition-colors">
                                        <div className="flex items-center gap-2 truncate">
                                            {file.type.startsWith('image/') ? <ImagePlus className="w-3 h-3 text-primary/70" /> : <FileText className="w-3 h-3 text-primary/70" />}
                                            <span className="truncate max-w-[150px]">{file.name}</span>
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
        </div>
    );
};
