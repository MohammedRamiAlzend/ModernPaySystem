/* eslint-disable react-hooks/set-state-in-effect */
import { useState, useEffect, useCallback, useMemo } from 'react';
import { createPortal } from 'react-dom';
import { Folder, FolderPermissionDto } from '@/features/archiving/model/types';
import { archivingService } from '@/features/archiving/api/archivingService';
import { MultiUserPicker } from '@/features/users/ui/MultiUserPicker';
import { UserDisplay } from '@/features/users/ui/UserDisplay';
import { DepartmentPicker } from '@/features/departments/ui/DepartmentPicker';
import { SubfolderTreeView } from '@/features/archiving/ui/SubfolderTreeView';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import { X, Shield, Loader2, Trash2, Lock, Unlock, Users, Building2, FolderTree, Search, Check, Minus } from 'lucide-react';
import { useUIStore } from '@/app/store/uiStore';
import { cn } from '@/shared/lib/utils';

interface FolderPermissionsModalProps {
    isOpen: boolean;
    folder: Folder | null;
    onClose: () => void;
}

type PermissionTab = 'users' | 'departments';

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

    // New state
    const [activeTab, setActiveTab] = useState<PermissionTab>('users');
    const [newUserIds, setNewUserIds] = useState<string[]>([]);
    const [newDepartmentIds, setNewDepartmentIds] = useState<string[]>([]);
    const [selectedSubFolderIds, setSelectedSubFolderIds] = useState<string[]>([]);
    const [saving, setSaving] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const [filterType, setFilterType] = useState<'all' | 'users' | 'departments'>('all');
    const [selectedPermissionIds, setSelectedPermissionIds] = useState<Set<string>>(new Set());

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
        setNewDepartmentIds([]);
        setSelectedSubFolderIds([]);
        setActiveTab('users');
        setSearchQuery('');
        setFilterType('all');
        setSelectedPermissionIds(new Set());
    }

    useEffect(() => {
        if (isOpen && folder) {
            loadPermissions(folder.id);
        }
    }, [isOpen, folder, loadPermissions]);

    const handleAddUsers = async () => {
        if (!folder) return;
        setSaving(true);
        let added = 0;

        const targetFolderIds = selectedSubFolderIds.length > 0
            ? selectedSubFolderIds
            : [folder.id];

        try {
            if (activeTab === 'users' && newUserIds.length > 0) {
                for (const userId of newUserIds) {
                    const result = await archivingService.createBulkFolderPermission({
                        folderIds: targetFolderIds,
                        userId,
                        accessLevel: 1,
                        isInherited: true
                    });
                    added += result.length;
                }
                setNewUserIds([]);
            } else if (activeTab === 'departments' && newDepartmentIds.length > 0) {
                for (const deptId of newDepartmentIds) {
                    const result = await archivingService.createBulkFolderPermission({
                        folderIds: targetFolderIds,
                        departmentId: deptId,
                        accessLevel: 1,
                        isInherited: true
                    });
                    added += result.length;
                }
                setNewDepartmentIds([]);
            }
            setSelectedSubFolderIds([]);
        } catch {
            showStatus({ type: 'error', title: 'خطأ', message: 'حدث خطأ أثناء إضافة الصلاحيات.' });
        }

        setSaving(false);
        if (added > 0) {
            showStatus({ type: 'success', title: 'تمت الإضافة', message: `تمت إضافة ${added} صلاحية بنجاح.` });
            await loadPermissions(folder.id);
        }
    };

    const toggleSelect = (id: string) => {
        setSelectedPermissionIds(prev => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id); else next.add(id);
            return next;
        });
    };

    const selectAll = () => {
        setSelectedPermissionIds(new Set(filteredPermissions.map(p => p.id)));
    };

    const deselectAll = () => {
        setSelectedPermissionIds(new Set());
    };

    const handleBulkDelete = async () => {
        if (selectedPermissionIds.size === 0) return;
        let deleted = 0;
        for (const id of selectedPermissionIds) {
            try {
                await archivingService.deleteFolderPermission(id);
                deleted++;
            } catch { /* skip */ }
        }
        setPermissions(prev => prev.filter(p => !selectedPermissionIds.has(p.id)));
        setSelectedPermissionIds(new Set());
        if (deleted > 0) showStatus({ type: 'success', title: 'تم', message: `تم إزالة ${deleted} صلاحية بنجاح.` });
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

    const filteredPermissions = useMemo(() => {
        let result = permissions;
        if (filterType === 'users') result = result.filter(p => !p.departmentId);
        if (filterType === 'departments') result = result.filter(p => p.departmentId);
        if (searchQuery.trim()) {
            const q = searchQuery.trim().toLowerCase();
            result = result.filter(p => {
                if (p.departmentId) return p.departmentName?.toLowerCase().includes(q);
                return p.userId?.toLowerCase().includes(q);
            });
        }
        return result;
    }, [permissions, searchQuery, filterType]);

    if (!isOpen || !folder) return null;

    const canAdd = activeTab === 'users' ? newUserIds.length > 0 : newDepartmentIds.length > 0;

    return createPortal(
        <div className="fixed inset-0 bg-black/40 backdrop-blur-[2px] flex items-center justify-center z-[200] animate-in fade-in duration-200" onClick={onClose}>
            <div className="bg-card border border-border rounded-3xl shadow-2xl w-full max-w-2xl max-h-[90vh] flex flex-col mx-4" dir="rtl" onClick={e => e.stopPropagation()}>
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
                    {/* Tabs */}
                    <div className="flex bg-muted/50 rounded-xl p-1">
                        <button
                            onClick={() => { setActiveTab('users'); setNewUserIds([]); setNewDepartmentIds([]); }}
                            className={cn(
                                'flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-bold transition-all',
                                activeTab === 'users' ? 'bg-background shadow-sm text-foreground' : 'text-muted-foreground hover:text-foreground'
                            )}
                        >
                            <Users className="w-4 h-4" />
                            مستخدمين
                        </button>
                        <button
                            onClick={() => { setActiveTab('departments'); setNewUserIds([]); setNewDepartmentIds([]); }}
                            className={cn(
                                'flex-1 flex items-center justify-center gap-2 py-2 rounded-lg text-sm font-bold transition-all',
                                activeTab === 'departments' ? 'bg-background shadow-sm text-foreground' : 'text-muted-foreground hover:text-foreground'
                            )}
                        >
                            <Building2 className="w-4 h-4" />
                            أقسام
                        </button>
                    </div>

                    {/* Add permission section */}
                    <div className="space-y-3">
                        <span className="text-xs font-bold text-muted-foreground">
                            {activeTab === 'users' ? 'إضافة مستخدمين للاطلاع' : 'إضافة أقسام للاطلاع'}
                        </span>

                        {activeTab === 'users' ? (
                            <MultiUserPicker
                                selectedUserIds={newUserIds}
                                onUsersChange={setNewUserIds}
                                label="مستخدمين جدد"
                                placeholder="ابحث عن مستخدم..."
                            />
                        ) : (
                            <DepartmentPicker
                                selectedDepartmentIds={newDepartmentIds}
                                onDepartmentsChange={setNewDepartmentIds}
                            />
                        )}

                        {/* Subfolder scope */}
                        {canAdd && (
                            <div className="border border-border/60 rounded-xl p-3 space-y-3 bg-muted/20">
                                <div className="flex items-center gap-2 text-xs font-bold text-muted-foreground">
                                    <FolderTree className="w-3.5 h-3.5" />
                                    نطاق الصلاحية
                                </div>
                                <SubfolderTreeView
                                    folderId={folder.id}
                                    selectedFolderIds={selectedSubFolderIds}
                                    onSelectionChange={setSelectedSubFolderIds}
                                />
                            </div>
                        )}

                        {canAdd && (
                            <Button
                                onClick={handleAddUsers}
                                disabled={saving}
                                className="w-full rounded-xl font-bold text-sm"
                                size="sm"
                            >
                                {saving ? <Loader2 className="h-4 w-4 animate-spin" /> : `إضافة ${activeTab === 'users' ? newUserIds.length : newDepartmentIds.length} ${activeTab === 'users' ? 'مستخدم' : 'قسم'}`}
                            </Button>
                        )}
                    </div>

                    {/* Existing permissions */}
                    <div className="space-y-3">
                        <div className="flex items-center justify-between">
                            <div className="flex items-center gap-2">
                                {filteredPermissions.length > 0 && (
                                    <button
                                        onClick={() => selectedPermissionIds.size === filteredPermissions.length ? deselectAll() : selectAll()}
                                        className="w-4 h-4 rounded border-2 flex items-center justify-center shrink-0 transition-colors"
                                        style={{ borderColor: selectedPermissionIds.size > 0 ? 'var(--primary)' : undefined, backgroundColor: selectedPermissionIds.size === filteredPermissions.length ? 'var(--primary)' : selectedPermissionIds.size > 0 ? 'var(--primary)' : undefined }}
                                    >
                                        {selectedPermissionIds.size === filteredPermissions.length && <Check className="w-3 h-3 text-primary-foreground" />}
                                        {selectedPermissionIds.size > 0 && selectedPermissionIds.size < filteredPermissions.length && <Minus className="w-3 h-3 text-primary-foreground" />}
                                    </button>
                                )}
                                <span className="text-xs font-bold text-muted-foreground">الصلاحيات الحالية ({filteredPermissions.length})</span>
                            </div>
                            <div className="flex items-center gap-1">
                                {selectedPermissionIds.size > 0 && (
                                    <button onClick={handleBulkDelete} className="text-[10px] px-2 py-1 rounded-lg font-bold bg-destructive/10 text-destructive hover:bg-destructive/20 transition-colors">
                                        <Trash2 className="w-3 h-3 inline ml-1" />حذف المحدد ({selectedPermissionIds.size})
                                    </button>
                                )}
                                <button onClick={() => setFilterType('all')} className={`text-[10px] px-2 py-1 rounded-lg font-bold transition-colors ${filterType === 'all' ? 'bg-primary/10 text-primary' : 'text-muted-foreground hover:text-foreground'}`}>الكل</button>
                                <button onClick={() => setFilterType('users')} className={`text-[10px] px-2 py-1 rounded-lg font-bold transition-colors ${filterType === 'users' ? 'bg-primary/10 text-primary' : 'text-muted-foreground hover:text-foreground'}`}>مستخدمين</button>
                                <button onClick={() => setFilterType('departments')} className={`text-[10px] px-2 py-1 rounded-lg font-bold transition-colors ${filterType === 'departments' ? 'bg-primary/10 text-primary' : 'text-muted-foreground hover:text-foreground'}`}>أقسام</button>
                            </div>
                        </div>
                        {permissions.length > 0 && (
                            <div className="relative">
                                <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-muted-foreground/60" />
                                <Input value={searchQuery} onChange={(e) => setSearchQuery(e.target.value)} placeholder="ابحث بالاسم..." className="h-8 text-xs pr-8 rounded-lg" />
                            </div>
                        )}
                        {loading ? (
                            <div className="flex items-center justify-center py-8 text-muted-foreground">
                                <Loader2 className="h-6 w-6 animate-spin" />
                            </div>
                        ) : filteredPermissions.length === 0 ? (
                            <p className="text-xs text-muted-foreground/60 text-center py-6">{searchQuery || filterType !== 'all' ? 'لا توجد صلاحيات تطابق البحث' : 'لا توجد صلاحيات مخصصة لهذا المجلد.'}</p>
                        ) : (
                            <div className="space-y-2">
                                {filteredPermissions.map(p => {
                                    const isSelected = selectedPermissionIds.has(p.id);
                                    return (
                                        <div
                                            key={p.id}
                                            onClick={() => toggleSelect(p.id)}
                                            className={`flex items-center justify-between p-3 rounded-xl border border-border/60 cursor-pointer transition-colors ${isSelected ? 'bg-primary/5 border-primary/30' : 'bg-muted/30 hover:bg-muted/50'}`}
                                        >
                                            <div className="flex items-center gap-3 min-w-0">
                                                <button
                                                    onClick={(e) => { e.stopPropagation(); toggleSelect(p.id); }}
                                                    className={`w-4 h-4 rounded border-2 flex items-center justify-center shrink-0 transition-colors ${isSelected ? 'bg-primary border-primary' : 'border-muted-foreground/30'}`}
                                                >
                                                    {isSelected && <Check className="w-3 h-3 text-primary-foreground" />}
                                                </button>
                                                <div className="w-8 h-8 rounded-lg bg-primary/10 text-primary flex items-center justify-center shrink-0">
                                                    {p.departmentId ? <Building2 className="h-4 w-4" /> : <Shield className="h-4 w-4" />}
                                                </div>
                                                <div className="flex flex-col min-w-0">
                                                    {p.departmentId ? (
                                                        <span className="text-xs font-bold truncate">{p.departmentName ?? 'قسم'}</span>
                                                    ) : (
                                                        <UserDisplay userId={p.userId!} showIcon={false} className="text-xs font-bold" />
                                                    )}
                                                    <span className="text-[10px] text-muted-foreground flex items-center gap-1">
                                                        {p.isInherited ? <Unlock className="h-3 w-3" /> : <Lock className="h-3 w-3" />}
                                                        {accessLevelLabel(p.accessLevel)}
                                                        {p.isInherited ? ' · موروث' : ''}
                                                    </span>
                                                </div>
                                            </div>
                                            <button
                                                onClick={(e) => { e.stopPropagation(); handleDelete(p.id); }}
                                                disabled={deletingId === p.id}
                                                className="p-2 rounded-lg text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors shrink-0"
                                                title="إزالة الصلاحية"
                                            >
                                                {deletingId === p.id ? <Loader2 className="h-4 w-4 animate-spin" /> : <Trash2 className="h-4 w-4" />}
                                            </button>
                                        </div>
                                    );
                                })}
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
        </div>,
        document.body
    );
};
