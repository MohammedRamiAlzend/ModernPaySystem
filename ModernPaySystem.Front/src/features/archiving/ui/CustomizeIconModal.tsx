import { useState } from 'react';
import { Folder } from '../model/types';
import { useFolderIcons } from '../model/queries';
import { useAssignIconToFolder } from '../model/mutations';
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from '@/shared/ui/dialog';
import { Button } from '@/shared/ui/button';
import { Loader2, Folder as FolderIconLucide, X } from 'lucide-react';

interface CustomizeIconModalProps {
    isOpen: boolean;
    folder: Folder | null;
    onClose: () => void;
}

export const CustomizeIconModal: React.FC<CustomizeIconModalProps> = ({ isOpen, folder, onClose }) => {
    const { data: icons = [], isLoading } = useFolderIcons();
    const assignIcon = useAssignIconToFolder();
    const [selectedIconId, setSelectedIconId] = useState<string | null>(null);

    if (!folder) return null;

    const currentIconId = selectedIconId ?? folder.iconId ?? null;

    const handleAssign = async () => {
        if (!folder) return;
        await assignIcon.mutateAsync({ folderId: folder.id, iconId: selectedIconId });
        onClose();
    };

    const handleRemoveIcon = async () => {
        if (!folder) return;
        await assignIcon.mutateAsync({ folderId: folder.id, iconId: null });
        onClose();
    };

    return (
        <Dialog open={isOpen} onOpenChange={onClose}>
            <DialogContent className="sm:max-w-2xl max-h-[80vh] overflow-y-auto">
                <DialogHeader>
                    <DialogTitle className="text-right">تخصيص أيقونة المجلد: {folder.name}</DialogTitle>
                </DialogHeader>

                <div className="flex flex-col gap-4 py-4 text-right">
                    {isLoading ? (
                        <div className="flex items-center justify-center py-12">
                            <Loader2 className="h-8 w-8 animate-spin text-primary" />
                        </div>
                    ) : (
                        <>
                            {/* Remove icon option */}
                            <div className="flex items-center gap-3 p-2">
                                <button
                                    onClick={() => setSelectedIconId(null)}
                                    className={`w-16 h-16 rounded-xl border-2 flex items-center justify-center transition-all ${
                                        currentIconId === null
                                            ? 'border-primary bg-primary/10'
                                            : 'border-border hover:border-muted-foreground/30'
                                    }`}
                                >
                                    <FolderIconLucide className="h-8 w-8 text-amber-500" />
                                </button>
                                <div className="flex flex-col">
                                    <span className="text-sm font-bold">الأيقونة الافتراضية</span>
                                    <span className="text-xs text-muted-foreground">إعادة تعيين إلى الأيقونة الافتراضية للنظام</span>
                                </div>
                            </div>

                            {/* Icon Grid */}
                            <div className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 gap-3">
                                {icons.map((icon) => (
                                    <button
                                        key={icon.id}
                                        onClick={() => setSelectedIconId(icon.id)}
                                        className={`flex flex-col items-center gap-1 p-2 rounded-xl border-2 transition-all ${
                                            currentIconId === icon.id
                                                ? 'border-primary bg-primary/10'
                                                : 'border-transparent hover:border-muted-foreground/20'
                                        }`}
                                    >
                                        <div className="w-12 h-12 rounded-lg bg-amber-500/10 flex items-center justify-center">
                                            <div className="w-8 h-8 flex items-center justify-center overflow-hidden [&_svg]:w-full [&_svg]:h-full [&_svg]:max-w-full [&_svg]:max-h-full" dangerouslySetInnerHTML={{ __html: icon.svgContent }} />
                                        </div>
                                        <span className="text-[10px] font-bold text-muted-foreground line-clamp-1">{icon.name}</span>
                                    </button>
                                ))}
                            </div>
                        </>
                    )}
                </div>

                <div className="flex items-center justify-between gap-3 pt-2 border-t border-border">
                    <Button variant="outline" onClick={onClose} className="rounded-xl font-bold">
                        إلغاء
                    </Button>
                    <div className="flex items-center gap-2">
                        {folder.iconId && (
                            <Button variant="destructive" onClick={handleRemoveIcon} className="rounded-xl font-bold" disabled={assignIcon.isPending}>
                                {assignIcon.isPending ? <Loader2 className="h-4 w-4 animate-spin ml-2" /> : <X className="h-4 w-4 ml-2" />}
                                إزالة الأيقونة
                            </Button>
                        )}
                        <Button onClick={handleAssign} disabled={assignIcon.isPending || selectedIconId === folder.iconId} className="rounded-xl font-bold">
                            {assignIcon.isPending ? <Loader2 className="h-4 w-4 animate-spin ml-2" /> : null}
                            حفظ
                        </Button>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
};
