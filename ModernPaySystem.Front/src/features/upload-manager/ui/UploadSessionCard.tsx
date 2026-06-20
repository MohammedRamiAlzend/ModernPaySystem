import { Button } from '@/shared/ui/button';
import { ChevronDown, ChevronUp, Folder, RotateCcw, Trash2, CheckCircle2 } from 'lucide-react';
import { useState } from 'react';
import type { UploadSession } from '../model/types';
import { FileUploadRow } from './FileUploadRow';
import { useUploadStore } from '../model/uploadStore';
import { cn } from '@/shared/lib/utils';

interface UploadSessionCardProps {
    session: UploadSession;
}

const statusBadge = {
    uploading: { label: 'جاري الرفع', className: 'bg-primary/10 text-primary' },
    completed: { label: 'اكتمل', className: 'bg-emerald-500/10 text-emerald-600' },
    partial: { label: 'فشل جزئي', className: 'bg-destructive/10 text-destructive' },
    paused: { label: 'متوقف', className: 'bg-amber-500/10 text-amber-600' },
} as const;

export function UploadSessionCard({ session }: UploadSessionCardProps) {
    const [isExpanded, setIsExpanded] = useState(true);
    const { retryFile, retryAllFailed, removeSession, removeFileFromSession } = useUploadStore();

    const successCount = session.files.filter((f) => f.status === 'success').length;
    const failedCount = session.files.filter((f) => f.status === 'failed').length;
    const totalCount = session.files.length;

    const badge = statusBadge[session.status];
    const isCompleted = session.status === 'completed';

    return (
        <div className={cn(
            'rounded-xl border transition-colors',
            isCompleted ? 'border-emerald-500/20 bg-emerald-500/5' : 'border-border bg-card'
        )}>
            {/* Header */}
            <div
                className="flex items-center gap-2 px-3 py-2.5 cursor-pointer select-none"
                onClick={() => setIsExpanded(!isExpanded)}
            >
                <Folder className="w-4 h-4 text-amber-500/80 shrink-0" />
                <span className="text-xs font-bold truncate flex-1 text-right" title={session.recordTitle}>
                    {session.recordTitle}
                </span>
                <span className={cn('text-[10px] font-bold px-2 py-0.5 rounded-full shrink-0', badge.className)}>
                    {badge.label}
                </span>
                <span className="text-[10px] text-muted-foreground shrink-0">
                    {successCount}/{totalCount}
                </span>
                {isExpanded ? (
                    <ChevronUp className="w-3.5 h-3.5 text-muted-foreground shrink-0" />
                ) : (
                    <ChevronDown className="w-3.5 h-3.5 text-muted-foreground shrink-0" />
                )}
            </div>

            {/* Files List */}
            {isExpanded && (
                <div className="border-t border-border">
                    <div className="max-h-[200px] overflow-y-auto divide-y divide-border/50">
                        {session.files.map((file) => (
                            <FileUploadRow
                                key={file.id}
                                file={file}
                                onRetry={() => retryFile(session.id, file.id)}
                                onDelete={() => removeFileFromSession(session.id, file.id)}
                            />
                        ))}
                    </div>

                    {/* Actions */}
                    {(failedCount > 0 || isCompleted) && (
                        <div className="flex items-center gap-2 px-3 py-2 border-t border-border">
                            {failedCount > 0 && (
                                <Button
                                    variant="ghost"
                                    size="sm"
                                    className="h-7 text-[10px] font-bold text-primary hover:text-primary gap-1"
                                    onClick={() => retryAllFailed(session.id)}
                                >
                                    <RotateCcw className="w-3 h-3" />
                                    إعادة رفع الكل ({failedCount})
                                </Button>
                            )}
                            {isCompleted && (
                                <>
                                    <span className="flex items-center gap-1 text-[10px] text-emerald-600 font-bold">
                                        <CheckCircle2 className="w-3 h-3" />
                                        تم رفع جميع الملفات
                                    </span>
                                    <div className="flex-1" />
                                    <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-6 w-6 text-muted-foreground hover:text-destructive"
                                        onClick={() => removeSession(session.id)}
                                        title="إزالة"
                                    >
                                        <Trash2 className="w-3 h-3" />
                                    </Button>
                                </>
                            )}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
}
