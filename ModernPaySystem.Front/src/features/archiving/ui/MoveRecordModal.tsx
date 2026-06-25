import React, { useState, useEffect } from 'react';
import { Folder, ArchiveRecord } from '../model/types';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from '@/shared/ui/dialog';
import { Button } from '@/shared/ui/button';
import {
    Folder as FolderIcon,
    FileText,
    Move,
    Search,
    Loader2
} from 'lucide-react';
import { Input } from '@/shared/ui/input';

interface MoveRecordModalProps {
    isOpen: boolean;
    record: ArchiveRecord | null;
    folders: Folder[];
    currentFolderId?: string | null;
    onClose: () => void;
    onConfirm: (recordId: string, destinationFolderId: string) => void;
    isLoading?: boolean;
}

export const MoveRecordModal: React.FC<MoveRecordModalProps> = ({
    isOpen,
    record,
    folders,
    currentFolderId,
    onClose,
    onConfirm,
    isLoading = false
}) => {
    const [searchQuery, setSearchQuery] = useState('');
    const [selectedFolderId, setSelectedFolderId] = useState<string | null>(null);

    useEffect(() => {
        if (isOpen) {
            setSearchQuery('');
            setSelectedFolderId(null);
        }
    }, [isOpen]);

    const folderMap = new Map(folders.map(f => [f.id, f]));
    const getFolderPath = (folderId: string): string => {
        const folder = folderMap.get(folderId);
        if (!folder) return '';
        const parentPath = folder.parentId ? getFolderPath(folder.parentId) : '';
        return parentPath ? `${parentPath} / ${folder.name}` : folder.name;
    };

    const filteredFolders = folders.filter(f => {
        if (f.id === currentFolderId) return false;
        if (f.id === record?.folderId) return false;
        if (searchQuery.trim() === '') return true;
        const name = f.name.toLowerCase();
        const query = searchQuery.toLowerCase();
        return name.includes(query);
    });

    const rootFolders = filteredFolders.filter(f => !f.parentId);
    const childFolders = filteredFolders.filter(f => f.parentId);

    const renderFolderItem = (folder: Folder) => {
        const isSelected = selectedFolderId === folder.id;
        return (
            <button
                key={folder.id}
                onClick={() => setSelectedFolderId(folder.id)}
                className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-right text-sm transition-all duration-200 border ${
                    isSelected
                        ? 'bg-primary/10 border-primary text-primary font-medium shadow-sm'
                        : 'hover:bg-accent border-transparent text-foreground'
                }`}
            >
                <FolderIcon className={`w-5 h-5 shrink-0 ${isSelected ? 'text-primary' : 'text-amber-500/80'}`} />
                <div className="flex flex-col min-w-0">
                    <span className="truncate font-medium">{folder.name}</span>
                    {folder.parentId && (
                        <span className="text-[10px] text-muted-foreground truncate">
                            {getFolderPath(folder.parentId)}
                        </span>
                    )}
                </div>
            </button>
        );
    };

    return (
        <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
            <DialogContent className="max-w-lg w-[90vw] max-h-[80vh] flex flex-col p-6 rounded-2xl gap-4 bg-background">
                <DialogHeader className="text-right">
                    <DialogTitle className="flex items-center gap-2 text-xl font-bold text-primary">
                        <Move className="w-6 h-6 text-primary" />
                        <span>نقل المستند</span>
                    </DialogTitle>
                </DialogHeader>

                {record && (
                    <div className="flex items-center gap-3 bg-muted/30 rounded-xl px-4 py-3 border">
                        <FileText className="w-5 h-5 text-sky-500 shrink-0" />
                        <div className="flex flex-col text-right">
                            <span className="text-sm font-bold text-foreground">
                                {record.id.slice(0, 8)}
                            </span>
                            <span className="text-[10px] text-muted-foreground">
                                {record.physicalFiles?.length || 0} ملف
                            </span>
                        </div>
                    </div>
                )}

                <div className="relative shrink-0">
                    <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                    <Input
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        placeholder="ابحث عن مجلد..."
                        className="pr-10"
                    />
                </div>

                <div className="flex-1 overflow-y-auto border rounded-xl bg-muted/10 p-2 min-h-[200px] max-h-[350px]">
                    {filteredFolders.length === 0 ? (
                        <div className="h-full flex items-center justify-center text-muted-foreground text-sm">
                            لا توجد مجلدات متاحة للنقل
                        </div>
                    ) : (
                        <div className="flex flex-col gap-1">
                            {rootFolders.length > 0 && (
                                <div className="flex flex-col gap-1">
                                    <span className="text-[10px] font-bold text-muted-foreground/60 px-2 py-1">
                                        المجلدات الرئيسية
                                    </span>
                                    {rootFolders.map(renderFolderItem)}
                                </div>
                            )}
                            {childFolders.length > 0 && (
                                <div className="flex flex-col gap-1 mt-2">
                                    <span className="text-[10px] font-bold text-muted-foreground/60 px-2 py-1">
                                        المجلدات الفرعية
                                    </span>
                                    {childFolders.map(renderFolderItem)}
                                </div>
                            )}
                        </div>
                    )}
                </div>

                <DialogFooter className="flex flex-row justify-between items-center sm:justify-between shrink-0 gap-2">
                    <Button variant="outline" onClick={onClose} className="rounded-xl">
                        إلغاء
                    </Button>
                    <Button
                        onClick={() => record && selectedFolderId && onConfirm(record.id, selectedFolderId)}
                        disabled={!selectedFolderId || isLoading}
                        className="rounded-xl px-6"
                    >
                        {isLoading ? (
                            <>
                                <Loader2 className="w-4 h-4 animate-spin ml-2" />
                                جاري النقل...
                            </>
                        ) : (
                            'نقل المستند'
                        )}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};