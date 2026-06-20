import { Progress } from '@/shared/ui/progress';
import { Button } from '@/shared/ui/button';
import { CheckCircle2, XCircle, Loader2, RotateCcw, Clock, Trash2 } from 'lucide-react';
import type { FileUploadItem } from '../model/types';
import { cn } from '@/shared/lib/utils';

interface FileUploadRowProps {
    file: FileUploadItem;
    onRetry: () => void;
    onDelete?: () => void;
}

const statusConfig = {
    pending: {
        icon: Clock,
        iconClass: 'text-muted-foreground',
        label: 'قيد الانتظار',
    },
    uploading: {
        icon: Loader2,
        iconClass: 'text-primary animate-spin',
        label: 'جاري الرفع',
    },
    success: {
        icon: CheckCircle2,
        iconClass: 'text-emerald-500',
        label: 'تم',
    },
    failed: {
        icon: XCircle,
        iconClass: 'text-destructive',
        label: 'فشل',
    },
} as const;

/** تنسيق حجم الملف بوحدة مقروءة */
const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(1))} ${sizes[i]}`;
};

export function FileUploadRow({ file, onRetry, onDelete }: FileUploadRowProps) {
    const config = statusConfig[file.status];
    const StatusIcon = config.icon;

    return (
        <div className="flex flex-col gap-1.5 px-2 py-1.5">
            <div className="flex items-center gap-2">
                <StatusIcon className={cn('w-4 h-4 shrink-0', config.iconClass)} />
                <span
                    className={cn(
                        'text-xs truncate flex-1 text-right font-medium',
                        file.status === 'failed' && 'text-destructive'
                    )}
                    title={file.fileName}
                >
                    {file.fileName}
                </span>
                <span className="text-[10px] text-muted-foreground shrink-0">
                    {formatFileSize(file.fileSize)}
                </span>
                {file.status === 'uploading' && (
                    <span className="text-[10px] font-bold text-primary shrink-0 min-w-[32px] text-left">
                        {file.progress}%
                    </span>
                )}
                {file.status === 'failed' && (
                    <div className="flex items-center gap-1 shrink-0">
                        <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            className="h-6 w-6 text-primary hover:text-primary hover:bg-primary/10"
                            onClick={onRetry}
                            title="إعادة الرفع"
                        >
                            <RotateCcw className="w-3.5 h-3.5" />
                        </Button>
                        {onDelete && (
                            <Button
                                type="button"
                                variant="ghost"
                                size="icon"
                                className="h-6 w-6 text-muted-foreground hover:text-destructive hover:bg-destructive/10"
                                onClick={onDelete}
                                title="إزالة من القائمة"
                            >
                                <Trash2 className="w-3.5 h-3.5" />
                            </Button>
                        )}
                    </div>
                )}
            </div>
            {file.status === 'uploading' && (
                <Progress value={file.progress} className="h-1" />
            )}
            {file.status === 'failed' && file.errorMessage && (
                <p className="text-[10px] text-destructive/80 pr-6 truncate" title={file.errorMessage}>
                    {file.errorMessage}
                </p>
            )}
        </div>
    );
}
