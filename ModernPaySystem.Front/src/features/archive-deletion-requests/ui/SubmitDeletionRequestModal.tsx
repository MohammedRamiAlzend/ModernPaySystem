import React, { useState } from 'react';
import { useSubmitDeletionRequest } from '../model/mutations';
import { ArchiveDeletionTargetType } from '../model/types';
import { useUIStore } from '@/app/store/uiStore';
import { Button } from '@/shared/ui/button';
import { Label } from '@/shared/ui/label';
import { X, AlertTriangle, FileText, Folder } from 'lucide-react';

interface SubmitDeletionRequestModalProps {
    isOpen: boolean;
    targetType: ArchiveDeletionTargetType;
    targetId: string;
    targetDisplayName?: string;
    onClose: () => void;
}

export function SubmitDeletionRequestModal({ isOpen, targetType, targetId, targetDisplayName, onClose }: SubmitDeletionRequestModalProps) {
    const { showStatus } = useUIStore();
    const submitMutation = useSubmitDeletionRequest();
    const [justification, setJustification] = useState('');

    const [prevIsOpen, setPrevIsOpen] = useState(false);
    const [prevTargetId, setPrevTargetId] = useState<string | null>(null);

    if (isOpen !== prevIsOpen || targetId !== prevTargetId) {
        setPrevIsOpen(isOpen);
        setPrevTargetId(targetId);
        setJustification('');
    }

    if (!isOpen) return null;

    const isRecord = targetType === ArchiveDeletionTargetType.Record;
    const typeLabel = isRecord ? 'المستند' : 'المجلد';
    const icon = isRecord ? <FileText className="h-5 w-5" /> : <Folder className="h-5 w-5" />;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!justification.trim()) {
            showStatus({
                type: 'warning',
                title: 'تنبيه',
                message: `يرجى كتابة سبب طلب حذف ${typeLabel}`
            });
            return;
        }

        submitMutation.mutate(
            {
                targetType,
                targetId,
                justification: justification.trim()
            },
            {
                onSuccess: () => {
                    showStatus({
                        type: 'success',
                        title: 'تم بنجاح',
                        message: `تم إرسال طلب حذف ${typeLabel} إلى مدير القسم للمراجعة`
                    });
                    onClose();
                },
                onError: (err: any) => {
                    showStatus({
                        type: 'error',
                        title: 'خطأ',
                        message: err?.response?.data?.message || 'فشل إرسال طلب الحذف. يرجى المحاولة لاحقاً.'
                    });
                }
            }
        );
    };

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
            <div className="bg-card border border-border rounded-3xl p-6 max-w-lg w-full shadow-2xl flex flex-col gap-6 text-right overflow-hidden">
                <div className="flex justify-between items-start border-b border-border pb-4 flex-shrink-0">
                    <button onClick={onClose} className="text-muted-foreground hover:text-foreground transition-colors p-1 rounded-lg">
                        <X className="h-5 w-5" />
                    </button>
                    <div className="flex flex-col gap-1">
                        <h2 className="text-base font-bold text-foreground">
                            تقديم طلب حذف {typeLabel}
                        </h2>
                        {targetDisplayName && (
                            <p className="text-xs text-muted-foreground font-medium">
                                {targetDisplayName}
                            </p>
                        )}
                    </div>
                </div>

                <form onSubmit={handleSubmit} className="flex flex-col gap-5">
                    <div className="flex items-start gap-3 bg-destructive/10 border border-destructive/20 p-4 rounded-2xl">
                        <AlertTriangle className="h-5 w-5 text-destructive flex-shrink-0 mt-0.5" />
                        <div className="flex flex-col gap-1 text-xs">
                            <span className="font-bold text-destructive">تنبيه هام</span>
                            <span className="text-muted-foreground font-medium leading-relaxed">
                                طلب الحذف هذا سيؤدي إلى حذف {typeLabel} بشكل دائم بعد الموافقة.
                                {isRecord && ' جميع الملفات المرفقة بهذا المستند سيتم حذفها أيضاً.'}
                                الرجاء التأكد من صحة طلبك قبل الإرسال.
                            </span>
                        </div>
                    </div>

                    <div className="flex items-center gap-3 bg-muted/20 border border-border p-3 rounded-2xl">
                        {icon}
                        <div className="flex flex-col gap-0.5">
                            <span className="text-xs font-bold text-foreground">{typeLabel} المستهدف</span>
                            <span className="text-[10px] text-muted-foreground font-mono">{targetId}</span>
                        </div>
                    </div>

                    <div className="flex flex-col gap-2">
                        <Label className="text-xs font-semibold text-muted-foreground">سبب طلب الحذف *</Label>
                        <textarea
                            value={justification}
                            onChange={(e) => setJustification(e.target.value)}
                            placeholder={`يرجى كتابة سبب طلب حذف ${typeLabel} بالتفصيل...`}
                            className="flex min-h-[100px] w-full rounded-2xl border border-border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 text-right"
                            required
                        />
                    </div>

                    <div className="flex gap-3 justify-start border-t border-border pt-4 flex-shrink-0">
                        <Button
                            type="button"
                            variant="outline"
                            onClick={onClose}
                            className="rounded-xl px-5"
                        >
                            إلغاء
                        </Button>
                        <Button
                            type="submit"
                            disabled={submitMutation.isPending}
                            variant="destructive"
                            className="rounded-xl px-6 font-bold"
                        >
                            {submitMutation.isPending ? 'جاري الإرسال...' : 'إرسال طلب الحذف'}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
