import React, { useState, useEffect } from 'react';
import { Folder, ArchiveRecord } from '../model/types';
import { 
    Folder as FolderIcon, 
    FileText, 
    MoreVertical, 
    Edit3, 
    Trash2, 
    Download, 
    Eye,
    Plus,
    FolderPlus
} from 'lucide-react';

import { Button } from '@/shared/ui/button';

interface ExplorerViewProps {
    folders: Folder[];
    records: ArchiveRecord[];
    onFolderDoubleClick: (folder: Folder) => void;
    onRecordClick: (record: ArchiveRecord) => void;
    onFolderEdit?: (folder: Folder) => void;
    onFolderDelete?: (folder: Folder) => void;
    onRecordEdit?: (record: ArchiveRecord) => void;
    onRecordDelete?: (record: ArchiveRecord) => void;
    onRecordDownloadZip?: (record: ArchiveRecord) => void;
    onRecordRequestEdit?: (record: ArchiveRecord) => void;
    onCreateFolder?: () => void;
    onCreateRecord?: () => void;
    isLoading?: boolean;
    hasMore?: boolean;
    onLoadMore?: () => void;
}


export const ExplorerView: React.FC<ExplorerViewProps> = ({
    folders,
    records,
    onFolderDoubleClick,
    onRecordClick,
    onFolderEdit,
    onFolderDelete,
    // onRecordEdit,
    onRecordDelete,
    onRecordDownloadZip,
    onRecordRequestEdit,
    onCreateFolder,
    onCreateRecord,
    isLoading = false,
    hasMore = false,
    onLoadMore
}) => {
    const [activeMenuId, setActiveMenuId] = useState<string | null>(null);
    const [contextMenu, setContextMenu] = useState<{
        x: number;
        y: number;
        type: 'empty' | 'folder' | 'record';
        targetId: string;
    } | null>(null);

    useEffect(() => {
        const handleCloseMenu = () => setContextMenu(null);
        window.addEventListener('click', handleCloseMenu);
        window.addEventListener('contextmenu', handleCloseMenu);
        return () => {
            window.removeEventListener('click', handleCloseMenu);
            window.removeEventListener('contextmenu', handleCloseMenu);
        };
    }, []);

    const toggleMenu = (id: string, e: React.MouseEvent) => {
        e.stopPropagation();
        setActiveMenuId(activeMenuId === id ? null : id);
        setContextMenu(null);
    };

    const handleAction = (action: () => void) => {
        action();
        setActiveMenuId(null);
    };

    const handleContextMenu = (e: React.MouseEvent, type: 'empty' | 'folder' | 'record', targetId: string = '') => {
        e.preventDefault();
        e.stopPropagation();
        setActiveMenuId(null);
        setContextMenu({
            x: e.clientX,
            y: e.clientY,
            type,
            targetId
        });
    };

    const targetFolder = folders.find(f => f.id === contextMenu?.targetId);
    const targetRecord = records.find(r => r.id === contextMenu?.targetId);

    return (
        <div 
            className="flex flex-col gap-6 min-h-[350px] w-full pb-12 select-none" 
            onClick={() => {
                setActiveMenuId(null);
                setContextMenu(null);
            }}
            onContextMenu={(e) => handleContextMenu(e, 'empty')}
        >
            {/* Folders Section */}
            {folders.length > 0 && (
                <div className="flex flex-col gap-3">
                    <span className="text-xs font-bold text-muted-foreground/60 text-right">المجلدات ({folders.length})</span>
                    <div className="grid grid-cols-2 sm:grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-4">
                        {folders.map((folder) => (
                            <div
                                key={folder.id}
                                onDoubleClick={() => onFolderDoubleClick(folder)}
                                onContextMenu={(e) => handleContextMenu(e, 'folder', folder.id)}
                                className="group relative bg-muted/20 hover:bg-amber-500/5 border border-border/80 hover:border-amber-500/30 rounded-2xl p-4 flex flex-col items-center justify-center cursor-pointer transition-all duration-300 select-none text-center"
                            >
                                {/* Context Menu Button */}
                                <div className="absolute top-2 left-2">
                                    <button
                                        onClick={(e) => toggleMenu(folder.id, e)}
                                        className="p-1 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100"
                                    >
                                        <MoreVertical className="h-4 w-4" />
                                    </button>

                                    {activeMenuId === folder.id && (
                                        <div className="absolute left-0 mt-1 w-32 bg-card border border-border rounded-xl shadow-xl z-20 py-1 text-right" onClick={(e) => e.stopPropagation()}>
                                            {onFolderEdit && (
                                                <button
                                                    onClick={() => handleAction(() => onFolderEdit(folder))}
                                                    className="w-full px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted flex items-center justify-end gap-2"
                                                >
                                                    <span>تعديل الاسم</span>
                                                    <Edit3 className="h-3.5 w-3.5" />
                                                </button>
                                            )}
                                            {onFolderDelete && (
                                                <button
                                                    onClick={() => handleAction(() => onFolderDelete(folder))}
                                                    className="w-full px-3 py-2 text-xs font-semibold text-destructive hover:bg-destructive/10 flex items-center justify-end gap-2"
                                                >
                                                    <span>حذف</span>
                                                    <Trash2 className="h-3.5 w-3.5" />
                                                </button>
                                            )}
                                        </div>
                                    )}
                                </div>

                                <div className="w-12 h-12 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">
                                    <FolderIcon className="h-6 w-6 fill-amber-500/20" />
                                </div>
                                <span className="text-xs font-bold text-foreground line-clamp-2 break-all w-full px-1">
                                    {folder.name}
                                </span>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* Records Section */}
            {records.length > 0 && (
                <div className="flex flex-col gap-3">
                    <span className="text-xs font-bold text-muted-foreground/60 text-right">المستندات المؤرشفة ({records.length})</span>
                    <div className="grid grid-cols-2 sm:grid-cols-4 md:grid-cols-6 lg:grid-cols-8 gap-4">
                        {records.map((record) => (
                            <div
                                key={record.id}
                                onClick={() => onRecordClick(record)}
                                onContextMenu={(e) => handleContextMenu(e, 'record', record.id)}
                                className="group relative bg-muted/20 hover:bg-primary/5 border border-border/80 hover:border-primary/20 rounded-2xl p-4 flex flex-col items-center justify-center cursor-pointer transition-all duration-300 select-none text-center"
                            >
                                {/* Context Menu Button */}
                                <div className="absolute top-2 left-2" onClick={(e) => e.stopPropagation()}>
                                    <button
                                        onClick={(e) => toggleMenu(record.id, e)}
                                        className="p-1 rounded-lg text-muted-foreground hover:text-foreground hover:bg-muted transition-colors opacity-0 group-hover:opacity-100 focus:opacity-100"
                                    >
                                        <MoreVertical className="h-4 w-4" />
                                    </button>

                                    {activeMenuId === record.id && (
                                        <div className="absolute left-0 mt-1 w-36 bg-card border border-border rounded-xl shadow-xl z-20 py-1 text-right">
                                            <button
                                                onClick={() => handleAction(() => onRecordClick(record))}
                                                className="w-full px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted flex items-center justify-end gap-2"
                                            >
                                                <span>عرض وتحميل</span>
                                                <Eye className="h-3.5 w-3.5" />
                                            </button>
                                            {/* {onRecordEdit && (
                                                <button
                                                    onClick={() => handleAction(() => onRecordEdit(record))}
                                                    className="w-full px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted flex items-center justify-end gap-2"
                                                >
                                                    <span>تعديل</span>
                                                    <Edit3 className="h-3.5 w-3.5" />
                                                </button>
                                            )} */}
                                            {onRecordRequestEdit && (
                                                <button
                                                    onClick={() => handleAction(() => onRecordRequestEdit(record))}
                                                    className="w-full px-3 py-2 text-xs font-semibold text-amber-500 hover:bg-amber-500/10 flex items-center justify-end gap-2"
                                                >
                                                    <span>طلب تعديل</span>
                                                    <Edit3 className="h-3.5 w-3.5" />
                                                </button>
                                            )}
                                            {onRecordDownloadZip && (
                                                <button
                                                    onClick={() => handleAction(() => onRecordDownloadZip(record))}
                                                    className="w-full px-3 py-2 text-xs font-semibold text-foreground hover:bg-muted flex items-center justify-end gap-2"
                                                >
                                                    <span>تحميل الكل (ZIP)</span>
                                                    <Download className="h-3.5 w-3.5" />
                                                </button>
                                            )}
                                            {onRecordDelete && (
                                                <button
                                                    onClick={() => handleAction(() => onRecordDelete(record))}
                                                    className="w-full px-3 py-2 text-xs font-semibold text-destructive hover:bg-destructive/10 flex items-center justify-end gap-2"
                                                >
                                                    <span>حذف</span>
                                                    <Trash2 className="h-3.5 w-3.5" />
                                                </button>
                                            )}
                                        </div>
                                    )}
                                </div>

                                <div className="w-12 h-12 rounded-xl bg-sky-500/10 text-sky-500 flex items-center justify-center mb-3 group-hover:scale-110 transition-transform">
                                    <FileText className="h-6 w-6 fill-sky-500/20" />
                                </div>
                                <span className="text-xs font-bold text-foreground line-clamp-1 w-full truncate">
                                    {record.archivalNumber}
                                </span>
                                <span className="text-[10px] text-muted-foreground mt-1 block">
                                    {record.physicalFiles?.length || 0} ملف
                                </span>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* Empty State */}
            {folders.length === 0 && records.length === 0 && (
                <div className="py-24 flex flex-col items-center justify-center text-muted-foreground gap-3">
                    <svg className="w-16 h-16 stroke-[1.2] text-muted-foreground/45" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 12.75V12A2.25 2.25 0 014.5 9.75h15A2.25 2.25 0 0121.75 12v.75m-19.5 0A2.25 2.25 0 004.5 15h15a2.25 2.25 0 002.25-2.25m-19.5 0v.25A2.25 2.25 0 004.5 18h15a2.25 2.25 0 002.25-2.25v-.25" />
                    </svg>
                    <span className="text-sm font-semibold">المجلد فارغ تماماً</span>
                    <span className="text-xs text-muted-foreground/60">يمكنك إنشاء مجلدات فرعية أو أرشفة مستندات جديدة هنا.</span>
                </div>
            )}

            {/* Pagination / Load More */}
            {hasMore && onLoadMore && (
                <div className="flex justify-center py-6 mt-4">
                    <Button
                        variant="outline"
                        onClick={onLoadMore}
                        disabled={isLoading}
                        className="rounded-xl px-8 border-border text-foreground hover:bg-muted font-bold transition-all hover:scale-[1.02]"
                    >
                        {isLoading ? 'جاري التحميل...' : 'تحميل المزيد'}
                    </Button>
                </div>
            )}

            {/* Context Menu */}
            {contextMenu && (
                <div
                    style={{
                        position: 'fixed',
                        left: `${contextMenu.x}px`,
                        top: `${contextMenu.y}px`,
                        zIndex: 1000,
                    }}
                    className="w-48 bg-card/95 backdrop-blur-md border border-border/60 rounded-2xl shadow-xl shadow-black/10 py-1.5 animate-in fade-in zoom-in-95 duration-100 select-none text-right"
                    dir="rtl"
                    onClick={(e) => e.stopPropagation()}
                >
                    {contextMenu.type === 'empty' && (
                        <>
                            <button
                                onClick={() => {
                                    onCreateFolder?.();
                                    setContextMenu(null);
                                }}
                                className="w-full px-4 py-2.5 text-xs font-bold text-foreground hover:bg-muted flex items-center justify-end gap-2 group transition-colors text-right"
                            >
                                <span>إنشاء مجلد جديد</span>
                                <FolderPlus className="h-4 w-4 text-amber-500" />
                            </button>
                            {onCreateRecord && (
                                <button
                                    onClick={() => {
                                        onCreateRecord?.();
                                        setContextMenu(null);
                                    }}
                                    className="w-full px-4 py-2.5 text-xs font-bold text-foreground hover:bg-muted flex items-center justify-end gap-2 group transition-colors text-right"
                                >
                                    <span>أرشفة مستند جديد</span>
                                    <Plus className="h-4 w-4 text-primary" />
                                </button>
                            )}
                        </>
                    )}

                    {contextMenu.type === 'folder' && targetFolder && (
                        <>
                            {onFolderEdit && (
                                <button
                                    onClick={() => {
                                        onFolderEdit(targetFolder);
                                        setContextMenu(null);
                                    }}
                                    className="w-full px-4 py-2.5 text-xs font-bold text-foreground hover:bg-muted flex items-center justify-end gap-2 group transition-colors text-right"
                                >
                                    <span>تعديل الاسم</span>
                                    <Edit3 className="h-4 w-4 text-amber-500" />
                                </button>
                            )}
                            {onFolderDelete && (
                                <button
                                    onClick={() => {
                                        onFolderDelete(targetFolder);
                                        setContextMenu(null);
                                    }}
                                    className="w-full px-4 py-2.5 text-xs font-bold text-destructive hover:bg-destructive/10 flex items-center justify-end gap-2 group transition-colors text-right"
                                >
                                    <span>حذف المجلد</span>
                                    <Trash2 className="h-4 w-4" />
                                </button>
                            )}
                        </>
                    )}

                    {contextMenu.type === 'record' && targetRecord && (
                        <>
                            <button
                                onClick={() => {
                                    onRecordClick(targetRecord);
                                    setContextMenu(null);
                                }}
                                className="w-full px-4 py-2.5 text-xs font-bold text-foreground hover:bg-muted flex items-center justify-end gap-2 group transition-colors text-right"
                            >
                                <span>عرض وتحميل</span>
                                <Eye className="h-4 w-4 text-sky-500" />
                            </button>
                            {onRecordRequestEdit && (
                                <button
                                    onClick={() => {
                                        onRecordRequestEdit(targetRecord);
                                        setContextMenu(null);
                                    }}
                                    className="w-full px-4 py-2.5 text-xs font-bold text-amber-500 hover:bg-amber-500/10 flex items-center justify-end gap-2 group transition-colors text-right"
                                >
                                    <span>طلب تعديل</span>
                                    <Edit3 className="h-4 w-4" />
                                </button>
                            )}
                            {onRecordDownloadZip && (
                                <button
                                    onClick={() => {
                                        onRecordDownloadZip(targetRecord);
                                        setContextMenu(null);
                                    }}
                                    className="w-full px-4 py-2.5 text-xs font-bold text-foreground hover:bg-muted flex items-center justify-end gap-2 group transition-colors text-right"
                                >
                                    <span>تحميل الكل (ZIP)</span>
                                    <Download className="h-4 w-4 text-emerald-500" />
                                </button>
                            )}
                            {onRecordDelete && (
                                <button
                                    onClick={() => {
                                        onRecordDelete(targetRecord);
                                        setContextMenu(null);
                                    }}
                                    className="w-full px-4 py-2.5 text-xs font-bold text-destructive hover:bg-destructive/10 flex items-center justify-end gap-2 group transition-colors text-right"
                                >
                                    <span>حذف المستند</span>
                                    <Trash2 className="h-4 w-4" />
                                </button>
                            )}
                        </>
                    )}
                </div>
            )}
        </div>
    );
};