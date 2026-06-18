import React, { useState } from 'react';
import { useSystemDrives, useSubdirectories } from '../model/queries';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogFooter,
} from '@/shared/ui/dialog';
import { Button } from '@/shared/ui/button';
import { Input } from '@/shared/ui/input';
import {
    Folder,
    HardDrive,
    ArrowUp,
    FolderOpen,
    Loader2,
    Search,
    ChevronLeft
} from 'lucide-react';
import { Alert, AlertDescription } from '@/shared/ui/alert';

interface FolderPickerModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSelect: (path: string) => void;
    initialPath?: string;
}

export const FolderPickerModal: React.FC<FolderPickerModalProps> = ({
    isOpen,
    onClose,
    onSelect,
    initialPath = '',
}) => {
    const [currentPath, setCurrentPath] = useState<string>(initialPath);
    const [searchQuery, setSearchQuery] = useState<string>('');
    const [customPath, setCustomPath] = useState<string>('');

    const [prevIsOpen, setPrevIsOpen] = useState<boolean>(isOpen);

    if (isOpen !== prevIsOpen) {
        setPrevIsOpen(isOpen);
        if (isOpen) {
            setCurrentPath(initialPath);
            setCustomPath(initialPath);
            setSearchQuery('');
        }
    }

    // Fetch logical drives (if path is empty)
    const {
        data: drives = [],
        isLoading: isDrivesLoading,
        isError: isDrivesError,
    } = useSystemDrives();

    // Fetch subdirectories for current path
    const {
        data: subdirs = [],
        isLoading: isSubdirsLoading,
        isError: isSubdirsError,
        error: subdirsError,
    } = useSubdirectories(currentPath);

    const isLoading = currentPath === '' ? isDrivesLoading : isSubdirsLoading;
    const isError = currentPath === '' ? isDrivesError : isSubdirsError;

    // Detect path separator
    const getSeparator = (path: string) => (path.includes('/') ? '/' : '\\');

    // Breadcrumbs parsing
    const separator = getSeparator(currentPath);
    const breadcrumbs = currentPath ? currentPath.split(separator).filter(Boolean) : [];

    const handleBreadcrumbClick = (index: number) => {
        const parts = currentPath.split(separator).filter(Boolean);
        let newPath = parts.slice(0, index + 1).join(separator);
        if (currentPath.match(/^[a-zA-Z]:/) && index === 0) {
            newPath += separator; // ensure "C:\" style
        }
        setCurrentPath(newPath);
        setCustomPath(newPath);
    };

    const handleGoUp = () => {
        const parts = currentPath.split(separator).filter(Boolean);
        if (parts.length <= 1) {
            setCurrentPath('');
            setCustomPath('');
        } else {
            let parentPath = parts.slice(0, -1).join(separator);
            if (currentPath.match(/^[a-zA-Z]:/) && parts.length === 2) {
                parentPath += separator; // e.g. C:\
            }
            setCurrentPath(parentPath);
            setCustomPath(parentPath);
        }
    };

    const handleSelectFolder = (folderPath: string) => {
        setCurrentPath(folderPath);
        setCustomPath(folderPath);
    };

    const handleConfirm = () => {
        onSelect(customPath);
        onClose();
    };

    // Filter subdirectories based on search query
    const filteredItems = (currentPath === '' ? drives : subdirs).filter((item) => {
        const name = currentPath === '' ? item : item.split(separator).pop() || '';
        return name.toLowerCase().includes(searchQuery.toLowerCase());
    });

    const getFolderName = (item: string) => {
        if (currentPath === '') return item;
        return item.split(separator).pop() || '';
    };

    return (
        <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
            <DialogContent className="max-w-2xl w-[90vw] h-[80vh] flex flex-col p-6 rounded-2xl gap-4 bg-background">
                <DialogHeader className="text-right">
                    <DialogTitle className="flex items-center gap-2 text-xl font-bold text-primary">
                        <FolderOpen className="w-6 h-6 text-primary" />
                        <span>مستكشف مجلدات الخادم</span>
                    </DialogTitle>
                </DialogHeader>

                {/* Path display & Navigation bar */}
                <div className="flex flex-col gap-2">
                    <div className="flex items-center gap-2">
                        <Button
                            variant="outline"
                            size="icon"
                            disabled={!currentPath || isLoading}
                            onClick={handleGoUp}
                            title="المجلد الأعلى"
                            className="shrink-0"
                        >
                            <ArrowUp className="w-4 h-4" />
                        </Button>
                        <div className="flex-1 flex items-center gap-1 bg-muted/30 px-3 py-2 rounded-xl overflow-x-auto border text-sm text-muted-foreground whitespace-nowrap min-h-[40px]">
                            <button
                                onClick={() => { setCurrentPath(''); setCustomPath(''); }}
                                className="hover:text-primary hover:underline font-medium"
                            >
                                خادم النظام
                            </button>
                            {breadcrumbs.map((crumb, idx) => (
                                <React.Fragment key={idx}>
                                    <ChevronLeft className="w-3.5 h-3.5 shrink-0 text-muted-foreground/50" />
                                    <button
                                        onClick={() => handleBreadcrumbClick(idx)}
                                        className="hover:text-primary hover:underline font-medium"
                                    >
                                        {crumb}
                                    </button>
                                </React.Fragment>
                            ))}
                        </div>
                    </div>

                    {/* Manual Path Editing */}
                    <div className="flex items-center gap-2">
                        <Input
                            value={customPath}
                            onChange={(e) => setCustomPath(e.target.value)}
                            placeholder="المسار الحالي..."
                            className="font-mono text-xs text-left"
                            dir="ltr"
                        />
                    </div>
                </div>

                {/* Search / Filter */}
                <div className="relative shrink-0">
                    <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                    <Input
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        placeholder="ابحث عن مجلد..."
                        className="pr-10"
                    />
                </div>

                {/* Directory Content Area */}
                <div className="flex-1 overflow-y-auto border rounded-xl bg-muted/10 p-2 min-h-0">
                    {isLoading ? (
                        <div className="h-full flex flex-col items-center justify-center gap-2 text-muted-foreground">
                            <Loader2 className="w-8 h-8 animate-spin text-primary" />
                            <span className="text-sm">جاري تحميل المجلدات...</span>
                        </div>
                    ) : isError ? (
                        <div className="h-full flex items-center justify-center p-4">
                            <Alert variant="destructive" className="max-w-md">
                                <AlertDescription className="text-center font-medium">
                                    {subdirsError && 'message' in (subdirsError as any)
                                        ? (subdirsError as any).message
                                        : 'فشل تحميل محتويات المجلد. يرجى التحقق من المسار أو صلاحيات الوصول.'}
                                </AlertDescription>
                            </Alert>
                        </div>
                    ) : filteredItems.length === 0 ? (
                        <div className="h-full flex items-center justify-center text-muted-foreground text-sm">
                            لا توجد مجلدات تطابق البحث أو المجلد فارغ
                        </div>
                    ) : (
                        <div className="grid grid-cols-1 sm:grid-cols-2 gap-1">
                            {filteredItems.map((item, idx) => {
                                const isDrive = currentPath === '';
                                const isSelected = customPath === item;
                                return (
                                    <button
                                        key={idx}
                                        onClick={() => handleSelectFolder(item)}
                                        onDoubleClick={() => {
                                            setCurrentPath(item);
                                            setCustomPath(item);
                                            setSearchQuery('');
                                        }}
                                        className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-right text-sm transition-all duration-200 border ${
                                            isSelected
                                                ? 'bg-primary/10 border-primary text-primary font-medium shadow-sm'
                                                : 'hover:bg-accent border-transparent text-foreground'
                                        }`}
                                    >
                                        {isDrive ? (
                                            <HardDrive className={`w-5 h-5 shrink-0 ${isSelected ? 'text-primary' : 'text-muted-foreground/70'}`} />
                                        ) : (
                                            <Folder className={`w-5 h-5 shrink-0 ${isSelected ? 'text-primary' : 'text-amber-500/80'}`} />
                                        )}
                                        <span className="truncate" dir="ltr">
                                            {getFolderName(item)}
                                        </span>
                                    </button>
                                );
                            })}
                        </div>
                    )}
                </div>

                <DialogFooter className="flex flex-row justify-between items-center sm:justify-between shrink-0 gap-2">
                    <Button variant="outline" onClick={onClose} className="rounded-xl">
                        إلغاء
                    </Button>
                    <Button
                        onClick={handleConfirm}
                        disabled={!customPath || isLoading}
                        className="rounded-xl px-6"
                    >
                        تحديد المجلد
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
};
