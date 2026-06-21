import { Button } from '@/shared/ui/button';
import { Upload, X, Minus, Maximize2, Trash2 } from 'lucide-react';
import { useUploadStore } from '../model/uploadStore';
import { useAuthStore } from '@/app/store/authStore';
import { UploadSessionCard } from './UploadSessionCard';
import { cn } from '@/shared/lib/utils';

/**
 * اللوحة العائمة لإدارة رفع الملفات.
 * تظهر في الزاوية السفلية اليسرى وتبقى ظاهرة أثناء التنقل بين الصفحات.
 */
export function UploadManagerPanel() {
    const {
        sessions,
        isPanelOpen,
        isPanelMinimized,
        togglePanel,
        toggleMinimize,
        clearCompletedSessions,
    } = useUploadStore();
    const currentUserId = useAuthStore((s) => s.user?.id);

    // عرض جلسات المستخدم الحالي فقط
    const sessionsArray = Object.values(sessions).filter(
        (s) => s.userId === currentUserId
    );

    // لا تظهر شيئاً إذا لم تكن هناك جلسات
    if (sessionsArray.length === 0) return null;

    // إحصائيات عامة
    const totalFiles = sessionsArray.reduce((sum, s) => sum + s.files.length, 0);
    const successFiles = sessionsArray.reduce(
        (sum, s) => sum + s.files.filter((f) => f.status === 'success').length,
        0
    );
    const uploadingFiles = sessionsArray.reduce(
        (sum, s) => sum + s.files.filter((f) => f.status === 'uploading').length,
        0
    );
    const failedFiles = sessionsArray.reduce(
        (sum, s) => sum + s.files.filter((f) => f.status === 'failed').length,
        0
    );
    const hasCompleted = sessionsArray.some((s) => s.status === 'completed');
    const isAllDone = successFiles === totalFiles;

    // عند عدم فتح اللوحة — زر عائم صغير فقط
    if (!isPanelOpen) {
        return (
            <div className="fixed bottom-4 left-4 z-50 animate-in slide-in-from-bottom-2 duration-300">
                <Button
                    onClick={togglePanel}
                    className={cn(
                        'rounded-2xl shadow-xl px-4 py-2.5 flex items-center gap-2 font-bold text-xs',
                        isAllDone
                            ? 'bg-emerald-500 hover:bg-emerald-600 text-white'
                            : 'bg-primary hover:bg-primary/90 text-primary-foreground'
                    )}
                >
                    <Upload className="w-4 h-4" />
                    <span>
                        {isAllDone
                            ? `✓ تم رفع ${totalFiles} ملف`
                            : `رفع ${successFiles}/${totalFiles}`}
                    </span>
                    {uploadingFiles > 0 && (
                        <span className="inline-block w-2 h-2 rounded-full bg-white/80 animate-pulse" />
                    )}
                </Button>
            </div>
        );
    }

    // اللوحة المصغرة
    if (isPanelMinimized) {
        return (
            <div className="fixed bottom-4 left-4 z-50 animate-in slide-in-from-bottom-2 duration-300">
                <div className="bg-card border border-border rounded-2xl shadow-2xl flex items-center gap-2 px-4 py-2.5">
                    <Upload className="w-4 h-4 text-primary" />
                    <span className="text-xs font-bold">
                        {isAllDone
                            ? `✓ اكتمل رفع ${totalFiles} ملف`
                            : `رفع ${successFiles}/${totalFiles} ملف...`}
                    </span>
                    {uploadingFiles > 0 && (
                        <span className="inline-block w-2 h-2 rounded-full bg-primary animate-pulse" />
                    )}
                    {failedFiles > 0 && (
                        <span className="text-[10px] font-bold text-destructive">
                            ({failedFiles} فشل)
                        </span>
                    )}
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-6 w-6"
                        onClick={toggleMinimize}
                        title="تكبير"
                    >
                        <Maximize2 className="w-3.5 h-3.5" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-6 w-6"
                        onClick={togglePanel}
                        title="إغلاق"
                    >
                        <X className="w-3.5 h-3.5" />
                    </Button>
                </div>
            </div>
        );
    }

    // اللوحة الكاملة
    return (
        <div className="fixed bottom-4 left-4 z-50 w-[400px] max-h-[500px] flex flex-col animate-in slide-in-from-bottom-4 duration-300">
            <div className="bg-card border border-border rounded-2xl shadow-2xl overflow-hidden flex flex-col">
                {/* Header */}
                <div className="flex items-center gap-2 px-4 py-3 border-b border-border bg-muted/30">
                    <Upload className="w-4 h-4 text-primary" />
                    <span className="text-sm font-bold flex-1">مدير رفع الملفات</span>
                    <span className="text-[10px] text-muted-foreground">
                        {successFiles}/{totalFiles}
                    </span>
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-6 w-6"
                        onClick={toggleMinimize}
                        title="تصغير"
                    >
                        <Minus className="w-3.5 h-3.5" />
                    </Button>
                    <Button
                        variant="ghost"
                        size="icon"
                        className="h-6 w-6"
                        onClick={togglePanel}
                        title="إخفاء"
                    >
                        <X className="w-3.5 h-3.5" />
                    </Button>
                </div>

                {/* Sessions List */}
                <div className="flex-1 overflow-y-auto p-2 flex flex-col gap-2 max-h-[380px]">
                    {sessionsArray.map((session) => (
                        <UploadSessionCard key={session.id} session={session} />
                    ))}
                </div>

                {/* Footer */}
                {hasCompleted && (
                    <div className="border-t border-border px-3 py-2 flex justify-end">
                        <Button
                            variant="ghost"
                            size="sm"
                            className="h-7 text-[10px] font-bold text-muted-foreground gap-1"
                            onClick={clearCompletedSessions}
                        >
                            <Trash2 className="w-3 h-3" />
                            مسح المكتملة
                        </Button>
                    </div>
                )}
            </div>
        </div>
    );
}
