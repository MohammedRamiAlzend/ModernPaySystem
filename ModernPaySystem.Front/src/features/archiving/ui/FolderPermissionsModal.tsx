/* eslint-disable react-hooks/set-state-in-effect */
import { useState, useEffect, useCallback } from 'react';
import { Folder, FolderPermissionDto } from '@/features/archiving/model/types';
import { archivingService } from '@/features/archiving/api/archivingService';
import { MultiUserPicker } from '@/features/users/ui/MultiUserPicker';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { Button } from '@/shared/ui/button';
import { X, Shield, Loader2, Trash2, Lock, Unlock } from 'lucide-react';
import { useUIStore } from '@/app/store/uiStore';

interface FolderPermissionsModalProps {
    isOpen: boolean;
    folder: Folder | null;
    onClose: () => void;
}

const accessLevelLabel = (level: number): string => {
    switch (level) {
        case 0: return 'بدون وصول';
        case 1: return 'عرض';
        case 2: return 'تعديل';
        case 3: return 'تحكم كامل';
        default: return `مستوى ${level}`;
    }
};

export const FolderPermissionsModal = ({
    isOpen,
    folder,
    onClose
}: FolderPermissionsModalProps) => {
    const { showStatus } = useUIStore();
    const [permissions, setPermissions] = useState<FolderPermissionDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [deletingId, setDeletingId] = useState<string | null>(null);
    const [newUserIds, setNewUserIds] = useState<string[]>([]);
    const [saving, setSaving] = useState(false);

    const loadPermissions = useCallback(async (folderId: string) => {
        Promise.resolve().then(() => setLoading(true));
        try {
            const data = await archivingService.getFolderPermissions(folderId);
            setPermissions(data);
        } catch {
            showStatus({ type: 'error', title: 'خطأ', message: 'تعذر تحميل صلاحيات المجلد.' });
        } finally {
            setLoading(false);
        }
    }, [showStatus]);

    const [prevFolderId, setPrevFolderId] = useState<string | null>(null);
    if (folder && folder.id !== prevFolderId) {
        setPrevFolderId(folder.id);
        setNewUserIds([]);
    }

    useEffect(() => {
        if (isOpen && folder) {
            loadPermissions(folder.id);
        }
    }, [isOpen, folder, loadPermissions]);

    const handleAddUsers = async () => {
        if (!folder || newUserIds.length === 0) return;
        setSaving(true);
        let added = 0;
        for (const userId of newUserIds) {
            try {
                await archivingService.createFolderPermission(folder.id, {
                    userId,
                    accessLevel: 1,
                    isInherited: true
                });
                added++;
            } catch {
                // skip duplicates / errors
            }
        }
        setSaving(false);
        setNewUserIds([]);
        if (added > 0) {
            showStatus({ type: 'success', title: 'تمت الإضافة', message: `تمت إضافة ${added} مستخدم بنجاح.` });
            await loadPermissions(folder.id);
        }
    };

    const handleDelete = async (permissionId: string) => {
        setDeletingId(permissionId);
        try {
            await archivingService.deleteFolderPermission(permissionId);
            setPermissions(prev => prev.filter(p => p.id !== permissionId));
            showStatus({ type: 'success', title: 'تم', message: 'تم إزالة الصلاحية بنجاح.' });
        } catch {
            showStatus({ type: 'error', title: 'خطأ', message: 'تعذر إزالة الصلاحية.' });
        } finally {
            setDeletingId(null);
        }
    };

    if (!isOpen || !folder) return null;

    return (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-[2px] flex items-center justify-center z-[100] animate-in fade-in duration-200" onClick={onClose}>
            <div className="bg-card border border-border rounded-3xl shadow-2xl w-full max-w-lg max-h-[80vh] flex flex-col" dir="rtl" onClick={e => e.stopPropagation()}>
                {/* Header */}
                <div className="flex items-center justify-between px-6 pt-6 pb-4 border-b border-border/60">
                    <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-2xl bg-amber-500/10 text-amber-500 flex items-center justify-center">
                            <Shield className="h-5 w-5" />
                        </div>
                        <div className="flex flex-col">
                            <h2 className="text-base font-bold text-foreground">إدارة صلاحيات المجلد</h2>
                            <p className="text-[11px] text-muted-foreground font-medium truncate max-w-[300px]">{folder.name}</p>
                        </div>
                    </div>
                    <button onClick={onClose} className="p-2 rounded-xl hover:bg-muted transition-colors text-muted-foreground hover:text-foreground">
                        <X className="h-4 w-4" />
                    </button>
                </div>

                {/* Body */}
                <div className="flex-1 overflow-y-auto px-6 py-4 space-y-5">
                    {/* Add users section */}
                    <div className="space-y-3">
                        <span className="text-xs font-bold text-muted-foreground">إضافة مستخدمين للاطلاع</span>
                        <MultiUserPicker
                            selectedUserIds={newUserIds}
                            onUsersChange={setNewUserIds}
                            label="مستخدمين جدد"
                            placeholder="ابحث عن مستخدم..."
                            departmentOnly={true}
                        />
                        {newUserIds.length > 0 && (
                            <Button
                                onClick={handleAddUsers}
                                disabled={saving}
                                className="w-full rounded-xl font-bold text-sm"
                                size="sm"
                            >
                                {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : `إضافة ${newUserIds.length} مستخدم`}
                            </Button>
                        )}
                    </div>

                    {/* Existing permissions */}
                    <div className="space-y-3">
                        <span className="text-xs font-bold text-muted-foreground">الصلاحيات الحالية ({permissions.length})</span>
                        {loading ? (
                            <div className="flex items-center justify-center py-8 text-muted-foreground">
                                <Loader2 className="h-6 w-6 animate-spin" />
                            </div>
                        ) : permissions.length === 0 ? (
                            <p className="text-xs text-muted-foreground/60 text-center py-6">لا توجد صلاحيات مخصصة لهذا المجلد.</p>
                        ) : (
                            <div className="space-y-2">
                                {permissions.map(p => (
                                    <div key={p.id} className="flex items-center justify-between p-3 rounded-xl bg-muted/30 border border-border/60">
                                        <div className="flex items-center gap-3 min-w-0">
                                            <div className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0">
                                                <Shield className="h-4 w-4" />
                                            </div>
                                            <div className="flex flex-col min-w-0">
                                                <UserDisplay userId={p.userId} showIcon={false} className="text-xs font-bold" />
                                                <span className="text-[10px] text-muted-foreground flex items-center gap-1">
                                                    {p.isInherited ? <Unlock className="h-3 w-3" /> : <Lock className="h-3 w-3" />}
                                                    {accessLevelLabel(p.accessLevel)}
                                                    {p.isInherited ? ' · موروث' : ''}
                                                </span>
                                            </div>
                                        </div>
                                        <button
                                            onClick={() => handleDelete(p.id)}
                                            disabled={deletingId === p.id}
                                            className="p-2 rounded-lg text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors shrink-0"
                                            title="إزالة الصلاحية"
                                        >
                                            {deletingId === p.id
                                                ? <Loader2 className="h-4 w-4 animate-spin" />
                                                : <Trash2 className="h-4 w-4" />
                                            }
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>
                </div>

                {/* Footer */}
                <div className="px-6 py-4 border-t border-border/60 flex justify-end">
                    <Button onClick={onClose} variant="ghost" className="rounded-xl font-bold text-sm">
                        إغلاق
                    </Button>
                </div>
            </div>
        </div>
    );
};
