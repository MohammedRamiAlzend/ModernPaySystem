import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import { MultiUserPicker } from '@/features/users/ui/MultiUserPicker';

interface FolderModalProps {
    isOpen: boolean;
    mode: 'create' | 'edit';
    folderName: string;
    onFolderNameChange: (name: string) => void;
    onClose: () => void;
    onSubmit: (e: React.FormEvent) => void;
    isSaving: boolean;
    initialPermissionIds?: string[];
    onPermissionIdsChange?: (ids: string[]) => void;
}

export function FolderModal({
    isOpen,
    mode,
    folderName,
    onFolderNameChange,
    onClose,
    onSubmit,
    isSaving,
    initialPermissionIds = [],
    onPermissionIdsChange
}: FolderModalProps) {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in">
            <div className="bg-card border border-border rounded-3xl p-6 max-w-md w-full shadow-2xl flex flex-col gap-6 text-right">
                <div className="flex flex-col gap-1 border-b border-border pb-4">
                    <h2 className="text-base font-bold text-foreground">
                        {mode === 'create' ? 'إنشاء مجلد جديد' : 'تعديل اسم المجلد'}
                    </h2>
                    <p className="text-xs text-muted-foreground font-medium">
                        {mode === 'create'
                            ? 'أدخل اسم المجلد الذي ترغب بإنشائه في المسار الحالي'
                            : 'أدخل الاسم الجديد للمجلد'}
                    </p>
                </div>

                <form onSubmit={onSubmit} className="flex flex-col gap-4">
                    <div className="flex flex-col gap-2">
                        <Label className="text-xs font-semibold text-muted-foreground">اسم المجلد</Label>
                        <Input
                            value={folderName}
                            onChange={(e) => onFolderNameChange(e.target.value)}
                            placeholder="مثال: الفواتير الواردة 2026"
                            className="rounded-2xl h-11 bg-background border-border"
                            autoFocus
                        />
                    </div>

                    {mode === 'create' && (
                        <div className="flex flex-col gap-2 border-t border-border pt-4">
                            <MultiUserPicker
                                selectedUserIds={initialPermissionIds}
                                onUsersChange={onPermissionIdsChange ?? (() => {})}
                                label="صلاحيات إضافية (اختياري)"
                                placeholder="اضف مستخدم للاطلاع فقط..."
                            />
                        </div>
                    )}

                    <div className="flex justify-end gap-3 pt-2">
                        <Button
                            type="button"
                            variant="ghost"
                            onClick={onClose}
                            className="rounded-xl px-5"
                            disabled={isSaving}
                        >
                            إلغاء
                        </Button>
                        <Button
                            type="submit"
                            className="rounded-xl px-8 font-bold shadow-lg shadow-primary/20"
                            disabled={isSaving || !folderName.trim()}
                        >
                            {isSaving ? 'جاري الحفظ...' : 'حفظ'}
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
}
